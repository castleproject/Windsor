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

namespace Castle.MicroKernel.Context
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Default arguments store used to store items where no specialized store exists
	/// </summary>
	public class FallbackArgumentsStore : IArgumentsStore
	{
		private readonly IDictionary<object ,object> items = new Dictionary<object, object>();

		public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Contains(object key)
		{
			return items.ContainsKey(key);
		}

		public int Count
		{
			get { return items.Count; }
		}

		public bool Supports(Type keyType)
		{
			// basically that means we support any type
			return keyType != null;
		}

		public void Add(object key, object value)
		{
			items.Add(key, value);
		}

		public void Clear()
		{
			items.Clear();
		}

		public bool Remove(object key)
		{
			return items.Remove(key);
		}

		public bool Insert(object key, object value)
		{
			var isOverwriting = items.ContainsKey(key);
			items[key] = value;
			return isOverwriting == false;
		}

		public object GetItem(object key)
		{
			object value;
			items.TryGetValue(key, out value);
			return value;
		}
	}
}