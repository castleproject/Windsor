# IGenericImplementationMatchingStrategy interface

## Open generic components

Windsor supports so called *open generic components*, that is components that are implemented by generic types were generic arguments are not supplied.

For example:

```csharp
Container.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(MyRepository<>)).LifestyleTransient());
```

This single component can then be used to satisfy dependencies on various closed versions of `IRepository<T>`:

```csharp
var customerRepository = Container.Resolve<IRepository<Customer>>();
var orderRepository = Container.Resolve<IRepository<Order>>();
```

## The problem

On occasion your repository may be implemented like this:

```csharp
public class MyRepository<T>: IRepository<T>, IRepository
{
   // implementation
}

// registration
Container.Register(Component.For(typeof(IRepository<>)).Forward<IRepository>().ImplementedBy(typeof(MyRepository<>)).LifestyleTransient());
```

In this case the open generic component exposes two services: the generic and non-generic version of repository interface. What should happen when the nen-generic one is requested?

```csharp
var repository = Container.Resolve<IRepository>();
```

What implementation should Windsor supply? Should it be `MyRepository<Customer>`? `MyRepository<Order>`? Or perhaps `MyRepository<object>`?

It is also not uncommon for the implementation type to have more generic parameters than the service it exposes. The class may look like this:

```csharp
public class MyRepository<T, TUnitOfWork>: IRepository<T>
{
   // implementation
}
```

In this case to query for `Customer`s you might want to be using `MyRepository<Customer, NHibernateUnitOfWork>`, but how should Windsor know about it, when `IRepository<Customer>` is requested?

## The solution

The answer is, it's not up to Windsor to decide, because  it simply doesn't have all the information. Windsor however provides an extension point: `Castle.MicroKernel.Handlers.IGenericImplementationMatchingStrategy` interface that you can implement to supply the missing information, so that Windsor can successfully close the open generic type. The interface has a single method:

```csharp
public interface IGenericImplementationMatchingStrategy
{
   Type[] GetGenericArguments(ComponentModel model, CreationContext context);
}
```

### Implementation

The two parameters should give you all the contextual information about the component and current resolution process to make an informed decision as to what values for generic arguments should be supplied. Going back to the example of generic and non-generic `IRepository`, we might supply implementation similar to this:

```csharp
public class RepositoryGenericCloser: IGenericImplementationMatchingStrategy
{
   public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
   {
      if (context.RequestedType == typeof(IRepository))
      {
         return new[] { typeof(object) };
      }
      return null;
   }
}
```

In other words, when the non-generic `IRepository` was requested we return `object` as the generic argument, that is Windsor will supply `MyRepository<object>` as the implementation.

Otherwise we return null, which means Windsor will fall back to its default behavior, that is close the implementation over generic arguments of the service. (use `MyRepository<Foo>` when `IRepository<Foo>` was requested).

### Registration

To instruct Windsor to use your implementation of `IGenericImplementationMatchingStrategy` use the following registration code:

```csharp
Container.Register(Component.For(typeof(IRepository<>))
   .Forward<IRepository>()
   .ImplementedBy(typeof(MyRepository<>), new RepositoryGenericCloser())
   .LifestyleTransient());
```