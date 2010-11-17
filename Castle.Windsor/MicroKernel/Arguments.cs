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

namespace Castle.MicroKernel
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	/// <summary>
	///   Represents collection of arguments used when resolving a component.
	/// </summary>
	public class Arguments : IDictionary
	{
		private readonly IList<IArgumentsStore> stores = new List<IArgumentsStore>();
		private readonly object syncRoot = new object();
		private int count;

		public Arguments(object namedArgumentsAsAnonymousType, params IArgumentsStore[] customStores)
			: this(new ReflectionBasedDictionaryAdapter(namedArgumentsAsAnonymousType), customStores)
		{
		}

		public Arguments(IDictionary values, params IArgumentsStore[] customStores)
			: this(customStores)
		{
			foreach (DictionaryEntry entry in values)
			{
				Add(entry.Key, entry.Value);
			}
		}

		public Arguments(object[] typedArguments, params IArgumentsStore[] customStores)
			: this(customStores)
		{
			foreach (var @object in typedArguments)
			{
				if (@object == null)
				{
					throw new ArgumentNullException("typedArguments",
					                                "Given array has null values. Only non-null values can be used as arguments.");
				}

				Add(@object.GetType(), @object);
			}
		}

		public Arguments(params IArgumentsStore[] customStores)
		{
			if (customStores != null)
			{
				foreach (var store in customStores)
				{
					if (store == null)
					{
						continue;
					}
					// first one wins, so stores passed via .ctor will get picked over the default ones
					count += store.Count;
					stores.Add(store);
				}
			}
			stores.Add(new NamedArgumentsStore());
			stores.Add(new TypedArgumentsStore());
			stores.Add(new FallbackArgumentsStore());
		}

		public int Count
		{
			get
			{
				Debug.Assert(count == stores.Sum(s => s.Count), "count == stores.Sum(s => s.Count)");
				return count;
			}
		}

		public object this[object key]
		{
			get
			{
				if (key == null)
				{
					throw new ArgumentNullException("key");
				}
				var store = GetSupportingStore(key);
				return store.GetItem(key);
			}
			set
			{
				if (key == null)
				{
					throw new ArgumentNullException("key");
				}
				var store = GetSupportingStore(key);
				if (store.Insert(key, value))
				{
					count++;
				}
			}
		}

		public ICollection Keys
		{
			get
			{
				var values = new List<object>(Count);
				foreach (DictionaryEntry value in this)
				{
					values.Add(value.Key);
				}
				return values;
			}
		}

		public ICollection Values
		{
			get
			{
				var values = new List<object>(Count);
				foreach (DictionaryEntry value in this)
				{
					values.Add(value.Value);
				}
				return values;
			}
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return syncRoot; }
		}

		bool IDictionary.IsFixedSize
		{
			get { return false; }
		}

		bool IDictionary.IsReadOnly
		{
			get { return false; }
		}

		public void Add(object key, object value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			var store = GetSupportingStore(key);
			store.Add(key, value);
			count++;
		}

		public void Clear()
		{
			if (count == 0)
			{
				return;
			}
			foreach (var store in stores)
			{
				store.Clear();
			}
			count = 0;
		}

		public bool Contains(object key)
		{
			if (count == 0)
			{
				return false;
			}
			return stores.Any(s => s.Contains(key));
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return new ComponentArgumentsEnumerator(stores);
		}

		public void Remove(object key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (count == 0)
			{
				return;
			}
			var store = GetSupportingStore(key);
			if (store.Remove(key))
			{
				count--;
			}
		}

		protected virtual IArgumentsStore GetSupportingStore(object key)
		{
			var keyType = key.GetType();
			var store = stores.FirstOrDefault(s => s.Supports(keyType));
			if (store == null)
			{
				throw new NotSupportedException(string.Format("Key type {0} is not supported.", keyType));
			}
			return store;
		}

		void ICollection.CopyTo(Array array, int index)
		{
			var currentIndex = index;
			foreach (DictionaryEntry item in this)
			{
				array.SetValue(item.Value, currentIndex);
				currentIndex++;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}