// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

#if NET45

namespace Castle.Facilities.Owin
{
	using System;
	using System.Web.Mvc;
	using System.Web.Http;

	using Castle.MicroKernel;

	using global::Owin;

	using Castle.Windsor;
	using Castle.MicroKernel.Registration;
	using Castle.Core.Configuration;

	public static class WindsorOwinExtensions
	{
		public static void UseWindsorOwinWebHost<TStartup>(this IAppBuilder app, IWindsorContainer container)
		{
			DependencyServiceLocator.Container = container;

			// Mvc: For managing the lifestyle of scopes 
			ControllerBuilder.Current.SetControllerFactory(typeof(WebHost.MsMvcScopedControllerFactory));

			// Mvc: For resolving using `GetService|GetServices` implicitly bound to scopes with HttpContext.Current accessor
			DependencyResolver.SetResolver(new WebHost.MsMvcDependencyResolver());

			// Mvc: For registering all Controller's as scoped 
			container.Register(Classes.FromAssembly(typeof(TStartup).Assembly)
				.BasedOn<Controller>().LifestyleScoped(typeof(WebHost.Lifestyles.MsSystemWebHttpContextScopeAccessor)));

			// WebApi: For resolving using `GetService|GetServices` and managing scopes, PerWebRequest using HttpContext.Current
			GlobalConfiguration.Configuration.DependencyResolver = new MsWebApiDependencyResolver<WebHost.MsWebApiDependencyScope>();

			// WebApi: For registering all ApiController's as scoped 
			container.Register(Classes.FromAssembly(typeof(TStartup).Assembly)
				.BasedOn<ApiController>().LifestyleScoped(typeof(WebHost.Lifestyles.MsSystemWebHttpContextScopeAccessor)));
		}

		public static void UseWindsorOwinSelfHost<TStartup>(this IAppBuilder app, IWindsorContainer container, HttpConfiguration config)
		{
			DependencyServiceLocator.Container = container;

			// Mvc: Does not run on SelfHost and requires IIS

			// WebApi: For resolving using `GetService|GetServices` and managing scopes, PerWebRequest using CallContextLifetimeScope
			config.DependencyResolver = new MsWebApiDependencyResolver<SelfHost.MsWebApiDependencyScope>();

			// WebApi: For registering all ApiController's as scoped 
			container.Register(Classes.FromAssembly(typeof(TStartup).Assembly)
				.BasedOn<ApiController>().LifestyleScoped());
		}
	}

}

#endif