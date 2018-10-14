// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

	using Castle.Core;
	using Castle.Windsor;

	/// <summary>
	/// Represents a collection of named and typed arguments used for dependencies resolved via <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
	/// See: https://github.com/castleproject/Windsor/blob/master/docs/arguments.md
	/// </summary>
	public sealed class Arguments
		: IEnumerable<KeyValuePair<object, object>> // Required for collection initializers
	{
		private static readonly ArgumentsComparer Comparer = new ArgumentsComparer();

		private readonly Dictionary<object, object> dictionary;

		/// <summary>
		/// Initializes a new instance of the <see cref="Arguments"/> class that is empty.
		/// </summary>
		public Arguments()
		{
			dictionary = new Dictionary<object, object>(Comparer);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Arguments"/> class that contains elements copied from the specified <see cref="Arguments"/>.
		/// </summary>
		public Arguments(Arguments arguments)
		{
			dictionary = new Dictionary<object, object>(arguments.dictionary, Comparer);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<KeyValuePair<object, object>> GetEnumerator() => dictionary.GetEnumerator();
		public int Count => dictionary.Count;

		public void Add(object key, object value)
		{
			CheckKeyType(key);
			dictionary.Add(key, value);
		}

		public bool Contains(object key)
		{
			CheckKeyType(key);
			return dictionary.ContainsKey(key);
		}

		public void Remove(object key)
		{
			CheckKeyType(key);
			dictionary.Remove(key);
		}

		public object this[object key]
		{
			get
			{
				CheckKeyType(key);
				if (dictionary.TryGetValue(key, out object value))
				{
					return value;
				}
				return null;
			}
			set
			{
				CheckKeyType(key);
				dictionary[key] = value;
			}
		}

		/// <summary>
		/// Adds a collection of named and/or typed arguments.
		/// </summary>
		public Arguments Add(IEnumerable<KeyValuePair<object, object>> arguments)
		{
			foreach (KeyValuePair<object, object> item in arguments)
			{
				Add(item.Key, item.Value);
			}
			return this;
		}

		/// <summary>
		/// Adds a named argument.
		/// </summary>
		public Arguments AddNamed(string key, object value)
		{
			Add(key, value);
			return this;
		}

		/// <summary>
		/// Adds a collection of named arguments, <see cref="Dictionary{TKey,TValue}"/> implements this interface.
		/// </summary>
		public Arguments AddNamed(IEnumerable<KeyValuePair<string, object>> arguments)
		{
			foreach (var item in arguments)
			{
				AddNamed(item.Key, item.Value);
			}
			return this;
		}

		/// <summary>
		/// Adds a collection of named arguments from public properties of a standard or anonymous type.
		/// </summary>
		public Arguments AddProperties(object instance)
		{
			foreach (DictionaryEntry item in new ReflectionBasedDictionaryAdapter(instance))
			{
				Add(item.Key, item.Value);
			}
			return this;
		}

		/// <summary>
		/// Adds a typed argument.
		/// </summary>
		public Arguments AddTyped(Type key, object value)
		{
			Add(key, value);
			return this;
		}

		/// <summary>
		/// Adds a typed argument.
		/// </summary>
		public Arguments AddTyped<TDependencyType>(TDependencyType value)
		{
			AddTyped(typeof(TDependencyType), value);
			return this;
		}

		/// <summary>
		/// Adds a collection of typed arguments.
		/// </summary>
		public Arguments AddTyped(IEnumerable<object> arguments)
		{
			foreach (object item in arguments)
			{
				AddTyped(item.GetType(), item);
			}
			return this;
		}

		/// <summary>
		/// Adds a collection of typed arguments.
		/// </summary>
		public Arguments AddTyped(params object[] arguments)
		{
			foreach (object item in arguments)
			{
				AddTyped(item.GetType(), item);
			}
			return this;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Arguments"/> class and adds a collection of named arguments,
		/// <see cref="Dictionary{TKey,TValue}"/> implements this interface.
		/// </summary>
		public static Arguments FromNamed(IEnumerable<KeyValuePair<string, object>> arguments)
		{
			return new Arguments().AddNamed(arguments);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Arguments"/> class and adds a collection of named arguments
		/// from public properties of a standard or anonymous type.
		/// </summary>
		public static Arguments FromProperties(object instance)
		{
			return new Arguments().AddProperties(instance);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Arguments"/> class and adds a collection of typed arguments,
		/// <see cref="Dictionary{TKey,TValue}"/> implements this interface.
		/// </summary>
		public static Arguments FromTyped(IEnumerable<KeyValuePair<Type, object>> arguments)
		{
			return new Arguments().AddTyped(arguments);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Arguments"/> class and adds a collection of typed arguments.
		/// </summary>
		public static Arguments FromTyped(IEnumerable<object> arguments)
		{
			return new Arguments().AddTyped(arguments);
		}

		private void CheckKeyType(object key)
		{
			if (!(key is string) && !(key is Type))
			{
				throw new ArgumentException($"The argument '{key}' should be of type string or System.Type.");
			}
		}

		private sealed class ArgumentsComparer : IEqualityComparer<object>
		{
			public new bool Equals(object x, object y)
			{
				if (x is string a)
				{
					return StringComparer.OrdinalIgnoreCase.Equals(a, y as string);
				}
				return object.Equals(x, y);
			}

			public int GetHashCode(object obj)
			{
				if (obj is string str)
				{
					return StringComparer.OrdinalIgnoreCase.GetHashCode(str);
				}
				return obj.GetHashCode();
			}
		}
	}
}