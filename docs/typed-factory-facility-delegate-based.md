# Typed Factory Facility - delegate-based factories

In some simple cases it may feel like an overkill to create additional `interface` when all you want is to resolve single component. For cases like this Windsor (version 2.5 or newer) lets you take dependency on a `delegate` rather than `interface`, and the `delegate` will callback to the container to provide the instance for you when you invoke it.

## Using factories

There's nothing magical with `delegate`-based factories. You use them as regular delegates, while Windsor does all the heavy lifting.

```csharp
public class UsingDelegate
{
   private Func<IFoo> factory;

   public void UsingDelegate(Func<IFoo> fooFactory)
   {
      factory = fooFactory;
   }

   public void SomeOperation()
   {
      var foo = factory();
      foo.Bar();
   }

}
```

In this case creating a `UsingDelegate` does not trigger creation of (potentially expensive to create - think NHibernate's `SessionFactory`) `IFoo`. `IFoo` will only be created when call to `SomeOperation` occurs, which may be a log time after `UsingDelegate` was created.

## Registering factories (implicitly)

The act of exposing delegate as dependency is enough to instruct the facility that it should provide a factory. This is called implicit registration. It can't be just any delegate though. Similar rules as with `interface`-based factories apply:

* delegate must have a non-`void` return type.
* delegate can't have out arguments

:information_source: **Lifestyle:** The factories registered that way will have [transient](lifestyles.md#transient) lifestyle.

## Registering factories explicitly

When you register the factory implicitly you give up the ability to configure the factory. If you need some non-default configuration, you can register, and configure the factory explicitly.

```csharp
container.Register(
	 Component.For<Func<IFoo>>().AsFactory(c => c.SelectedWith("nonDefaultFactory"))
);
```

## How dependencies are matched

Dependencies are matched a little differently for `delegate`-based factories.

### First by name

For delegate:

```csharp
public delegate IFoo FooFactory(IBar bar);
```

If component registered for service `IFoo` has dependency named `bar` it will be satisfied with the value passed to the delegate.

### Then by type

If instead the component has dependency named `mySuperBar` for type `IBar`, it won't find it on the `FooFactory` delegate. It will then go and see if there's any dependency for type `IBar` regardless of it's name, and if it finds one, it will use that instead.

:information_source: **Duplicated dependency types are not allowed for `Func<>` family of delegates:** When using generic purpose delegate `Func<>` (any of them) it is not possible to have delegate with duplicated argument types. Since their names are just meaningless `arg1`, `arg2` etc, it is not possible to discriminate between arguments in `Func<IFoo, IFoo>`. In this case use dedicated `delegate` type, or better yet - `interface`-based factory.}

## Releasing components resolved via factory

It is important to remember that if the [Release Policy](release-policy.md) of your container tracks a component you resolve, the factory will hold a reference to the component as well. This is to let Windsor release the components you resolve via the factory, when the factory itself gets released.

:warning: **`NoTrackingReleasePolicy` will not release the factory:** Since release policies that do not track components are not able to release them, it's important to use `LifecycledComponentsReleasePolicy` (which is the default) when using `delegate`-based factories to be able to properly release the components resolved via the factory.

## See also

* [Typed Factory Facility](typed-factory-facility.md)
* [Typed Factory Facility - interface-based factories](typed-factory-facility-interface-based.md)