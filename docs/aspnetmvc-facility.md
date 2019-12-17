# ASP.NET MVC Facility

The ASP.NET MVC facility provides Castle Windsor integration using a custom [IControllerFactory](https://msdn.microsoft.com/en-us/library/system.web.mvc.icontrollerfactory(v=vs.118).aspx) for .NET Framework web based projects.

## How does it work?

The IControllerFactory governs the creation and disposal on controllers in the Mvc framework. Once you have created
your container and registered your controllers, this facility will manage Resolve/Release for web requests. You
can find an explanation of lifestyles [here](aspnet-lifestyles.md).

## What do I need to set it up?

First you need to install the `Castle.Facilities.AspNet.Mvc` facility from Nuget. Then you will need to add the AspNetMvcFacility in your application startup. 
Below is an example of how this could work.

```csharp
public class MvcApplication : System.Web.HttpApplication
{
	protected void Application_Start()
	{
		AreaRegistration.RegisterAllAreas();
		FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
		RouteConfig.RegisterRoutes(RouteTable.Routes);
		BundleConfig.RegisterBundles(BundleTable.Bundles);

		var container = new WindsorContainer();
		container.Register(Component.For<HomeService>().LifestyleScoped()); // <- `Per Web Request`
		container.Register(Component.For<HomeController>().LifestyleScoped()); // <- `Per Web Request`

		container.AddFacility<AspNetMvcFacility>(x => x.AddControllerAssembly<MvcApplication>().WithLifestyleScopedPerWebRequest());
	}
}
```

The scoped lifestyles are an optional extra if you are using scopes in your web project, they will emulate a `Per Web Request` 
lifestyle. If not, you can simply register your controllers as transient and they will be implicitly lifestyled for the 
duration of a web request but behave as transients normally do for services consumed by controllers.

