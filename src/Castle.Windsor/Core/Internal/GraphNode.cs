// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Core.Internal
{
	using System;
	using System.Threading;

	[Serializable]
	public class GraphNode :
#if !SILVERLIGHT
		MarshalByRefObject,
#endif
		IVertex
	{
		private SimpleThreadSafeCollection<GraphNode> outgoing;

		public void AddDependent(GraphNode node)
		{
			var collection = outgoing;
			if (collection == null)
			{
				var @new = new SimpleThreadSafeCollection<GraphNode>();
				collection = Interlocked.CompareExchange(ref outgoing, @new, null) ?? @new;
			}
			collection.Add(node);
		}

		/// <summary>The nodes that this node depends on</summary>
		public GraphNode[] Dependents
		{
			get
			{
				var collection = outgoing;
				if (collection == null)
				{
					return new GraphNode[0];
				}
				return collection.ToArray();
			}
		}

		IVertex[] IVertex.Adjacencies
		{
			get { return Dependents; }
		}
	}
}