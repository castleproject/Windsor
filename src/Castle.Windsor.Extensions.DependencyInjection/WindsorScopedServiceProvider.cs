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
	using Castle.Core.Logging;
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
		private ILogger _logger = NullLogger.Instance;
		private readonly IWindsorContainer container;

		public WindsorScopedServiceProvider(IWindsorContainer container)
		{
			this.container = container;
			scope = ExtensionContainerScopeCache.Current;

			if (container.Kernel.HasComponent(typeof(ILoggerFactory)))
			{
				var loggerFactory = container.Resolve<ILoggerFactory>();
				_logger = loggerFactory.Create(typeof(WindsorScopedServiceProvider));
			}
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
			// we have also another consideration. When we use orleans we find that orleans dispose the Scoped service provider and
			// since we have a single container, we cannot dispose here. The global container must be disposed from external code.
			// container.Dispose();
		}

		private object ResolveInstanceOrNull(Type serviceType, bool isOptional)
		{
			if (container.Kernel.HasComponent(serviceType))
			{
				//this is complicated by the concept of keyed service, because if you are about to resolve WITHOUTH KEY you do not
				//need to resolve keyed services. Now Keyed services are available only in version 8 but we register with an helper
				//all registered services so we can know if a service was really registered with keyed service or not.
				var componentRegistrations = container.Kernel.GetHandlers(serviceType);

				//now since the caller requested a NON Keyed component, we need to skip all keyed components.
				var realRegistrations = componentRegistrations.Where(x => !x.ComponentModel.Name.StartsWith(KeyedRegistrationHelper.KeyedRegistrationPrefix)).ToList();
				string registrationName = null;
				if (realRegistrations.Count == 1)
				{
					registrationName = realRegistrations[0].ComponentModel.Name;
				}
				else if (realRegistrations.Count == 0)
				{
					//No component is registered for the interface without key, resolution cannot be done.
					registrationName = null;
				}
				else if (realRegistrations.Count > 1)
				{
					//ok we have a big problem, we have multiple registration and different semantic, because
					//Microsoft.DI wants the latest registered service to win
					//Caste instead wants the first registered service to win.

					//how can we live with this to have a MINIMUM (never zero) impact on everything that registers things?
					//we need to determine who registered the components.
					var registeredByMicrosoftDi = realRegistrations.Any(r => r.ComponentModel.ExtendedProperties.Any(ep => RegistrationAdapter.RegistrationKeyExtendedPropertyKey.Equals(ep.Key)));

					if (!registeredByMicrosoftDi)
					{
						if (_logger.IsDebugEnabled)
						{
							_logger.Debug($@"Multiple components registered for service {serviceType.FullName} All services {string.Join(",", realRegistrations.Select(r => r.ComponentModel.Implementation.Name))}");
						}

						//ok we are in a situation where no component was registered through the adapter, this is the situatino of a component
						//registered purely in castle (this should mean that the user want to use castle semantic).
						//let the standard castle rules apply.
						return container.Resolve(serviceType);
					}
					else
					{
						//If we are here at least one of the component was registered throuh Microsoft.DI, this means that the code that regiestered
						//the component want to use the semantic of Microsoft.DI. This means that we need to use different set of rules.

						//RULES:
						//more than one component is registered for the interface without key, we have some ambiguity that is resolved, based on test
						//found in framework with this rule. In this situation we do not use the same rule of Castle where the first service win but
						//we use the framework rule that:
						//1. Last component win.
						//2. closed service are preferred over open generic.

						//take first non generic
						for (int i = realRegistrations.Count - 1; i >= 0; i--)
						{
							if (!realRegistrations[i].ComponentModel.Implementation.IsGenericTypeDefinition)
							{
								registrationName = realRegistrations[i].ComponentModel.Name;
								break;
							}
						}

						//if we did not find any non generic, take the last one.
						if (registrationName == null)
						{
							registrationName = realRegistrations[realRegistrations.Count - 1].ComponentModel.Name;
						}
					}

					if (_logger.IsDebugEnabled)
					{
						_logger.Debug($@"Multiple components registered for service {serviceType.FullName}. Selected component {registrationName}
all services {string.Join(",", realRegistrations.Select(r => r.ComponentModel.Implementation.Name))}");
					}
				}

				if (registrationName == null)
				{
					return null;
				}
				return container.Resolve(registrationName, serviceType);
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

		private static bool ComponentIsDefault(KeyValuePair<object, object> property)
		{
			if (!Core.Internal.Constants.DefaultComponentForServiceFilter.Equals(property.Key))
			{
				//not the property we are looking for
				return false;
			}

			if (property.Value is bool boolValue)
			{
				return boolValue;
			}

			if (property.Value is Predicate<Type> predicate)
			{
				//this is a method info that we can invoke to get the value.
				return predicate(null);
			}

			return false;
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