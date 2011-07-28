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

namespace Castle.MicroKernel.Registration
{
	using System;
	using System.Collections;
	using System.ComponentModel;

	using Castle.Core;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class ComponentDependencyRegistrationExtensions
	{
		/// <summary>
		///   Inserts a new named argument with given key. If an argument for this name already exists, it will be overwritten.
		/// </summary>
		public static IDictionary Insert(this IDictionary arguments, string key, object value)
		{
			arguments[key] = value;
			return arguments;
		}

		/// <summary>
		///   Inserts a new typed argument with given type. If an argument for this type already exists, it will be overwritten.
		/// </summary>
		public static IDictionary Insert(this IDictionary arguments, Type dependencyType, object value)
		{
			arguments[dependencyType] = value;
			return arguments;
		}

		/// <summary>
		///   Inserts a set of typed arguments. Property names of the anonymous type will be used as key.
		/// </summary>
		public static IDictionary InsertAnonymous(this IDictionary arguments, object namedArgumentsAsAnonymousType)
		{
			foreach (DictionaryEntry item in new ReflectionBasedDictionaryAdapter(namedArgumentsAsAnonymousType))
			{
				arguments[item.Key] = item.Value;
			}

			return arguments;
		}

		/// <summary>
		///   Inserts a new typed argument with given type. If an argument for this type already exists, it will be overwritten.
		/// </summary>
		public static IDictionary InsertTyped<TDependencyType>(this IDictionary arguments, TDependencyType value)
		{
			arguments[typeof(TDependencyType)] = value;
			return arguments;
		}

		/// <summary>
		///   Inserts a set of typed arguments. Actual type of the arguments will be used as key.
		/// </summary>
		public static IDictionary InsertTypedCollection(this IDictionary arguments, object[] typedArgumentsArray)
		{
			foreach (var item in typedArgumentsArray)
			{
				arguments[item.GetType()] = item;
			}

			return arguments;
		}
	}
}