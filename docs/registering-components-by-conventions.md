# Registering components by conventions

## Registering Multiple Types at once

[Registering components one-by-one](registering-components-one-by-one.md) can be very repetitive job. Also remembering to register each new type you add can quickly lead to frustration. Fortunately, in majority of cases, you don't have to do it, nor should you. By using `Classes` or `Types` entry classes you can perform group registration of types based on some specified characteristics you specify. This is the way you will find using most often when writing applications with Windsor.

## Three steps

Registration of multiple types usually takes roughly the following form:

```csharp
container.Register(Classes.FromThisAssembly()
    .InSameNamespaceAs<RootComponent>()
    .WithService.DefaultInterfaces()
    .LifestyleTransient());
```

You can identify three distinct steps in the registration call.

### Selecting assembly

First step is to point Windsor to the assembly (or assemblies) it should scan. You do it by using:

```csharp
Classes.FromThisAssembly()...
```

or one of its sister methods.

:information_source: **Should I use `Classes` or `Types`?:** There are two ways to start registration by convention. One of them is using `Classes` static class, like in the example above. Second is using `Types` static class. They both expose exactly the same methods. The difference between them is, that `Types` will allow you to register all (or to be precise, if you use default settings, all public ) types from given assembly, that is classes, interfaces, structs, delegates, and enums. `Classes` on the other hand pre-filters the types to only consider non-abstract classes. Most of the time `Classes` is what you will use, but `Types` can be very useful in some advanced scenarios, like registration of [interface based typed factories](typed-factory-facility-interface-based.md).

:warning: **What about `AllTypes`?:** Both `Classes` and `Types` are new in Windsor 3. Previous versions had just one type to do the job - `AllTypes`. In Windsor 3, usage of `AllTypes` is discouraged, because its name is misleading. While it suggests that it behaves like `Types`, truth is it's exactly the same as `Classes`, pre-filtering all types to just non-abstract classes. To avoid confusion, use one of the two new types.

### Selecting base type/condition

Once you selected an assembly you do first base step of filtering the types you want to register. This will narrow the scope of the objects in one of the following ways:

1. By base type/implemented interface, for example:
  * `Classes.FromThisAssembly().BasedOn<IMessage>()`
1. By namespace, for example:
  * :information_source: This method (and its overloads) are new in Windsor 3
  * `Classes.FromAssemblyInDirectory(new AssemblyFilter("bin")).InNamespace("Acme.Crm.Extensions")`
1. By any condtion, for example:
  * `Classes.FromAssemblyContaining<MyController>().Where( t=> Attribute.IsDefined(t, typeof(CacheAttribute)))`
1. No restrictions at this point:
  * `Classes.FromAssemblyNamed("Acme.Crm.Services").Pick()`

### Additional filtering and configuration

Once you selected source for the types and base condition you can also configure the types or additionally filter some of them out. All the details of the API you can use at this point are discussed below.

:warning: **`BasedOn`, `Where` and `Pick` do logical `or`:** Be wary when using more than one of `BasedOn`, `Where` and `Pick` at once. The following:

```csharp
container.Register(
    Classes.FromThisAssembly()
        .BasedOn<IMessage>()
        .BasedOn(typeof(IMessageHandler<>)).WithService.Base()
        .Where(Component.IsInNamespace("Acme.Crm.MessageDTOs"))
);
```

Will register all messages **and also** all message handlers **and also** all message DTOs. This is usually not the behavior you want, and to avoid confusion in Windsor 3 this call chaining will give you a compiler warning. Be explicit and register those three sets of components in three separate calls.

#### Registering all descendants of given type (for example all controllers in MVC application)

Here's an example from a Monorail configuration:

```csharp
container.Register(
    Classes.FromThisAssembly()
        .BasedOn<SmartDispatcherController>()
        .Configure(c => c.Lifestyle.Transient)
);
```

We are registering all types which implement `SmartDispatcherController` from the executing assembly. This is a quick way of adding all your controllers to the container. It will also automatically register all new controllers as you add them to your application.

#### Default Service

Keep in mind that `Of` as well as `Pick` and other filtering methods only narrow down the set of types we want to register. **They do not specify the service that these types provide** and unless you specify one the default will be used, that is the implementation type itself. In other words, the above registration will register all types inheriting from `SmartDispatcherController` with service being their own type, not the `SmartDispatcherController` so trying to call

```csharp
var controller = container.Resolve<SmartDispatcherController>();
```

will throw an exception.

In this default case types may be requested only by their implementation type (the default service).

```csharp
var controller = container.Resolve<MyHomeController>();
```

## Selecting service for the component

By default the service of the component is the type itself. There are several situations in which this is not sufficient. Windsor lets you specify the service type explicitly.

### `Base()`

```csharp
container.Register(
    Classes.FromThisAssembly()
        .BasedOn(typeof(ICommand<>)).WithService.Base(),
    Classes.FromThisAssembly()
        .BasedOn(typeof(IValidator<>)).WithService.Base()
);
```

Here we register all types implementing closed version of `ICommand<>` and `IValidator<>`, respectively (Now, read it again, slowly). To give you an example - when we request `ICommand<AddCustomer>` we'll receive an instance of type implementing `ICommand<AddCustomer>`, most likely named something like `AddCustomerCommand`.

The most important aspect of this specification, and often most confusing, is the presence of the `WithService.Base()`. This tells the registration strategy to choose the closed version of the interface specification as the service type. In this case, something like `IValidator<Customer>` might be selected. Without it, the type would be registered against the open version of the interface and you would most likely not get the desired resolution capability.

### `DefaultInterfaces()`

:information_source: This method was renamed in Windsor 3 from `DefaultInterface` to emphasize the fact that it can match more than just one service for a component.

```csharp
container.Register(
    Classes.FromThisAssembly()
        .InNamespace("Acme.Crm.Services")
        .WithService.DefaultInterfaces()
);
```

This method performs matching based on type name and interface's name. Often you'll find that you have interface/implementation pairs like this: `ICustomerRepository`/`CustomerRepository`, `IMessageSender`/`SmsMessageSender`, `INotificationService`/`DefaultNotificationService`. This is scenario where you might want to use `DefaultInterfaces` method to match your services. It will look at all the interfaces implemented by selected types, and use as type's services these that have matching names. Matching names, means that the implementing class contains in its name the name of the interface (without the *I* on the front).

### `FromInterface()`

Another very common scenario is having the ability to register all types that share a common interface, but are otherwise unrelated.

```csharp
container.Register(
    Classes.FromThisAssembly()
        .BasedOn<IService>().WithService.FromInterface()
);
```

Here we register service classes from the executing assembly.  In this case, the `IService` interface might be a marker interface identifying the role of a component in the system.  Unlike `WithService.Base()`, the service type selected for this registration is chosen from the interface that extended `IService`.  The following is an example to help illustrate this.

Lets say you have a marker interface `IService` to designate all services in your assembly.

```csharp
public interface IService {}

public interface ICalculatorService : IService
{
     float Add(float op1, float op2);
}

public class CalculatorService : ICalculatorService
{
     public float Add(float op1, float op2)
     {
         return op1 + op2;
     }
}
```

The above registration would be equivalent to

```
container.Register(
    Component.For<ICalculatorService>().ImplementedBy<CalculatorService>()
);
```

As you can see, the actual service interface is NOT `IService`, but rather the interface extending `IService`, which is `ICalculatorService` in this case.

### `AllInterfaces()`

When a component implements multiple interfaces and you want to use it as a service for all of them, use `WithService.AllInterfaces()` method.

### `Self()`

To register the component implementation type explicitly as a service use `WithService.Self()`

### `Select()`

If none of the above options suits you you can provide your own selection logic as a delegate and pass it to `WithService.Select()` method.

:information_source: **Services are cumulative:** Multiple calls to `WithService.Something()` are allowed and they are cumulative. That means that if you call:

```csharp
Classes.FromThisAssembly()
   .BasedOn<IFoo>()
   .WithService.Self()
   .WithService.Base()
```

the types matched will be registered as both IFoo, and themselves. In other words the above would be equivalent to doing the following for each type implementing `IFoo`

```csharp
Component.For<IFoo, FooImpl>().ImplementedBy<FooImpl>();
```

### Registering non-public types

By default only types visible from outside of the assembly will be registered. If you want to include non-public types, you have to start with specifying assembly first, and then call `IncludeNonPublicTypes`

```csharp
container.Register(
    Classes.FromThisAssembly()
        .IncludeNonPublicTypes()
        .BasedOn<NonPublicComponent>()
);
```

:warning: **Don't expose non-public types:** It is rarely a good idea to expose via container types that wouldn't be available otherwise. Usually they are not public for a reason. Think twice before using this option.

### Configuring registration

When you register your types you can also configure them to set all the same properties as when registering types one by one. For this, you use configure method. The most common case is to assign a lifestyle to your components other than the default Singleton.

```csharp
container.Register(
    Classes.FromAssembly(Assembly.GetExecutingAssembly())
        .BasedOn<ICommon>()
        .Configure(component => component.LifestyleTransient())
);
```

This scenario is so common that Windsor 3 provides a shortcut to it

:information_source: Below method and its sister methods are new in Windsor 3

```csharp
container.Register(
    Classes.FromAssembly(Assembly.GetExecutingAssembly())
        .BasedOn<ICommon>()
        .LifestyleTransient()
);
```

In addition to (or instead of) specifying the lifestyle you can set many other configuration options:

```csharp
container.Register(
    Classes.FromAssembly(Assembly.GetExecutingAssembly())
        .BasedOn<ICommon>()
        .LifestyleTransient()
        .Configure(component => component.Named(component.Implementation.FullName + "XYZ"))
);
```

In here we register classes implementing `ICommon`, set their lifestyle and name.

You can do also a more fine grained configuration, setting some additional properties for a subset of your components:

```csharp
container.Register(
    Classes.FromThisAssembly()
        .BasedOn<ICommon>()
        .LifestyleTransient()
        .Configure(
            component => component.Named(component.Implementation.FullName + "XYZ")
        )
        .ConfigureFor<CommonImpl1>(
            component => component.DependsOn(Property.ForKey("key1").Eq(1))
        )
        .ConfigureFor<CommonImpl2>(
            component => component.DependsOn(Property.ForKey("key2").Eq(2))
        )
);
```

In here, we do the same thing as above, but in addition for types implementing two other interfaces we set additional inline dependencies.

:information_source: See the "Conditional component registration" section below for a discussion on better filtering the types you register.