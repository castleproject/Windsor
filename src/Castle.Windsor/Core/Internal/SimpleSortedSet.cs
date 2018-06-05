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
	using System.Collections;
	using System.Collections.Generic;

	public class SimpleSortedSet<T> : ICollection<T>
	{
		private readonly IComparer<T> comparer;
		private readonly List<T> items = new List<T>();

		public SimpleSortedSet() : this(Comparer<T>.Default)
		{
		}

		public SimpleSortedSet(IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public SimpleSortedSet(IEnumerable<T> other, IComparer<T> comparer) : this(comparer)
		{
			foreach (var item in other)
			{
				Add(item);
			}
		}

		public T this[int index]
		{
			get { return items[index]; }
		}

		public int Count
		{
			get { return items.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		public void Add(T item)
		{
			var count = Count;
			for (var i = 0; i < count; i++)
			{
				var result = comparer.Compare(item, items[i]);
				if (result < 0)
				{
					items.Insert(i, item);
					return;
				}
				if (result == 0)
				{
					return;
				}
			}
			items.Add(item);
		}

		public void Clear()
		{
			items.Clear();
		}

		public bool Contains(T item)
		{
			return items.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			items.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return items.Remove(item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}