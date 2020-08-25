# .NET Core 3.0 Dependency injection extension
Service Provider using Castle Windsor container for ASP.NET Core 3.x. Fully replaces ASP.NET Core Default Service Provider (container). 

## How does it work?

### Registering services
You can register service either `IServiceCollection` in startup class. Just as you would with standard .NET Dependency Injection add your registration to the `Startup` class.

Alternatively you can create `ConfigureServices` method in `Startup` class that gets passed the `IWindsorContainer` instance and register the services there
```
public void ConfigureContainer (IWindsorContainer container)
{
	container.Install(new MyInstaller());
}
```

To match lifestyle of the registered components to .NET Service Scope, use `LifeStyle.ScopedToNetServiceScope()`.

### Services lifestyle
Because there are subtle differences between .NET and Castle Windsor lifestyle semantics services registered via `IServiceCollection` are following .NET semantics:


| .NET lifestyle | Description |
|:-:|:-:|
| Scoped | Lifecycle of the component is bound to the scope associated with `IServiceProvider` that resolved it. Even if it's not the closest one. The instance is disposed when `IServiceScope` is disposed. |
| Transient | New instance is provided when resolved but lifecycle is bound to the scope associated with `IServiceProvider` that resolved it. All instances are disposed when `IServiceScope` is disposed. |
| Singleton | Only one instance ever exists and it's disposed once outermost `IServiceProvider` is disposed (usually application shutdown). |

## What do I need to set it up?
### Using the .NET lifestyle semantics
All services injected into controllers will have to be registered with the lifestyles described in "Services lifestyle"

1. Add `Castle.Windsor.Extensions.DependencyInjection` package to your application.
2. Add `UseWindsorContainerServiceProvider()` when creating the Host
    ```
    Host.CreateDefaultBuilder(args)
         .UseWindsorContainerServiceProvider()
    ```
    This will register an `IServiceProviderFactory`
2. Any services registered in `Startup.ConfigureServices` will be registered with `IWindsorContainer`. No need to cross-wire since `IWindsorContainer` is the only `IServiceProvider`
4. To access the container directly inject either `IWindsorContainer` or `IServiceProvider`

### Using the Castle Windsor lifestyle semantics

This will allow you to leave your existing Castle Windsor registrations untouched.

Using .NET lifestyle semantics, any service using the Castle Windsor transient lifestyle, and directly injected into the controller, will not be released.
To allow the standard Windsor behavior for services directly injected in the controllers, you will have to allow Castle Windsor to resolve and release the controllers.
To do that add AddControllerAsServices to the call to ConfigureServices in your Startup class:

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddControllersAsServices();
    }


