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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	using Castle.MicroKernel.Context;

	public class FactoryParametersStore : IArgumentsStore
	{
		private readonly IDictionary<FactoryParameter,object> items = new Dictionary<FactoryParameter, object>();

		public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
		{
			foreach (var item in items)
			{
				yield return new KeyValuePair<object, object>(item.Key, item.Value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Contains(object key)
		{
			var parameter = key as FactoryParameter;
			if (parameter == null)
			{
				return false;
			}

			return items.ContainsKey(parameter);
		}

		public bool Supports(Type keyType)
		{
			var supports = typeof(FactoryParameter).IsAssignableFrom(keyType);
			return supports;
		}

		public void Add(object key, object value)
		{
			items.Add((FactoryParameter)key, value);
		}

		public void Clear()
		{
			items.Clear();
		}

		public void Remove(object key)
		{
			var parameter = key as FactoryParameter;
			if (parameter == null)
			{
				return;
			}

			items.Remove(parameter);
			
		}

		public void Insert(object key, object value)
		{
			var parameter = key as FactoryParameter;
			if (parameter == null)
			{
				return;
			}

			items[parameter] = value;
		}

		public object GetItem(object key)
		{

			var parameter = key as FactoryParameter;
			if (parameter == null)
			{
				return null;
			}

			object value;
			items.TryGetValue(parameter, out value);
			return value;
		}

		public int Count
		{
			get { return items.Count; }
		}
	}
}