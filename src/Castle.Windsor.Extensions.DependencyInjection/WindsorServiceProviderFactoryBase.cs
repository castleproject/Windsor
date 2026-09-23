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
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Extensions.DependencyInjection.Extensions;
	using Castle.Windsor.Extensions.DependencyInjection.Resolvers;
	using Castle.Windsor.Extensions.DependencyInjection.Scope;
	using Microsoft.Extensions.DependencyInjection;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;

	public abstract class WindsorServiceProviderFactoryBase : IServiceProviderFactory<IWindsorContainer>, IDisposable
	{
		private static readonly ConcurrentDictionary<IKernel, WindsorServiceProviderFactoryBase> _factoryBaseMap = new();

		internal static ExtensionContainerRootScope GetRootScopeForKernel(IKernel kernel)
		{
			if (_factoryBaseMap.TryGetValue(kernel, out var factory))
			{
				return factory.RootScope;
			}
			throw new NotSupportedException("We are trying to access scopt for a container that was not associated with any WindsorServiceProviderFactoryBase. This is not supported.");
		}

		internal ExtensionContainerRootScope RootScope { get; private set; }

		protected IWindsorContainer rootContainer;
		private bool _disposedValue;

		public virtual IWindsorContainer Container => rootContainer;

		public virtual IWindsorContainer CreateBuilder(IServiceCollection services)
		{
			return BuildContainer(services);
		}

		public virtual IServiceProvider CreateServiceProvider(IWindsorContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException(nameof(container));
			}

			EnsureInfrastructureRegistered(container);
			return container.Resolve<IServiceProvider>();
		}

		protected virtual void CreateRootScope()
		{
			//first time we create the root scope we will initialize a root new scope 
			RootScope = ExtensionContainerRootScope.BeginRootScope();
		}

		protected virtual void CreateRootContainer()
		{
			SetRootContainer(new WindsorContainer());
		}

		protected virtual void SetRootContainer(IWindsorContainer container)
		{
			rootContainer = container;
			//Set the map associating this factoryh with the container.
			_factoryBaseMap[rootContainer.Kernel] = this;
#if NET8_0_OR_GREATER
			rootContainer.Kernel.Resolver.AddSubResolver(new KeyedServicesSubDependencyResolver(rootContainer));
#endif
			AddSubSystemToContainer(rootContainer);
		}

		protected virtual void AddSubSystemToContainer(IWindsorContainer container)
		{
			container.Kernel.AddSubSystem(
				SubSystemConstants.NamingKey,
				new SubSystems.DependencyInjectionNamingSubsystem()
			);
		}

		protected virtual IWindsorContainer BuildContainer(IServiceCollection serviceCollection)
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

			var token = (rootContainer.Kernel as IKernelInternal)?.OptimizeDependencyResolution();
			try
			{
				RegisterContainer(rootContainer);
				RegisterProviders(rootContainer);
				RegisterFactories(rootContainer);

				AddSubResolvers();

				RegisterServiceCollection(serviceCollection);
			}
			finally
			{
				token?.Dispose();
			}

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
				.For<IServiceProvider, ISupportRequiredService
#if NET6_0_OR_GREATER
				, IServiceProviderIsService
#endif
#if NET8_0_OR_GREATER
				, IKeyedServiceProvider, IServiceProviderIsKeyedService
#endif
				>()
				.ImplementedBy<WindsorScopedServiceProvider>()
					.LifeStyle
					.ScopedToNetServiceScope());
		}

		protected virtual void RegisterFactories(IWindsorContainer container)
		{
			RegisterScopeFactory(container);
			RegisterServiceProviderFactory(container);
		}

		protected virtual void RegisterScopeFactory(IWindsorContainer container)
		{
			container.Register(Component
				.For<IServiceScopeFactory>()
				.ImplementedBy<WindsorScopeFactory>()
				.LifestyleSingleton());
		}

		protected virtual void RegisterServiceProviderFactory(IWindsorContainer container)
		{
			container.Register(Component
				.For<IServiceProviderFactory<IWindsorContainer>>()
				.Instance(this)
				.LifestyleSingleton());
		}

		protected virtual void RegisterServiceCollection(IServiceCollection serviceCollection)
		{
			var registrations = serviceCollection
				.Select(service => service.CreateWindsorRegistration(rootContainer))
				.ToArray();

			rootContainer.Register(registrations);
		}

		protected virtual void AddSubResolvers()
		{
			rootContainer.Kernel.Resolver.AddSubResolver(new RegisteredCollectionResolver(rootContainer.Kernel));
			rootContainer.Kernel.Resolver.AddSubResolver(new OptionsSubResolver(rootContainer.Kernel));
			rootContainer.Kernel.Resolver.AddSubResolver(new LoggerDependencyResolver(rootContainer.Kernel));
		}

		private void EnsureInfrastructureRegistered(IWindsorContainer container)
		{
			if (!container.Kernel.GetHandlers(typeof(IWindsorContainer)).Any())
			{
				RegisterContainer(container);
			}

			if (!container.Kernel.GetHandlers(typeof(IServiceProvider)).Any())
			{
				RegisterProviders(container);
			}

			if (!container.Kernel.GetHandlers(typeof(IServiceScopeFactory)).Any())
			{
				RegisterScopeFactory(container);
			}

			if (!container.Kernel.GetHandlers(typeof(IServiceProviderFactory<IWindsorContainer>)).Any())
			{
				RegisterServiceProviderFactory(container);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					//when this is disposed just remove the kernel from the map.
					_factoryBaseMap.TryRemove(this.Container.Kernel, out var _);
				}

				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
