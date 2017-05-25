// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using System.Collections.Generic;
#if FEATURE_SYSTEM_CONFIGURATION
	using System.Configuration;
#endif
	using System.Reflection;
	using System.Resources;

	using Castle.Core.Configuration;

	public sealed class Dependency
	{
		private readonly object item;

		internal Dependency(object item)
		{
			this.item = item;
		}

		/// <summary>
		/// Specifies that value <paramref name = "valueAsString" /> should be used to satisfy dependencies matched by <paramref name = "dependencyName" />. The value is provided as a string and will be
		/// converted to appropriate type when resolving.
		/// </summary>
		/// <param name = "dependencyName"> </param>
		/// <param name = "valueAsString"> </param>
		/// <returns> </returns>
		public static Parameter OnConfigValue(string dependencyName, string valueAsString)
		{
			return Parameter.ForKey(dependencyName).Eq(valueAsString);
		}

		/// <summary>
		/// Specifies that value <paramref name = "value" /> should be used to satisfy dependencies matched by <paramref name = "dependencyName" />. The value is provided as a string and will be converted to
		/// appropriate type when resolving.
		/// </summary>
		/// <param name = "dependencyName"> </param>
		/// <param name = "value"> </param>
		/// <returns> </returns>
		public static Parameter OnConfigValue(string dependencyName, IConfiguration value)
		{
			return Parameter.ForKey(dependencyName).Eq(value);
		}

#if FEATURE_SYSTEM_CONFIGURATION
		/// <summary>
		/// Specifies that value from application configuration file's appSettings section named <paramref name = "settingName" /> should be used to satisfy dependencies matched by
		///     <paramref name = "dependencyName" />. The value is provided as a string and will be converted to appropriate type when resolving.
		/// </summary>
		/// <param name = "dependencyName"> </param>
		/// <param name = "settingName"> </param>
		/// <returns> </returns>
		public static Parameter OnAppSettingsValue(string dependencyName, string settingName)
		{
			var value = ConfigurationManager.AppSettings.Get(settingName);
			return Parameter.ForKey(dependencyName).Eq(value);
		}

		/// <summary>
		/// Specifies that value from application configuration file's appSettings section named <paramref name = "name" /> should be used to satisfy dependencies matched by <paramref name = "name" />. The value
		/// is provided as a string and will be converted to appropriate type when resolving.
		/// </summary>
		/// <param name = "name"> </param>
		/// <returns> </returns>
		public static Parameter OnAppSettingsValue(string name)
		{
			return OnAppSettingsValue(name, name);
		}
#endif

		/// <summary>
		/// Specifies that component registered with <paramref name = "componentName" /> should be used to satisfy dependencies matched by <paramref name = "dependencyName" />
		/// </summary>
		public static ServiceOverride OnComponent(string dependencyName, string componentName)
		{
			return Property.ForKey(dependencyName).Is(componentName);
		}

		/// <summary>
		/// Specifies that component registered with <paramref name = "componentName" /> should be used to satisfy dependencies matched by <paramref name = "dependencyType" />
		/// </summary>
		public static ServiceOverride OnComponent(Type dependencyType, string componentName)
		{
			return Property.ForKey(dependencyType).Is(componentName);
		}

		/// <summary>
		/// Specifies that component registered with <paramref name = "componentType" /> should be used to satisfy dependencies matched by <paramref name = "dependencyName" />
		/// </summary>
		public static ServiceOverride OnComponent(string dependencyName, Type componentType)
		{
			return Property.ForKey(dependencyName).Is(componentType);
		}

		/// <summary>
		/// Specifies that component registered with <paramref name = "componentType" /> should be used to satisfy dependencies matched by <paramref name = "dependencyType" />
		/// </summary>
		public static ServiceOverride OnComponent(Type dependencyType, Type componentType)
		{
			return Property.ForKey(dependencyType).Is(componentType);
		}

		/// <summary>
		/// Specifies that component registered with <typeparamref name = "TComponentType" /> should be used to satisfy dependencies matched by <typeparamref name = "TDependencyType" />
		/// </summary>
		public static ServiceOverride OnComponent<TDependencyType, TComponentType>()
		{
			return Property.ForKey<TDependencyType>().Is<TComponentType>();
		}

		/// <summary>
		/// Specifies that components registered with <paramref name = "componentNames" /> should be used to satisfy collection dependencies matched by <paramref name = "collectionDependencyName" />
		/// </summary>
		public static ServiceOverride OnComponentCollection(string collectionDependencyName, params string[] componentNames)
		{
			return ServiceOverride.ForKey(collectionDependencyName).Eq(componentNames);
		}

		/// <summary>
		/// Specifies that components registered with <paramref name = "componentNames" /> should be used to satisfy collection dependencies matched by <paramref name = "collectionDependencyType" />
		/// </summary>
		public static ServiceOverride OnComponentCollection(Type collectionDependencyType, params string[] componentNames)
		{
			return ServiceOverride.ForKey(collectionDependencyType).Eq(componentNames);
		}

		/// <summary>
		/// Specifies that components registered with <paramref name = "componentNames" /> should be used to satisfy collection dependencies matched by <typeparamref name = "TCollectionDependencyType" />
		/// </summary>
		public static ServiceOverride OnComponentCollection<TCollectionDependencyType>(params string[] componentNames)
			where TCollectionDependencyType : IEnumerable
		{
			return ServiceOverride.ForKey(typeof(TCollectionDependencyType)).Eq(componentNames);
		}

		/// <summary>
		/// Specifies that components registered with <paramref name = "componentTypes" /> should be used to satisfy collection dependencies matched by <paramref name = "collectionDependencyName" />
		/// </summary>
		public static ServiceOverride OnComponentCollection(string collectionDependencyName, params Type[] componentTypes)
		{
			return ServiceOverride.ForKey(collectionDependencyName).Eq(componentTypes);
		}

		/// <summary>
		/// Specifies that components registered with <paramref name = "componentTypes" /> should be used to satisfy collection dependencies matched by <paramref name = "collectionDependencyType" />
		/// </summary>
		public static ServiceOverride OnComponentCollection(Type collectionDependencyType, params Type[] componentTypes)
		{
			return ServiceOverride.ForKey(collectionDependencyType).Eq(componentTypes);
		}

		/// <summary>
		/// Specifies that components registered with <paramref name = "componentTypes" /> should be used to satisfy collection dependencies matched by <typeparamref name = "TCollectionDependencyType" />
		/// </summary>
		public static ServiceOverride OnComponentCollection<TCollectionDependencyType>(params Type[] componentTypes)
			where TCollectionDependencyType : IEnumerable
		{
			return ServiceOverride.ForKey(typeof(TCollectionDependencyType)).Eq(componentTypes);
		}

		/// <summary>
		/// Specifies that value <paramref name = "value" /> should be used to satisfy dependencies matched by <paramref name = "dependencyName" />
		/// </summary>
		public static Property OnValue(string dependencyName, object value)
		{
			return Property.ForKey(dependencyName).Eq(value);
		}

		/// <summary>
		/// Specifies that value <paramref name = "value" /> should be used to satisfy dependencies matched by <paramref name = "dependencyType" />
		/// </summary>
		public static Property OnValue(Type dependencyType, object value)
		{
			return Property.ForKey(dependencyType).Eq(value);
		}

		/// <summary>
		/// Specifies that value <paramref name = "value" /> should be used to satisfy dependencies matched by <typeparamref name = "TDependencyType" />
		/// </summary>
		public static Property OnValue<TDependencyType>(object value)
		{
			return Property.ForKey<TDependencyType>().Eq(value);
		}

		internal bool Accept<TItem>(ICollection<TItem> items) where TItem : class
		{
			var castItem = item as TItem;
			if (castItem != null)
			{
				items.Add(castItem);
				return true;
			}
			return false;
		}

		public static Property OnResource<TResources>(string dependencyName, string resourceName)
		{
			var resourceManagerProperty = typeof(TResources).GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(ResourceManager), Type.EmptyTypes,
			                                                             null);
			if (resourceManagerProperty == null)
			{
				throw new ArgumentException(string.Format("Type {0} does not appear to be a correct 'resources' type. It doesn't have 'ResourceManager' property.", typeof(TResources)));
			}
			ResourceManager resourceManager;
			try
			{
				resourceManager = (ResourceManager)resourceManagerProperty.GetValue(null, null);
			}
			catch (Exception e)
			{
				throw new ArgumentException(string.Format("Could not read property {1} on type {0}", typeof(TResources), resourceManagerProperty), e);
			}
			return OnResource(dependencyName, resourceManager, resourceName);
		}

		public static Property OnResource(string dependencyName, ResourceManager resourceManager, string resourceName)
		{
			return Property.ForKey(dependencyName).Eq(resourceManager.GetObject(resourceName));
		}
	}
}