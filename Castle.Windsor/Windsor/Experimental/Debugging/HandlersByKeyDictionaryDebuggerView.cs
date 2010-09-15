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
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.MicroKernel;

	public class HandlersByKeyDictionaryDebuggerView
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly HandlerByKeyDebuggerView[] items;

		public HandlersByKeyDictionaryDebuggerView(IEnumerable<KeyValuePair<string, IHandler>> key2Handler)
		{
			items = key2Handler.Select(h =>
			                           new HandlerByKeyDebuggerView(
			                           	h.Key,
			                           	h.Value,
			                           	new DefaultComponentView(h.Value)
			                           	)).ToArray();
		}


		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public HandlerByKeyDebuggerView[] Items
		{
			get { return items; }
		}
	}
}