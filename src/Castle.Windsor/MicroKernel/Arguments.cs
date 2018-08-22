// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
	/// Represents collection of named or typed arguments used for dependencies resolved via <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
	/// Please see: https://github.com/castleproject/Windsor/blob/master/docs/arguments.md
	/// </summary>
	public class Arguments : IDictionary
	{
		private bool isReadOnly;
		private readonly IDictionary dictionary;
		private static readonly ArgumentsComparer Comparer = new ArgumentsComparer();

		/// <summary>
		/// Constructor for creating named/typed dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		public Arguments()
		{
			dictionary = new Dictionary<object, object>(Comparer);
		}

		/// <summary>
		/// Constructor for creating named dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		/// <param named="named"></param>
		/// <param named="value"></param>
		public Arguments(string named, object value) : this()
		{
			dictionary[named] = value;
		}

		/// <summary>
		/// Constructor for creating typed dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		/// <param named="typed"></param>
		/// <param named="value"></param>
		public Arguments(Type typed, object value) : this()
		{
			dictionary[typed] = value;
		}


		/// <summary>
		/// Constructor for creating named/typed dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		public Arguments(IDictionary values)
			: this()
		{
			foreach (DictionaryEntry entry in values)
			{
				Add(entry.Key, entry.Value);
			}
		}

		/// <summary>
		/// Indexer for creating named/typed dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		public object this[object key]
		{
			get => dictionary[key];
			set
			{
				EnsureWritable();
				dictionary[key] = value;
			}
		}

		public int Count => dictionary.Count;

		public ICollection Keys => dictionary.Keys;

		public ICollection Values => dictionary.Values;

		bool ICollection.IsSynchronized => dictionary.IsSynchronized;

		object ICollection.SyncRoot => dictionary.SyncRoot;

		bool IDictionary.IsFixedSize => dictionary.IsFixedSize;

		bool IDictionary.IsReadOnly => isReadOnly;

		public static readonly Arguments Empty = new Arguments() { isReadOnly = true };

		/// <summary>
		/// Method for adding named/typed dependency arguments for <see cref="IWindsorContainer.Resolve{T}(Castle.MicroKernel.Arguments)"/>
		/// </summary>
		/// <param named="namedOrTyped">Type or named parameter</param>
		/// <param named="value">Instance dependency</param>
		public void Add(object namedOrTyped, object value)
		{
			EnsureWritable();
			dictionary.Add(namedOrTyped, value);
		}

		public void Clear()
		{
			EnsureWritable();
			dictionary.Clear();
		}

		public Arguments Clone()
		{
			if (dictionary is Dictionary<object, object>)
			{
				return new Arguments(dictionary);
			}

			return new Arguments(dictionary);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			dictionary.CopyTo(array, index);
		}

		public bool Contains(object key)
		{
			return dictionary.Contains(key);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		/// <summary>
		/// Inserts a new named argument with a given dependency. If an argument for this named key already exists, it will be overwritten.
		/// </summary>
		/// <param named="namedOrTyped">Named parameter</param>
		/// <param named="value">Instance dependency</param>
		public virtual Arguments Insert(string named, object value)
		{
			this[named] = value;
			return this;
		}

		/// <summary>
		/// Inserts dictionary with named/typed arguments. If an argument for this named/typed key already exists, it will be overwritten.
		/// </summary>
		/// <param named="values"></param>
		/// <returns><see cref="Arguments"/></returns>
		public virtual Arguments Insert(IDictionary values)
		{
			foreach (DictionaryEntry entry in values)
			{
				if (entry.Key is string || entry.Key is Type)
				{
					dictionary[entry.Key] = entry.Value;
				}
				else
				{
					throw new ArgumentException($"The argument namedOrTyped '{entry.Key}' should be of type System.String or System.Type");
				}
			}
			return this;
		}

		/// <summary>
		/// Method that can be used to named arguments into <see cref="Arguments"/> from a read-only dictionary.
		/// </summary>
		/// <param named="values"><see cref="IReadOnlyDictionary{S, O}"/> where the dictionary is shallow copied up front into <see cref="Arguments"/> and then used as dependencies</param>
		/// <returns><see cref="Arguments"/></returns>
		public virtual Arguments InsertNamed(IReadOnlyDictionary<string, object> values)
		{
			isReadOnly = false;
			foreach (var entry in values)
			{
				if (entry.Key is string)
				{
					dictionary[entry.Key] = entry.Value;
				}
				else
				{
					throw new ArgumentException($"The argument namedOrTyped '{entry.Key}' should be of type System.String");
				}
			}

			isReadOnly = true;
			return this;
		}

		/// <summary>
		/// Inserts a set of typed arguments. Property names of the anonymous type will be used as namedOrTyped.
		/// </summary>
		/// <param named="instance">Named property</param>
		public virtual Arguments InsertProperties(object instance)
		{
			foreach (DictionaryEntry item in new ReflectionBasedDictionaryAdapter(instance))
			{
				this[item.Key] = item.Value;
			}

			return this;
		}

		/// <summary>
		/// Inserts a new typed argument with given instance. If an argument for this type already exists, it will be overwritten.
		/// </summary>
		public virtual Arguments InsertTyped(Type typed, object value)
		{
			this[typed] = value;
			return this;
		}

		/// <summary>
		/// Inserts a new typed argument with given type. If an argument for this type already exists, it will be overwritten.
		/// </summary>
		public virtual Arguments InsertTyped<TDependencyType>(TDependencyType value)
		{
			InsertTyped(typeof(TDependencyType), value);
			return this;
		}

		/// <summary>
		/// Inserts many new typed argument from a params array. If an argument for this type already exists, it will be overwritten.
		/// </summary>
		public virtual Arguments InsertTyped(params object[] values)
		{
			foreach (var value in values)
			{
				InsertTyped(value.GetType(), value);
			}
			return this;
		}

		public void Remove(object namedOrTyped)
		{
			EnsureWritable();
			dictionary.Remove(namedOrTyped);
		}

		private void EnsureWritable()
		{
			if (isReadOnly)
			{
				throw new NotSupportedException("Collection is read-only");
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
				return x.Equals(y);
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