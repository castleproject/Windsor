// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using System.Diagnostics;

#if !SILVERLIGHT
	[DebuggerDisplay("{description,nq}", Name = "{name,nq}")]
	public class DebuggerViewItem
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly object description;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string name;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		private readonly object value;

		public DebuggerViewItem(string name, string description, object value)
		{
			this.name = name;
			this.description = description;
			this.value = value;
		}

		public DebuggerViewItem(string name, object value)
		{
			this.name = name;
			description = value;
			this.value = value;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public object Description
		{
			get { return description; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public string Name
		{
			get { return name; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public object Value
		{
			get { return value; }
		}
	}
#endif
}