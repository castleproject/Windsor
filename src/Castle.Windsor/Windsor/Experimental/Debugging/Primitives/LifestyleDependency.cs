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

		public IHandler Handler
		{
			get { return component.Handler; }
		}

		public DebuggerViewItem ViewItem
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
			return
#if !SILVERLIGHT
				component.Model.LifestyleType == LifestyleType.PerWebRequest ||
#endif
				component.Model.LifestyleType == LifestyleType.Transient;
		}

		private void ContributeItem(MetaComponent mismatched, StringBuilder message, IList<MetaComponent> items)
		{
			if (ImTheRoot())
			{
				items.Add(component);
				message.AppendFormat("Component '{0}' with lifestyle {1} ", component.Name,
				                     component.Model.GetLifestyleDescription());

				message.AppendFormat("depends on '{0}' with lifestyle {1}", mismatched.Name,
				                     mismatched.Model.GetLifestyleDescription());
				return;
			}
			parent.ContributeItem(mismatched, message, items);
			items.Add(component);
			message.AppendLine();
			message.AppendFormat("\tvia '{0}' with lifestyle {1}", component.Name,
								 component.Model.GetLifestyleDescription());
		}

		private MismatchedDependency GetItem()
		{
			var items = new List<MetaComponent>();
			var message = GetMismatchMessage(items);
			return new MismatchedDependency(message, items.ToArray());
		}

		private string GetKey()
		{
			return string.Format("\"{0}\" {1}", component.Name, component.Model.GetLifestyleDescription());
		}

		private string GetMismatchMessage(IList<MetaComponent> items)
		{
			var message = new StringBuilder();
			Debug.Assert(parent != null, "parent != null");
			//now we're going down letting the root to append first:
			parent.ContributeItem(component, message, items);
			items.Add(component);

			message.AppendLine();
			message.AppendFormat(
				"This kind of dependency is usually not desired and may lead to various kinds of bugs.");
			return message.ToString();
		}

		private string GetName(MetaComponent root)
		{
			return string.Format("\"{0}\" {1} ->", root.Name, root.Model.GetLifestyleDescription());
		}

		private bool ImTheRoot()
		{
			return parent == null;
		}
	}
}