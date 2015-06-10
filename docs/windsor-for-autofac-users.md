# Castle Windsor for Autofac users

This page is targeted towards people who already know Autofac container, and want to get up to speed with Castle Windsor. You will quickly learn about major differences between the two, and how best you can reuse what you already know about Autofac.

:information_source: **Autofac version:** Autofac's API changed quite a bit between version 1.x and 2.x. This guide talks about version 2.x

## Lifestyle (instance scope)

What's called an instance scope in Autofac, Windsor calls Lifestyle, as specified in the [`LifestyleType`](lifestyles.md) enum.

Autofac | Windsor | Notes
------- | ------- | -----
SingleInstance | Singleton | This is the default lifestyle in Windsor, as it is in Autofac
InstancePerDependency | Transient | [Windsor keeps references to transient components](release-policy.md)!
InstancePerLifetimeScope | any or Custom | When disposing container all components served by it will be disposed*
InstancePerMatchingLifetimeScope | any or Custom |

:information_source: **\*Disposing the container:** Actually this statement is not entirely true. When the component gets disposed is up to its [`LifestyleManager`](lifestyles.md)

There are also additional lifestyles in Windsor like per thread, per web request, per WCF session etc.Windsor also lets you quite easily write and plug in your own. Read more about them all [here](lifestyles.md).

## DeterministicDisposal

One of the most advertised features of Autofac is [Deterministic Disposal](http://code.google.com/p/autofac/wiki/DeterministicDisposal). The good news is, that if you like that feature, Windsor also supports it.

```csharp
var parent = new WindsorContainer();
parent.AddComponentLifeStyle<SomeComponent>( LifestyleType.Transient );
using (var child = new WindsorContainer())
{
    //register new container as child container to parent
    parent.AddChildContainer( child );
    var component = child.Resolve<SomeComponent>();
    // component, and any of its disposable dependencies, will
    // be disposed of when the using block completes
}
```

Notice you need to register your components with transient lifestyle, which is the same as `factory` scope in Autofac.
You're safe to resolve components with singleton lifestyle - they will not be disposed with the child container and you can continue to resolve and use them after the child container was disposed.

Since Windsor does not assume ownership of child containers for the entirety of their lifetime, it won't itself create a child container, so this part is up to you. If you want to mimic Autofac's behavior, you can resort to an extension method, that will encapsulate creation of a new container, and plugging it as a child container to given parent.

```csharp
public static IWindsorContainer CreateChildContainer(this IWindsorContainer parent)
{
    var child = new WindsorContainer();
    parent.AddChildContainer( child );
    return child;
}
```

Now you can use syntax similar to Autofac's

```csharp
using (var child = parent.CreateChildContainer())
{
    var component = child.Resolve<SomeComponent>();
    // do something...
}
```

## ContainerBuilder and Modules

Autofac uses Modules to partition registration into manageable pieces. Windsor allows you to do the same thing, with `IWindsorInstaller` interface. Also Windsor has no concept of ContainerBuilder. You register your components directly in the container. Read more about registration API [here](fluent-registration-api.md).