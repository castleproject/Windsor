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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Util;

	/// <summary>
	///   Inspect the component for <c>InterceptorAttribute</c> and
	///   the configuration for the interceptors node
	/// </summary>
	[Serializable]
	public class InterceptorInspector : IContributeComponentModelConstruction
	{
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			CollectFromAttributes(model);
			CollectFromConfiguration(model);
		}

		protected virtual void AddInterceptor(InterceptorReference interceptorRef, InterceptorReferenceCollection interceptors)
		{
			interceptors.Add(interceptorRef);
		}

		protected virtual void CollectFromAttributes(ComponentModel model)
		{
			var attributes = model.Implementation.GetAttributes<InterceptorAttribute>();
			foreach (var attribute in attributes)
			{
				AddInterceptor(attribute.Interceptor, model.Interceptors);
			}
		}

		protected virtual void CollectFromConfiguration(ComponentModel model)
		{
			if (model.Configuration == null)
			{
				return;
			}

			var interceptors = model.Configuration.Children["interceptors"];
			if (interceptors == null)
			{
				return;
			}

			CollectInterceptors(model, interceptors);
			var options = model.ObtainProxyOptions();
			CollectSelector(interceptors, options);
			CollectHook(interceptors, options);
		}

		protected virtual void CollectHook(IConfiguration interceptors, ProxyOptions options)
		{
			var hook = interceptors.Attributes["hook"];
			if (hook == null)
			{
				return;
			}

			var hookComponent = ReferenceExpressionUtil.ExtractComponentName(hook);
			if (hookComponent == null)
			{
				throw new Exception(
					String.Format("The value for the hook must be a reference to a component (Currently {0})", hook));
			}

			options.Hook = new ComponentReference<IProxyGenerationHook>(hookComponent);
		}

		protected virtual void CollectSelector(IConfiguration interceptors, ProxyOptions options)
		{
			var selector = interceptors.Attributes["selector"];
			if (selector == null)
			{
				return;
			}

			var selectorComponent = ReferenceExpressionUtil.ExtractComponentName(selector);
			if (selectorComponent == null)
			{
				throw new Exception(
					String.Format("The value for the selector must be a reference to a component (Currently {0})", selector));
			}

			options.Selector = new ComponentReference<IInterceptorSelector>(selectorComponent);
		}

		private void CollectInterceptors(ComponentModel model, IConfiguration interceptors)
		{
			foreach (var interceptor in interceptors.Children)
			{
				var interceptorComponent = ReferenceExpressionUtil.ExtractComponentName(interceptor.Value);
				if (interceptorComponent == null)
				{
					throw new Exception(
						String.Format("The value for the interceptor must be a reference to a component (Currently {0})", interceptor.Value));
				}

				var reference = new InterceptorReference(interceptorComponent);

				model.Interceptors.Add(reference);
			}
		}
	}
}