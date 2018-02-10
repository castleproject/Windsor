# ASP.NET Core Facility

The ASP.NET Core facility provides Castle Windsor integration using a custom activators for .NET Core web based projects.

 - [IControllerActivator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllers.icontrolleractivator?view=aspnetcore-2.0) 
 - [ITagHelperActivator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.razor.itaghelperactivator?view=aspnetcore-2.0) 
 - [IViewComponentActivator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.viewcomponents.iviewcomponentactivator.create?view=aspnetcore-2.0) 

## How does it work?

Custom activators are injected into the ASP.NET Core framework when the `services.AddCastleWindsor(container)` extension is called
from the `ConfigureServices(IServiceCollection services)` method in the `Startup` class. This allows components to be resolved from Windsor 
when web requests are made to the server. 

This method also adds a sub resolver for dealing with the resolution of ASP.NET Core framework 
types. An example might be something like an [ILoggerFactory](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerfactory?view=aspnetcore-2.0).
It is important to note that this extension also injects custom middleware for the management of scoped lifestyles. This middleware calls
the `WindsorContainer.BeginScope` extension. It will also dispose the scope once the request is completed.

`Controllers`, `ViewComponents` and `TagHelpers` are registered automatically for you when the `app.UseCastleWindsor<Startup>(container)` extension
is called from the `Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)` method in the `Startup` class.
Controllers are registered with a scoped lifestyle, and are assumed to be a single instance for the duration of the web request. The `ViewComponents`
and `TagHelpers` however are transient. It is assumed that they will called multiple times for a single request and cannot share state.
If your controllers, view components or taghelpers are in a different assembly, you call this extension multiple times supplying an example
type for which Windsor will then scan that assembly for framework types.

If you would like to change the lifestyles of any of these components, then you do not call the `app.UseCastleWindsor<Startup>(container)`
extension. You can opt for your own conventional registrations instead like so:

```csharp
container.Register(Classes.FromAssemblyInThisApplication(typeof(TStartup).Assembly)
	.BasedOn<Controller>().LifestyleScoped()); // <- Change lifestyle
container.Register(Classes.FromAssemblyInThisApplication(typeof(TStartup).Assembly)
	.BasedOn<ViewComponent>().LifestyleTransient());
container.Register(Classes.FromAssemblyInThisApplication(typeof(TStartup).Assembly)
	.BasedOn<TagHelper>().LifestyleTransient());
```

It is also very important to note that you should never register any framework services in Windsor. This is handled by the framework for you. In the 
case of [ILoggerFactory](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerfactory?view=aspnetcore-2.0) mentioned earlier, 
you will notice it is not installed anywhere in the Startup.cs example below.

You also have an optional extension for validating that you don't have any types installed from the 'Microsoft.AspNetCore' called `ValidateConfiguration`.
This could optionally be called from a unit test or you can add it to your Startup to guard against this at runtime.

## What do I need to set it up?

You will need to install the Castle.Facilities.AspNetCore nuget, after which you can add the missing code to your Startup.cs. 
Here is a complete example:

```csharp
public class Startup
{
	private readonly WindsorContainer container = new WindsorContainer();

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
		services.AddCastleWindsor(container); // <- Registers activators
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
	{
		app.UseCastleWindsor<Startup>(container); // <- Registers controllers, view components and tag helpers

		RegisterApplicationComponents();

		// Add custom middleware
		app.UseCastleWindsorMiddleware<CustomMiddleware>(container);

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
		// Custom Windsor registrations
		container.Register(Component.For<IUserService>().ImplementedBy<AspNetUserService>().LifestyleScoped());
	}
}

public interface IUserService { }

public class AspNetUserService : IUserService { }

// Example of some custom user-defined middleware component.
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

Special credit goes to:

 - [@dotnetjunkie](https://github.com/dotnetjunkie) for pioneering the discussions with the ASP.NET team for
non-conforming containers and providing valuable input on issue: https://github.com/castleproject/Windsor/issues/120 

 - [@hikalkan](https://github.com/hikalkan) for implementing https://github.com/volosoft/castle-windsor-ms-adapter. 
