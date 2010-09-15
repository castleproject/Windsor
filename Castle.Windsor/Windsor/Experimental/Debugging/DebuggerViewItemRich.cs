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

namespace Castle.Windsor.Experimental.Debugging
{
	using System.Diagnostics;

	[DebuggerDisplay("{key}", Name = "{name,nq}")]
	public class DebuggerViewItemRich
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string name;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string key;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		private object value;

		public DebuggerViewItemRich(string name, string key, object value)
		{
			this.name = name;
			this.key = key;
			this.value = value;
		}
	}
}