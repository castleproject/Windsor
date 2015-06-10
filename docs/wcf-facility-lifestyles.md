# WCF Facility - Lifestyles

WCF Facility contains two contextual lifestyles

## `PerWcfSession` lifestyle

If you're using sessionful binding you may want some components to live for the scope of the session. For that you use `PerWcfSession` lifestyle.

You attach the lifestyles using extension method, so you first need to add the following using statement:

```csharp
using Castle.Facilities.WcfIntegration;
```

Then you can register your component with the lifestyle:

```csharp
container.Register(
   Component.For<IOne>().ImplementedBy<One>().LifeStyle.PerWcfSession());
```

Now the instance of the component will be shared across the lifetime of the WCF session.

## `PerWcfOperation` lifestyle

If you want to scope component per operation, you use `PerWcfOperation` lifestyle. It works pretty much like `PerWcfSession` lifestyle.

```csharp
container.Register(
   Component.For<IOne>().ImplementedBy<One>().LifeStyle.PerWcfOperation());
```

:information_source: **Fallback behavior:** When the component with either of the above lifestyles is being resolved not within Wcf session or Wcf operation, an exception will be thrown.