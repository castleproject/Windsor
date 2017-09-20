# Resolvers

Windsor gives you ability to override/extend its default resolution strategy for components/dependencies by using sub dependency resolvers.

## How does it work

When resolving components Windsor will ask all `ISubDependencyResolver`s registered with its `IDependencyResolver` if they would like to provide the component themselves.

:information_source: See [How dependencies are resolved](how-dependencies-are-resolved.md) for more details.

### The `ISubDependencyResolver` interface

Resolvers are types implementing `ISubDependencyResolver` interface, which exposes two methods:

```csharp
bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency);
object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency);
```

First `CanResolve` is called to see if the resolver would like to provide given component, then if it returns `true`, `Resolve` will be called to let the resolver provide the value.

To plug your own custom resolver you need to add it to kernel's `IDependencyResolver`'s collection of sub-resolvers:

```csharp
container.Kernel.Resolver.AddSubResolver(new MyOwnResolver());
```

## Standard resolvers

Windsor provides a few resolvers that you can plug into the container out of the box.

### `CollectionResolver`

:information_source: This is a new type in Windsor 2.5

The `CollectionResolver` is used to satisfy dependency on a collection of components, for example:

:information_source: **Supported types:** There are many collection types that `CollectionResolver` supports. Namely `IEnumerable<T>`, `ICollection<T>`, `IList<T>` and `T[]`. This makes it the preferred choice over the other resolver types discussed below

```csharp
public class Foo
{
    public Foo(IEnumerable<IBar> bars)
    {
        // something
    }
}
```

It has the same constructors as array resolver discussed below.

### `ArrayResolver`

The `ArrayResolver` is used to satisfy dependency on an array of components, for example:

```csharp
public class Foo
{
    public Foo(IBar[] bars)
    {
    }
}
```

Without using `ArrayResolver` Windsor will try to locate a component for `IBar[]`, whereas what you would most likely have is multiple components for `IBar`. If you want to inject all the `IBar` components as the array, you use `ArrayResolver`. The resolver will resolve all `IBar` components it can, and then construct an array, put them in the array and provide the array as the value for the dependency.

#### `ArrayResolver` constructors

The `ArrayResolver` type exposes two constructors:

```csharp
public ArrayResolver(IKernel kernel);
public ArrayResolver(IKernel kernel, bool allowEmptyArray);
```

When you call the first one, or the second one with `allowEmptyArray` set to `false`, and no instance of `IBar` can be resolved the resolver will throw an exception. If you set `allowEmptyArray` to `true` an empty array will be provided instead.

You simply register it with the container, passing the kernel as argument to its constructor:

```csharp
container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel, true));
```

### `ListResolver`

The `ListResolver` is used to satisfy dependency on a generic list of components, for example:

```csharp
public class Foo
{
    public Foo(IList<IBar> bars)
    {
    }
}
```

It works identically to `ArrayResolver`.
