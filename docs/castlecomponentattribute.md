# CastleComponentAttribute

While it's usually advised to keep your code unaware of Windsor, it's sometimes desirable that components themselves specify knowledge about how they should be used by Windsor.

:information_source: **Attributes are mostly useful for extensions:** Most common usage of attributes is extensions/customizations to Windsor itself. For "normal" components prefer either explicit of convention driven wiring.

## `CastleComponentAttribute`

In addition to Lifestyle attributes that components can use to specify their preferred lifestyle, Windsor also has a more broadly applicable attribute - `CastleComponentAttribute`. Using this attribute you can specify not only lifestyle but also name and service of the component.

```csharp
[CastleComponent("GenericRepository", typeof(IRepository<>), Lifestyle = LifestyleType.Transient)]
public class Repository<T> : IRepository, IRepository<T>
{
    // some implementation
}
```

:information_source: **`CastleComponentAttribute` specifies defaults:** If you register a component using the attribute and then change some of the values in your registration code, the explicitly set value will win. In other words - the attribute specifies defaults, how the component prefers to be registered, but you can override that.

## Registering attributed components

You register attributed components much like you would register any other type.

```csharp
container.Register(AllTypes.FromThisAssembly()
    .Pick()
    .If(Component.IsCastleComponent));
```

The `Component.IsCastleComponent` is a helper filter method, that will return true for classes that have `CastleComponentAttribute` applied.