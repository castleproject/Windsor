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
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	[Singleton]
	public class DelegateFactory : ILazyComponentLoader
	{
		private static ITypedFactoryComponentSelector defaultSelector = new DefaultDelegateComponentSelector();

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

			return Component.For(service)
				.Named(key)
				.LifeStyle.Transient
				.Interceptors(new InterceptorReference(TypedFactoryFacility.InterceptorKey)).Last
				.UsingFactoryMethod((k, m, c) =>
				{
					var delegateProxyFactory = k.Resolve<IProxyFactoryExtension>(TypedFactoryFacility.DelegateProxyFactoryKey,
					                                                             new Arguments().Insert("targetDelegateType", service));
					var @delegate = k.ProxyFactory.Create(delegateProxyFactory, k, m, c);

					k.ReleaseComponent(delegateProxyFactory);
					return @delegate;
				}).DynamicParameters((k, d) =>
				{
					var selector = defaultSelector;
					d.Insert(selector);
				});
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

		protected virtual bool ShouldLoad(string key, Type service)
		{
			return true;
		}

		#endregion

		protected static bool HasReturn(MethodInfo invoke)
		{
			return invoke.ReturnType != typeof(void);
		}

		protected static MethodInfo GetInvokeMethod(Type @delegate)
		{
			var invoke = @delegate.GetMethod("Invoke");
			Debug.Assert(invoke != null, "invoke != null");
			return invoke;
		}
	}
}