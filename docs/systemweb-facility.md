# System Web Facility

The system web facility provides legacy `scoped` management of lifestyles using [http modules](https://msdn.microsoft.com/library/ms178468.aspx) for .NET Framework web based projects.

## How does it work?

Components resolved within a PerWebRequest lifestyle will survive until the end of the current ASP.NET request via HttpContext.Current.

## What do I need to set it up?

You will need 3 things to get this going:

- Assembly reference to System.Web
- NuGet reference for [Microsoft.Web.Infrastructure](https://www.nuget.org/packages/Microsoft.Web.Infrastructure/)
- NuGet reference for [Castle.Facilities.AspNet.SystemWeb](https://www.nuget.org/packages/Castle.Facilities.AspNet.SystemWeb/)

Since Windsor 3.0, Microsoft.Web.Infrastructure has been used to automatically register the HTTP module, however the HTTP module can still be manually registered in your web.config file:

```xml
<configuration>
    <system.web>
        <httpModules>
           <add name="PerRequestLifestyle" type="Castle.Facilities.AspNet.SystemWeb.Lifestyle.PerWebRequestLifestyleModule, Castle.Facilities.AspNet.SystemWeb"/>
        </httpModules>
    </system.web>
</configuration>
```

## How do I register services?

You would new up a windsor container as usual then use the ComponentRegistration extensions of the facility to stipulate 
'PerWebRequest' lifestyles. Like so: 

```csharp
var container = new WindsorContainer();
container.Register(Component.For<MyComponent>().LifestylePerWebRequest());
```

The lifestyle can also be applied using an attribute:

```csharp
[PerWebRequest]
public class MyComponentWithAttributedLifestyle
{
}

var container = new WindsorContainer();
container.Register(Component.For<MyComponentWithAttributedLifestyle>().Named("P"));
```
