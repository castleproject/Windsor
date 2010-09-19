// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Windsor.Experimental.Debugging.Primitives
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	using Castle.Core;
	using Castle.MicroKernel;

	public class LifestyleDependency
	{
		private readonly IHandler handler;

		private readonly LifestyleDependency parent;

		private readonly KeyValuePair<string, IList<Type>> value;

		public LifestyleDependency(IHandler handler, KeyValuePair<string, IList<Type>> value,
		                           LifestyleDependency parent = null)
		{
			this.handler = handler;
			this.value = value;
			this.parent = parent;
		}

		public IHandler Handler
		{
			get { return handler; }
		}

		public string Name
		{
			get { return value.Key; }
		}

		public DebuggerViewItem BuildItem()
		{
			LifestyleDependency root;
			var item = GetItem(out root);
			return new DebuggerViewItem(GetName(root), GetKey(), item);
		}

		public DebuggerViewItem GetItemView()
		{
			var item = new ComponentDebuggerView(handler, value, new DefaultComponentView(handler, value.Value.ToArray()));
			return new DebuggerViewItem(value.Key, GetLifestyleDescription(handler.ComponentModel), item);
		}

		public bool Mismatched()
		{
			return MismatchedDirectly();
		}

		private void ContributeItem(LifestyleDependency mismatched, StringBuilder message, IList<LifestyleDependency> items)
		{
			if (ImTheRoot())
			{
				items.Add(this);
				message.AppendFormat("Component '{0}' with lifestyle {1} ", value.Key,
				                     GetLifestyleDescription(handler.ComponentModel));

				message.AppendFormat("depends on '{0}' with lifestyle {1}", mismatched.value.Key,
				                     GetLifestyleDescription(mismatched.handler.ComponentModel));
				return;
			}
			parent.ContributeItem(mismatched, message, items);
			items.Add(this);
			message.AppendLine();
			message.AppendFormat("\tvia '{0}' with lifestyle {1}", value.Key,
			                     GetLifestyleDescription(handler.ComponentModel));
		}

		private MismatchedDependency GetItem(out LifestyleDependency root)
		{
			var items = new List<LifestyleDependency>();
			var message = GetMismatchMessage(items);
			root = items.First();
			return new MismatchedDependency(message, items.ToArray());
		}

		private string GetKey()
		{
			return string.Format("\"{0}\" {1}", value.Key, GetLifestyleDescription(handler.ComponentModel));
		}

		private string GetLifestyleDescription(ComponentModel componentModel)
		{
			if (componentModel.LifestyleType != LifestyleType.Custom)
			{
				return componentModel.LifestyleType.ToString();
			}
			return string.Format("custom ({0})", componentModel.CustomLifestyle.FullName);
		}

		private string GetMismatchMessage(IList<LifestyleDependency> items)
		{
			var message = new StringBuilder();
			Debug.Assert(parent != null, "parent != null");
			//now we're going down letting the root to append first:
			parent.ContributeItem(this, message, items);
			items.Add(this);

			message.AppendLine();
			message.AppendFormat(
				"This kind of dependency is usually not desired and may lead to various kinds of bugs.");
			return message.ToString();
		}

		private string GetName(LifestyleDependency root)
		{
			return string.Format("\"{0}\" {1} ->", root.value.Key, GetLifestyleDescription(root.handler.ComponentModel));
		}

		private bool ImTheRoot()
		{
			return parent == null;
		}

		private bool MismatchedDirectly()
		{
			return handler.ComponentModel.LifestyleType == LifestyleType.Transient ||
			       handler.ComponentModel.LifestyleType == LifestyleType.PerWebRequest;
		}
	}
}