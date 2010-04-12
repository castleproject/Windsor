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

	public class TypedArgumentsStore : IArgumentsStore
	{
		private readonly IDictionary<Type, object> arguments = new Dictionary<Type, object>();

		public int Count
		{
			get { return arguments.Count; }
		}

		public void Add(object key, object value)
		{
			VerifyKey(key);
			VerifyValueType((Type)key, value);

			arguments.Add((Type)key, value);
		}

		public void Clear()
		{
			arguments.Clear();
		}

		public bool Contains(object key)
		{
			if (!(key is Type))
			{
				return false;
			}

			return arguments.ContainsKey(key as Type);
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
			arguments.TryGetValue((Type)key, out value);
			return value;
		}

		public void Insert(object key, object value)
		{
			VerifyKey(key);
			VerifyValueType((Type)key, value);

			arguments[(Type)key] = value;
		}

		public void Remove(object key)
		{
			VerifyKey(key);
			arguments.Remove((Type)key);
		}

		public bool Supports(Type keyType)
		{
			return typeof(Type).IsAssignableFrom(keyType);
		}

		private void VerifyKey(object key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (!(key is Type))
			{
				throw new ArgumentException(string.Format("Key type {0} is not supported.", key.GetType()));
			}
		}

		private void VerifyValueType(Type key, object value)
		{
			if (!key.IsInstanceOfType(value))
			{
				throw new ArgumentException(
					string.Format("Value '{0}' of type {1} is not assignable to type {2}, which was used as its key.",
					              value ?? "NULL", value != null ? value.GetType().ToString() : "UNKNOWN", key.GetType()));
			}
		}
	}
}