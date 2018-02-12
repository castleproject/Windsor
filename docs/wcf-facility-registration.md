# WCF Facility - Registration

## How to register WCF Facility

It is very easy to register WCF Facility using the AddFacility method of the IWindsorContainer. The code below shows how this can be done.

```csharp
container.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
    .Register(Component.For<IMyService>()
    .ImplementedBy<MyService>()
    .AsWcfService(new DefaultServiceModel()))
```

In the above example, IMyservice is the service contract and MyService is the implementation of the contract.

You can also use the DependsOn() method to specify custom dependencies for the service as in the example below.

```csharp
container.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
    .Register(Component.For<IMyService>()
    .DependsOn(new { number = 42 })
    .ImplementedBy<MyService>()
    .AsWcfService(new DefaultServiceModel()))
```

It is required to update the .svc file to set the Castle.Windsor factory:

```
<%@ ServiceHost
  Language="C#"
  Service="MyService"
  Factory="Castle.Facilities.WcfIntegration.DefaultServiceHostFactory, Castle.Facilities.WcfIntegration" %>
```

Additionally, it is possible to load the configuration from a separate config file. For example:

```csharp
Container.AddFacility<WcfFacility>()
    .Install(Castle.Windsor.Installer.Configuration.FromXmlFile("Windsor.config"));

```

In this case, you must add a Windsor.config file to the project - an example is:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <components>
    <component id="BistroServices" service="IMyService" type="MyService" lifestyle="transient">
    </component>
  </components>
</configuration>
```

## non-HTTP Protocol WCF Services

For HTTP protocol WCF services hosted under IIS, the Application_Start is called when the application is first started. To be more precisely, it is called when the HttpApplication type is instantiated and this occurs when the first HTTP request comes into the application. Under the hood, the global.asax file is compiled into a derived type of System.Web.HttpApplication and the corresponding global methods are mapped to the events exposed from HttpApplication.

### Using AppInitialize

The global.asax instantiation does not work for non-HTTP protocols such as net.tcp and net.pipe that are enabled through hosting via Windows Activation Service (WAS) on IIS7 and later as there is no counterpart for HttpApplication.

Fortunately, ASP.NET provides a simple hook that works in a protocol agnostic way. The hook is based on the following AppInitialize method:

```csharp
public static void AppInitialize();
```

This method can be put in any type that is defined in a C# file in the application's `App_Code` directory. When the AppDomain is started, ASP.NET reflects whether there is a type that has such as method signature.

So, Create a class and put it into a folder called `App_Code` in the root of your project and give it a method signature as defined below. You can then initialise your IoC container in there and register it with the DefaultServiceHostFactory.

```csharp
public class AnyClassName
{
    public static void AppInitialize()
    {
        DefaultServiceHostFactory.RegisterContainer(((CastleContainer)IoC.Container).WindsorContainer.Kernel);
    }
}
```

:information_source: If you are switching from WCF activation to Windsor's WcfFacility, please make sure to remove the ServiceBehavior attribute from service type.