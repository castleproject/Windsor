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
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	///   Simple type for thread safe adding/reading to/from keyed store. The difference between this and built in concurrent dictionary is that in this case adding is happening under a lock so never more than one thread will be adding at a time.
	/// </summary>
	/// <typeparam name="TKey"> </typeparam>
	/// <typeparam name="TValue"> </typeparam>
	public class SimpleThreadSafeDictionary<TKey, TValue> : IDisposable
	{
		private readonly Dictionary<TKey, TValue> inner = new Dictionary<TKey, TValue>();
		private readonly Lock @lock = Lock.Create();
		private bool disposed;

		public bool Contains(TKey key)
		{
			using (@lock.ForReading())
			{
				return inner.ContainsKey(key);
			}
		}

		public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				TValue value;
				if (TryGet(key, out value))
				{
					return value;
				}
				token.Upgrade();
				if (TryGet(key, out value))
				{
					return value;
				}
				value = factory(key);
				inner.Add(key, value);
				return value;
			}
		}

		/// <summary>
		///   returns all values and clears the dictionary
		/// </summary>
		/// <returns> </returns>
		public void Dispose()
		{
			using (@lock.ForWriting())
			{
				disposed = true;
				var values = inner.Values.ToArray();
				inner.Clear();
				values.Reverse().OfType<IDisposable>().ForEach(d => d.Dispose());
			}
		}

		private bool TryGet(TKey key, out TValue value)
		{
			if (disposed)
			{
				throw new ObjectDisposedException("this");
			}
			return inner.TryGetValue(key, out value);
		}
	}
}