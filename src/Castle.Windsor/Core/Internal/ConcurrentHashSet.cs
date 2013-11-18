// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//	 http://www.apache.org/licenses/LICENSE-2.0
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

	public class ConcurrentHashSet<T> : ICollection<T>
	{
		private readonly Lock @lock = Lock.Create();
		private readonly HashSet<T> implementation = new HashSet<T>();

		public void Add(T item)
		{
            using (@lock.ForWriting())
            {
                implementation.Add(item);
            }
		}

		public void Clear()
		{
            using (@lock.ForWriting())
            {
                implementation.Clear();
            }
		}

		public bool Contains(T item)
		{
            using (@lock.ForReading())
            {
                return implementation.Contains(item);
            }
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
            using (@lock.ForReading())
            {
                implementation.CopyTo(array, arrayIndex);
            }
		}

		public bool Remove(T item)
		{
            using (@lock.ForWriting())
            {
                return implementation.Remove(item);
            }
		}

		public int Count
		{
			get
			{
                using (@lock.ForReading())
                {
                    return implementation.Count;
                }
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public IEnumerator<T> GetEnumerator()
		{
            using (@lock.ForReading())
            {
                var hashSetCopy = new List<T>(implementation).AsReadOnly();
                return hashSetCopy.GetEnumerator();
            }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
