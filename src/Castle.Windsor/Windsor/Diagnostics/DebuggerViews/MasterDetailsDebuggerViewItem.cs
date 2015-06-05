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
#if !SILVERLIGHT
	using System.Diagnostics;

	public class MasterDetailsDebuggerViewItem
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly object[] details;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly object master;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string masterDescription;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string masterName;

		public MasterDetailsDebuggerViewItem(object master, string masterDescription, string masterName, object[] details)
		{
			this.master = master;
			this.masterDescription = masterDescription;
			this.masterName = masterName;
			this.details = details;
		}

		/// <summary>
		///   Stupid name, but debugger views in Visual Studio display items in alphabetical order so if we want
		///   to have that item on top its name must be alphabetically before <see cref = "Details" />
		/// </summary>
		[DebuggerDisplay("{masterDescription,nq}", Name = "{masterName,nq}")]
		public object AMaster
		{
			get { return master; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Details
		{
			get { return details; }
		}
	}
#endif
}