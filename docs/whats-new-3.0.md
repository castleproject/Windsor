# What's new in Windsor 3.0

## Introduction

Windsor 3 released in late 2011 is a major release, and contains many new features. Below is a (non-comprehensive) list of those new features and improvements introduced in the framework.

{s:info|The list is not comprehensive|Please note that the list is far from comprehensive. Windsor 3 contains many changes and improvements, many of which are too minor to mention them here. Also this page is a work in progress and so far describes only a small subset of intended features. Moreover, list of breaking changes is available in `BreakingChanges.txt` file distributed along with Windsor}.

## Tooling

### Improved and extended debugger views

Debugger views introduced in Windsor 2.5 have been extended to contain new diagnostics/reports to make it even easier to find important information and detect common problems. Also existing debugger views were improved so that important informations are more readable and accessible.

* [Blog post detailing new Service locator usage detection diagnostic](http://kozmic.pl/2011/04/24/whats-new-in-windsor-3-service-locator-usage-detection/)
* [Blog post detailing improvements in debugger views](http://kozmic.pl/2011/04/25/whats-new-in-windsor-3-container-debugger-diagnostics-improvements/)

## Features

### New facility: WCF integration

Although [WCF-Integration-Facility|WCF Integration Facility] has been in development (and used in production by some people) for quite some time, this is the first official release of the facility.

### Added two new lifestyles: scoped and bound

Windsor 3 comes with rebuilt lifestyle management system. This makes it significantly easier to add new lifetyles, and two new lifestyles are provided out of the box:

* Scoped lifestyle - allows objects to be reused within any arbitrarily selected logical scope. The default scope is denoted by `BeginScope` extension method, but any custom scopes can be used by implementing new `IScopeAccessor` interface.

```csharp
using Castle.MicroKernel.Lifestyle;
// ...
Container.Register(Component.For<A>().LifestyleScoped());

using (Container.BeginScope())
{
	var a1 = Container.Resolve<A>();
	var a2 = Container.Resolve<A>();
	Assert.AreSame(a1, a2);
}
// When the scope is disposed, all resolved components with scoped lifestyle are automatically released. In other words, there's no need to call
// Container.Release(a1);
// Container.Release(a2);
```

* Bound lifestyle - allows objects to be reused within dependency subtree. In other words, if 'A' is scoped by 'B', then 'B' and all its dependencies will get the same instance of 'A'.

```csharp
Container.Register(
	Component.For<A>().LifestyleBoundTo<CBA>(),
	Component.For<B>().LifeStyle.Transient,
	Component.For<CBA>().LifeStyle.Transient);

var cba = Container.Resolve<CBA>();
Assert.AreSame(cba.A, cba.B.A);
```

:information_source: **Not for lazy and factories:** Notice that if you're using `Lazy<ISomething>` or a typed factory, getting value of Lazy, or resolving objects from a typed factory does not bind the resolved objects to the parent scope.

### Added support for `Lazy<T>` components

It works in a similar manner to delegate-based typed factories (`Func<T>`, allowing you to register component for service `ISomeService` and then take dependency on `Lazy<ISomeService>`. Windsor will then lazily resolve the instance of the component, the first time you access `lazy.Value` property).

In order to activate the feature you need to register `LazyOfTComponentLoader` in the container.

```csharp
container.Register(
   Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>(),
   Component.For<ISomeService>().ImplementedBy<ServiceImpl>().LifestyleTransient()
   );

var lazy = container.Resolve<Lazy<ISomeService>>();

lazy.Value.DoSomething();
```

### Introduced `IHandlerFilter` interface

The interface is similar to `IHandlerSelector` but allow you to filter/sort handlers requested by `container.ResolveAll()` method (see [issue IOC-268](http://issues.castleproject.org/issue/IOC-268)).

### Introduced `IGenericImplementationMatchingStrategy` interface

The interface can be used by generic handlers to correctly determine generic arguments for closed implementation type. This can be especially useful if implementation type has more generic arguments than the service requested.

```csharp
// sample types
public interface IScreen<TViewModel> { /* ... */}

public class Screen<TViewModel,TView>: IScreen<TViewModel> { /* ... */}

// registration
container.Register(
   Component.For(typeof(IScreen<>)).ImplementedBy(typeof(Screen<, >), new ScreeViewModelViewGenerics())
);

// usage
var screen = container.Resolve<IScreen<HomeViewModel>>();
screen.Display();

// interface implementation
public class ScreeViewModelViewGenerics : IGenericImplementationMatchingStrategy
{
   public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
   {
      // when IScreen<HomeViewModel> is requested returns generic arguments: HomeViewModel, HomeView
      var viewModel = context.RequestedType.GetGenericArguments().Single();
      var view = Type.GetType(viewModel.FullName.Replace("ViewModel", "View"));

      return new[] { viewModel, view };
   }
}
```

See [the documentation](igenericimplementationmatchingstrategy.md) for more details and examples.

### Component can now "force" being default for its services, without needing to be the first one registered

```csharp
Container.Register(
    Classes.FromThisAssembly()
        .BasedOn<IEmptyService>()
        .WithService.Base()
        .ConfigureFor<EmptyServiceA>(c => c.IsDefault()));
var obj = Container.Resolve<IEmptyService>();
```

### No need to register `PerWebRequestLifestyleModule` via config file anymore

Adding stuff to web.config is now optional provided `Microsoft.Web.Infrastructure.dll` is installed on the computer. The assembly comes as part of ASP.NET MVC 3 or newer and some other tools like WebMatrix.

### Added ability to scan all assemblies in an application for installers

```csharp
container.Install(FromAssembly.InThisApplication())
```

If your application consists of the following assemblies:

`MyApp.exe`, `MyApp.Core.dll`, `log4net.dll`, `Castle.Windsor.dll`, `ThirdPartyCompany.PureMagic.dll` only the first ones will be scanned. The decision is based on the first element of the assembly name. So assuming the assembly calling the aforementioned method is `MyApp.exe`, Windsor will scan all assemblies that it references with names having `MyApp` as first part of their name and run installers from those asssemblies. As with other methods on `FromAssembly` class, you can provide an `InstallerFactory` to further restrict/inspect the assemblies.

## Notable additions/changes to registration API

### Added `OnDestroy` method

The method is analogous to `OnCreate`, and allows you to specify ad-hoc decommission concerns (code that will run when instance is released.

```csharp
container.Register(Component.For<MyClass>()
   .LifestyleTransient()
   .OnDestroy(myInstance => myInstance.ByeBye())
);
```

:warning: **Instance tracking and `OnDestroy`:** Notice that in order to decommission the object, Windsor will need to track it. Be mindful of that when managing usage of your component instances and make sure they get released when no longer needed.

### Filtering of property dependencies

By default Windsor will expose settable properties on your component's implementation type as non-mandatory dependencies. If you want to make them mandatory, or not expose them - now you can control that from the registration API.

```csharp
Container.Register(
    Component.For<MyType>().Properties(PropertyFilter.IgnoreAll));
);
```

Code above will register `MyType` but will not expose any of its properties.

The `PropertyFilter` enum has value to cover most common cases

```csharp
Container.Register(
    Component.For<MyType>().Properties(PropertyFilter.RequireBase));
);
```

Code above will expose `MyType`s properties, but will make properties defined by its base class(es) mandatory.

There's also an overload of `Properties` method which takes a custom predicate for scenarios not covered by the `PropertyFilter`.

```csharp
Container.Register(
    Component.For<MyType>().Properties(p => p.PropertyType == typeof(IBus), isRequired: true));
);
```

Code above will only expose properties of type `IBus` and make them mandatory.

### Added `Dependency` class

The class in conjunction with `DependsOn()` methods enables some new capabilities (and simplifies some other scenarios). For example you can now easily provide dependency value from appSettings section of the .config file

```csharp
Container.Register(
   Component.For<ClassWithArguments>()
      .DependsOn(
         Dependency.OnAppSettingsValue("arg1"),
         Dependency.OnAppSettingsValue("arg2", "number"))
);

var instance = Container.Resolve<ClassWithArguments>();
```

### Added `Classes` and `Types` registration API entry types, for batch registration of components

`Classes` is an alias (has the same API and behavior) to `AllTypes`. However it is clearer in what types it registers (just non-abstract classes rather than truly *all* types).

The `Types` class is the true *all types* as it does not pre-filter the types, so you can use it, for example in cases where you want to register interfaces with no implementation, like in case of interface-based typed factories.

```csharp
container.Register(
   Types.FromThisAssembly()
   .Where(Component.IsInNamespace("Foo.Factories"))
      .Configure(t => t.AsFactory())
   );

var factory = Container.Resolve<IFooFactory>();
factory.GiveMeFoo();
```

## Improvements

### Abandoned concept of forwarding handlers

Components (and handlers) supporting multiple services are now first class citizens in Windsor world. Notice that is a breaking change that affects multiple areas of the codebase (see [issue IOC-247](http://issues.castleproject.org/issue/IOC-247)).

### Improved exception messages

Many exception messages thrown by Windsor are now more descriptive, contain more useful information and suggest action to fix the problem.

### Some improvements in XML support

The id attribute is now optional for facilities and components (see [issue IOC-256](http://issues.castleproject.org/issue/IOC-256), [issue IOC-257](http://issues.castleproject.org/issue/IOC-257)). Custom type for lifestyle is now also enough (see [issue IOC-255](http://issues.castleproject.org/issue/IOC-255)).