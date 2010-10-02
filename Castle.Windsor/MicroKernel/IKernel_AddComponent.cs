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
	using System.ComponentModel;

	using Castle.Core;

	using System.Collections;

	public partial interface IKernel : IServiceProviderEx, IKernelEvents, IDisposable
	{
		[Obsolete("Use Register(Component.For(classType).Named(key)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent(String key, Type classType);

		[Obsolete("Use Register(Component.For(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent(String key, Type classType, LifestyleType lifestyle);

		[Obsolete("Use Register(Component.For(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent(String key, Type classType, LifestyleType lifestyle, bool overwriteLifestyle);

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent(String key, Type serviceType, Type classType);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent(String key, Type serviceType, Type classType, LifestyleType lifestyle);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent(string key, Type serviceType, Type classType, LifestyleType lifestyle, bool overwriteLifestyle);

		[Obsolete("Use Register(Component.For<T>()) instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent<T>();

		[Obsolete("Use Register(Component.For<T>().Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent<T>(LifestyleType lifestyle);

		[Obsolete("Use Register(Component.For<T>().Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent<T>(LifestyleType lifestyle, bool overwriteLifestyle);

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>()) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent<T>(Type serviceType);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy<T>().Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent<T>(Type serviceType, LifestyleType lifestyle);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy<T>().Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponent<T>(Type serviceType, LifestyleType lifestyle, bool overwriteLifestyle);

		[Obsolete("Use Register(Component.For<T>().Instance(instance)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponentInstance<T>(object instance);

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>().Instance(instance)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponentInstance<T>(Type serviceType, object instance);

		[Obsolete("Use Register(Component.For(instance.GetType()).Named(key).Instance(instance)) or generic version instead.")
		]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponentInstance(String key, object instance);

		[Obsolete("Use Register(Component.For(serviceType).Named(key).Instance(instance)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponentInstance(String key, Type serviceType, object instance);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Instance(instance)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponentInstance(string key, Type serviceType, Type classType, object instance);

		[Obsolete(
			"Use Register(Component.For(classType).Named(key).ExtendedProperties(extendedProperties)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponentWithExtendedProperties(String key, Type classType, IDictionary extendedProperties);

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).ExtendedProperties(extendedProperties)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		void AddComponentWithExtendedProperties(String key, Type serviceType, Type classType, IDictionary extendedProperties);
	}
}