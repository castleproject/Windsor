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
	using System.Collections.Generic;

	public class SimpleThreadSafeCollection<T>
	{
		private readonly List<T> implementation = new List<T>();
		private readonly Lock @lock = Lock.Create();

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

		public void Add(T item)
		{
			using (@lock.ForWriting())
			{
				implementation.Add(item);
			}
		}

		public bool Remove(T item)
		{
			using (@lock.ForWriting())
			{
				return implementation.Remove(item);
			}
		}

		public T[] ToArray()
		{
			using (@lock.ForReading())
			{
				return implementation.ToArray();
			}
		}
	}
}