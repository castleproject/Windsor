# Lazy Component Loaders

The preferred way of registering components in Windsor is to do it upfront via [Windsor Installers](installers.md). Windsor exposes another way - registering the components on the spot, right when it's first requested. This feature is targeted mostly at integration with external frameworks - like WCF.

Lazy component loaders are types implementing `ILazyComponentLoader` interface. It exposes just single method:

```csharp
IRegistration Load(string key, Type service)
```

Implementation should either return `null` which means it's not interested in loading the service, or service for given `key` and/or `type`.

## Example

For example let's assume you have set up a convention that default implementation of your interfaces would be a class called `Default`interfaceName living in the same namespace. So for an interface `Acme.Services.IFoo` the default implementation would be called `Acme.Services.DefaultFoo`. Now if you wanted to lazily register these implementation you could create a loader that could look something like the following:

## External resources

* [(Nov 15, 2009) Blogpost by Krzysztof Koźmic](http://kozmic.pl/archive/2009/11/15/castle-windsor-lazy-loading.aspx)