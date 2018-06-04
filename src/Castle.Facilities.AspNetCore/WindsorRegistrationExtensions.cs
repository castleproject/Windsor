// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.AspNetCore
{
	using System;
	using System.Linq;

	using Castle.Facilities.AspNetCore.Resolvers;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers.SpecializedResolvers;
	using Castle.Windsor;

	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Razor.TagHelpers;
	using Microsoft.Extensions.DependencyInjection;

	public static class WindsorRegistrationExtensions
	{
		/// <summary>
		/// Sets up framework level activators for Controllers, TagHelpers and ViewComponents and adds additional sub dependency resolvers
		/// </summary>
		/// <param name="services"><see cref="IServiceCollection"/></param>
		/// <param name="container"><see cref="IWindsorContainer"/></param>
		/// <param name="configure">Configuration options for controlling registration and lifestyles of controllers, tagHelpers and viewComponents</param>
		/// <param name="serviceProviderFactory">Optional factory for creating a custom <see cref="IServiceProvider"/></param>
		public static IServiceProvider AddWindsor(this IServiceCollection services, IWindsorContainer container, Action<WindsorRegistrationOptions> configure = null, Func<IServiceProvider> serviceProviderFactory = null)
		{
			var options = new WindsorRegistrationOptions();
			configure?.Invoke(options);

			InstallWindsorIntegration(services, container);
			AddApplicationComponentsToWindsor(container, options);
			InstallFrameworkIntegration(services, container);
			return InitialiseFrameworkServiceProvider(services, serviceProviderFactory, container);
		}

		/// <summary>
		/// For making types available to the <see cref="IServiceCollection"/> using 'late bound' factories which resolve from Windsor. This makes things like the @Inject directive in Razor work.
		/// </summary>
		/// <param name="registration">The component registration that gets copied across to the <see cref="IServiceCollection"/></param>
		public static ComponentRegistration CrossWired(this ComponentRegistration registration)
		{
			registration.Attribute(AspNetCoreFacility.IsCrossWiredIntoServiceCollectionKey).Eq(Boolean.TrueString);
			return registration;
		}

		/// <summary>
		/// For making types available to the <see cref="IServiceCollection"/> using 'late bound' factories which resolve from Windsor. This makes things like the @Inject directive in Razor work.
		/// </summary>
		/// <param name="registration">The component registration that gets copied across to the IServiceCollection</param>
		public static ComponentRegistration<T> CrossWired<T>(this ComponentRegistration<T> registration) where T : class
		{
			registration.Attribute(AspNetCoreFacility.IsCrossWiredIntoServiceCollectionKey).Eq(Boolean.TrueString);
			return registration;
		}

		/// <summary>
		/// For registering middleware that is resolved from Windsor
		/// </summary>
		/// <param name="registration"><see cref="ComponentRegistration"/></param>
		/// <returns><see cref="ComponentRegistration"/></returns>
		public static ComponentRegistration AsMiddleware(this ComponentRegistration registration)
		{
			registration.Attribute(AspNetCoreFacility.IsRegisteredAsMiddlewareIntoApplicationBuilderKey).Eq(Boolean.TrueString);
			return registration;
		}

		/// <summary>
		/// For registering middleware that is resolved from Windsor
		/// </summary>
		/// <typeparam name="T">A generic type that implements <see cref="IMiddleware"/></typeparam>
		/// <param name="registration"><see cref="ComponentRegistration{T}"/></param>
		/// <returns><see cref="ComponentRegistration{T}"/></returns>
		public static ComponentRegistration<T> AsMiddleware<T>(this ComponentRegistration<T> registration) where T : class, IMiddleware
		{
			registration.Attribute(AspNetCoreFacility.IsRegisteredAsMiddlewareIntoApplicationBuilderKey).Eq(Boolean.TrueString);
			return registration;
		}

		private static void AddApplicationComponentsToWindsor(IWindsorContainer container, WindsorRegistrationOptions options)
		{
			if (!options.ControllerRegistrations.Any())
			{
				container.Register(Classes.FromAssemblyInThisApplication(options.EntryAssembly).BasedOn<Controller>().LifestyleScoped());
			}

			foreach (var controllerRegistration in options.ControllerRegistrations)
			{
				container.Register(Classes.FromAssemblyInThisApplication(controllerRegistration.Item1).BasedOn<Controller>().Configure(x => x.LifeStyle.Is(controllerRegistration.Item2)));
			}

			if (!options.TagHelperRegistrations.Any())
			{
				container.Register(Classes.FromAssemblyInThisApplication(options.EntryAssembly).BasedOn<ViewComponent>().LifestyleScoped());
			}

			foreach (var controllerRegistration in options.TagHelperRegistrations)
			{
				container.Register(Classes.FromAssemblyInThisApplication(controllerRegistration.Item1).BasedOn<ViewComponent>().Configure(x => x.LifeStyle.Is(controllerRegistration.Item2)));
			}

			if (!options.ViewComponentRegistrations.Any())
			{
				container.Register(Classes.FromAssemblyInThisApplication(options.EntryAssembly).BasedOn<TagHelper>().LifestyleScoped());
			}

			foreach (var controllerRegistration in options.ViewComponentRegistrations)
			{
				container.Register(Classes.FromAssemblyInThisApplication(controllerRegistration.Item1).BasedOn<TagHelper>().Configure(x => x.LifeStyle.Is(controllerRegistration.Item2)));
			}
		}

		private static IServiceProvider InitialiseFrameworkServiceProvider(IServiceCollection services, Func<IServiceProvider> serviceProviderFactory, IWindsorContainer container)
		{
			var serviceProvider = serviceProviderFactory?.Invoke() ?? services.BuildServiceProvider(validateScopes: false);
			container.Register(Component.For<IServiceProvider>().Instance(serviceProvider));
			foreach (var acceptServiceProvider in container.ResolveAll<IAcceptServiceProvider>())
			{
				acceptServiceProvider.AcceptServiceProvider(serviceProvider);
			}
			return serviceProvider;
		}

		private static void InstallFrameworkIntegration(IServiceCollection services, IWindsorContainer container)
		{
			services.AddRequestScopingMiddleware(() => new []{ container.RequireScope(), container.Resolve<IServiceProvider>().CreateScope() });
			services.AddCustomTagHelperActivation(container.Resolve);
			services.AddCustomControllerActivation(container.Resolve, container.Release);
			services.AddCustomViewComponentActivation(container.Resolve, container.Release);
		}

		private static void InstallWindsorIntegration(IServiceCollection services, IWindsorContainer container)
		{
			container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel));

			var loggerDependencyResolver = new LoggerDependencyResolver();
			container.Register(Component.For<IAcceptServiceProvider>().Instance(loggerDependencyResolver));
			container.Kernel.Resolver.AddSubResolver(loggerDependencyResolver);

			var frameworkDependencyResolver = new FrameworkDependencyResolver(services);
			container.Register(Component.For<IAcceptServiceProvider>().Instance(frameworkDependencyResolver));
			container.Kernel.Resolver.AddSubResolver(frameworkDependencyResolver);
		}
	}
}