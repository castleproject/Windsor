# Component Activators

## What is a Component activator?

Component activator is used by Windsor to instantiate components, and perform all additional set up logic, like setting properties, decorating services with proxy etc. Activators implement `IComponentActivator` interface.

:information_source: **Activators instantiate components - always:** Each call to activator's `Create` method should result in a new instance being returned. If you want to manage instance lifetime, that is make a component a singleton, or reuse it withing a scope of web request etc you need a [LifeStyle manager](lifestyles.md).

## Writing custom activators

The `IComponentActivator` interface has two methods:

```csharp
object Create(CreationContext context);
void Destroy(object instance);
```

In addition to these methods, since all Activators are instantiated by Windsor, it is required that all activators expose a public constructor with the following signature:

```csharp
public MyActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
```

Their names are pretty descriptive. `Create` instantiates and returns a new instance of the component, while `Destroy` releases the instance. Usually you won't implement the interface directly. Preferred approach is to use base class provided by Windsor.

### `AbstractComponentActivator`

`AbstractComponentActivator` provides all the basic bootstrapping for you so that you can concentrate on actual logic of your activator. It is highly recommended that you inherit your activator from this class.

Alternatively if you want to just slightly tweak the behavior of `DefaultComponentActivator` you can inherit directly from this class. It's entire functionality is virtual so you can override just a single piece of it, if that's what you're after.

### `IDependencyAwareActivator` interface

When activator itself provides dependencies to the component (for example factory activator, or instance activator) they should implement `IDependencyAwareActivator` to notify Windsor about that fact so that Windsor skips checking if the component's dependencies are available.

The interface exposes a single method:

```csharp
bool CanProvideRequiredDependencies(ComponentModel component);
```

The method should return `true` if the activator wants to take things into its own *hands* and provide all required dependencies to the component.