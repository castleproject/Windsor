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

namespace Castle.Windsor.Diagnostics.DebuggerViews
{
#if !SILVERLIGHT
	using System.Diagnostics;

	public class ReleasePolicyTrackedObjectsDebuggerViewItem
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly ComponentDebuggerView component;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly object[] instances;

		public ReleasePolicyTrackedObjectsDebuggerViewItem(ComponentDebuggerView component, object[] instances)
		{
			this.component = component;
			this.instances = instances;
		}

		[DebuggerDisplay("{component.Description,nq}")]
		public ComponentDebuggerView Component
		{
			get { return component; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Instances
		{
			get { return instances; }
		}
	}
#endif
}