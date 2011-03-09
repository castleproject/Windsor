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

namespace Castle.Windsor
{
	using System;
	using System.Collections;
	using System.ComponentModel;

	using Castle.Core;
	using Castle.MicroKernel;

	public partial class WindsorContainer
	{
		[Obsolete("Use Resolve(key, new Arguments()) or Resolve<TService>(key) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual object this[String key]
		{
			get { return Resolve<object>(key); }
		}

		[Obsolete("Use Resolve(service) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual object this[Type service]
		{
			get { return Resolve(service); }
		}

		[Obsolete("Use Register(Component.For(classType).Named(key)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IWindsorContainer AddComponent(String key, Type classType)
		{
			kernel.AddComponent(key, classType);
			return this;
		}

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IWindsorContainer AddComponent(String key, Type serviceType, Type classType)
		{
			kernel.AddComponent(key, serviceType, classType);
			return this;
		}

		[Obsolete("Use Register(Component.For<T>()) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponent<T>()
		{
			var t = typeof(T);
			AddComponent(t.FullName, t);
			return this;
		}

		[Obsolete("Use Register(Component.For<T>().Named(key)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponent<T>(string key)
		{
			AddComponent(key, typeof(T));
			return this;
		}

		[Obsolete("Use Register(Component.For<I>().ImplementedBy<T>().Named(key)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponent<I, T>(string key) where T : class
		{
			AddComponent(key, typeof(I), typeof(T));
			return this;
		}

		[Obsolete("Use Register(Component.For<I>().ImplementedBy<T>()) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponent<I, T>() where T : class
		{
			var t = typeof(T);
			AddComponent(t.FullName, typeof(I), t);
			return this;
		}

		[Obsolete("Use Register(Component.For(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentLifeStyle(string key, Type classType, LifestyleType lifestyle)
		{
			kernel.AddComponent(key, classType, lifestyle, true);
			return this;
		}

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentLifeStyle(string key, Type serviceType, Type classType, LifestyleType lifestyle)
		{
			kernel.AddComponent(key, serviceType, classType, lifestyle, true);
			return this;
		}

		[Obsolete("Use Register(Component.For<T>().Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentLifeStyle<T>(LifestyleType lifestyle)
		{
			var t = typeof(T);
			AddComponentLifeStyle(t.FullName, t, lifestyle);
			return this;
		}

		[Obsolete("Use Register(Component.For<I>().ImplementedBy<T>().Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentLifeStyle<I, T>(LifestyleType lifestyle) where T : class
		{
			var t = typeof(T);
			AddComponentLifeStyle(t.FullName, typeof(I), t, lifestyle);
			return this;
		}

		[Obsolete("Use Register(Component.For<T>().Named(key).Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentLifeStyle<T>(string key, LifestyleType lifestyle)
		{
			AddComponentLifeStyle(key, typeof(T), lifestyle);
			return this;
		}

		[Obsolete("Use Register(Component.For<I>().ImplementedBy<T>().Named(key).Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentLifeStyle<I, T>(string key, LifestyleType lifestyle) where T : class
		{
			AddComponentLifeStyle(key, typeof(I), typeof(T), lifestyle);
			return this;
		}

		[Obsolete("Use Register(Component.For<I>().ImplementedBy<T>().ExtendedProperties(extendedProperties)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentProperties<I, T>(IDictionary extendedProperties) where T : class
		{
			var t = typeof(T);
			AddComponentWithProperties(t.FullName, typeof(I), t, extendedProperties);
			return this;
		}

		[Obsolete(
			"Use Register(Component.For<I>().ImplementedBy<T>().Named(key).ExtendedProperties(extendedProperties)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentProperties<I, T>(string key, IDictionary extendedProperties) where T : class
		{
			AddComponentWithProperties(key, typeof(I), typeof(T), extendedProperties);
			return this;
		}

		[Obsolete("Use Register(Component.For(classType).Named(key).ExtendedProperties(extendedProperties)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IWindsorContainer AddComponentWithProperties(string key, Type classType, IDictionary extendedProperties)
		{
			kernel.AddComponentWithExtendedProperties(key, classType, extendedProperties);
			return this;
		}

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).ExtendedProperties(extendedProperties)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IWindsorContainer AddComponentWithProperties(string key, Type serviceType, Type classType,
		                                                            IDictionary extendedProperties)
		{
			kernel.AddComponentWithExtendedProperties(key, serviceType, classType, extendedProperties);
			return this;
		}

		[Obsolete("Use Register(Component.For<T>().ExtendedProperties(extendedProperties)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentWithProperties<T>(IDictionary extendedProperties)
		{
			var t = typeof(T);
			AddComponentWithProperties(t.FullName, t, extendedProperties);
			return this;
		}

		[Obsolete("Use Register(Component.For<T>().Named(key).ExtendedProperties(extendedProperties)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddComponentWithProperties<T>(string key, IDictionary extendedProperties)
		{
			AddComponentWithProperties(key, typeof(T), extendedProperties);
			return this;
		}

		/// <summary>
		///   Registers a facility within the container.
		/// </summary>
		/// <param name = "idInConfiguration"></param>
		/// <param name = "facility"></param>
		[Obsolete("Use AddFacility(IFacility) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IWindsorContainer AddFacility(String idInConfiguration, IFacility facility)
		{
			kernel.AddFacility(idInConfiguration, facility);
			return this;
		}

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the container.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "idInConfiguration"></param>
		/// <returns></returns>
		[Obsolete("Use AddFacility<TFacility>() instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddFacility<T>(String idInConfiguration) where T : IFacility, new()
		{
			kernel.AddFacility<T>(idInConfiguration);
			return this;
		}

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the container.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "idInConfiguration"></param>
		/// <param name = "configureFacility">The callback for creation.</param>
		/// <returns></returns>
		[Obsolete("Use AddFacility<TFacility>(Action<TFacility>) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IWindsorContainer AddFacility<T>(String idInConfiguration, Action<T> configureFacility)
			where T : IFacility, new()
		{
			kernel.AddFacility(idInConfiguration, configureFacility);
			return this;
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		[Obsolete("Use Resolve<object>(key, arguments) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual object Resolve(String key, IDictionary arguments)
		{
			return kernel.Resolve<object>(key, arguments);
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "argumentsAsAnonymousType"></param>
		/// <returns></returns>
		[Obsolete("Use Resolve<object>(key, argumentsAsAnonymousType) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual object Resolve(String key, object argumentsAsAnonymousType)
		{
			return Resolve<object>(key, new ReflectionBasedDictionaryAdapter(argumentsAsAnonymousType));
		}
	}
}