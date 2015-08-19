# Castle Windsor for StructureMap users

This page is targeted towards people who already know StructureMap container, and want to get up to speed with Castle Windsor. You will quickly learn about major differences between the two, and how best you can reuse what you already know about StructureMap.

## Bootstrapping your application

A common technique for using StructureMap is to have a static `Bootstrapper` class that initialises the static, `ObjectFactory` container, often by adding instances of type `Registry`. For example:

```csharp
public static class Bootstrapper
{
    public static void Configure()
    {
        ObjectFactory.Initialize(x => x.AddRegistry<MyAppRegistry>());
    }
}
```

The entry point to the application will call the `Bootstrapper` to initialize the container, and then resolve the type or types needed to start running the application using `ObjectFactory.GetInstance<T>()`.

```csharp
Bootstrapper.Configure();
var shell = ObjectFactory.GetInstance<IApplicationShell>();
shell.Show();
```

With Windsor the standard approach is to [bootstrap a single instance of the container](three-calls-pattern.md) at the entry point of the application and resolve types from that. Windsor's equivalent to a `Registry` is an installer -- a class which implements `IWindsorInstaller`. The following code will create a new container and install all public `IWindsorInstaller` implementations in the current assembly into the container.

```csharp
public IWindsorContainer BootstrapContainer()
{
    return new WindsorContainer()
        .Install( FromAssembly.This() );
}
```

This will be used in the entry point to the application in a similar way to StructureMap. Windsor's equivalent to `GetInstance<T>()` is `Resolve<T>()`.

```csharp
var container = BootstrapContainer();
var shell = container.Resolve<IApplicationShell>();
shell.Show();
```

See [Three Calls Pattern](three-calls-pattern.md) for the details and recommendations on how to bootstrap the Windsor container.

## From Registry to Installer

For StructureMap type mappings are configured in the constructor of a `Registry` sub-class (this example uses the StructureMap 2.6+ syntax).

```csharp
class MyAppRegistry : Registry
{
    public MyAppRegistry()
    {
        For<IFoo>().Use<Foo>();
        For<IBar>().Use<Bar>();
    }
}
```

For Windsor this is done by implementing the `Install` method of the `IWindsorInstaller` interface.

```csharp
public class MyAppInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        container.Register(
            Component.For<IFoo>().ImplementedBy<Foo>(),
            Component.For<IBar>().ImplementedBy<Bar>()
        );
    }
}
```

### Auto-wiring: scanning with conventions

StructureMap has a `Scan()` method in the `Registry` base class to automatically add type mappings based on various conventions. A common convention is to map implementations to interfaces based on name (e.g. map `IFoo` as implemented by `Foo`):

```csharp
public MyAppRegistry()
{
    Scan(x =>
    {
        x.TheCallingAssembly();
        x.WithDefaultConventions();
    });
    //No longer required (will be wired up by convention):
    //For<IFoo>().Use<Foo>();
    //For<IBar>().Use<Bar>();
}
```

For Windsor we do this by *picking* types using a specific service.

```csharp
public class MyAppInstaller : IWindsorInstaller
{
    public void Install(IWindsorContainer container, IConfigurationStore store)
    {
        container.Register(
            AllTypes.FromThisAssembly().Pick()
                .WithService.DefaultInterfaces()
                .Configure(c => c.Lifestyle.Transient);
            //No longer required (will be wired up by convention):
            //Component.For<IFoo>().ImplementedBy<Foo>(),
            //Component.For<IBar>().ImplementedBy<Bar>()
        );
    }
}
```

#### Adding dependencies

To add a dependency to an auto-wired component, you need to use `ConfigureFor`:

```csharp
container.Register(
    AllTypes.FromThisAssembly().Pick()
        .WithService.DefaultInterfaces()
        .ConfigureFor<ISomething>(c => c.DependsOn(new[] {
            Property.ForKey("someKey").Eq("someValue"),
        })
);
```

### Resolving collections

When multiple implementations of a single interface are registered with StructureMap, then an instance of each implementation will be injected when a collection of that type is requested. For example, if we have these mappings:

```csharp
For<IFoo>().Use<ThisFoo>();
For<IFoo>().Use<ThatFoo>();
```

then `ObjectFactory.GetInstance<IEnumerable<IFoo>>()` will yield an instance of `ThisFoo` and an instance of `ThatFoo`.

This behaviour needs to be explicitly configured for Windsor by adding a `Resolver`. The following configuration will return an array containing `ThisFoo` and `ThatFoo` when an array of `IFoo` is resolved (`container.Resolve<IFoo[]>()`).

```csharp
public void Install(IWindsorContainer container, IConfigurationStore store)
{
    container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
    container.Register(
        Component.For<IFoo>().ImplementedBy<ThisFoo>(),
        Component.For<IFoo>().ImplementedBy<ThatFoo>()
        AllTypes.FromThisAssembly().Pick().WithService.DefaultInterfaces();
    );
}
```

### Namespaces

Windsor partitions components into a number of different namespaces. Here is a quick summary of the namespaces used so far.

Behaviour | Required namespaces
--------- | -------------------
Creating a container (`new WindsorContainer().Install()`) | `Castle.Windsor`
Basic installer (implementing `IWindsorInstaller`) | `Castle.Windsor`, `Castle.MicroKernel.Registration`, `Castle.MicroKernel.SubSystems.Configuration`
Resolvers | `Castle.MicroKernel.Resolvers` and sub-namespaces like `SpecializedResolvers` which include `ArrayResolver`

## Lifestyle (instance scope)

What's called an instance scope in StructureMap, Windsor calls Lifestyle, as specified in the [`LifestyleType`](lifestyles.md) enum.

StructureMap | Windsor | Notes
------------ | ------- | -----
Singleton | Singleton | This is the default lifestyle in Windsor
PerRequest | Transient | [Windsor keeps references to transient components](release-policy.md)!
ThreadLocal | PerThread |
HttpContext | PerWebRequest |
HttpSession | None/Custom | There's no direct equivalent in Windsor for this lifestyle, but implementing one is trivial
Hybrid | None/Custom | There's no direct equivalent in Windsor for this lifestyle, but implementing one is trivial

## ConnectImplementationsToTypesClosing

StructureMap has `ConnectImplementationsToTypesClosing` that can used to register non-generic types with their base generic service. How can I do that with Windsor?

```csharp
kernel.Register(AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
                        .BasedOn(typeof(ICommand<>))
                        .WithService.Base());
```

## Registering single type with multiple services

You can easily register single component type, to satisfy more than one service in Castle Windsor. For the follwing StructureMap code:

```csharp
ForRequestedType<IEventStoreUnitOfWork<IDomainEvent>>()
                .CacheBy(InstanceScope.Hybrid)
                .TheDefault.Is.OfConcreteType<EventStoreUnitOfWork<IDomainEvent>>();

ForRequestedType<IUnitOfWork>()
                .TheDefault.Is.ConstructedBy(x => x.GetInstance<IEventStoreUnitOfWork<IDomainEvent>>());
```

Corresponding Windsor code would be:

```csharp
container.Register(
    Component.For<IUnitOfWork, IEventStoreUnitOfWork<IDomainEvent>>()
        .ImplementedBy<EventStoreUnitOfWork<IDomainEvent>>().LifeStyle.PerWebRequest
);
```

The ability to register component with multiple services is called [Type Forwarding](forwarded-types.md), and can be also set from XML configuration.
