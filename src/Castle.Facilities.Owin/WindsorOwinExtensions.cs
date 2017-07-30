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