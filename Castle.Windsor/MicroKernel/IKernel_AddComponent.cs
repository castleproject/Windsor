// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	using Castle.Core;
	using System.Collections;

	public partial interface IKernel : IServiceProviderEx, IKernelEvents, IDisposable
	{
		/// <summary>
		/// Adds a concrete class as a component
		/// </summary>
		/// <param name="key"></param>
		/// <param name="classType"></param>
		[Obsolete("Use Register(Component.For(classType).Named(key)) instead.")]
		void AddComponent(String key, Type classType);

		/// <summary>
		/// Adds a concrete class
		/// as a component with the specified <paramref name="lifestyle"/>.
		/// </summary>
		/// <param name="key">The key with which to index the component.</param>
		/// <param name="classType">The <see cref="Type"/> of the component.</param>
		/// <param name="lifestyle">The specified <see cref="LifestyleType"/> for the component.</param>
		/// <remarks>
		/// If you have indicated a lifestyle for the specified <paramref name="classType"/> using
		/// attributes, this method will not overwrite that lifestyle. To do that, use the
		/// <see cref="AddComponent(string,Type,LifestyleType,bool)"/> method.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="key"/>, or <paramref name="classType"/>
		/// are <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="lifestyle"/> is <see cref="LifestyleType.Undefined"/>.
		/// </exception>
		[Obsolete("Use Register(Component.For(classType).Named(key)).Lifestyle.Is(lifestyle) instead.")]
		void AddComponent(String key, Type classType, LifestyleType lifestyle);

		/// <summary>
		/// Adds a concrete class
		/// as a component with the specified <paramref name="lifestyle"/>.
		/// </summary>
		/// <param name="key">The key with which to index the component.</param>
		/// <param name="classType">The <see cref="Type"/> of the component.</param>
		/// <param name="lifestyle">The specified <see cref="LifestyleType"/> for the component.</param>
		/// <param name="overwriteLifestyle">
		/// If <see langword="true"/>, then ignores all other configurations
		/// for lifestyle and uses the value in the <paramref name="lifestyle"/> parameter.
		/// </param>
		/// <remarks>
		/// If you have indicated a lifestyle for the specified <paramref name="classType"/> using
		/// attributes, this method will not overwrite that lifestyle. To do that, use the
		/// <see cref="AddComponent(string,Type,Type,LifestyleType,bool)"/> method.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="key"/> or <paramref name="classType"/>
		/// are <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException" />
		/// Thrown if <paramref name="lifestyle"/> is <see cref="LifestyleType.Undefined"/>.
		[Obsolete("Use Register(Component.For(classType).Named(key)).Lifestyle.Is(lifestyle) instead.")]
		void AddComponent(String key, Type classType, LifestyleType lifestyle, bool overwriteLifestyle);

		/// <summary>
		/// Adds a concrete class and an interface 
		/// as a component
		/// </summary>
		/// <param name="key">The key with which to index the component.</param>
		/// <param name="serviceType">The service <see cref="Type"/> that this component implements.</param>
		/// <param name="classType">The <see cref="Type"/> of the component.</param>
		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key)) instead.")]
		void AddComponent(String key, Type serviceType, Type classType);

		/// <summary>
		/// Adds a concrete class and an interface 
		/// as a component with the specified <paramref name="lifestyle"/>.
		/// </summary>
		/// <param name="key">The key with which to index the component.</param>
		/// <param name="serviceType">The service <see cref="Type"/> that this component implements.</param>
		/// <param name="classType">The <see cref="Type"/> of the component.</param>
		/// <param name="lifestyle">The specified <see cref="LifestyleType"/> for the component.</param>
		/// <remarks>
		/// If you have indicated a lifestyle for the specified <paramref name="classType"/> using
		/// attributes, this method will not overwrite that lifestyle. To do that, use the
		/// <see cref="AddComponent(string,Type,Type,LifestyleType,bool)"/> method.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="key"/>, <paramref name="serviceType"/>, or <paramref name="classType"/>
		/// are <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="lifestyle"/> is <see cref="LifestyleType.Undefined"/>.
		/// </exception>
		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key)).Lifestyle.Is(lifestyle) instead.")]
		void AddComponent(String key, Type serviceType, Type classType, LifestyleType lifestyle);

		/// <summary>
		/// Adds a concrete class and an interface 
		/// as a component with the specified <paramref name="lifestyle"/>.
		/// </summary>
		/// <param name="key">The key with which to index the component.</param>
		/// <param name="serviceType">The service <see cref="Type"/> that this component implements.</param>
		/// <param name="classType">The <see cref="Type"/> of the component.</param>
		/// <param name="lifestyle">The specified <see cref="LifestyleType"/> for the component.</param>
		/// <param name="overwriteLifestyle">
		/// If <see langword="true"/>, then ignores all other configurations
		/// for lifestyle and uses the value in the <paramref name="lifestyle"/> parameter.
		/// </param>
		/// <remarks>
		/// If you have indicated a lifestyle for the specified <paramref name="classType"/> using
		/// attributes, this method will not overwrite that lifestyle. To do that, use the
		/// <see cref="AddComponent(string,Type,Type,LifestyleType,bool)"/> method.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="key"/>, <paramref name="serviceType"/>, or <paramref name="classType"/>
		/// are <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="lifestyle"/> is <see cref="LifestyleType.Undefined"/>.
		/// </exception>
		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key)).Lifestyle.Is(lifestyle) instead.")]
		void AddComponent(string key, Type serviceType, Type classType, LifestyleType lifestyle, bool overwriteLifestyle);

		/// <summary>
		/// Adds a concrete class as a component
		/// </summary>
		[Obsolete("Use Register(Component.For<T>()) instead.")]
		void AddComponent<T>();

		/// <summary>
		/// Adds a concrete class
		/// as a component with the specified <paramref name="lifestyle"/>.
		/// </summary>
		/// <param name="lifestyle">The specified <see cref="LifestyleType"/> for the component.</param>
		/// <remarks>
		/// If you have indicated a lifestyle for the specified T using
		/// attributes, this method will not overwrite that lifestyle. To do that, use the
		/// <see cref="AddComponent(string,Type,LifestyleType,bool)"/> method.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="lifestyle"/> is <see cref="LifestyleType.Undefined"/>.
		/// </exception>
		[Obsolete("Use Register(Component.For<T>().Lifestyle.Is(lifestyle)) instead.")]
		void AddComponent<T>(LifestyleType lifestyle);

		/// <summary>
		/// Adds a concrete class
		/// as a component with the specified <paramref name="lifestyle"/>.
		/// </summary>
		/// <param name="lifestyle">The specified <see cref="LifestyleType"/> for the component.</param>
		/// <param name="overwriteLifestyle">
		/// If <see langword="true"/>, then ignores all other configurations
		/// for lifestyle and uses the value in the <paramref name="lifestyle"/> parameter.
		/// </param>
		/// <remarks>
		/// If you have indicated a lifestyle for the specified T using
		/// attributes, this method will not overwrite that lifestyle. To do that, use the
		/// <see cref="AddComponent(string,Type,LifestyleType,bool)"/> method.
		/// </remarks>
		/// <exception cref="ArgumentException" />
		/// Thrown if <paramref name="lifestyle"/> is <see cref="LifestyleType.Undefined"/>.
		[Obsolete("Use Register(Component.For<T>().Lifestyle.Is(lifestyle)) instead.")]
		void AddComponent<T>(LifestyleType lifestyle, bool overwriteLifestyle);

		/// <summary>
		/// Adds a concrete class and an interface 
		/// as a component
		/// </summary>
		/// <param name="serviceType">The service <see cref="Type"/> that this component implements.</param>
		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>() instead.")]
		void AddComponent<T>(Type serviceType);

		/// <summary>
		/// Adds a concrete class and an interface 
		/// as a component with the specified <paramref name="lifestyle"/>.
		/// </summary>
		/// <param name="serviceType">The service <see cref="Type"/> that this component implements.</param>
		/// <param name="lifestyle">The specified <see cref="LifestyleType"/> for the component.</param>
		/// <remarks>
		/// If you have indicated a lifestyle for the specified T using
		/// attributes, this method will not overwrite that lifestyle. To do that, use the
		/// <see cref="AddComponent(string,Type,Type,LifestyleType,bool)"/> method.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// are <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="lifestyle"/> is <see cref="LifestyleType.Undefined"/>.
		/// </exception>
		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>().Lifestyle.Is(lifestyle)) instead.")]
		void AddComponent<T>(Type serviceType, LifestyleType lifestyle);

		/// <summary>
		/// Adds a concrete class and an interface 
		/// as a component with the specified <paramref name="lifestyle"/>.
		/// </summary>
		/// <param name="serviceType">The service <see cref="Type"/> that this component implements.</param>
		/// <param name="lifestyle">The specified <see cref="LifestyleType"/> for the component.</param>
		/// <param name="overwriteLifestyle">
		/// If <see langword="true"/>, then ignores all other configurations
		/// for lifestyle and uses the value in the <paramref name="lifestyle"/> parameter.
		/// </param>
		/// <remarks>
		/// attributes, this method will not overwrite that lifestyle. To do that, use the
		/// <see cref="AddComponent(string,Type,Type,LifestyleType,bool)"/> method.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// are <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="lifestyle"/> is <see cref="LifestyleType.Undefined"/>.
		/// </exception>
		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>().Lifestyle.Is(lifestyle)) instead.")]
		void AddComponent<T>(Type serviceType, LifestyleType lifestyle, bool overwriteLifestyle);

		/// <summary>
		/// Used mostly by facilities. Adds an instance
		/// to be used as a component.
		/// </summary>
		/// <param name="instance"></param>
		[Obsolete("Use Register(Component.For<T>().Instance(instance) instead.")]
		void AddComponentInstance<T>(object instance);

		/// <summary>
		/// Used mostly by facilities. Adds an instance
		/// to be used as a component.
		/// </summary>
		/// <param name="serviceType"></param>
		/// <param name="instance"></param>
		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>().Instance(instance) instead.")]
		void AddComponentInstance<T>(Type serviceType, object instance);

		/// <summary>
		/// Adds a concrete class as a component and specify the extended properties.
		/// Used by facilities, mostly.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="classType"></param>
		/// <param name="extendedProperties"></param>
		[Obsolete("Use Register(Component.For(classType).Named(key).ExtendedProperties(extendedProperties) instead.")]
		void AddComponentWithExtendedProperties(String key, Type classType, IDictionary extendedProperties);

		/// <summary>
		/// Adds a concrete class and an interface 
		/// as a component and specify the extended properties.
		/// Used by facilities, mostly.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="serviceType"></param>
		/// <param name="classType"></param>
		/// <param name="extendedProperties"></param>
		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).ExtendedProperties(extendedProperties) instead.")]
		void AddComponentWithExtendedProperties(String key, Type serviceType, Type classType, IDictionary extendedProperties);


		/// <summary>
		/// Used mostly by facilities. Adds an instance
		/// to be used as a component.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="instance"></param>
		[Obsolete("Use Register(Component.For<object>().Named(key).Instance(instance) instead.")]
		void AddComponentInstance(String key, object instance);

		/// <summary>
		/// Used mostly by facilities. Adds an instance
		/// to be used as a component.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="serviceType"></param>
		/// <param name="instance"></param>
		[Obsolete("Use Register(Component.For(serviceType).Named(key).Instance(instance) instead.")]
		void AddComponentInstance(String key, Type serviceType, object instance);

		/// <summary>
		/// Used mostly by facilities. Adds an instance
		/// to be used as a component.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="serviceType"></param>
		/// <param name="instance"></param>
		/// <param name="classType"></param>
		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Instance(instance) instead.")]
		void AddComponentInstance(string key, Type serviceType, Type classType, object instance);
	}
}
