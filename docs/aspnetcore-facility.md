# ASP.NET Core Facility

The ASP.NET Core facility provides Castle Windsor integration using a custom activators for .NET Core web based projects.

 - [IControllerActivator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllers.icontrolleractivator?view=aspnetcore-2.0) 
 - [ITagHelperActivator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.razor.itaghelperactivator?view=aspnetcore-2.0) 
 - [IViewComponentActivator](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.viewcomponents.iviewcomponentactivator.create?view=aspnetcore-2.0) 

## How does it work?

Custom activators are injected into the ASP.NET Core framework when the `services.AddWindsor(container)` extension is called
from the `ConfigureServices(IServiceCollection services)` method in the `Startup` class. This allows components to be resolved from Windsor 
when web requests are made to the server for TagHelpers, ViewComponents and Controllers. 

This method also adds a sub resolver for dealing with the resolution of ASP.NET Core framework types. An example might be something like an 
[ILoggerFactory](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerfactory?view=aspnetcore-2.0). 
It is important to note that this extension also injects custom middleware for the management of implicitly windsor scoped lifestyles. It will also 
dispose the implicit scope once the request is completed.

### Registering Controllers, ViewComponents and TagHelpers

`Controllers`, `ViewComponents` and `TagHelpers` are registered automatically for you when the `services.AddWindsor(container)` extension
is called. All components are registered with a scoped lifestyle, and are assumed to be a single instance for the duration of the web request. 
You can optionally override the lifestyles for each of these if you prefer using the `options` callback in the example below:

```csharp
services.AddWindsor(container, opts =>
{
	opts.RegisterControllers(typeof(HomeController).Assembly, LifestyleType.Transient);
	opts.RegisterTagHelpers(typeof(EmailTagHelper).Assembly, LifestyleType.Transient);
	opts.RegisterViewComponents(typeof(AddressComponent).Assembly, LifestyleType.Transient);
});
```

This is also useful if your framework components live in a separate assemblies or are not defined in the same assembly as your web application.
Alternatively if your framework components all live in one assembly and you dont need to change lifestyles then you can simply use the following:

```csharp
services.AddWindsor(container, opts =>
{
	opts.UseEntryAssembly(typeof(HomeController).Assembly);
});
```
This is good for trouble shooting situations where nothing get's registered because of problems in the hosting environment where 
GetEntryAssembly/GetCallingAssembly does not work as expected. 

### Cross Wiring into the IServiceCollection

There is an additional feature you can use to `Cross Wire` components into the IServiceCollection. This is useful 
for cases where the framework needs to know how to resolve a component from Windsor. An example would be components 
consumed with the Razor @Inject directive in MVC projects. 

```csharp
container.Register(Component.For<IUserService>().ImplementedBy<AspNetUserService>().LifestyleScoped().CrossWired());
```

This would then allow you to consume the IUserService component in your Razor view like so:

```html
@inject WebApp.IUserService user

@foreach (var user in user.GetAll())
{
	<div>
		<h1>@user</h1>
	</div>
}
```

For this to work you have add the facility in the configure services method before you register anything. A helpful exception
will be thrown in case you forget.

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
	// Setup component model contributors for making windsor services available to IServiceProvider
	Container.AddFacility<AspNetCoreFacility>(f => f.CrossWiresInto(services));
	
	//...
}
```
:warning: **Nesting CrossWired Dependencies** The ServiceProvider is quite greedy in the way that it manages disposables, if both Windsor and the ServiceProvider try track them together things end up getting disposed more than once. So to make this work Windsor has to relinquish disposable concerns to the ServiceProvider for `Cross Wired` components. If `Cross Wired` components depend to on each other, you might end up introducing a memory leak into your application(except for singletons) especially if they did not get disposed/released properly. Please use `Cross Wired` components sparingly.

### Registering custom middleware

If you don't have any DI requirements in your middleware you can simply register it in the IServiceCollection like so: 

```csharp
services.AddSingleton<FrameworkMiddleware>(); // Do this if you don't care about using Windsor
```

Alternatively if you would like to inject dependencies into your middleware that are registered with Castle Windsor then you 
can use register your middleware using the `AsMiddleware` component registration extension. This should always been done from 
the Configure method in Startup.

```csharp
// Add custom middleware, do this if your middleware uses DI from Windsor
Container.Register(Component.For<CustomMiddleware>().DependsOn(Dependency.OnValue<ILoggerFactory>(loggerFactory)).LifestyleScoped().AsMiddleware());
```

For this to work, Windsor needs to hold a reference to the `IApplicationBuilder` which becomes available from the 'Configure' 
method as a parameter on startup. This is done by calling the `RegistersMiddlewareInto` first like so:

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
	// For making component registrations aware of IApplicationBuilder
	Container.GetFacility<AspNetCoreFacility>().RegistersMiddlewareInto(app);

	// Add custom middleware, do this if your middleware uses DI from Windsor
	Container.Register(Component.For<CustomMiddleware>().DependsOn(Dependency.OnValue<ILoggerFactory>(loggerFactory)).LifestyleScoped().AsMiddleware());

	// ...
}	
```

## What do I need to set it up?

You will need to install the Castle.Facilities.AspNetCore nuget, after which you can add the missing code to your Startup.cs. 
Here is a complete example:

```csharp
public class Startup
{
	private static readonly WindsorContainer Container = new WindsorContainer();

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

	// This method gets called by the runtime. Use this method to add application services to the application.
	public IServiceProvider ConfigureServices(IServiceCollection services)
	{
		// Setup component model contributors for making windsor services available to IServiceProvider
		Container.AddFacility<AspNetCoreFacility>(f => f.CrossWiresInto(services));

		// Add framework services.
		services.AddMvc();
		services.AddLogging((lb) => lb.AddConsole().AddDebug());
		services.AddSingleton<FrameworkMiddleware>(); // Do this if you don't care about using Windsor to inject dependencies

		// Custom application component registrations, ordering is important here
		RegisterApplicationComponents(services);

		// Castle Windsor integration, controllers, tag helpers and view components, this should always come after RegisterApplicationComponents
		return services.AddWindsor(Container, 
			opts => opts.UseEntryAssembly(typeof(HomeController).Assembly), // <- Recommended
			() => services.BuildServiceProvider(validateScopes:false)); // <- Optional
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
	{
		// For making component registrations of middleware easier
		Container.GetFacility<AspNetCoreFacility>().RegistersMiddlewareInto(app);

		// Add custom middleware, do this if your middleware uses DI from Windsor
		Container.Register(Component.For<CustomMiddleware>().DependsOn(Dependency.OnValue<ILoggerFactory>(loggerFactory)).LifestyleScoped().AsMiddleware());

		// Add framework configured middleware, use this if you dont have any DI requirements
		app.UseMiddleware<FrameworkMiddleware>();

		// Serve static files
		app.UseStaticFiles();

		// Mvc default route
		app.UseMvc(routes =>
		{
			routes.MapRoute(
				"default",
				"{controller=Home}/{action=Index}/{id?}");
		});
	}

	private void RegisterApplicationComponents(IServiceCollection services)
	{
		// Application components
		Container.Register(Component.For<IHttpContextAccessor>().ImplementedBy<HttpContextAccessor>());
		Container.Register(Component.For<IScopedDisposableService>().ImplementedBy<ScopedDisposableService>().LifestyleScoped().IsDefault());
		Container.Register(Component.For<ITransientDisposableService>().ImplementedBy<TransientDisposableService>().LifestyleTransient().IsDefault());
		Container.Register(Component.For<ISingletonDisposableService>().ImplementedBy<SingletonDisposableService>().LifestyleSingleton().IsDefault());
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
	private readonly IScopedDisposableService scopedDisposableService;

	public CustomMiddleware(ILoggerFactory loggerFactory, IScopedDisposableService scopedDisposableService)
	{
		this.scopedDisposableService = scopedDisposableService;
	}

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		// Do something before
		await next(context);
		// Do something after
	}
}
```

### Torn lifestyles

If you register an application component such as a singleton in Windsor and ASP.NET Core framework two instances might appear. 
This is known as a `torn lifestyle`. We hope to have avoided this in this facility's design but it is useful to know what 
symptoms to look for if this does happen.

Please report any evidence of this on our issue tracker.

References:
 - https://simpleinjector.readthedocs.io/en/latest/tornlifestyle.html

### Captive Dependencies

This is when Windsor services with `long lived`(ie. Singleton) lifestyles, consume components that were registered with 
`short lived`(ie. Transient, Scoped) lifestyles. The service dependency is effectively held captive by it's consumer 
lifestyle. Symptoms here could include dispose method's not firing for transients and scopes that are consumed by singletons. 

References:
 - http://blog.ploeh.dk/2014/06/02/captive-dependency/

### Special credit:

 - [@dotnetjunkie](https://github.com/dotnetjunkie) for pioneering the discussions with the ASP.NET team for non-conforming containers and providing valuable input on issue: https://github.com/castleproject/Windsor/issues/120 
 - [@ploeh](https://github.com/ploeh) for defining `Captive Dependencies`: http://blog.ploeh.dk/2014/06/02/captive-dependency/
 - [@hikalkan](https://github.com/hikalkan) for implementing https://github.com/volosoft/castle-windsor-ms-adapter. 
 - [@generik0](https://github.com/generik0) for providing invaluable feedback into the public API's and test driving this in a real world application.
