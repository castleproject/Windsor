# .NET Core 3.0 Dependency injection extension
Service Provider using Castle Windsor container for ASP.NET Core 3.x. Fully replaces ASP.NET Core Default Service Provider (container). 

## How does it work?

### Registering services
You can register service either `IServiceCollection` in startup class. Just as you would with standard .NET Dependency Injection add your registration to the `Startup` class.

Alternatively you can create `ConfigureServices` method in `Startup` class that gets passed the `IWindsorContainer` instance and register the services there
Please note. ConfigureContainer is called after ConfigureServices in the Host Startup Chain
```c#
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
1. Add `Castle.Windsor.Extensions.DependencyInjection` package to your application.
2. Add `UseWindsorContainerServiceProvider()` when creating the Host
```c#
Host.CreateDefaultBuilder(args)
	.UseWindsorContainerServiceProvider()
```
    This will register an `IServiceProviderFactory`
2. Any services registred in `Startup.ConfigureServices` will be registered with `IWindsorContainer`. No need to cross-wire since `IWindsorContainer` is the only `IServiceProvider`
4. To access the container directly inject either `IWindsorContainer` or `IServiceProvider`


## How do I utilizing an external container to this factory?
This factory creates its own container. However we have expose the IServiceProviderFactory CreateBuilder method that the host uses to create the windsor container. By overwriting the default implemenation you can pass your own already created container to the builder.

1. Create or use an exisitn implementation that exposes IServiceProviderFactory<IWindsorContainer> (could e.g. be your bootstrapper). Or extend WindsorServiceProviderFactory
2. overwrite IWindsorContainer CreateBuilder(IServiceCollection services) with:

```c#
var container = services.CreateContainer(this, YourContainer);
return container;
```
3. When creating the host (instead of above): 
```c#
Host.CreateDefaultBuilder(args)
	.UseWindsorContainerServiceProvider(YourIServiceProviderFactory<IWindsorContainer>)
```
 

