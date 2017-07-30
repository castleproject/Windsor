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

			// Mvc: For controlling `BeginScope` in scoped lifestyle implicitly bound to requests with HttpContext.Current accessor
			ControllerBuilder.Current.SetControllerFactory(typeof(WebHost.MsMvcScopedControllerFactory));

			// Mvc: For controlling `BeginScope` on `GetService|GetServices` in `System.Web.Mvc.IDependencyResolver` implicitly bound to requests with HttpContext.Current accessor
			DependencyResolver.SetResolver(new WebHost.MsMvcDependencyResolver());

			// Mvc: For controlling `BeginScope` on all inheritors of `System.Web.Mvc.Controller` implicitly bound to requests with HttpContext.Current accessor
			container.Register(Classes.FromAssembly(typeof(TStartup).Assembly)
				.BasedOn<Controller>().LifestyleScoped(typeof(WebHost.Lifestyles.MsSystemWebHttpContextScopeAccessor)));

			// WebApi: For controlling `BeginScope` on `GetService|GetServices` in `System.Web.Http.Dependencies.IDependencyResolver` implicitly bound to requests with HttpContext.Current accessor
			GlobalConfiguration.Configuration.DependencyResolver = new MsWebApiDependencyResolver<WebHost.MsWebApiDependencyScope>();

			// WebApi: For controlling `BeginScope` on all inheritors of `System.Web.Http.ApiController` implicitly bound to requests with HttpContext.Current accessor
			container.Register(Classes.FromAssembly(typeof(TStartup).Assembly)
				.BasedOn<ApiController>().LifestyleScoped(typeof(WebHost.Lifestyles.MsSystemWebHttpContextScopeAccessor)));
		}

		public static void UseWindsorOwinSelfHost<TStartup>(this IAppBuilder app, IWindsorContainer container, HttpConfiguration config)
		{
			DependencyServiceLocator.Container = container;

			// Mvc: Does not run on SelfHost and requires IIS

			// WebApi: For controlling `BeginScope` on `GetService|GetServices` in `System.Web.Http.Dependencies.IDependencyResolver` implicitly bound to requests with CallContext accessor
			config.DependencyResolver = new MsWebApiDependencyResolver<SelfHost.MsWebApiDependencyScope>();

			// WebApi: For controlling `BeginScope` on all inheritors of `System.Web.Http.ApiController` implicitly bound to requests with CallContext accessor
			container.Register(Classes.FromAssembly(typeof(TStartup).Assembly)
				.BasedOn<ApiController>().LifestyleScoped());
		}
	}

}

#endif