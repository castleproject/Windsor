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
	using System.Threading;

	public class ConcurrentHashSet<T> : ICollection<T>
	{
		private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
		private readonly HashSet<T> implementation = new HashSet<T>();

		public void Add(T item)
		{
			try
			{
				@lock.EnterWriteLock();
				implementation.Add(item);
			}
			finally
			{
				if (@lock.IsWriteLockHeld)
					@lock.ExitWriteLock();
			}
		}

		public void Clear()
		{
			try
			{
				@lock.EnterWriteLock();
				implementation.Clear();
			}
			finally
			{
				if (@lock.IsWriteLockHeld)
					@lock.ExitWriteLock();
			}
		}

		public bool Contains(T item)
		{
			try
			{
				@lock.EnterReadLock();
				return implementation.Contains(item);
			}
			finally
			{
				if (@lock.IsReadLockHeld)
					@lock.ExitReadLock();
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			try
			{
				@lock.EnterReadLock();
				implementation.CopyTo(array, arrayIndex);
			}
			finally
			{
				if (@lock.IsReadLockHeld)
					@lock.ExitReadLock();
			}
		}

		public bool Remove(T item)
		{
			try
			{
				@lock.EnterWriteLock();
				return implementation.Remove(item);
			}
			finally
			{
				if (@lock.IsWriteLockHeld)
					@lock.ExitWriteLock();
			}
		}

		public int Count
		{
			get
			{
				try
				{
					@lock.EnterReadLock();
					return implementation.Count;
				}
				finally
				{
					if (@lock.IsReadLockHeld)
						@lock.ExitReadLock();
				}
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			try
			{
				@lock.EnterReadLock();
				var hashSetCopy = new List<T>(implementation).AsReadOnly();
				return hashSetCopy.GetEnumerator();
			}
			finally
			{
				if (@lock.IsReadLockHeld)
					@lock.ExitReadLock();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
