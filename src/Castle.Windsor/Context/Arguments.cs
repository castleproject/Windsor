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
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	/// <summary>
	/// Represents collection of arguments used when resolving a component.
	/// </summary>
	public class Arguments : IDictionary
	{
		private readonly IList<IArgumentsStore> stores = new List<IArgumentsStore>();
		private readonly object syncRoot = new object();

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
					if (store == null) continue;

					// first one wins, so stores passed via .ctor will get picked over the default ones
					stores.Add(store);
				}
			}
			AddStores(stores);
		}

		public int Count
		{
			get { return stores.Sum(s => s.Count); }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return syncRoot; }
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
				store.Insert(key, value);
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

		bool IDictionary.IsFixedSize
		{
			get { return false; }
		}

		bool IDictionary.IsReadOnly
		{
			get { return false; }
		}

		protected virtual void AddStores(IList<IArgumentsStore> list)
		{
			list.Add(new NamedArgumentsStore());
			list.Add(new TypedArgumentsStore());
			list.Add(new FallbackArgumentsStore());
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

		public void Add(object key, object value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			var store = GetSupportingStore(key);
			store.Add(key, value);
		}

		public void Clear()
		{
			foreach (var store in stores)
			{
				store.Clear();
			}
		}

		public bool Contains(object key)
		{
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

			var store = GetSupportingStore(key);
			store.Remove(key);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}