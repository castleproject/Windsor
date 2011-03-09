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
	using System.ComponentModel;

	using Castle.Core;

	public partial interface IKernel : IKernelEvents, IDisposable
	{
		/// <summary>
		///   Returns the component instance by the key
		/// </summary>
		[Obsolete("Use Resolve<object>(key) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		object this[String key] { get; }

		/// <summary>
		///   Returns the component instance by the service type
		/// </summary>
		[Obsolete("Use Resolve(service) or generic strongly typed version instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		object this[Type service] { get; }

		[Obsolete("Use Register(Component.For(classType).Named(key)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent(String key, Type classType);

		[Obsolete("Use Register(Component.For(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent(String key, Type classType, LifestyleType lifestyle);

		[Obsolete("Use Register(Component.For(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent(String key, Type classType, LifestyleType lifestyle, bool overwriteLifestyle);

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent(String key, Type serviceType, Type classType);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent(String key, Type serviceType, Type classType, LifestyleType lifestyle);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent(string key, Type serviceType, Type classType, LifestyleType lifestyle, bool overwriteLifestyle);

		[Obsolete("Use Register(Component.For<T>()) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent<T>();

		[Obsolete("Use Register(Component.For<T>().Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent<T>(LifestyleType lifestyle);

		[Obsolete("Use Register(Component.For<T>().Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent<T>(LifestyleType lifestyle, bool overwriteLifestyle);

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>()) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent<T>(Type serviceType);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy<T>().Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent<T>(Type serviceType, LifestyleType lifestyle);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy<T>().Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponent<T>(Type serviceType, LifestyleType lifestyle, bool overwriteLifestyle);

		[Obsolete("Use Register(Component.For<T>().Instance(instance)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponentInstance<T>(object instance);

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>().Instance(instance)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponentInstance<T>(Type serviceType, object instance);

		[Obsolete("Use Register(Component.For(instance.GetType()).Named(key).Instance(instance)) or generic version instead.")
		]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponentInstance(String key, object instance);

		[Obsolete("Use Register(Component.For(serviceType).Named(key).Instance(instance)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponentInstance(String key, Type serviceType, object instance);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Instance(instance)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponentInstance(string key, Type serviceType, Type classType, object instance);

		[Obsolete(
			"Use Register(Component.For(classType).Named(key).ExtendedProperties(extendedProperties)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponentWithExtendedProperties(String key, Type classType, IDictionary extendedProperties);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).ExtendedProperties(extendedProperties)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		void AddComponentWithExtendedProperties(String key, Type serviceType, Type classType, IDictionary extendedProperties);

		/// <summary>
		///   Adds a <see cref = "IFacility" /> to the kernel.
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "facility"></param>
		/// <returns></returns>
		[Obsolete("Use AddFacility(IFacility) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		IKernel AddFacility(String key, IFacility facility);

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the kernel.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "key"></param>
		[Obsolete("Use AddFacility<TFacility>() instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		IKernel AddFacility<T>(String key) where T : IFacility, new();

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the kernel.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "key"></param>
		/// <param name = "onCreate">The callback for creation.</param>
		[Obsolete("Use AddFacility<TFacility>(Action<TFacility>) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		IKernel AddFacility<T>(String key, Action<T> onCreate)
			where T : IFacility, new();

		/// <summary>
		///   Returns the component instance by the component key
		///   using dynamic arguments
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		[Obsolete("Use Resolve<object>(key, arguments) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		object Resolve(String key, IDictionary arguments);

		/// <summary>
		///   Returns the component instance by the component key
		///   using dynamic arguments
		/// </summary>
		/// <param name = "key">Key to resolve</param>
		/// <param name = "argumentsAsAnonymousType">Arguments to resolve the services</param>
		/// <returns></returns>
		[Obsolete("Use Resolve<object>(key, argumentsAsAnonymousType) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		object Resolve(String key, object argumentsAsAnonymousType);
	}
}