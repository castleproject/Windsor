# ASP.NET Core Facility

The ASP.NET Core facility provides Castle Windsor integration using a custom activators for .NET Core web based projects.

 - [IControllerActivator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllers.icontrolleractivator?view=aspnetcore-2.0) 
 - [ITagHelperActivator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.razor.itaghelperactivator?view=aspnetcore-2.0) 
 - [IViewComponentActivator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.viewcomponents.iviewcomponentactivator.create?view=aspnetcore-2.0) 

## How does it work?

Custom activators are injected into the ASP.NET Core framework when the `services.AddCastleWindsor(container)` extension is called
from the `ConfigureServices(IServiceCollection services)` method in the `Startup` class. This allows components to be resolved from Windsor 
when web requests are made to the server for TagHelpers, ViewComponents and Controllers. 

This method also adds a sub resolver for dealing with the resolution of ASP.NET Core framework types. An example might be something like an 
[ILoggerFactory](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerfactory?view=aspnetcore-2.0). 
It is important to note that this extension also injects custom middleware for the management of scoped lifestyles. This middleware calls the 
`WindsorContainer.BeginScope` extension for scoped lifestyles. It will also dispose the scope once the request is completed.

`Controllers`, `ViewComponents` and `TagHelpers` are registered automatically for you when the `services.AddCastleWindsor(container)` extension
is called. Controllers are registered with a scoped lifestyle, and are assumed to be a single instance for the duration of the web request. 
The `ViewComponents` and `TagHelpers` however are transient. It is assumed that they will called multiple times for a single request and cannot share state.
If your controllers, view components or taghelpers are in a different assembly, you call this extension multiple times supplying an example
type for which Windsor will then scan that assembly for framework types.

It is also very important to note anything registered in Windsor will automatically become available in the IServiceCollection as a late bound component. This
was needed to make @Inject directives work for Razor views.

## What do I need to set it up?

You will need to install the Castle.Facilities.AspNetCore nuget, after which you can add the missing code to your Startup.cs. 
Here is a complete example:

```csharp
public class Startup
{
	public static readonly WindsorContainer Container = new WindsorContainer();

	public Startup(IHostingEnvironment env)
	{
		var builder = new ConfigurationBuilder()
			.SetBasePath(env.ContentRootPath)
			.AddJsonFile("appsettings.json", true, true)
			.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
			.AddEnvironmentVariables();

		Configuration = builder.Build();
	}

	public IConfigurationRoot Configuration { get; }

	// This method gets called by the runtime. Use this method to add services to the container.
	public void ConfigureServices(IServiceCollection services)
	{
		// Add framework services.
		services.AddMvc();
		services.AddLogging((lb) => lb.AddConsole().AddDebug());
		services.AddSingleton<FrameworkMiddleware>(); // Do this if you don't care about using Windsor

		// Castle Windsor integration, controllers, tag helpers and view components
		services.AddCastleWindsor(Container);

		// Custom application component registrations
		RegisterApplicationComponents();
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
	{
		// Add custom middleware, do this if your middleware uses DI from Windsor
		app.UseCastleWindsorMiddleware<CustomMiddleware>(Container);

		// Add framework configured middleware
		app.UseMiddleware<FrameworkMiddleware>();

		app.UseStaticFiles();

		app.UseMvc(routes =>
		{
			routes.MapRoute(
				"default",
				"{controller=Home}/{action=Index}/{id?}");
		});
	}

	private void RegisterApplicationComponents()
	{
		// Application registrations
		Container.Register(Component.For<IHttpContextAccessor>().ImplementedBy<HttpContextAccessor>());
		Container.Register(Component.For<IUserService>().ImplementedBy<AspNetUserService>().LifestyleScoped());
	}
}

public interface IUserService : IDisposable
{
	IEnumerable<string> GetAll();
}

public class AspNetUserService : IUserService
{
	public IEnumerable<string> GetAll()
	{
		return new[] { "AnyUser" };
	}

	public void Dispose()
	{
	}
}

// Example of framework configured middleware component, can't consume types registered in Windsor
public class FrameworkMiddleware : IMiddleware
{
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		// Do something before
		await next(context);
		// Do something after
	}
}

// Example of some custom user-defined middleware component. Resolves types from Windsor.
public sealed class CustomMiddleware : IMiddleware
{
	private readonly IUserService userService;

	public CustomMiddleware(ILoggerFactory loggerFactory, IUserService userService)
	{
		this.userService = userService;
	}

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		// Do something before
		await next(context);
		// Do something after
	}
}
```

## Registering ASP.NET Core framework components in Windsor

You can do it via the IServiceCollection or the IWindsorContainer but not both. This is because if it is registered through Windsor it automatically gets 
registered in the IServiceCollection using a factory method which calls back to the container to resolve it. If they have been registered through the 
IServiceCollection they are still resolvable through a sub dependency resolver for which Windsor does not activate or track instances.

### Torn lifestyles

If you register a framework component as scoped in Windsor and the ASP.NET Core framework has registered it as scoped also, two instances might appear. This
problem is further obfiscated if those objects are stateful. This is known as a `torn lifestyle`. We hope to have avoided this in this facility's design.

References:
 - https://simpleinjector.readthedocs.io/en/latest/tornlifestyle.html

### Captive Dependencies

This is when Windsor services with `long lived`(ie. Singleton) lifestyles, consume framework components that were registered with `short lived`(ie. Transient, Scoped) lifestyles. 
The framework service dependency is effectively held captive by it's consumer lifestyle. These `side effects` can be hard to diagnose. 

References:
 - http://blog.ploeh.dk/2014/06/02/captive-dependency/

Special credit goes to:

 - [@dotnetjunkie](https://github.com/dotnetjunkie) for pioneering the discussions with the ASP.NET team for non-conforming containers and providing valuable input on issue: https://github.com/castleproject/Windsor/issues/120 
 - [@ploeh](https://github.com/ploeh) for defining `Captive Dependencies`: http://blog.ploeh.dk/2014/06/02/captive-dependency/
 - [@hikalkan](https://github.com/hikalkan) for implementing https://github.com/volosoft/castle-windsor-ms-adapter. 
 - [@generik0](https://github.com/generik0) for discussing here: https://github.com/castleproject/Windsor/pull/389
