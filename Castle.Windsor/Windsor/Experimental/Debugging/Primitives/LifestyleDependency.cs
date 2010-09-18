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

	public class LifestyleDependency : IComponentDebuggerExtension
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly IHandler handler;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly LifestyleDependency parent;

		private readonly KeyValuePair<string, IList<Type>> value;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public string Name
		{
			get { return value.Key; }
		}

		public LifestyleDependency(IHandler handler, KeyValuePair<string, IList<Type>> value,
		                           LifestyleDependency parent = null)
		{
			this.handler = handler;
			this.value = value;
			this.parent = parent;
		}

		public bool Mismatched()
		{
			return MismatchedDirectly(handler);
		}

		public IEnumerable<DebuggerViewItem> Attach()
		{
			yield return new DebuggerViewItem(GetName(), GetKey(), GetItem());
		}

		private void AppendMismatchMessage(StringBuilder message, LifestyleDependency mismatched)
		{
			if (ImTheRoot())
			{
				message.AppendFormat("Component '{0}' with lifestyle {1} ", value.Key,
				                     GetLifestyleDescription(handler.ComponentModel));

				message.AppendFormat("depends on '{0}' with lifestyle {1}", mismatched.value.Key,
				                     GetLifestyleDescription(mismatched.handler.ComponentModel));
				return;
			}
			parent.AppendMismatchMessage(message, mismatched);
			message.AppendLine();
			message.AppendFormat("\tvia '{0}' with lifestyle {1}", value.Key,
			                     GetLifestyleDescription(handler.ComponentModel));
		}

		private MismatchedDependency GetItem()
		{
			return new MismatchedDependency(new DefaultComponentView(handler, value.Value.ToArray()),
			                                GetMismatchMessage());
		}

		private string GetKey()
		{
			return handler.ComponentModel.LifestyleType.ToString();
		}

		private string GetLifestyleDescription(ComponentModel componentModel)
		{
			if (componentModel.LifestyleType != LifestyleType.Custom)
			{
				return componentModel.LifestyleType.ToString();
			}
			return string.Format("custom ({0})", componentModel.CustomLifestyle.FullName);
		}

		private string GetMismatchMessage()
		{
			var message = new StringBuilder();
			Debug.Assert(parent != null, "parent != null");
			//now we're going down letting the root to append first:
			parent.AppendMismatchMessage(message, this);
			message.AppendLine();
			message.AppendFormat(
				"This kind of dependency is usually not desired and may lead to various kinds of bugs.");
			return message.ToString();
		}

		private string GetName()
		{
			return string.Format("Mismatched {0}depedency on \"{1}\"",
			                     (MismatchedDirectly(handler) ? "" : "indirect "),
			                     value.Key);
		}

		private bool ImTheRoot()
		{
			return parent == null;
		}

		private bool MismatchedDirectly(IHandler handler)
		{
			return handler.ComponentModel.LifestyleType == LifestyleType.Transient ||
			       handler.ComponentModel.LifestyleType == LifestyleType.PerWebRequest;
		}
	}
}