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
			foreach (IReference<IInterceptor> interceptorRef in GetInterceptorsFor(model))
			{
				try
				{
					var interceptor = interceptorRef.Resolve(kernel, context);
					SetOnBehalfAware(interceptor as IOnBehalfAware, model);
					interceptors.Add(interceptor);
				}
				catch (Exception e)
				{
					foreach (var interceptor in interceptors)
					{
						kernel.ReleaseComponent(interceptor);
					}

					if(e is InvalidCastException)
					{
						var message = String.Format(
						"An interceptor registered for {0} doesn't implement the {1} interface",
						model.Name, typeof(IInterceptor).Name);

						throw new DependencyResolverException(message);
					}
					throw;
				}
			}

			return interceptors.ToArray();
		}


		public void AddInterceptorSelector(IModelInterceptorsSelector selector)
		{
			selectors.Add(selector);
		}

		public bool ShouldCreateProxy(ComponentModel model)
		{
			if (selectors.Any(s => s.HasInterceptors(model)))
			{
				return true;
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