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
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Registration;

	public partial class DefaultKernel
	{
		[Obsolete("Use Resolve(key, new Arguments()) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual object this[String key]
		{
			get { return Resolve<object>(key); }
		}

		[Obsolete("Use Resolve(service) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual object this[Type service]
		{
			get { return Resolve(service); }
		}

		[Obsolete("Use Register(Component.For(classType).Named(key)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void AddComponent(String key, Type classType)
		{
			AddComponent(key, classType, classType);
		}

		[Obsolete("Use Register(Component.For(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent(string key, Type classType, LifestyleType lifestyle)
		{
			AddComponent(key, classType, classType, lifestyle);
		}

		[Obsolete("Use Register(Component.For(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent(string key, Type classType, LifestyleType lifestyle, bool overwriteLifestyle)
		{
			AddComponent(key, classType, classType, lifestyle, overwriteLifestyle);
		}

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void AddComponent(String key, Type serviceType, Type classType)
		{
			AddComponent(key, serviceType, classType, LifestyleType.Singleton);
		}

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent(string key, Type serviceType, Type classType, LifestyleType lifestyle)
		{
			AddComponent(key, serviceType, classType, lifestyle, false);
		}

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent(string key, Type serviceType, Type classType, LifestyleType lifestyle, bool overwriteLifestyle)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (classType == null)
			{
				throw new ArgumentNullException("classType");
			}
			if (LifestyleType.Undefined == lifestyle)
			{
				throw new ArgumentException("The specified lifestyle must be Thread, Transient, or Singleton.", "lifestyle");
			}
			var model = ComponentModelFactory.BuildModel(new ComponentName(key, true), new[] { serviceType }, classType, null);

			if (overwriteLifestyle || LifestyleType.Undefined == model.LifestyleType)
			{
				model.LifestyleType = lifestyle;
			}

			RaiseComponentModelCreated(model);

			var handler = HandlerFactory.Create(model);
			RegisterHandler(key, handler);
		}

		[Obsolete("Use Register(Component.For<T>()) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent<T>()
		{
			var classType = typeof(T);
			AddComponent(classType.FullName, classType);
		}

		[Obsolete("Use Register(Component.For<T>().Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent<T>(LifestyleType lifestyle)
		{
			var classType = typeof(T);
			AddComponent(classType.FullName, classType, lifestyle);
		}

		[Obsolete("Use Register(Component.For<T>().Lifestyle.Is(lifestyle)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent<T>(LifestyleType lifestyle, bool overwriteLifestyle)
		{
			var classType = typeof(T);
			AddComponent(classType.FullName, classType, lifestyle, overwriteLifestyle);
		}

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>()) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent<T>(Type serviceType)
		{
			var classType = typeof(T);
			AddComponent(classType.FullName, serviceType, classType);
		}

		[Obsolete(
			"Use Register(Component.For(serviceType).ImplementedBy<T>().Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent<T>(Type serviceType, LifestyleType lifestyle)
		{
			var classType = typeof(T);
			AddComponent(classType.FullName, serviceType, classType, lifestyle);
		}

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>().Lifestyle.Is(lifestyle)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponent<T>(Type serviceType, LifestyleType lifestyle, bool overwriteLifestyle)
		{
			var classType = typeof(T);
			AddComponent(classType.FullName, serviceType, classType, lifestyle, overwriteLifestyle);
		}

		[Obsolete("Use Register(Component.For(instance.GetType()).Named(key).Instance(instance)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponentInstance(String key, object instance)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}

			var classType = instance.GetType();
			var model = new ComponentModel(new ComponentName(key, true), new[] { classType }, classType, new Arguments().Insert("instance", instance))
			{
				LifestyleType = LifestyleType.Singleton,
				CustomComponentActivator = typeof(ExternalInstanceActivator)
			};

			RaiseComponentModelCreated(model);
			var handler = HandlerFactory.Create(model);
			RegisterHandler(key, handler);
		}

		[Obsolete("Use Register(Component.For(serviceType).Named(key).Instance(instance)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponentInstance(String key, Type serviceType, object instance)
		{
			AddComponentInstance(key, serviceType, instance.GetType(), instance);
		}

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).Instance(instance)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponentInstance(string key, Type serviceType, Type classType, object instance)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			if (classType == null)
			{
				throw new ArgumentNullException("classType");
			}

			var model = new ComponentModel(new ComponentName(key, true), new[] { serviceType }, classType, new Arguments().Insert("instance", instance))
			{
				LifestyleType = LifestyleType.Singleton,
				CustomComponentActivator = typeof(ExternalInstanceActivator)
			};

			RaiseComponentModelCreated(model);
			var handler = HandlerFactory.Create(model);
			RegisterHandler(key, handler);
		}

		[Obsolete("Use Register(Component.For<T>().Instance(instance)) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponentInstance<T>(object instance)
		{
			var serviceType = typeof(T);
			AddComponentInstance(serviceType.FullName, serviceType, instance);
		}

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy<T>().Instance(instance)) or generic version instead."
			)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void AddComponentInstance<T>(Type serviceType, object instance)
		{
			var classType = typeof(T);
			AddComponentInstance(classType.FullName, serviceType, classType, instance);
		}

		[Obsolete("Use Register(Component.For(classType).Named(key).ExtendedProperties(extendedProperties)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void AddComponentWithExtendedProperties(String key, Type classType, IDictionary extendedProperties)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (extendedProperties == null)
			{
				throw new ArgumentNullException("extendedProperties");
			}
			if (classType == null)
			{
				throw new ArgumentNullException("classType");
			}

			var model = ComponentModelFactory.BuildModel(new ComponentName(key, true), new[] { classType }, classType, extendedProperties);
			RaiseComponentModelCreated(model);
			var handler = HandlerFactory.Create(model);
			RegisterHandler(key, handler);
		}

		[Obsolete("Use Register(Component.For(serviceType).ImplementedBy(classType).Named(key).ExtendedProperties(extendedProperties)) or generic version instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual void AddComponentWithExtendedProperties(String key, Type serviceType, Type classType, IDictionary extendedProperties)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (extendedProperties == null)
			{
				throw new ArgumentNullException("extendedProperties");
			}
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (classType == null)
			{
				throw new ArgumentNullException("classType");
			}

			var model = ComponentModelFactory.BuildModel(new ComponentName(key, true), new[] { serviceType }, classType, extendedProperties);
			RaiseComponentModelCreated(model);
			var handler = HandlerFactory.Create(model);
			RegisterHandler(key, handler);
		}

		[Obsolete("Use AddFacility(IFacility) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IKernel AddFacility(String key, IFacility facility)
		{
			return AddFacility(facility);
		}

		[Obsolete("Use AddFacility<TFacility>() instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IKernel AddFacility<T>(String key) where T : IFacility, new()
		{
			return AddFacility(new T());
		}

		[Obsolete("Use AddFacility<TFacility>(Action<TFacility>) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IKernel AddFacility<T>(String key, Action<T> onCreate)
			where T : IFacility, new()
		{
			return AddFacility(onCreate);
		}

		/// <summary>
		///   Returns the component instance by the component key
		///   using dynamic arguments
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		[Obsolete("Use Resolve<object>(key, arguments) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public object Resolve(string key, IDictionary arguments)
		{
			return (this as IKernelInternal).Resolve(key, service: null, arguments: arguments, policy: ReleasePolicy);
		}

		/// <summary>
		///   Returns the component instance by the component key
		///   using dynamic arguments
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "argumentsAsAnonymousType"></param>
		/// <returns></returns>
		[Obsolete("Use Resolve<object>(key, argumentsAsAnonymousType) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public object Resolve(string key, object argumentsAsAnonymousType)
		{
			return (this as IKernelInternal).Resolve(key, null, new ReflectionBasedDictionaryAdapter(argumentsAsAnonymousType), ReleasePolicy);
		}
	}
}