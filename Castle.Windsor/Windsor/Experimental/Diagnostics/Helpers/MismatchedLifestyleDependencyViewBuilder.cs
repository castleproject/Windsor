// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Experimental.Diagnostics.Helpers
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.Windsor.Experimental.Diagnostics.DebuggerViews;

#if !SILVERLIGHT
	public class MismatchedLifestyleDependencyViewBuilder
	{
		private readonly IHandler handler;
		private readonly MismatchedLifestyleDependencyViewBuilder parent;

		public MismatchedLifestyleDependencyViewBuilder(IHandler handler, MismatchedLifestyleDependencyViewBuilder parent = null)
		{
			this.handler = handler;
			this.parent = parent;
		}

		public IHandler Handler
		{
			get { return handler; }
		}

		public DebuggerViewItem ViewItem
		{
			get
			{
				var item = GetItem();
				var key = GetKey();
				var name = GetName(item.Handlers.First());
				return new DebuggerViewItem(name, key, item);
			}
		}

		private ComponentModel Model
		{
			get { return handler.ComponentModel; }
		}

		private string Name
		{
			get { return Model.Name; }
		}

		public bool Mismatched()
		{
			return Model.LifestyleType == LifestyleType.PerWebRequest ||
			       Model.LifestyleType == LifestyleType.Transient;
		}

		private void ContributeItem(IHandler mismatched, StringBuilder message, List<IHandler> items)
		{
			if (ImTheRoot())
			{
				items.Add(handler);
				message.AppendFormat("Component '{0}' with lifestyle {1} ", Name,
				                     Model.GetLifestyleDescription());

				message.AppendFormat("depends on '{0}' with lifestyle {1}",
				                     mismatched.ComponentModel.Name,
				                     mismatched.ComponentModel.GetLifestyleDescription());
				return;
			}
			parent.ContributeItem(mismatched, message, items);
			items.Add(handler);
			message.AppendLine();
			message.AppendFormat("\tvia '{0}' with lifestyle {1}", Name,
			                     Model.GetLifestyleDescription());
		}

		private MismatchedDependencyDebuggerViewItem GetItem()
		{
			var items = new List<IHandler>();
			var message = GetMismatchMessage(items);
			return new MismatchedDependencyDebuggerViewItem(message, items.ToArray());
		}

		private string GetKey()
		{
			return string.Format("\"{0}\" »{1}«", Name, Model.GetLifestyleDescription());
		}

		private string GetMismatchMessage(List<IHandler> items)
		{
			var message = new StringBuilder();
			Debug.Assert(parent != null, "parent != null");
			//now we're going down letting the root to append first:
			parent.ContributeItem(handler, message, items);
			items.Add(handler);

			message.AppendLine();
			message.AppendFormat(
				"This kind of dependency is usually not desired and may lead to various kinds of bugs.");
			return message.ToString();
		}

		private string GetName(IHandler root)
		{
			var indirect = (parent.Handler == root) ? string.Empty : "indirectly ";
			return string.Format("\"{0}\" »{1}« {2}depends on", root.ComponentModel.Name, root.ComponentModel.GetLifestyleDescription(), indirect);
		}

		private bool ImTheRoot()
		{
			return parent == null;
		}
	}
#endif
}