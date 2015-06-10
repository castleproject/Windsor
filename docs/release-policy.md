# Release Policy

To employ proper [lifecycle](lifecycle.md) management for its components Windsor uses release policy which is tasked with keeping track of components created by Windsor and releasing them when needed.

Its contract is defined by `IReleasePolicy` interface and can be accessed (or changed) via `ReleasePolicy` property of `IKernel`

```csharp
var policy = container.Kernel.ReleasePolicy;
container.Kernel.ReleasePolicy = someOtherPolicy;
```

:warning: **Don't change release policy of working container:** While Windsor allows you to change its release policy, you should never do it after some components were already resolved. If you do, Windsor won't be able to release them properly anymore. If you're changing the policy, do it as one of very first operations on the container, before resolving any components.

## Default policy

By default Windsor will use `LifecycledComponentsReleasePolicy` which keeps track of all components that were created, and upon releasing them, invokes all their decommission lifecycle steps.

:information_source: **Always release components:** When release policy tracks your components Garbage Collector is not able to reclaim them. That's why it's crucial that you always release your components you resolve (either via call to `Resolve`/`ResolveAll` or via a [typed factory](typed-factory-facility.md)), especially ones that don't get released automatically. In particular this statement is true for [transient](lifestyles.md#transient) components, since unless you release them, all their instances will live on until you dispose the container.

## `NoTrackingReleasePolicy`

In cases when you don't want Windsor to track your components, you can resort to `NoTrackingReleasePolicy`. It never tracks the components created, opting out of performing proper component decommission. Its usage is generally discouraged and targeted at limited scenarios of integration with legacy systems or external frameworks that don't allow you to properly release the components.

## See also

* [Lifecycle](lifecycle.md)

## External resources

* [(Feb 19, 2010) Blog post by Davy Brion discussing hybrid release policy for integration with external framework](http://davybrion.com/blog/2010/02/avoiding-memory-leaks-with-nservicebus-and-your-own-castle-windsor-instance/)