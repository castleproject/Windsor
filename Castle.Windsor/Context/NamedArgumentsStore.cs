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

	public class NamedArgumentsStore : IArgumentsStore
	{
		private readonly IDictionary<string, object> arguments =
			new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		public int Count
		{
			get { return arguments.Count; }
		}

		public void Add(object key, object value)
		{
			VerifyKey(key);
			arguments.Add((string)key, value);
		}

		public void Clear()
		{
			arguments.Clear();
		}

		public bool Contains(object key)
		{
			if (!(key is string))
			{
				return false;
			}

			return arguments.ContainsKey((string)key);
		}

		public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
		{
			foreach (var item in arguments)
			{
				yield return new KeyValuePair<object, object>(item.Key, item.Value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public object GetItem(object key)
		{
			VerifyKey(key);
			object value;
			arguments.TryGetValue((string)key, out value);
			return value;
		}

		public void Insert(object key, object value)
		{
			VerifyKey(key);
			arguments[(string)key] = value;
		}

		public void Remove(object key)
		{
			VerifyKey(key);
			arguments.Remove((string)key);
		}

		public bool Supports(Type keyType)
		{
			return keyType == typeof(string);
		}

		private void VerifyKey(object key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (!(key is string))
			{
				throw new ArgumentException(string.Format("Key type {0} is not supported.", key.GetType()));
			}
		}
	}
}