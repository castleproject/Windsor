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

using Castle.Core;
using Castle.MicroKernel;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Castle.Facilities.AspNetCore
{
	using System;
	using System.Linq;
	using System.Reflection;

	using Castle.Facilities.AspNetCore.Resolvers;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Razor.TagHelpers;
	using Microsoft.Extensions.DependencyInjection;

	public static class CastleWindsorRegistrationExtensions
	{
		/// <summary>
		/// Sets up framework level activators for Controllers, TagHelpers and ViewComponents and adds additional sub dependency resolvers
		/// </summary>
		/// <param name="services">ASP.NET Core service collection from Microsoft.Extensions.DependencyInjection</param>
		/// <param name="container">Windsor container which activators call resolve against</param>
		/// <param name="startupAssembly">Assembly to scan and register ASP.NET Core components</param>
		public static void AddCastleWindsor(this IServiceCollection services, IWindsorContainer container, Assembly startupAssembly = null)
		{
			AddFrameworkResolversToWindsor(services, container);
			InstallFrameworkIntegration(services, container);
			AutoCrosswireAsLateboundComponents(services, container);
			AddApplicationComponentsToWindsor(container, startupAssembly ?? Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// For registering middleware that consumes services in the constructor known to Castle Windsor. You can use 
		/// conventional methods for registering your middleware but then you have to re-register your dependencies
		/// in the ASP.NET IServiceCollection. You should avoid doing this if possible and use this extension instead. 
		/// </summary>
		/// <typeparam name="T">Type of service that implements Microsoft.AspNetCore.Http.IMiddleware</typeparam>
		/// <param name="app">Application builder</param>
		/// <param name="container">Windsor container</param>
		public static void UseCastleWindsorMiddleware<T>(this IApplicationBuilder app, IWindsorContainer container) where T : class, IMiddleware
		{
			container.Register(Component.For<T>());
			app.Use(async (context, next) =>
			{
				var resolve = container.Resolve<T>();
				await resolve.InvokeAsync(context, async (ctx) => await next());
				container.Release(resolve);
			});
		}

		private static void AddApplicationComponentsToWindsor(IWindsorContainer container, Assembly assembly)
		{
			container.Register(Classes.FromAssemblyInThisApplication(assembly).BasedOn<Controller>().LifestyleScoped());
			container.Register(Classes.FromAssemblyInThisApplication(assembly).BasedOn<ViewComponent>().LifestyleTransient());
			container.Register(Classes.FromAssemblyInThisApplication(assembly).BasedOn<TagHelper>().LifestyleTransient());
		}

		private static void AddFrameworkResolversToWindsor(IServiceCollection services, IWindsorContainer container)
		{
			container.Kernel.Resolver.AddSubResolver(new LoggerDependencyResolver(services));
			container.Kernel.Resolver.AddSubResolver(new FrameworkConfigurationDependencyResolver(services));
		}

		private static void AutoCrosswireAsLateboundComponents(IServiceCollection services, IWindsorContainer container)
		{
			container.Kernel.ComponentRegistered += (key, handler) =>
			{
				if (handler.CurrentState == HandlerState.Valid)
				{
					foreach (var requestedServiceType in handler.ComponentModel.Services)
					{
						if (typeof(IMiddleware).IsAssignableFrom(requestedServiceType)) continue;

						AutoCrosswireSingletonComponents(services, container, handler, requestedServiceType);
						AutoCrosswireScopedComponents(services, container, handler, requestedServiceType);
						AutoCrosswireTransientComponents(services, container, handler, requestedServiceType);
					}
				}
			};
		}

		private static void AutoCrosswireTransientComponents(IServiceCollection services, IWindsorContainer container, IHandler handler, Type service)
		{
			if (handler.ComponentModel.LifestyleType == LifestyleType.Transient)
			{
				services.AddTransient(service, p =>
				{
					container.RequireScope();
					return container.Resolve(service);
				});
			}
		}

		private static void AutoCrosswireScopedComponents(IServiceCollection services, IWindsorContainer container, IHandler handler, Type service)
		{
			if (handler.ComponentModel.LifestyleType == LifestyleType.Scoped)
			{
				services.AddScoped(service, p =>
				{
					container.RequireScope();
					return container.Resolve(service);
				});
			}
		}

		private static void AutoCrosswireSingletonComponents(IServiceCollection services, IWindsorContainer container, IHandler handler, Type service)
		{
			if (handler.ComponentModel.LifestyleType == LifestyleType.Undefined
			    || handler.ComponentModel.LifestyleType == LifestyleType.Singleton)
			{
				services.AddSingleton(service, p =>
				{
					container.RequireScope();
					return container.Resolve(service);
				});
			}
		}

		private static void InstallFrameworkIntegration(IServiceCollection services, IWindsorContainer container)
		{
			services.AddRequestScopingMiddleware(container.RequireScope);
			services.AddCustomTagHelperActivation(container.Resolve);
			services.AddCustomControllerActivation(container.Resolve, container.Release);
			services.AddCustomViewComponentActivation(container.Resolve, container.Release);
		}
	}
}