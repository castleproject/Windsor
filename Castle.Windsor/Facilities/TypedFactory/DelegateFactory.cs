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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	[Singleton]
	public class DelegateFactory : ILazyComponentLoader
	{
		private readonly IKernel kernel;

		public DelegateFactory(IKernel kernel)
		{
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			this.kernel = kernel;
		}

		#region ILazyComponentLoader Members

		public IRegistration Load(string key, Type service)
		{
			if (service == null)
			{
				return null;
			}

			var invoke = ExtractInvokeMethod(service);
			if (invoke == null)
			{
				return null;
			}

			if (ShouldLoad(key, service) == false)
			{
				return null;
			}

			var serviceName = ExtractServiceName(key);
			var handler = GetHandlerToBeResolvedByDelegate(invoke, serviceName);
			if (handler == null)
			{
				return null;
			}




			return Component.For(service)
				.Named(key)
				.LifeStyle.Transient
				.UsingFactoryMethod((k,m, c) =>
				{
					var delegateProxyFactory = k.Resolve<IProxyFactoryExtension>(TypedFactoryFacility.DelegateProxyFactoryKey,
					                                                             new Arguments(new { targetDelegateType = service }));
					var @delegate = k.ProxyFactory.Create(delegateProxyFactory, k, m, c);
					
					k.ReleaseComponent(delegateProxyFactory);
					return @delegate;
				})
				.DynamicParameters((k, d) =>
				{
					var selector = new DefaultDelegateComponentSelector();
					d.Insert<ITypedFactoryComponentSelector>(selector);
				})
				.Interceptors(new InterceptorReference(TypedFactoryFacility.InterceptorKey)).Last;
		}

		public static MethodInfo ExtractInvokeMethod(Type service)
		{
			if (!typeof(MulticastDelegate).IsAssignableFrom(service))
			{
				return null;
			}
			
			var invoke = GetInvokeMethod(service);
			if (!HasReturn(invoke)) 
			{
				return null;
			}
			
			return invoke;
		}

		protected virtual string ExtractServiceName(string key)
		{
			return key;
		}

		protected virtual bool ShouldLoad(string key, Type service)
		{
			return true;
		}

		#endregion

		protected virtual IHandler GetHandlerToBeResolvedByDelegate(MethodInfo invoke, string serviceName)
		{
			if(!string.IsNullOrEmpty(serviceName))
			{
				var handler = kernel.GetHandler(serviceName);
				if (handler != null)
				{
					return handler;
				}
			}

			var handlers = kernel.GetAssignableHandlers(invoke.ReturnType);
			if (handlers.Length == 1)
			{
				return handlers.Single();
			}
			var potentialHandler = handlers.SingleOrDefault(h => h.ComponentModel.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
			if (potentialHandler == null)
			{
				throw new NoUniqueComponentException(invoke.ReturnType,
				                                     "Delegate factory ({0}) was unable to uniquely nominate component to resolve for service '{1}'. " +
				                                     "You may provide your own selection logic, by registering custom DelegateFactory with key TypedFactoryFacility.DelegateFactoryKey " +
				                                     "before registering the facility.");
			}

			return potentialHandler;
		}

		protected static bool HasReturn(MethodInfo invoke)
		{
			return invoke.ReturnType != typeof(void);
		}

		protected static MethodInfo GetInvokeMethod(Type @delegate)
		{
			MethodInfo invoke = @delegate.GetMethod("Invoke");
			Debug.Assert(invoke != null, "invoke != null");
			return invoke;
		}
	}
}