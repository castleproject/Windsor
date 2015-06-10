# What's new in Windsor 3.1

Windsor 3.1 is a minor update over version 3.0. It does contain a set of new features and fixes that make it a worthwhile update. Below is a (non-comprehensive) list of highlights of the release.

:information_source: For more details see `changes.txt` and `breakingchanges.txt` files distributed as part of the package)

## Selective open generic components

In version 3.1 a new extension point has been added which allows open generic components to be selective about which closed versions they want to support. Windsor uses this now internally for `Lazy<T>;` dependencies and for `Func<T>` dependencies. Windsor will now opt out of satisfying `Lazy<int>` or `Func<string>` (since there can never be a `string` or `int` component in Windsor.

The new extension point is exposed via `IGenericServiceStrategy` interface, and can be associated with a component using registration API when registering open generic component.

```csharp
Container.Register(Component.For(typeof(IMyGenericService<>)).ImplementedBy(typeof(MyImplementation<>), new MyServiceStrategy()));
```

The interface has very simple contract with a single method:

```csharp
bool Supports(Type service, ComponentModel component);
```

The method should return `true` if the `component` can and wants to be used to satisfy the `service` (which is guaranteed to be a closed version of one of open generic services of this component).

## Fallback components

The default behavior in Windsor is that whichever components for a given service gets registered first will be the default one. There are cases however, especially in extensibility scenarios where (for example) a facility integrating Windsor with some other tool wants to register some components but allow you to override them so that your implementation is used.

There was no easy way of achieving that in older versions of Windsor (other than making sure you register your implementation before adding the facility, which was... not pretty).
In version 3.1 you can now explicitly mark a component as fallback (for all, or selected services it exposes) so that it will be only picked unless other, non-fallback, component for those services is registered and available.

```csharp
Container.Register(Component.For<IFoo>().ImplementedBy<FallbackFoo>().IsFallback(),
                   Component.For<IFoo>().ImplementedBy<MyBetterFoo>());
// ...
var foo = Container.Resolve<IFoo>(); // will resolve MyBetterFoo
```

## Disabling signed module in proxy generator

IT is now a lot simpler to ensure all proxy types are generated into an unsigned module. If you want to disable signed module replace the default proxy factory with the following.

```csharp
Container.Kernel.ProxyFactory = new DefaultProxyFactory(disableSignedModule: true);
```

## Changes to `InterceptorAttribute`

It is now possible to associate multiple interceptors with a type via attributes:

```csharp
[Interceptor(typeof(InterceptorOne))]
[Interceptor(typeof(InterceptorTwo))]
public class MyClass
{
   // some implementation
}
```

The attribute is now allowed to be placed on other elements than just classes, like methods or properties. Windsor still only looks for the attribute on classes, but it is possible to extend it.

## Enhancements to logging facility

### Global logger name

It is now possible to specify a global root log name (works with extended loggers only). All logs coming though Windsor will appear under this root name.

```csharp
Container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.ExtendedLog4net).ToLog("MyLogName"));
```

### Opting out of logging framework configuration for log4net and NLog

In complex applications using multiple frameworks other than Windsor you may find those other frameworks want to take over configuration of your logging framework (be it log4net or NLog). Since both NLog and log4net store their configuration in a static property this may lead to some issues.

For those cases to avoid frameworks competing for ownership of logging configuration Windsor can now opt out of configuring the logging framework using the following code:

```csharp
Container.AddFacility<LoggingFacility>(f => f.UseLog4Net().ConfiguredExternally());
```

Calling `ConfiguredExternally()` is telling Windsor *"Do use log4net (or NLog) but do not attempt to configure it. It will be configured somewhere else"*.

### Specifying custom NLog configuration file

A new overload to `UseNLog` has been added to allow you to specify custom configuration file (other than the default called `nlog.config` in a single call.

```csharp
Container.AddFacility<LoggingFacility>(f => f.UseNLog("someOtherFile.xml"));
```

Notice that similar overload exists for log4Net since Windsor 3.0.

## See also

* [What's new in Windsor 3.0](whats-new-3.0..md)