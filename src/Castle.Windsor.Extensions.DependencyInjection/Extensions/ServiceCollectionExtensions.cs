// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//	 http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Windsor.Extensions.DependencyInjection.Extensions
{
	using System;
	using System.Reflection;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Extensions.DependencyInjection.Resolvers;

	using Microsoft.Extensions.DependencyInjection;

	public static class ServiceCollectionExtensions
	{
		public static IWindsorContainer CreateContainer(this IServiceCollection serviceCollection, WindsorServiceProviderFactory factory)
		{
			var container = new WindsorContainer();
			container.Kernel.AddSubSystem(
				SubSystemConstants.NamingKey,
				new SubSystems.DependencyInjectionNamingSubsystem()
			);

			if (serviceCollection == null)
			{
				return container;
			}

			container.Register(
					Component
						.For<IWindsorContainer>()
						.Instance(container),
					Component
						.For<IServiceProvider, ISupportRequiredService>()
						.ImplementedBy<WindsorScopedServiceProvider>()
						.LifeStyle.ScopedToNetServiceScope(),
					Component
						.For<IServiceScopeFactory>()
						.ImplementedBy<WindsorScopeFactory>()
						.LifestyleSingleton(),
					Component
						.For<WindsorServiceProviderFactory>()
						.Instance(factory)
						.LifestyleSingleton()
			);

			container.Kernel.Resolver.AddSubResolver(new RegisteredCollectionResolver(container.Kernel));
			container.Kernel.Resolver.AddSubResolver(new OptionsSubResolver(container.Kernel));
			container.Kernel.Resolver.AddSubResolver(new LoggerDependencyResolver(container.Kernel));

			foreach (var service in serviceCollection)
			{
				container.Register(service.CreateWindsorRegistration());
			}

			return container;
		}


		public static IRegistration CreateWindsorRegistration(this Microsoft.Extensions.DependencyInjection.ServiceDescriptor service)
		{
			if (service.ServiceType.ContainsGenericParameters)
			{
				return RegistrationAdapter.FromOpenGenericServiceDescriptor(service);
			}
			else
			{
				var method = typeof(RegistrationAdapter).GetMethod("FromServiceDescriptor", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(service.ServiceType);
				return method.Invoke(null, new object[] { service }) as IRegistration;
			}
		}
	}
}