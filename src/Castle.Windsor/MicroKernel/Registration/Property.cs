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

	/// <summary>
	///   Represents a key/value pair.
	/// </summary>
	public class Property
	{
		private readonly object key;
		private readonly object value;

		public Property(object key, object value)
		{
			this.key = key;
			this.value = value;
		}

		/// <summary>
		///   Gets the property key.
		/// </summary>
		public object Key
		{
			get { return key; }
		}

		/// <summary>
		///   Gets the property value.
		/// </summary>
		public object Value
		{
			get { return value; }
		}

		/// <summary>
		///   Create a <see cref = "PropertyKey" /> with key.
		/// </summary>
		/// <param key = "key">The property key.</param>
		/// <returns>The new <see cref = "PropertyKey" /></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static PropertyKey ForKey(String key)
		{
			return new PropertyKey(key);
		}

		/// <summary>
		///   Create a <see cref = "PropertyKey" /> with key.
		/// </summary>
		/// <param key = "key">The property key.</param>
		/// <returns>The new <see cref = "PropertyKey" /></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static PropertyKey ForKey(Type key)
		{
			return new PropertyKey(key);
		}

		/// <summary>
		///   Create a <see cref = "PropertyKey" /> with key.
		/// </summary>
		/// <param key = "key">The property key.</param>
		/// <returns>The new <see cref = "PropertyKey" /></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static PropertyKey ForKey<TKey>()
		{
			return new PropertyKey(typeof(TKey));
		}

		/// <summary>
		///   Specifies that component registered with <paramref name = "componentName" /> should be used to satisfy dependencies matched by <paramref
		///    name = "dependencyName" />
		/// </summary>
		public static ServiceOverride WithComponent(string dependencyName, string componentName)
		{
			return ForKey(dependencyName).Is(componentName);
		}

		/// <summary>
		///   Specifies that component registered with <paramref name = "componentName" /> should be used to satisfy dependencies matched by <paramref
		///    name = "dependencyType" />
		/// </summary>
		public static ServiceOverride WithComponent(Type dependencyType, string componentName)
		{
			return ForKey(dependencyType).Is(componentName);
		}

		/// <summary>
		///   Specifies that component registered with <paramref name = "componentType" /> should be used to satisfy dependencies matched by <paramref
		///    name = "dependencyName" />
		/// </summary>
		public static ServiceOverride WithComponent(string dependencyName, Type componentType)
		{
			return ForKey(dependencyName).Is(componentType);
		}

		/// <summary>
		///   Specifies that component registered with <paramref name = "componentType" /> should be used to satisfy dependencies matched by <paramref
		///    name = "dependencyType" />
		/// </summary>
		public static ServiceOverride WithComponent(Type dependencyType, Type componentType)
		{
			return ForKey(dependencyType).Is(componentType);
		}

		/// <summary>
		///   Specifies that component registered with <typeparamref name = "TComponentType" /> should be used to satisfy dependencies matched by <typeparamref
		///    name = "TDependencyType" />
		/// </summary>
		public static ServiceOverride WithComponent<TDependencyType, TComponentType>()
		{
			return ForKey<TDependencyType>().Is<TComponentType>();
		}

		/// <summary>
		///   Specifies that components registered with <paramref name = "componentNames" /> should be used to satisfy collection dependencies matched by <paramref
		///    name = "collectionDependencyName" />
		/// </summary>
		public static ServiceOverride WithComponents(string collectionDependencyName, params string[] componentNames)
		{
			return ServiceOverride.ForKey(collectionDependencyName).Eq(componentNames);
		}

		/// <summary>
		///   Specifies that components registered with <paramref name = "componentNames" /> should be used to satisfy collection dependencies matched by <paramref
		///    name = "collectionDependencyType" />
		/// </summary>
		public static ServiceOverride WithComponents(Type collectionDependencyType, params string[] componentNames)
		{
			return ServiceOverride.ForKey(collectionDependencyType).Eq(componentNames);
		}

		/// <summary>
		///   Specifies that components registered with <paramref name = "componentNames" /> should be used to satisfy collection dependencies matched by <typeparamref
		///    name = "TCollectionDependencyType" />
		/// </summary>
		public static ServiceOverride WithComponents<TCollectionDependencyType>(params string[] componentNames) where TCollectionDependencyType : IEnumerable
		{
			return ServiceOverride.ForKey(typeof(TCollectionDependencyType)).Eq(componentNames);
		}

		/// <summary>
		///   Specifies that components registered with <paramref name = "componentTypes" /> should be used to satisfy collection dependencies matched by <paramref
		///    name = "collectionDependencyName" />
		/// </summary>
		public static ServiceOverride WithComponents(string collectionDependencyName, params Type[] componentTypes)
		{
			return ServiceOverride.ForKey(collectionDependencyName).Eq(componentTypes);
		}

		/// <summary>
		///   Specifies that components registered with <paramref name = "componentTypes" /> should be used to satisfy collection dependencies matched by <paramref
		///    name = "collectionDependencyType" />
		/// </summary>
		public static ServiceOverride WithComponents(Type collectionDependencyType, params Type[] componentTypes)
		{
			return ServiceOverride.ForKey(collectionDependencyType).Eq(componentTypes);
		}

		/// <summary>
		///   Specifies that components registered with <paramref name = "componentTypes" /> should be used to satisfy collection dependencies matched by <typeparamref
		///    name = "TCollectionDependencyType" />
		/// </summary>
		public static ServiceOverride WithComponents<TCollectionDependencyType>(params Type[] componentTypes) where TCollectionDependencyType : IEnumerable
		{
			return ServiceOverride.ForKey(typeof(TCollectionDependencyType)).Eq(componentTypes);
		}

		/// <summary>
		///   Specifies that value <paramref name = "value" /> should be used to satisfy dependencies matched by <paramref
		///    name = "dependencyName" />
		/// </summary>
		public static Property WithValue(string dependencyName, object value)
		{
			return ForKey(dependencyName).Eq(value);
		}

		/// <summary>
		///   Specifies that value <paramref name = "value" /> should be used to satisfy dependencies matched by <paramref
		///    name = "dependencyType" />
		/// </summary>
		public static Property WithValue(Type dependencyType, object value)
		{
			return ForKey(dependencyType).Eq(value);
		}

		/// <summary>
		///   Specifies that value <paramref name = "value" /> should be used to satisfy dependencies matched by <typeparamref
		///    name = "TDependencyType" />
		/// </summary>
		public static Property WithValue<TDependencyType>(object value)
		{
			return ForKey<TDependencyType>().Eq(value);
		}
	}

	/// <summary>
	///   Represents a property key.
	/// </summary>
	public class PropertyKey
	{
		private readonly object key;

		internal PropertyKey(object key)
		{
			this.key = key;
		}

		/// <summary>
		///   The property key key.
		/// </summary>
		public object Key
		{
			get { return key; }
		}

		/// <summary>
		///   Builds the <see cref = "Property" /> with key/value.
		/// </summary>
		/// <param key = "value">The property value.</param>
		/// <returns>The new <see cref = "Property" /></returns>
		public Property Eq(Object value)
		{
			return new Property(key, value);
		}

		/// <summary>
		///   Builds a service override using other component registered with given <paramref name = "componentName" /> as value for dependency with given <see
		///    cref = "Key" />.
		/// </summary>
		/// <param name = "componentName"></param>
		/// <returns></returns>
		public ServiceOverride Is(string componentName)
		{
			return GetServiceOverrideKey().Eq(componentName);
		}

		/// <summary>
		///   Builds a service override using other component registered with given <paramref name = "componentImplementation" /> and no explicit name, as value for dependency with given <see
		///    cref = "Key" />.
		/// </summary>
		/// <returns></returns>
		public ServiceOverride Is(Type componentImplementation)
		{
			if (componentImplementation == null)
			{
				throw new ArgumentNullException("componentImplementation");
			}
			return GetServiceOverrideKey().Eq(componentImplementation.FullName);
		}

		/// <summary>
		///   Builds a service override using other component registered with given <typeparam name = "TComponentImplementation" /> and no explicit name, as value for dependency with given <see
		///    cref = "Key" />.
		/// </summary>
		/// <returns></returns>
		public ServiceOverride Is<TComponentImplementation>()
		{
			return Is(typeof(TComponentImplementation));
		}

		private ServiceOverrideKey GetServiceOverrideKey()
		{
			if (key is Type)
			{
				return ServiceOverride.ForKey((Type)key);
			}
			return ServiceOverride.ForKey((string)key);
		}
	}
}