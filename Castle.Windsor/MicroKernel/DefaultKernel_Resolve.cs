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
	using System.Collections;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel.Handlers;

	public partial class DefaultKernel
	{
		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name="key"></param>
		/// <param name="service"></param>
		/// <returns></returns>
		public virtual object Resolve(String key, Type service)
		{
			return (this as IKernelInternal).Resolve(key, service, null, releasePolicy);
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name="key"></param>
		/// <param name="service"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public virtual object Resolve(String key, Type service, IDictionary arguments)
		{
			return (this as IKernelInternal).Resolve(key, service, arguments, ReleasePolicy);
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name="key"></param>
		/// <param name="service"></param>
		/// <param name="arguments"></param>
		/// <param name="policy"></param>
		/// <returns></returns>
		object IKernelInternal.Resolve(String key, Type service, IDictionary arguments, IReleasePolicy policy)
		{
			var handler = (this as IKernelInternal).LoadHandlerByKey(key, service, arguments);
			return ResolveComponent(handler, service ?? typeof(object), arguments, policy);
		}

		/// <summary>
		///   Returns the component instance by the service type
		///   using dynamic arguments
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public T Resolve<T>(IDictionary arguments)
		{
			return (T)Resolve(typeof(T), arguments);
		}

		/// <summary>
		///   Returns the component instance by the service type
		///   using dynamic arguments
		/// </summary>
		/// <param name="argumentsAsAnonymousType"></param>
		/// <returns></returns>
		public T Resolve<T>(object argumentsAsAnonymousType)
		{
			return (T)Resolve(typeof(T),new ReflectionBasedDictionaryAdapter(argumentsAsAnonymousType));
		}

		/// <summary>
		///   Returns the component instance by the component key
		/// </summary>
		/// <returns></returns>
		public T Resolve<T>()
		{
			return (T)Resolve(typeof(T), null);
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name="key">Component's key</param>
		/// <typeparam name="T">Service type</typeparam>
		/// <returns>
		///   The Component instance
		/// </returns>
		public T Resolve<T>(String key)
		{
			return (T)(this as IKernelInternal).Resolve(key, typeof(T), arguments: null, policy: releasePolicy);
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <typeparam name="T">Service type</typeparam>
		/// <param name="key">Component's key</param>
		/// <param name="arguments"></param>
		/// <returns>
		///   The Component instance
		/// </returns>
		public T Resolve<T>(String key, IDictionary arguments)
		{
			return (T)(this as IKernelInternal).Resolve(key, typeof(T), arguments, releasePolicy);
		}

		/// <summary>
		///   Returns the component instance by the service type
		/// </summary>
		public object Resolve(Type service)
		{
			return (this as IKernelInternal).Resolve(service, null, releasePolicy);
		}

		/// <summary>
		///   Returns the component instance by the service type
		///   using dynamic arguments
		/// </summary>
		/// <param name="service"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public object Resolve(Type service, IDictionary arguments)
		{
			return (this as IKernelInternal).Resolve(service, arguments, releasePolicy);

		}

		object IKernelInternal.Resolve(Type service, IDictionary arguments, IReleasePolicy policy)
		{
			var handler = (this as IKernelInternal).LoadHandlerByType(null, service, arguments);
			return ResolveComponent(handler, service, arguments, policy);
		}

		/// <summary>
		///   Returns the component instance by the service type
		///   using dynamic arguments
		/// </summary>
		/// <param name="service"></param>
		/// <param name="argumentsAsAnonymousType"></param>
		/// <returns></returns>
		public object Resolve(Type service, object argumentsAsAnonymousType)
		{
			return Resolve(service, new ReflectionBasedDictionaryAdapter(argumentsAsAnonymousType));
		}

		/// <summary>
		///   Returns the component instance by the component key
		///   using dynamic arguments
		/// </summary>
		/// <param name="key"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public object Resolve(string key, IDictionary arguments)
		{
			return (this as IKernelInternal).Resolve(key, service: null, arguments: arguments, policy: releasePolicy);
		}

		/// <summary>
		///   Returns the component instance by the component key
		///   using dynamic arguments
		/// </summary>
		/// <param name="key"></param>
		/// <param name="argumentsAsAnonymousType"></param>
		/// <returns></returns>
		public object Resolve(string key, object argumentsAsAnonymousType)
		{
			return (this as IKernelInternal).Resolve(key, null, new ReflectionBasedDictionaryAdapter(argumentsAsAnonymousType), releasePolicy);
		}

		/// <summary>
		///   Returns all the valid component instances by
		///   the service type
		/// </summary>
		/// <param name="service">The service type</param>
		public Array ResolveAll(Type service)
		{
			return (this as IKernelInternal).ResolveAll(service, null, releasePolicy);
		}

		Array IKernelInternal.ResolveAll(Type service, IDictionary arguments, IReleasePolicy policy)
		{
			var resolved = new List<object>();
			foreach (var handler in GetHandlers(service))
			{
				if (handler.IsBeingResolvedInContext(currentCreationContext))
				{
					continue;
				}

				try
				{
					var component = ResolveComponent(handler, service, arguments, policy);
					resolved.Add(component);
				}
				catch (GenericHandlerTypeMismatchException)
				{
					// that's the only case where we ignore the component and allow it to not be resolved.
					// only because we have no way to actually test if generic constraints can be satisfied.
				}
			}

			var components = Array.CreateInstance(service, resolved.Count);
			((ICollection)resolved).CopyTo(components, 0);
			return components;
			
		}

		/// <summary>
		///   Returns all the valid component instances by
		///   the service type
		/// </summary>
		/// <param name="service">The service type</param>
		/// <param name="arguments">
		///   Arguments to resolve the services
		/// </param>
		public Array ResolveAll(Type service, IDictionary arguments)
		{
			return (this as IKernelInternal).ResolveAll(service, arguments, ReleasePolicy);
		}

		/// <summary>
		///   Returns all the valid component instances by
		///   the service type
		/// </summary>
		/// <param name="service">The service type</param>
		/// <param name="argumentsAsAnonymousType">
		///   Arguments to resolve the services
		/// </param>
		public Array ResolveAll(Type service, object argumentsAsAnonymousType)
		{
			return (this as IKernelInternal).ResolveAll(service, new ReflectionBasedDictionaryAdapter(argumentsAsAnonymousType), ReleasePolicy);
		}

		/// <summary>
		///   Returns component instances that implement TService
		/// </summary>
		/// <typeparam name="TService"></typeparam>
		/// <param name="argumentsAsAnonymousType"></param>
		/// <returns></returns>
		public TService[] ResolveAll<TService>(object argumentsAsAnonymousType)
		{
			return (TService[])(this as IKernelInternal).ResolveAll(typeof(TService), new ReflectionBasedDictionaryAdapter(argumentsAsAnonymousType), releasePolicy);
		}

		/// <summary>
		///   Returns component instances that implement TService
		/// </summary>
		/// <typeparam name="TService"></typeparam>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public TService[] ResolveAll<TService>(IDictionary arguments)
		{
			return (TService[])(this as IKernelInternal).ResolveAll(typeof(TService), arguments, releasePolicy);
		}

		/// <summary>
		///   Returns component instances that implement TService
		/// </summary>
		/// <typeparam name="TService"></typeparam>
		/// <returns></returns>
		public TService[] ResolveAll<TService>()
		{
			return (TService[])(this as IKernelInternal).ResolveAll(typeof(TService), null, releasePolicy);
		}
	}
}