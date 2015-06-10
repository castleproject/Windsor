# Conditional component registration

In cases when you want to register a component only if (or unless) certain conditions are met, there are appropriate methods exposed in the API.

## Registering component only if it was not registered previously

```csharp
container.Register(
    Component.For(typeof(IRepository<>))
        .ImplementedBy(typeof(Repository<>))
        .OnlyNewServices()
);
```

Here we register component only if no component for `IRepository<>` was previously registered.

## Filtering components in multiple components registration

Corresponding methods are exposed also when registering multiple components:

```csharp
container.Register(
    Classes.Of<ICustomer>()
        .FromAssembly(Assembly.GetExecutingAssembly())
        .Unless(t => typeof(SpecificCustomer).IsAssignableFrom(t))
);
```

Here we register all types implementing ICustomer, except for SpecificCustomer and its descendants.

```csharp
container.Register(
   AllTypes.Of<ICustomer>()
      .FromAssembly(Assembly.GetExecutingAssembly())
      .If(t => t.FullName.Contains("Chain"))
);
```

For better readability multiple service registration exposes additional filtering methods

```
container.Register(
    AllTypes.FromAssembly(Assembly.GetExecutingAssembly())
        .Where(Component.IsInSameNamespaceAs<FooRepository>())
        .WithService.FirstInterface()
);
```

You can also use Linq to do the filtering

```csharp
container.Register(
    Classes.Of<CustomerChain1>()
        .Pick(from type in Assembly.GetExecutingAssembly().GetExportedTypes()
              where type.IsDefined(typeof(SerializableAttribute), true)
              select type
        )
);
```

:information_source: **Convention over Configuration:** Notice what the above gives up - by virtue of putting a class in a specific namespace, inheriting from specific interface or having a specific attribute our components get registered and made available as services. We defined something very important here - conventions that free us from coding everything explicitly. This can't be overemphasized.

## See also

[Fluent Registration API](fluent-registration-api.md)