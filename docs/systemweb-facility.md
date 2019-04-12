# System Web Facility

The System Web facility provides legacy `scoped` management of lifestyles using [HTTP modules](https://msdn.microsoft.com/library/ms178468.aspx) for .NET Framework web based projects.

## How does it work?

Components resolved within a per web request lifestyle will survive until the end of the current ASP.NET request via `HttpContext.Current`.

## What do I need to set it up?

You will need 3 things to get this going:

- Assembly reference to `System.Web`
- NuGet reference for [Microsoft.Web.Infrastructure](https://www.nuget.org/packages/Microsoft.Web.Infrastructure/)
- NuGet reference for [Castle.Facilities.AspNet.SystemWeb](https://www.nuget.org/packages/Castle.Facilities.AspNet.SystemWeb/)

Since Windsor 3.0, `Microsoft.Web.Infrastructure` has been used to automatically register the HTTP module, however the HTTP module can still be manually registered in your `web.config` file:

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

Instanate a Windsor container as usual, then use the `ComponentRegistration` extensions of the
facility to configure a 'PerWebRequest' lifestyle:

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

Configuring the lifestyle with XML configuration requires the custom `scopeAccessorType` of the
facility to be specified, this performs the same function as the extension method:

```xml
<components>
  <component
    service="IService, App"
    type="MyComponent, App"
    lifestyle="scoped"
    scopeAccessorType="Castle.Facilities.AspNet.SystemWeb.WebRequestScopeAccessor, Castle.Facilities.AspNet.SystemWeb">
  </component>
</components>
```

