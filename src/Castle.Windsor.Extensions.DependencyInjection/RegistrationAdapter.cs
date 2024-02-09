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
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Extensions.DependencyInjection.Extensions;
	using Microsoft.Extensions.DependencyInjection;
	using System;

	internal static class RegistrationAdapter
	{
		public static IRegistration FromOpenGenericServiceDescriptor(
			Microsoft.Extensions.DependencyInjection.ServiceDescriptor service,
			IWindsorContainer windsorContainer)
		{
			ComponentRegistration<object> registration;

#if NET8_0_OR_GREATER
			if (service.IsKeyedService)
			{
				registration = Component.For(service.ServiceType)
					.Named(KeyedRegistrationHelper.GetInstance(windsorContainer).GetOrCreateKey(service.ServiceKey, service));
				if (service.KeyedImplementationType != null)
				{
					registration = UsingImplementation(registration, service);
				}
				else
				{
					throw new System.ArgumentException("Unsupported ServiceDescriptor");
				}
			}
			else
			{
				registration = Component.For(service.ServiceType)
					.NamedAutomatically(UniqueComponentName(service));
				if (service.ImplementationType != null)
				{
					registration = UsingImplementation(registration, service);
				}
				else
				{
					throw new System.ArgumentException("Unsupported ServiceDescriptor");
				}
			}
#else
			registration = Component.For(service.ServiceType)
				.NamedAutomatically(UniqueComponentName(service));
			if (service.ImplementationType != null)
			{
				registration = UsingImplementation(registration, service);
			}
			else
			{
				throw new System.ArgumentException("Unsupported ServiceDescriptor");
			}
#endif
			return ResolveLifestyle(registration, service)
				.IsDefault();
		}

		public static IRegistration FromServiceDescriptor(
			Microsoft.Extensions.DependencyInjection.ServiceDescriptor service,
			IWindsorContainer windsorContainer)
		{
			ComponentRegistration<object> registration;
#if NET8_0_OR_GREATER
			if (service.IsKeyedService)
			{
				registration = Component.For(service.ServiceType)
					.Named(KeyedRegistrationHelper.GetInstance(windsorContainer).GetOrCreateKey(service.ServiceKey, service));

				if (service.KeyedImplementationFactory != null)
				{
					registration = UsingFactoryMethod(registration, service);
				}
				else if (service.KeyedImplementationInstance != null)
				{
					registration = UsingInstance(registration, service);
				}
				else if (service.KeyedImplementationType != null)
				{
					registration = UsingImplementation(registration, service);
				}
			}
			else
			{
				registration = Component.For(service.ServiceType)
					.NamedAutomatically(UniqueComponentName(service));
				if (service.ImplementationFactory != null)
				{
					registration = UsingFactoryMethod(registration, service);
				}
				else if (service.ImplementationInstance != null)
				{
					registration = UsingInstance(registration, service);
				}
				else if (service.ImplementationType != null)
				{
					registration = UsingImplementation(registration, service);
				}
			}
#else
			registration = Component.For(service.ServiceType)
				.NamedAutomatically(UniqueComponentName(service));
			if (service.ImplementationFactory != null)
			{
				registration = UsingFactoryMethod(registration, service);
			}
			else if (service.ImplementationInstance != null)
			{
				registration = UsingInstance(registration, service);
			}
			else if (service.ImplementationType != null)
			{
				registration = UsingImplementation(registration, service);
			}
#endif
			return ResolveLifestyle(registration, service)
				.IsDefault();
		}

		public static string OriginalComponentName(string uniqueComponentName)
		{
			if (uniqueComponentName == null)
			{
				return null;
			}
			if (!uniqueComponentName.Contains("@"))
			{
				return uniqueComponentName;
			}
			return uniqueComponentName.Split('@')[0];
		}

		internal static string UniqueComponentName(Microsoft.Extensions.DependencyInjection.ServiceDescriptor service)
		{
			var result = "";
#if NET8_0_OR_GREATER
			if (service.IsKeyedService)
			{
				if (service.KeyedImplementationType != null)
				{
					result = service.KeyedImplementationType.FullName;
				}
				else if (service.KeyedImplementationInstance != null)
				{
					result = service.KeyedImplementationInstance.GetType().FullName;
				}
				else
				{
					result = service.KeyedImplementationFactory.GetType().FullName;
				}
			}
			else
			{
				if (service.ImplementationType != null)
				{
					result = service.ImplementationType.FullName;
				}
				else if (service.ImplementationInstance != null)
				{
					result = service.ImplementationInstance.GetType().FullName;
				}
				else
				{
					result = service.ImplementationFactory.GetType().FullName;
				}
			}
#else
if (service.ImplementationType != null)
			{
				result = service.ImplementationType.FullName;
			}
			else if (service.ImplementationInstance != null)
			{
				result = service.ImplementationInstance.GetType().FullName;
			}
			else
			{
				result = service.ImplementationFactory.GetType().FullName;
			}

			
#endif
			result = result + "@" + Guid.NewGuid().ToString();

			return result;
		}

		private static ComponentRegistration<TService> UsingFactoryMethod<TService>(
			ComponentRegistration<TService> registration,
			Microsoft.Extensions.DependencyInjection.ServiceDescriptor service) where TService : class
		{
			return registration.UsingFactoryMethod((kernel) =>
			{
				//TODO: We should register a factory in castle that in turns call the implementation factory??
				var serviceProvider = kernel.Resolve<System.IServiceProvider>();
#if NET8_0_OR_GREATER
				if (service.IsKeyedService)
				{
					return service.KeyedImplementationFactory(serviceProvider, service.ServiceKey) as TService;
				}
				else
				{
					return service.ImplementationFactory(serviceProvider) as TService;
				}
#else
				return service.ImplementationFactory(serviceProvider) as TService;
				
#endif
			});
		}

		private static ComponentRegistration<TService> UsingInstance<TService>(ComponentRegistration<TService> registration, Microsoft.Extensions.DependencyInjection.ServiceDescriptor service) where TService : class
		{
#if NET8_0_OR_GREATER
			if (service.IsKeyedService)
			{
				return registration.Instance(service.KeyedImplementationInstance as TService);
			}
			else
			{
				return registration.Instance(service.ImplementationInstance as TService);
			}
#else
			return registration.Instance(service.ImplementationInstance as TService);
#endif
		}

		private static ComponentRegistration<TService> UsingImplementation<TService>(ComponentRegistration<TService> registration, Microsoft.Extensions.DependencyInjection.ServiceDescriptor service) where TService : class
		{
#if NET8_0_OR_GREATER
			if (service.IsKeyedService)
			{
				return registration.ImplementedBy(service.KeyedImplementationType);
			}
			else
			{
				return registration.ImplementedBy(service.ImplementationType);
			}
#else
			return registration.ImplementedBy(service.ImplementationType);
#endif
		}

		private static ComponentRegistration<TService> ResolveLifestyle<TService>(ComponentRegistration<TService> registration, Microsoft.Extensions.DependencyInjection.ServiceDescriptor service) where TService : class
		{
			switch (service.Lifetime)
			{
				case ServiceLifetime.Singleton:
					return registration.LifeStyle.NetStatic();
				case ServiceLifetime.Scoped:
					return registration.LifeStyle.ScopedToNetServiceScope();
				case ServiceLifetime.Transient:
					return registration.LifestyleNetTransient();

				default:
					throw new System.ArgumentException($"Invalid lifetime {service.Lifetime}");
			}
		}
	}
}