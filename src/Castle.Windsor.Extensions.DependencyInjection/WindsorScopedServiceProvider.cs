// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Extensions.DependencyInjection
{
	using Castle.MicroKernel.Handlers;
	using Castle.Windsor;
	using Castle.Windsor.Extensions.DependencyInjection.Scope;
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	internal class WindsorScopedServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable
#if NET6_0_OR_GREATER
	, IServiceProviderIsService
#endif
#if NET8_0_OR_GREATER
		, IKeyedServiceProvider, IServiceProviderIsKeyedService
#endif
	{
		private readonly ExtensionContainerScopeBase scope;
		private bool disposing;

		private readonly IWindsorContainer container;

		public WindsorScopedServiceProvider(IWindsorContainer container)
		{
			this.container = container;
			scope = ExtensionContainerScopeCache.Current;
		}

		public object GetService(Type serviceType)
		{
			using (_ = new ForcedScope(scope))
			{
				return ResolveInstanceOrNull(serviceType, true);
			}
		}

#if NET8_0_OR_GREATER

		public object GetKeyedService(Type serviceType, object serviceKey)
		{
			using (_ = new ForcedScope(scope))
			{
				return ResolveInstanceOrNull(serviceType, serviceKey, true);
			}
		}

		public object GetRequiredKeyedService(Type serviceType, object serviceKey)
		{
			using (_ = new ForcedScope(scope))
			{
				return ResolveInstanceOrNull(serviceType, serviceKey, false);
			}
		}

#endif

		public object GetRequiredService(Type serviceType)
		{
			using (_ = new ForcedScope(scope))
			{
				return ResolveInstanceOrNull(serviceType, false);
			}
		}

		public void Dispose()
		{
			// root scope should be tied to the root IserviceProvider, so
			// it has to be disposed with the IserviceProvider to which is tied to
			if (!(scope is ExtensionContainerRootScope)) return;
			if (disposing) return;
			disposing = true;
			var disposableScope = scope as IDisposable;
			disposableScope?.Dispose();
			// disping the container here is questionable... what if I want to create another IServiceProvider form the factory?
			container.Dispose();
		}

		private object ResolveInstanceOrNull(Type serviceType, bool isOptional)
		{
			if (container.Kernel.HasComponent(serviceType))
			{
#if NET8_0_OR_GREATER
				//this is complicated by the concept of keyed service, because if you are about to resolve WITHOUTH KEY you do not
				//need to resolve keyed services. Now Keyed services are available only in version 8 but we register with an helper
				//all registered services so we can know if a service was really registered with keyed service or not.
				var componentRegistration = container.Kernel.GetHandler(serviceType);
				if (componentRegistration.ComponentModel.Name.StartsWith(KeyedRegistrationHelper.KeyedRegistrationPrefix))
				{
					//Component was registered as keyed component, so we really need to resolve with null because this is the old interface
					//so no key is provided.
					return null;
				}
#endif
				return container.Resolve(serviceType);
			}

			if (serviceType.GetTypeInfo().IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				//ok we want to resolve all references, NON keyed services
				var typeToResolve = serviceType.GenericTypeArguments[0];
				var allRegisteredTypes = container.Kernel.GetHandlers(typeToResolve);
				var allNonKeyedService = allRegisteredTypes.Where(x => !x.ComponentModel.Name.StartsWith(KeyedRegistrationHelper.KeyedRegistrationPrefix)).ToList();
				//now we need to resolve one by one all these services
				var listType = typeof(List<>).MakeGenericType(typeToResolve);
				var objects = (System.Collections.IList)Activator.CreateInstance(listType);

				if (allNonKeyedService.Count == 0)
				{
					return objects;
				}
				else if (allNonKeyedService.Count == allRegisteredTypes.Length)
				{
					//simply resolve all
					return container.ResolveAll(typeToResolve);
				}

				//if we reach here some of the services are kyed and some are not, so we need to resolve one by one.
				for (int i = 0; i < allNonKeyedService.Count; i++)
				{
					var service = allNonKeyedService[i];
					object obj;
					//type is non generic, we can directly resolve.
					try
					{
						obj = container.Resolve(allNonKeyedService[i].ComponentModel.Name, typeToResolve);
						objects.Add(obj);
					}
					catch (GenericHandlerTypeMismatchException)
					{
						//ignore, this is the standard way we know that we cannot instantiate an open generic with a given type.
					}
				}

				return objects;
			}

			if (isOptional)
			{
				return null;
			}

			return container.Resolve(serviceType);
		}

#if NET6_0_OR_GREATER

		public bool IsService(Type serviceType)
		{
			if (serviceType.IsGenericTypeDefinition)
			{
				//Framework does not want the open definition to return true
				return false;
			}

			//IEnumerable always return true
			if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				var enumerableType = serviceType.GenericTypeArguments[0];
				if (container.Kernel.HasComponent(enumerableType))
				{
					return true;
				}
				//Try to check if the real type is registered: framework test IEnumerableWithIsServiceAlwaysReturnsTrue1
				var interfaces = enumerableType.GetInterfaces();
				return interfaces.Any(container.Kernel.HasComponent);
			}
			return container.Kernel.HasComponent(serviceType);
		}

#endif

#if NET8_0_OR_GREATER

		private object ResolveInstanceOrNull(Type serviceType, object serviceKey, bool isOptional)
		{
			if (serviceKey == null)
			{
				return ResolveInstanceOrNull(serviceType, isOptional);
			}

			KeyedRegistrationHelper keyedRegistrationHelper = KeyedRegistrationHelper.GetInstance(container);

			if (container.Kernel.HasComponent(serviceType))
			{
				var keyRegistrationHelper = keyedRegistrationHelper.GetKey(serviceKey, serviceType);
				//this is a keyed service, actually we need to grab the name from the service key
				if (keyRegistrationHelper != null)
				{
					return keyRegistrationHelper.Resolve(container, serviceKey);
				}
			}

			if (serviceType.GetTypeInfo().IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				var typeToResolve = serviceType.GenericTypeArguments[0];
				var registrations = keyedRegistrationHelper.GetKeyedRegistrations(typeToResolve);
				var regisrationsWithKey = registrations.Where(x => x.Key == serviceKey).ToList();

				if (regisrationsWithKey.Count > 0)
				{
					var listType = typeof(List<>).MakeGenericType(typeToResolve);
					var objects = (System.Collections.IList)Activator.CreateInstance(listType);
					foreach (var registration in regisrationsWithKey)
					{
						var obj = registration.Resolve(container, serviceKey);
						objects.Add(obj);
					}
					return objects;
				}
			}

			if (isOptional)
			{
				return null;
			}

			return container.Resolve(serviceType);
		}

		public bool IsKeyedService(Type serviceType, object serviceKey)
		{
			//we just need to know if the key is registered.
			if (serviceKey == null)
			{
				//test NonKeyedServiceWithIsKeyedService shows that for real inversion of control when sercvice key is null
				//it just mean that we need to know if the service is registered.
				return IsService(serviceType);
			}
			return KeyedRegistrationHelper.GetInstance(container).HasKey(serviceKey);
		}

#endif
	}
}