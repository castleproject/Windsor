# Typed Factory Facility

Typed Factory Facility provides automatically generated [Abstract Factories](http://en.wikipedia.org/wiki/Abstract_factory_pattern) that you can use to create components in your code, while still remaining agnostic to the presence of the container. It is part of `Castle.Windsor.dll`.

:information_source: **Typed factories vs alternatives:** Use typed factories wherever you need to pull something from the container. Alternative approaches, like Service Locator, or referencing the container directly are both inferior to typed factories and should be avoided. [See this post for more in-depth discussion](http://devlicio.us/blogs/krzysztof_kozmic/archive/2009/12/24/castle-typed-factory-facility-reborn.aspx).

All the following code samples will assume that the following variable is in scope

```csharp
IKernel kernel = new DefaultKernel();
```

This will obviously also work if you use `IWindsorContainer` instead

```csharp
IWindsorContainer container = new WindsorContainer();
```

## Why would I need it?

Preferable way of working with the container is to have the container inject all the required dependencies upon creation of the object. There are some scenarios where an object needs to have other objects supplied to it after it was created, for example to handle some external event. In this case the object needs to pull its dependencies. Best way to do it is to use a factory that will supply the objects as requested. Typed Factory Facility implements the factories for you (under interface you provide) and supplies instances to you from the container.

## Kinds of typed factories

Typed Factory Facility provides two distinct kinds of factories: [`interface`-based](typed-factory-facility-interface-based.md) and [`delegate`-based](typed-factory-facility-delegate-based.md). Notice that there are quite a lot of differences between how they work.

:information_source: **Prefer `interface`-based factories:** It is strongly suggested to use `interface`-based factories. They are more powerful, cover wider range of scenarios and give you ability to release your components.

## Registering the facility

To use the facility you have to register it with the container.
You need the appropriate namespace in scope:

```csharp
using Castle.Facilities.TypedFactory;
```

Then you can register the facility with the container

```csharp
kernel.AddFacility<TypedFactoryFacility>();
```

## Using typed factories

* [`interface`-based factories](typed-factory-facility-interface-based.md)
* [`delegate`-based factories](typed-factory-facility-delegate-based.md)