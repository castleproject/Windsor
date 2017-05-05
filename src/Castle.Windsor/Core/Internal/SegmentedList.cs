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

namespace Castle.Core.Internal
{
	using System.Collections.Generic;
	using System.Linq;

	public class SegmentedList<T>
	{
		private readonly List<T>[] segments;

		public SegmentedList(int segmentCount)
		{
			segments = new List<T>[segmentCount];
		}

		public void AddFirst(int segmentIndex, T item)
		{
			GetSegment(segmentIndex).Insert(0, item);
		}

		public void AddLast(int segmentIndex, T item)
		{
			GetSegment(segmentIndex).Add(item);
		}

		public T[] ToArray()
		{
			return segments.Where(l => l != null)
				.SelectMany(l => l)
				.ToArray();
		}

		private List<T> GetSegment(int segmentIndex)
		{
			var group = segments[segmentIndex];
			if (group == null)
			{
				group = segments[segmentIndex] = new List<T>(4);
			}
			return @group;
		}
	}
}