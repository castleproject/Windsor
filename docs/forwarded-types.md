# Forwarded Types

Forwarded types are additional services exposed by a component. For example, given the following component:

```csharp
public class FooBar : IFoo, IBar
{
    public void DoFoo()
    {
        // some implementation
    }

    public void DoBar()
    {
        // some implementation
    }
}
```

Windsor will let you expose this single component as both `IFoo` and `IBar` service.

```csharp
//register FooBar as both IFoo and IBar with singleton lifestyle

var foo = container.Resolve<IFoo>();
var bar = container.Resolve<IBar>();

Debug.Assert(object.ReferenceEquals(foo, bar));
```

This can be done either via [fluent registration API](registering-components-one-by-one.md#registering-component-with-multiple-services) or [XML configuration](registering-components.md#component-with-multiple-services-forwarded-types).