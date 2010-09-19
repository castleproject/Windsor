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
	using System.Diagnostics;
	using System.Linq;

	public class MismatchedDependency
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly MetaComponent[] components;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string description;

		public MismatchedDependency(string description, MetaComponent[] components)
		{
			this.components = components;
			this.description = description;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public MetaComponent[] Components
		{
			get { return components; }
		}

		public string Description
		{
			get { return description; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public DebuggerViewItem[] ViewItems
		{
			get { return components.Select(BuildComponentView).ToArray(); }
		}

		private DebuggerViewItem BuildComponentView(MetaComponent component)
		{
			var defaultExtension = new DefaultComponentView(component.Handler, component.ForwardedTypes);
			var item = new ComponentDebuggerView(component, defaultExtension);
			return new DebuggerViewItem(component.Name, component.Model.GetLifestyleDescription(), item);
		}
	}
}