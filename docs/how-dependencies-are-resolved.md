# How dependencies are resolved

Components in Windsor are seldom independent. After all, one of Windsor's main tasks is wiring and managing dependencies. When a component has some dependencies, Windsor goes through several steps in order to resolve them.

## Dependency Resolver

Windsor uses dependency resolver (type implementing `IDependencyResolver` interface) to resolve your component's dependencies. Default dependency resolver (`DefaultDependencyResolver` class) looks in the following places in order to resolve the dependencies.

## Creation Context

First of all dependency resolver asks `CreationContext` for the dependency. Creation context tries to resolve the dependency first by name, then by type using dependencies provided inline. That means, it comes from either of the following:

1. Arguments passed to `Resolve`: `container.Resolve<IFoo>(new Arguments(new { inlineDependency = "its value" }));`
1. Arguments passed via [`DynamicParameters`](inline-dependencies#supplying-dynamic-dependencies) method of [fluent registration API](fluent-registration-api.md).

:information_source: **Other sources:** Notice that also [Typed Factory Facility](typed-factory-facility.md) forwards the arguments passed to factory methods as inline parameters.

## Handler

When no inline argument can satisfy the dependency the resolver asks handler if it can satisfy it. Handler tries to resolve the dependency first by name, then by type. The values come from `ComponentModel`'s `CustomDependencies` collection, which usually means parameters passed to `DependsOn` method.

```csharp
kernel.Register(Component.For<ClassWithArguments>()
    .DependsOn(
        Property.ForKey<string>().Eq("typed"),
        Property.ForKey<int>().Eq(2)
    )
);
```

## Subresolvers

If previous places weren't able to resolve the dependency resolver will ask each of its [sub resolvers (`ISubDependencyResolver`)](resolvers.md) if they can provide the dependency.

## Kernel

When none of the above is able to resolve the dependency container will try to do it himself. Depending on the type of the dependency it will try to do the following:

* For parameter dependency it will inspect `ComponentModel`'s `Parameters` collection for matching dependency. These include parameters passed from XML, or via `Parameters` method on fluent API.
* For [service override dependency](registering-components-one-by-one.md#supplying-the-component-for-a-dependency-to-use-service-override), it will try to resolve component matching the key specified.
* For service dependency it will try to resolve any component matching the service specified.

If none of the above works, a `DependencyResolverException` will be thrown.

## See also

* [How components are created](how-components-are-created.md)
* [How constructor is selected](how-constructor-is-selected.md)