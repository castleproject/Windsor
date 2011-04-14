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
	using Castle.MicroKernel.Context;

	/// <summary>
	///   Represents collection of arguments used when resolving a component.
	/// </summary>
	public class Arguments : IDictionary
#if !SILVERLIGHT
		, ICloneable
#endif
	{
		protected IDictionary arguments;

		public Arguments(object namedArgumentsAsAnonymousType, params IArgumentsComparer[] customComparers)
			: this(new ReflectionBasedDictionaryAdapter(namedArgumentsAsAnonymousType), customComparers)
		{
		}

		public Arguments(IDictionary values, params IArgumentsComparer[] customComparers)
			: this(customComparers)
		{
			foreach (DictionaryEntry entry in values)
			{
				Add(entry.Key, entry.Value);
			}
		}

		public Arguments(object[] typedArguments, params IArgumentsComparer[] customComparers)
			: this(customComparers)
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

		public Arguments(params IArgumentsComparer[] customComparers)
		{
			if (customComparers == null || customComparers.Length == 0)
			{
				arguments = new Dictionary<object, object>(new ArgumentsComparer());
			}
			else
			{
				arguments = new Dictionary<object, object>(new ArgumentsComparerExtended(customComparers));
			}
		}

		public int Count
		{
			get { return arguments.Count; }
		}

		public object this[object key]
		{
			get { return arguments[key]; }
			set { arguments[key] = value; }
		}

		public ICollection Keys
		{
			get { return arguments.Keys; }
		}

		public ICollection Values
		{
			get { return arguments.Values; }
		}

		bool ICollection.IsSynchronized
		{
			get { return arguments.IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return arguments.SyncRoot; }
		}

		bool IDictionary.IsFixedSize
		{
			get { return arguments.IsFixedSize; }
		}

		bool IDictionary.IsReadOnly
		{
			get { return arguments.IsReadOnly; }
		}

		public void Add(object key, object value)
		{
			arguments.Add(key, value);
		}

		public void Clear()
		{
			arguments.Clear();
		}

		public bool Contains(object key)
		{
			return arguments.Contains(key);
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return arguments.GetEnumerator();
		}

		public void Remove(object key)
		{
			arguments.Remove(key);
		}

#if !SILVERLIGHT
		protected virtual Arguments CreateDeepCopy()
		{
			var dictionary = arguments as Dictionary<object, object>;
			if (dictionary != null)
			{
				var comparerExtended = dictionary.Comparer as ArgumentsComparerExtended;
				if (comparerExtended != null)
				{
					return new Arguments(arguments, comparerExtended.CustomComparers);
				}
			}

			return new Arguments(arguments);
		}
		
		object ICloneable.Clone()
		{
			return CreateDeepCopy();
		}
#endif

		void ICollection.CopyTo(Array array, int index)
		{
			arguments.CopyTo(array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private class ArgumentsComparer : IEqualityComparer<object>
		{
			public new virtual bool Equals(object x, object y)
			{
				var a = x as string;
				if (a != null)
				{
					return StringComparer.OrdinalIgnoreCase.Equals(a, y as string);
				}
				return Object.Equals(x, y);
			}

			public virtual int GetHashCode(object obj)
			{
				var str = obj as string;
				if (str != null)
				{
					return StringComparer.OrdinalIgnoreCase.GetHashCode(str);
				}
				return obj.GetHashCode();
			}
		}

		private class ArgumentsComparerExtended : ArgumentsComparer
		{
			private readonly List<IArgumentsComparer> nestedComparers = new List<IArgumentsComparer>();

			public ArgumentsComparerExtended(IEnumerable<IArgumentsComparer> customStores)
			{
				nestedComparers = new List<IArgumentsComparer>(customStores);
			}

			public IArgumentsComparer[] CustomComparers
			{
				get { return nestedComparers.ToArray(); }
			}

			public override bool Equals(object x, object y)
			{
				foreach (var store in nestedComparers)
				{
					bool areEqual;
					if (store.RunEqualityComparison(x, y, out areEqual))
					{
						return areEqual;
					}
				}
				return base.Equals(x, y);
			}

			public override int GetHashCode(object obj)
			{
				foreach (var store in nestedComparers)
				{
					int hashCode;
					if (store.RunHasCodeCalculation(obj, out hashCode))
					{
						return hashCode;
					}
				}
				return base.GetHashCode(obj);
			}
		}
	}
}