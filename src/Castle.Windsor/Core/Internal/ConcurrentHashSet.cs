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
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;

	public class ConcurrentHashSet<T> : ICollection<T>
	{
		private readonly ConcurrentDictionary<T, bool> implementation;

		public ConcurrentHashSet()
		{
			implementation = new ConcurrentDictionary<T, bool>();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return implementation.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T item)
		{
			implementation.TryAdd(item, true);
		}

		public void Clear()
		{
			implementation.Clear();
		}

		public bool Contains(T item)
		{
			return implementation.ContainsKey(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			implementation.Keys.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			bool dummy;
			return implementation.TryRemove(item, out dummy);
		}

		public int Count 
		{
			get { return implementation.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}
	}
}
