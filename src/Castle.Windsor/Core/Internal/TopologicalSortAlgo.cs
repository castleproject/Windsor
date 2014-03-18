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

namespace Castle.Core.Internal
{
	using System.Collections.Generic;
	using System.Diagnostics;

	public abstract class TopologicalSortAlgo
	{
		public static IVertex[] Sort(IVertex[] graphNodes)
		{
			var colors = new ColorsSet(graphNodes);
			var discovery = new TimestampSet();
			var finish = new TimestampSet();
			var list = new LinkedList<IVertex>();

			var time = 0;

			foreach (var node in graphNodes)
			{
				if (colors.ColorOf(node) == VertexColor.White)
				{
					Visit(node, colors, discovery, finish, list, ref time);
				}
			}

			var vertices = new IVertex[list.Count];
			list.CopyTo(vertices, 0);
			return vertices;
		}

		private static void Visit(IVertex node, ColorsSet colors, TimestampSet discovery, TimestampSet finish, LinkedList<IVertex> list, ref int time)
		{
			colors.Set(node, VertexColor.Gray);

			discovery.Register(node, time++);

			foreach (var child in node.Adjacencies)
			{
				if (colors.ColorOf(child) == VertexColor.White)
				{
					Visit(child, colors, discovery, finish, list, ref time);
				}
			}

			finish.Register(node, time++);

			Debug.Assert(discovery.TimeOf(node) < finish.TimeOf(node));

			list.AddFirst(node);

			colors.Set(node, VertexColor.Black);
		}
	}
}