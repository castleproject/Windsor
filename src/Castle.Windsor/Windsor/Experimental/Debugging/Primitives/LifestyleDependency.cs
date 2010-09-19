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
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	using Castle.Core;
	using Castle.MicroKernel;

	public class LifestyleDependency
	{
		private readonly MetaComponent component;

		private readonly LifestyleDependency parent;

		public LifestyleDependency(MetaComponent component, LifestyleDependency parent = null)
		{
			this.component = component;
			this.parent = parent;
		}

		public DebuggerViewItem ComponentView
		{
			get
			{
				var item = new ComponentDebuggerView(component,
				                                     new DefaultComponentView(component.Handler, component.ForwardedTypes));
				return new DebuggerViewItem(component.Name, GetLifestyleDescription(component.Model), item);
			}
		}

		public IHandler Handler
		{
			get { return component.Handler; }
		}

		public DebuggerViewItem MismatchView
		{
			get
			{
				var item = GetItem();
				var key = GetKey();
				var name = GetName(item.Components.First());
				return new DebuggerViewItem(name, key, item);
			}
		}

		public string Name
		{
			get { return component.Name; }
		}

		public bool Mismatched()
		{
			return component.Model.LifestyleType == LifestyleType.Transient ||
			       component.Model.LifestyleType == LifestyleType.PerWebRequest;
		}

		private void ContributeItem(LifestyleDependency mismatched, StringBuilder message, IList<LifestyleDependency> items)
		{
			if (ImTheRoot())
			{
				items.Add(this);
				message.AppendFormat("Component '{0}' with lifestyle {1} ", component.Name,
				                     GetLifestyleDescription(component.Model));

				message.AppendFormat("depends on '{0}' with lifestyle {1}", mismatched.Name,
				                     GetLifestyleDescription(mismatched.Handler.ComponentModel));
				return;
			}
			parent.ContributeItem(mismatched, message, items);
			items.Add(this);
			message.AppendLine();
			message.AppendFormat("\tvia '{0}' with lifestyle {1}", component.Name,
			                     GetLifestyleDescription(component.Model));
		}

		private MismatchedDependency GetItem()
		{
			var items = new List<LifestyleDependency>();
			var message = GetMismatchMessage(items);
			return new MismatchedDependency(message, items.ToArray());
		}

		private string GetKey()
		{
			return string.Format("\"{0}\" {1}", component.Name, GetLifestyleDescription(component.Model));
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
			return string.Format("\"{0}\" {1} ->", root.Name, GetLifestyleDescription(root.Handler.ComponentModel));
		}

		private bool ImTheRoot()
		{
			return parent == null;
		}
	}
}