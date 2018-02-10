# ASP.NET WebAPI Facility

The ASP.NET WebAPI facility provides Castle Windsor integration using custom 

 - [IHttpControllerActivator](https://msdn.microsoft.com/en-us/library/system.web.http.dispatcher.ihttpcontrolleractivator(v=vs.118).aspx) for .NET Framework web hosted projects.
 - [IDependencyResolver](https://msdn.microsoft.com/en-us/library/system.web.http.dependencies.idependencyresolver(v=vs.118).aspx) for .NET Framework self-hosted projects.

## How does it work?

### Web Hosted

This is basically a WebApi that is co-hosted with an MVC project or served up in isolation using IIS. The activator is 
the most reliable way of resolving/releasing from Windsor using the latest NuGet's and tooling.

Scoped lifestyles are possible using the `WithLifestyleScopedPerWebRequest` method which is described in more detail [here](asp-lifestyles.md). 

### Self Hosted

This is a WebApi project that is served up using self hosting capabilities or with OWIN self hosting. Typically something
that runs in a windows service outside of IIS.

## What do I need to set it up?

You need to determine whether your webapi is hosted in or outside of IIS. Then pull down `Castle.Facilities.AspNet.WebApi` 
into your project from NuGet. 

### IIS or WebHosted

```csharp
public class MvcApplication : System.Web.HttpApplication
{
	protected void Application_Start()
	{
		AreaRegistration.RegisterAllAreas();
		GlobalConfiguration.Configure(WebApiConfig.Register);
		FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
		RouteConfig.RegisterRoutes(RouteTable.Routes);
		BundleConfig.RegisterBundles(BundleTable.Bundles);

		var container = new WindsorContainer();
		container.Register(Component.For<WebApiService>().LifestyleScoped()); // <- `Per Web Request`
		container.Register(Component.For<WebApiController>().LifestyleScoped()); // <- `Per Web Request`

		container.AddFacility<AspNetWebApiFacility>(x => x.WithLifestyleScopedPerWebRequest());
	}
}
```

The scoped lifestyles are an optional extra if you are using scopes in your web project, they will emulate a `Per Web Request` 
lifestyle. If not, you can simply register your controllers as transient and they will be implicitly lifestyled for the 
duration of a web request but behave as transients normally do for services consumed by controllers.

### Self Hosted

Here you will not need anything radically different from the web hosting scenario. You will however need to apply
the `UsingSelfHosting` method to your facility on startup. 

Here is a rather comprehensive example of how you could achieve this using OWIN. The same is also possible with vanilla
webapi self hosting. 

```csharp
public interface IAnyService
{
	string Get();
}

public class AnyService : IAnyService
{
	public string Get()
	{
		return Guid.NewGuid().ToString("N");
	}
}

public class ValuesController : ApiController
{
	private readonly IAnyService _anyService;

	public ValuesController(IAnyService anyService)
	{
		_anyService = anyService;
	}

	// GET api/values 
	public IEnumerable<string> Get()
	{
		return new string[] { "value1", "value2", _anyService.Get() };
	}
}

public class Startup
{
	public void Configuration(IAppBuilder appBuilder)
	{
		HttpConfiguration config = new HttpConfiguration();

		config.MapHttpAttributeRoutes();

		config.Routes.MapHttpRoute(
			name: "DefaultApi",
			routeTemplate: "api/{controller}/{id}",
			defaults: new { id = RouteParameter.Optional }
		);

		appBuilder.UseWebApi(config);

		var container = new WindsorContainer();

		container.Register(Component.For<ValuesController>().LifestyleScoped()); // <- `Per Web Request`
		container.Register(Component.For<IAnyService>().ImplementedBy<AnyService>().LifestyleScoped()); // <- `Per Web Request`

		container.AddFacility<AspNetWebApiFacility>(x => x.UsingConfiguration(config).UsingSelfHosting());
	}
}

class Program
{
	static void Main(string[] args)
	{
		string baseAddress = "http://localhost:9090/";

		using (WebApp.Start<Startup>(url: baseAddress))
		{
			HttpClient client = new HttpClient();

			var response = client.GetAsync(baseAddress + "api/values").Result;

			Console.WriteLine(response);
			Console.WriteLine(response.Content.ReadAsStringAsync().Result);
			Console.ReadLine();
		}
	}
}
```
