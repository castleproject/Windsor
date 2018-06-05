// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.TypedFactory.Internal
{
	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	[Singleton]
	public class DelegateFactory : ILazyComponentLoader
	{
		public IRegistration Load(string name, Type service, IDictionary arguments)
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
			if (invoke.ReturnType.IsPrimitiveTypeOrCollection())
			{
				return null;
			}

			if (service.GetTypeInfo().IsGenericType)
			{
				service = service.GetGenericTypeDefinition();
			}

			return Component.For(service)
				.ImplementedBy(service, DelegateServiceStrategy.Instance)
				.NamedAutomatically(GetName(service))
				.LifeStyle.Transient
				.Interceptors(new InterceptorReference(TypedFactoryFacility.InterceptorKey)).Last
				.Activator<DelegateFactoryActivator>()
				.DynamicParameters((k, d) =>
				                   	{
				                   		var selector = k.Resolve<ITypedFactoryComponentSelector>(TypedFactoryFacility.DefaultDelegateSelectorKey);
				                   		d.InsertTyped(selector);
				                   		return k2 => k2.ReleaseComponent(selector);
				                   	})
				.AddAttributeDescriptor(TypedFactoryFacility.IsFactoryKey, bool.TrueString);
		}

		protected string GetName(Type service)
		{
			var defaultName = ComponentName.DefaultNameFor(service);
			if (string.IsNullOrEmpty(defaultName))
			{
				return "auto-factory: " + Guid.NewGuid();
			}
			return "auto-factory: " + defaultName;
		}

		public static MethodInfo ExtractInvokeMethod(Type service)
		{
			if (!service.Is<MulticastDelegate>())
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

		protected static MethodInfo GetInvokeMethod(Type @delegate)
		{
			var invoke = @delegate.GetMethod("Invoke");
			Debug.Assert(invoke != null, "invoke != null");
			return invoke;
		}

		protected static bool HasReturn(MethodInfo invoke)
		{
			return invoke.ReturnType != typeof(void);
		}
	}
}