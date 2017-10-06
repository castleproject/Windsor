# System Web Facility

The system web facility provides legacy `scoped` management of lifestyles using [http modules](https://msdn.microsoft.com/library/ms178468.aspx) for desktop clr web based projects.

## How does it work?

Your objects consumed within a `PerWebRequest` lifestyle will effectively live for as long as that web request thread is active. It also use HttpContext.Current to track the lifecycle
of the instance. Once the web request has completed the instance gets released. 

## What do I need to set it up?

You will need 3 things to get this going:

- Assembly reference to System.Web
- NuGet reference for [Microsoft.Web.Infrastructure](https://www.nuget.org/packages/Microsoft.Web.Infrastructure/)
- NuGet reference for [Castle.AspNet.SystemWebFacility](https://www.nuget.org/packages/Castle.AspNet.SystemWebFacility/)

After the release of v3.0 we expect you would not need this, but if you are still having problems with module registration 
you might also need to update your web.config with the following:

```xml
<configuration>
    <system.web>
        <httpModules>
           <add name="PerRequestLifestyle" type="Castle.Facilities.AspNet.SystemWeb.Lifestyle.PerWebRequestLifestyleModule, Castle.AspNet.SystemWebFacility"/>
        </httpModules>
    </system.web>
</configuration>
```

## How do I register objects?

You would new up a windsor container as usual then use the ComponentRegistration extensions of the facility to stipulate 
'PerWebRequest' lifestyles. Like so: 

```csharp

var container = new WindsorContainer();
container.Register(Component.For<PerWebRequestComponent>().LifestylePerWebRequest());

```

If you need the attribute based lifestyle approach you it is also available:

```csharp

[PerWebRequest]
public class PerWebRequestComponentWithAttributedLifestyle
{
}

var container = new WindsorContainer();
container.Register(Component.For<PerWebRequestComponentWithAttributedLifestyle>().Named("P"));

```

## I have problems and need help!

Please sign in to [GitHub](https://github.com/castleproject/Windsor) and raise an issue. Please be clear and
concise, if we have a code repro we can most definitely guide you. 