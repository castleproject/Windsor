# Registering components one-by-one

## Basic Registration Examples

The starting point for registering anything in the container is the container's `Register` method, with has one or more `IRegistration` objects as parameter. The simplest way to create those objects is using the static `Castle.MicroKernel.Registration.Component` class. Its `For` method returns a `ComponentRegistration` that you can use to further configure the registration.

:information_source: **Isolate your registration code:** It is a recommended practice to keep your registration code in a dedicated class(es) implementing [`IWindsorInstaller`](installers.md).

:information_source: **Install infrastructure components first:** Some components may require a facility or other extension to the core container to be registered properly. As such it is recommended that you always register your [facilities, custom subsystems, Component model creation contributors etc](extension-points.md) before you start registering your components.

## To register a type in the container

```csharp
container.Register(
    Component.For<MyServiceImpl>()
);
```

This will register type `MyServiceImpl` as service `MyServiceImpl` with default lifestyle (Singleton).

## To register a type as non-default service

```csharp
container.Register(
    Component.For<IMyService>()
        .ImplementedBy<MyServiceImpl>()
);
```

Note that `For` and `ImplementedBy` also have non-generic overloads.

```csharp
// Same result as example above.
container.Register(
    Component.For(typeof(IMyService))
        .ImplementedBy(typeof(MyServiceImpl))
);
```

:information_source: **Services and Components:** You can find more information about services and components [here](services-and-components.md).

## To register a generic type

Suppose you have a `IRepository<TEntity>` interface, with `NHRepository<TEntity>` as the implementation.

You could register a repository for each entity class, but this is not needed.

```csharp
// Registering a repository for each entity is not needed.
container.Register(
    Component.For<IRepository<Customer>>()
        .ImplementedBy<NHRepository<Customer>>(),
    Component.For<IRepository<Order>>()
        .ImplementedBy<NHRepository<Order>>(),
//    and so on...
);
```

One `IRepository<>` (so called open generic type) registration, without specifying the entity, is enough.

```csharp
// Does not work (compiler won't allow it):
container.Register(
    Component.For<IRepository<>>()
        .ImplementedBy<NHRepository<>>()
);
```

Doing it like this however is not legal, and the above code would not compile. Instead you have to use `typeof()`

```csharp
// Use typeof() and do not specify the entity:
container.Register(
    Component.For(typeof(IRepository<>)
        .ImplementedBy(typeof(NHRepository<>)
);
```

## Configuring component's lifestyle

```csharp
container.Register(
   Component.For<IMyService>()
      .ImplementedBy<MyServiceImpl>()
      .LifeStyle.Transient
);
```

When the [lifestyle](lifestyles.md) is not set explicitly, the default Singleton lifestyle will be used.

## Register more components for the same service

You can do this simply by having more registrations for the same service.

```csharp
container.Register(
    Component.For<IMyService>().ImplementedBy<MyServiceImpl>(),
    Component.For<IMyService>().ImplementedBy<OtherServiceImpl>()
);
```

When a component has a dependency on `IMyService`, it will by default get the `IMyService` that was registered first (in this case `MyServiceImpl`).

:information_source: **In Windsor first one wins:** In Castle, the default implementation for a service is the first registered implementation. This is different from AutoFac for example, where the default is the last registered implementation ([http://code.google.com/p/autofac/wiki/ComponentCreation](http://code.google.com/p/autofac/wiki/ComponentCreation)).

You can force the later-registered component to become the default instance via the method `IsDefault`.

```csharp
container.Register(
    Component.For<IMyService>().ImplementedBy<MyServiceImpl>(),
    Component.For<IMyService>().Named("OtherServiceImpl").ImplementedBy<OtherServiceImpl>().IsDefault()
);
```

In the above example, any component that has a dependency on `IMyService`, will by default get an instance of `OtherServiceImpl`, even though it was registered later.

Of course, you can override which implementation is used by a component that needs it. This is done with service overrides.

When you explicitly call `container.Resolve<IMyService>()` (without specifying the name), the container will also return the first registered component for `IMyService` (`MyServiceImpl` in the above example).

:information_source: **Provide unique names for duplicated components:** If you want to register the same implementation more than once, be sure to provide different names for the registered components.

## Register existing instance

It is possible to register an existing object as a service.

```csharp
var customer = new CustomerImpl();
container.Register(
    Component.For<ICustomer>().Instance(customer)
);
```

:warning: **Registering instance ignores lifestyle:** When you register an existing instance, even if you specify a lifestyle it will be ignored. Also registering instance, will set the implementation type for you, so if you try to do it manually, an exception will be thrown.

## Using a delegate as component factory

You can use a delegate as a lightweight factory for a component:

```csharp
container
   .Register(
      Component.For<IMyService>()
         .UsingFactoryMethod(
            () => MyLegacyServiceFactory.CreateMyService())
);
```

`UsingFactoryMethod` method has two more overloads, which can provide you with access to kernel, and creation context if needed.

Example of `UsingFactoryMethod` with kernel overload (Converter<IKernel, IMyService>)

```csharp
container.Register(
    Component.For<IMyFactory>().ImplementedBy<MyFactory>(),
    Component.For<IMyService>()
         .UsingFactoryMethod(kernel => kernel.Resolve<IMyFactory>().Create())
);
```

In addition to `UsingFactoryMethod` method, there's a `UsingFactory` method. (without the "method" suffix :-) ). It can be regarded as a special version of `UsingFactoryMethod` method, which resolves an existing factory from the container, and lets you use it to create instance of your service.

```csharp
container.Register(
    Component.For<User>().Instance(user),
    Component.For<AbstractCarProviderFactory>(),
    Component.For<ICarProvider>()
        .UsingFactory((AbstractCarProviderFactory f) => f.Create(container.Resolve<User>()))
);
```

:warning: **Avoid UsingFactory:** It is advised to use `UsingFactoryMethod`, and to avoid `UsingFactory` when creating your services via factories. `UsingFactory` will be obsoleted/removed in future releases.

## OnCreate

It is sometimes needed to either inspect or modify created instance, before it is used. You can use `OnCreate` method to do this

```csharp
container.Register(
    Component.For<IService>()
        .ImplementedBy<MyService>()
        .OnCreate((kernel, instance) => instance.Name += "a")
);
```

The method has two overloads. One that works with a delegate to which an `IKernel` and newly created instance are passed. Another only takes the newly created instance.

:information_source: **`OnCreate` works only for components created by the container:** This method is not called for components where instance is provided externally (like when using Instance method). It is called only for components created by the container. This also includes components created via certain facilities ([Remoting Facility](facilities.md), [Factory Support Facility](facilities.md))

## To specify a name for the component

The default name for a registered component is the full type name of the implementing type. You can specify a different name using the Named() method.

```csharp
container.Register(
    Component.For<IMyService>()
        .ImplementedBy<MyServiceImpl>()
        .Named("myservice.default")
);
```

## Supplying the component for a dependency to use (Service override)

If a component needs or wants an other component to function, this is called a dependency. When registering, you can explicitly set which component to use using service overrides.

```csharp
container.Register(
    Component.For<IMyService>()
        .ImplementedBy<MyServiceImpl>()
        .Named("myservice.default"),
    Component.For<IMyService>()
        .ImplementedBy<OtherServiceImpl>()
        .Named("myservice.alternative"),

    Component.For<ProductController>()
        .DependsOn(ServiceOverride.ForKey("myService").Eq("myservice.alternative"))
);

public class ProductController
{
    // Will get a OtherServiceImpl for myService.
    // MyServiceImpl would be given without the service override.
    public ProductController(IMyService myService)
    {
    }
}
```

## Registering component with multiple services

It is possible to use single component for more than one service. For example if you have a class `FooBar` which implements both `IFoo` and `IBar` interfaces, you can configure your container, to return the same service when `IFoo` and `IBar` are requested. This ability is called type forwarding.

## Type forwarding

The easiest way to specify [type forwarding](forwarded-types.md) is to use multi-generic-parameter overload of `Component.For` method

```csharp
container.Register(
    Component.For<IUserRepository, IRepository>()
        .ImplementedBy<MyRepository>()
);
```

There are overloads for up to four forwarded services, which should always be enough. If you find yourself needing more you most likely are violating Single Responsibility Principle, and you might want to break your giant component into more, each doing just one thing.

There's also a non-generic overload, which takes either `IEnumerable<Type>` or `params Type[]`, in case you need open generics support or can't use generic version for whatever reason.

Moreover, you can use `Forward` method, which exposes identical behavior and overloads as `For` method.

```csharp
container.Register(
    Component.For<IUserRepository>()
        .Forward<IRepository, IRepository<User>>()
            .ImplementedBy<MyRepository>()
);
```

## Supplying inline dependencies

Not everything has to be a component in Windsor. Some components require parameters, like connection strings, buffer size etc. You can provide these parameters as [inline dependencies](inline-dependencies.md).

## See also

* [Conditional component registration](conditional-component-registration.md)
* [Registering components by conventions](registering-components-by-conventions.md)
