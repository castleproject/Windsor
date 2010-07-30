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

namespace Castle.Windsor.Proxy
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Interceptor;
	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Resolvers;

	public abstract class AbstractProxyFactory : IProxyFactory
	{
		private readonly IList<IModelInterceptorsSelector> selectors = new List<IModelInterceptorsSelector>();

		public abstract object Create(IKernel kernel, object instance, ComponentModel model, CreationContext context,
		                              params object[] constructorArguments);

		public abstract object Create(IProxyFactoryExtension customFactory, IKernel kernel, ComponentModel model,
		                              CreationContext context, params object[] constructorArguments);

		public abstract bool RequiresTargetInstance(IKernel kernel, ComponentModel model);

		protected IEnumerable<InterceptorReference> GetInterceptorsFor(ComponentModel model)
		{
			var interceptors = model.Interceptors.ToArray();
			foreach (var selector in selectors)
			{
				if (selector.HasInterceptors(model) == false)
				{
					continue;
				}

				interceptors = selector.SelectInterceptors(model, interceptors);
				if (interceptors == null)
				{
					interceptors = new InterceptorReference[0];
				}
			}

			foreach (var interceptor in interceptors)
			{
				yield return interceptor;
			}
		}

		/// <summary>
		/// Obtains the interceptors associated with the component.
		/// </summary>
		/// <param name="kernel">The kernel instance</param>
		/// <param name="model">The component model</param>
		/// <param name="context">The creation context</param>
		/// <returns>interceptors array</returns>
		protected IInterceptor[] ObtainInterceptors(IKernel kernel, ComponentModel model, CreationContext context)
		{
			var interceptors = new List<IInterceptor>();

			foreach (InterceptorReference interceptorRef in GetInterceptorsFor(model))
			{
				IHandler handler;
				if (interceptorRef.ReferenceType == InterceptorReferenceType.Interface)
				{
					handler = kernel.GetHandler(interceptorRef.ServiceType);
				}
				else
				{
					handler = kernel.GetHandler(interceptorRef.ComponentKey);
				}

				if (handler == null)
				{
					// This shoul be virtually impossible to happen
					// Seriously!
					throw new Exception("The interceptor could not be resolved");
				}

				if(handler.IsBeingResolvedInContext(context))
				{
					throw new DependencyResolverException(
						string.Format(
							"Cycle detected - interceptor {0} wants to use itself as its interceptor. This usually signifies a bug in custom {1}",
							handler.ComponentModel.Name, typeof(IModelInterceptorsSelector).Name));
				}

				try
				{
					var contextForInterceptor = RebuildContext(handler.Service, context);
					var interceptor = (IInterceptor)handler.Resolve(contextForInterceptor);

					interceptors.Add(interceptor);

					SetOnBehalfAware(interceptor as IOnBehalfAware, model);
				}
				catch (InvalidCastException)
				{
					var message = String.Format(
						"An interceptor registered for {0} doesn't implement the IInterceptor interface",
						model.Name);

					throw new Exception(message);
				}
			}

			return interceptors.ToArray();
		}

		private CreationContext RebuildContext(Type parameterType, CreationContext current)
		{
			if (parameterType.ContainsGenericParameters)
			{
				return current;
			}

			return new CreationContext(parameterType, current, true);
		}

		public void AddInterceptorSelector(IModelInterceptorsSelector selector)
		{
			selectors.Add(selector);
		}

		public bool ShouldCreateProxy(ComponentModel model)
		{
			foreach (var selector in selectors)
			{
				if (selector.HasInterceptors(model))
				{
					return true;
				}
			}

			if (model.Interceptors.HasInterceptors)
			{
				return true;
			}

			var options = ProxyUtil.ObtainProxyOptions(model, false);
			if (options == null)
			{
				return false;
			}

			return options.MixIns.Any() || options.AdditionalInterfaces.Any();
		}

		protected static void SetOnBehalfAware(IOnBehalfAware onBehalfAware, ComponentModel target)
		{
			if (onBehalfAware != null)
			{
				onBehalfAware.SetInterceptedComponentModel(target);
			}
		}
	}
}