# Startable Facility

Startable facility allows objects to be "Started" after they are created, and/or to be "Stopped" once they are released. The objects are instantiated eagerly, that means container will create the first instance without you having to explicitly call `container.Resolve`. This is mostly useful for singleton background services that you want to have running for the entire lifespan of your application.

Starting with version 2.5 of Windsor, the facility offers two modes of working - aggressive (default, same as in previous versions) and new - optimized for usage of [single install call](faq.md#how-do-i-interact-with-the-container-i-mean-how-and-where-do-i-actually-call-it).

Following code samples assume that the kernel with registered facility is in scope

```csharp
using Castle.Facilities.Startable;
IKernel container = new DefaultKernel();
```

## Making components startable

Component need not to be changed at all to become a startable. The only thing required is to call `Start` extention method during component's registration

```csharp
container.Register(
   Component.For<PocoComponent>()
      .Start()
);
```

After such a registration component will be resolved by Windsor and will not be destroyed by Garbage Collector until the end of it's container lifecycle (if the component implements `IDisposable` it's `Dispose` method will then be invoked automatically during container's deinitialization).

### Dedicated start/stop methods

Sometimes component have complex initialization and constructor is not enough for the job (e.g. configuring through `IConfiguration` object or [properties setters](inline-dependencies.md#setting-up-properties-property-forkey). In such a case component may introduce special method(s).

```csharp
container.Register(
    Component.For<PocoComponent>().DependsOn(Property.ForKey("Name").Eq("John"))
        .StartUsingMethod("Begin")
        .StopUsingMethod("End")
);
```

In this example `PocoComponent`'s `Begin` method will be called at the end of initialization (i.e. than the `Name` property is set to "John") whereas `End` method will be called during container's utilization phase.

Both `StartUsingMethod` and `StopUsingMethod` are extension methods living in facility's namespace.

You can skip either Start or Stop method if you don't need it.

:information_source: **Requirements:** Stop and Start methods have to be public, have return type of 'void' and zero parameters.

### Strongly typed POCO

You can also use strongly typed version of the above code:

```csharp
container.Register(
   Component.For<PocoComponent>().DependsOn(Property.ForKey<string>().Eq("init value"))
      .StartUsingMethod(c => c.Begin)
      .StopUsingMethod(c => c.End)
);
```

### `IStartable` interface

Another option to make component startable is to implement the `Castle.Core.IStartable` interface

```csharp
public interface IStartable
{
    void Start();
    void Stop();
}
```

When component implements it you can register the component just like any other usual component. Facility will automatically register it's Start() and Stop() methods.

## XML configuration

As most facilities, you can also configure Startable Facility via XML config file

If you don't register the facility in code, you can register it in config file:

```xml
<facility id="startable"
    type="Castle.Facilities.Startable.StartableFacility, Castle.Windsor" / >
```

And register your startable components using additional startable attributes:

```xml
<component id="mycomponent"
    type="Namespace.MyComponent, Assembly"
    startable="true" startMethod="StartListener" stopMethod="StopListener" / >
```

You need to specify `startable="true"` (which is equivalent to `Start` extension method discussed above) and optionally either `startMethod`, `stopMethod` or both.

## Aggressive (old) mode

For backward compatibility this mode is the default, so you just need to add the facility to the container.

```csharp
container.AddFacility<StartableFacility>();
```

Assuming we have a startable component, that prints out every step of its lifetime to the console like this...

```csharp
public class Startable : IStartable
{
   public Startable()
   {
      Console.WriteLine("Created!");
   }

   public void Start()
   {
      Console.WriteLine("Started!");
   }

   public void Stop()
   {
      Console.WriteLine("Stopped!");
   }
}

// later in code
container.Register(Component.For<Startable>());
Console.WriteLine("Registered!");

container.Dispose();
Console.WriteLine("Released!");
```

If we execute this code, the following will be printed out:

```
Created!
Started!
Registered!
Stopped!
Released!
```

:information_source: **Notice:** Notice that component was instantiated and started during its registration and that we didn't resolve the component explicitly.

## Deferred mode - Optimized for single call to `Install`

If you're using three step approach to interacting with Windsor and registering all your components during a single call to `container.Install` you can take another route with the facility, which offers better performance and behaves differently.

:information_source: **Prefer deferred mode:** Due to backward compatibility the deferred mode is off by default. However when possible you should prefer this mode over the old one.

When using this approach the facility is not trying aggressively to instantiate your components right on the spot, but instead waits for the end of the call to `Install`. Only at this point it will resolve and start all the startable components.

:information_source: **Deferred mode will thrown when component can't be resolved:** Using optimized mode, it is assumed that when the call to `Install` ends all components should be correctly configured and resolvable so that if Windsor can't resolve a component it will consider this an error and throw an exception, and not silently wait for the missing dependency to appear, like the old mode. When for some reasons you want to disable the exception and let the component just silently not start you can use `DeferredTryStart` method.

### How to activate it

The optimized mode is not the default so you need to enable it explicitly when registering the facility:

```csharp
container.AddFacility<StartableFacility>(f => f.DeferredStart());
```

If you want to disable the fail fast behavior, which throws exception when the component can not be started, you can do it by calling `DeferredTryStart` method.

```csharp
container.AddFacility<StartableFacility>(f => f.DeferredTryStart());
```

## External resources

* [Blog post by Mike Hadlow about the facility (Jan 16, 2010)](http://mikehadlow.blogspot.com/2010/01/10-advanced-windsor-tricks-5-startable.html)
* [Blog post by Alex Henderson about the facility (Apr 29, 2007)](http://blog.bittercoder.com/PermaLink,guid,a621ddda-acb5-4afd-84ff-faafb96a2fa1.aspx) - written long ago but still accurate