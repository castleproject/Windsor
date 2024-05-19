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
	using System;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Extensions.DependencyInjection.Extensions;
	using Castle.Windsor.Extensions.DependencyInjection.Resolvers;
	using Castle.Windsor.Extensions.DependencyInjection.Scope;

	using Microsoft.Extensions.DependencyInjection;

	public abstract class WindsorServiceProviderFactoryBase : IServiceProviderFactory<IWindsorContainer>
	{
		internal ExtensionContainerRootScope rootScope;
		protected IWindsorContainer rootContainer;

		public virtual IWindsorContainer Container => rootContainer;

		public virtual IWindsorContainer CreateBuilder(IServiceCollection services)
		{
			return BuildContainer(services, rootContainer);
		}

		public virtual IServiceProvider CreateServiceProvider(IWindsorContainer container)
		{
			return container.Resolve<IServiceProvider>();
		}

		protected virtual void CreateRootScope()
		{
			rootScope = ExtensionContainerRootScope.BeginRootScope();
		}

		protected virtual void CreateRootContainer()
		{
			SetRootContainer(new WindsorContainer());
		}

		protected virtual void SetRootContainer(IWindsorContainer container)
		{
			rootContainer = container;
			AddSubSystemToContainer(rootContainer);
		}

		protected virtual void AddSubSystemToContainer(IWindsorContainer container)
		{
			container.Kernel.AddSubSystem(
				SubSystemConstants.NamingKey,
				new SubSystems.DependencyInjectionNamingSubsystem()
			);
		}

		protected virtual IWindsorContainer BuildContainer(IServiceCollection serviceCollection, IWindsorContainer windsorContainer)
		{
			if (rootContainer == null)
			{
				CreateRootContainer();
			}
			if (rootContainer == null) throw new ArgumentNullException("Could not initialize container");

			if (serviceCollection == null)
			{
				return rootContainer;
			}

			RegisterContainer(rootContainer);
			RegisterProviders(rootContainer);
			RegisterFactories(rootContainer);

			AddSubResolvers();

			RegisterServiceCollection(serviceCollection, windsorContainer);

			return rootContainer;
		}

		protected virtual void RegisterContainer(IWindsorContainer container)
		{
			container.Register(
				Component
					.For<IWindsorContainer>()
					.Instance(container));
		}

		protected virtual void RegisterProviders(IWindsorContainer container)
		{
			container.Register(Component
				.For<IServiceProvider, ISupportRequiredService>()
				.ImplementedBy<WindsorScopedServiceProvider>()
				.DependsOn(Dependency.OnValue<ExtensionContainerRootScope>(rootScope))
				.LifeStyle.ScopedToNetServiceScope());
		}

		protected virtual void RegisterFactories(IWindsorContainer container)
		{
			container.Register(Component
					.For<IServiceScopeFactory>()
					.ImplementedBy<WindsorScopeFactory>()
					.DependsOn(Dependency.OnValue<ExtensionContainerRootScope>(rootScope))
					.LifestyleSingleton(),
				Component
					.For<IServiceProviderFactory<IWindsorContainer>>()
					.Instance(this)
					.LifestyleSingleton());
		}

		protected virtual void RegisterServiceCollection(IServiceCollection serviceCollection,IWindsorContainer container)
		{
			foreach (var service in serviceCollection)
			{
				rootContainer.Register(service.CreateWindsorRegistration());
			}
		}

		protected virtual void AddSubResolvers()
		{
			rootContainer.Kernel.Resolver.AddSubResolver(new RegisteredCollectionResolver(rootContainer.Kernel));
			rootContainer.Kernel.Resolver.AddSubResolver(new OptionsSubResolver(rootContainer.Kernel));
			rootContainer.Kernel.Resolver.AddSubResolver(new LoggerDependencyResolver(rootContainer.Kernel));
		}
	}
}
