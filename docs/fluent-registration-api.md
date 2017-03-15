# Fluent Registration API

The Registration API provides a fluent method of registering and configuring components. It is the recommended way of doing so, over meticulous registration in XML. It can be used as an alternative or in addition to XML configuration, with the advantage that it is typed and will be easier to include in refactoring. It may also be advantageous to mix it with some XML.

## Requirements

`Castle.Core.dll` and `Castle.Windsor.dll` are both required. You need to add a using directive to `Castle.MicroKernel.Registration` in order to use this API. The examples which follow assume you have created the Windsor Container thusly:

```csharp
IWindsorContainer container = new WindsorContainer();
```

We will use the container instance throughout the API documentation.

## Registering components one-by-one

Fluent API has ability to register components on a one-by-one basis, where you can explicitly configure all aspects of the component. ([read more](registering-components-one-by-one.md))

## Registering components by convention

Recommended approach to registering components is to do it using convention driven approach, which can significantly reduce friction and amount of code required to configure the application. ([read more](registering-components-by-conventions.md))

## Proxies

[Registering interceptors and proxy options](registering-interceptors-and-proxyoptions.md).

## Advanced topics

If you're interested in advanced topics, like pieces of the API targeted at extending the components read [advanced topics](fluent-registration-api-extensions.md).

## Using with XML Configuration

If you need to mix both styles of registration there are two options.  If the Xml configuration can happen first, simply use the constructor overload that takes an Xml file name before using the fluent api for the rest.

```csharp
IWindsorContainer container = new WindsorContainer("dependencies.config");

container.Register(
	Component.For....
);
```

If the fluent registrations must happen first, add the following after all of your fluent calls.

```csharp
container.Register(
	Component.For....
);

container.Install(
    Configuration.FromXmlFile("dependencies.config"));
```

## See also

* [Registering components one-by-one](registering-components-one-by-one.md)
* [Registering components by conventions](registering-components-by-conventions.md)
* [Windsor Installers](installers.md)
* [XML configuration reference](xml-registration-reference.md)
