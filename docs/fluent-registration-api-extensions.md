# Fluent Registration API Extensions

## Fluent API - advanced topics

If you've read (and understood) all of the usage part of [Fluent Registration API](fluent-registration-api.md), you already know everything you need to know, to use Castle Windsor container. If you've explored the API (with Intellisense or Reflector) You'll notice that there are a couple of public methods, we haven't discussed yet. They are geared towards really advanced scenarios, and mostly used by facilities and other extensions to the container rather than by end users. As such feel free to skip this part, because it is very unlikely you will ever use any piece of API discussed here.

:information_source: It is quite likely that the API discussed herein will be split from the client API and moved someplace else, to be explicitly targeted towards extension scenarios.

### Component Descriptors / Attribute descriptors

Configuration API exposes `AddDescriptor` method that you can use to plug into `ComponenModel` creation lifetime. Objects passed to this method are then later invoked, having chance to modify created `ComponentModel`, and/or its `IConfiguration` object. In addition `AddAttributeDescriptor` and `Attribute` methods are exposed, which are equivalent, and are just a syntactic sugar over `AddDescriptor`, covering single, but quite common case - inserting key/value pair to component `IConfiguration`'s `Attributes` collection. Descriptors are the main way used internally by container's fluent API, as well as most facilities to configure components.

### Custom Component Activator

In Windsor components are instantiated by `IComponentActivator` objects. It is possible to supply your own to create your component using generic `Activator` method.

```csharp
Kernel.Register(
    Component.For<ICustomer>()
        .Named("customer")
        .ImplementedBy<CustomerImpl>()
        .Activator<MyCustomerActivator>()
);
```

:information_source: **Activators are for instantiation:** Notice that activator should be used only for creation of new instances of your objects. To decide when to create a new object, use `ILifestyleManager`s. Also some methods, like `Instance`, or some facilities you may use (FactorySupportFacility for example) set their own activators to perform the job. Keep that in mind, and do not use this method when using other activation mechanisms. Also take a look at `OverWrite` method.

### Who wins?

What happens when two or more descriptors want to set the same attribute to different values? Which one will win? - The answer is - it depends. By default, as everywhere in Windsor - the first one wins. However you can turn it over, by enabling overwriting of already set values, by calling `OverWrite` method.

:information_source: Currently this behavior is only exposed by `LifestyleDescriptor` (which sets the lifestyle manager) and attribute descriptor. Also be very careful when using this method. You should strive to not have to use it at all. It is very likely that it will be removed in the future releases.

### Passing chunk of configuration

If you would like to pass an entire, bigger chunk of configuration all at once, you can use `Configuration` method of the Fluent API to do that. Generally doing it manually should be avoided, as it gets big and scary pretty quickly (here's real example from EventWiringFacility)

```csharp
container.Register(
    Component.For<MessagePublisher>()
        .Configuration(
            Child.ForName("subscribers").Eq(
                Child.ForName("subscriber").Eq(
                    Attrib.ForName("id").Eq("messageListener"),
                    Attrib.ForName("event").Eq("MessagePublished"),
                    Attrib.ForName("handler").Eq("OnMessagePublished")
                )
            )
        )
);
```

### Extended properties

Non-configuration values, used by extensions are kept in extended properties, which is a flat bag of metadata attached to `ComponentModel`. It is generally not intended to be used by user code.

```csharp
container.Register(
    Component.For<ICustomer>()
        .ImplementedBy<CustomerImpl>()
        .ExtendedProperties(
            Property.ForKey("key1").Eq("value1"),
            Property.ForKey("key2").Eq(2)
        )
);
```

### What does ActAs do?

ActAs is one of the most confusing methods in the API. Most notably it is used in [Windsor WCF integration facility](wcf-facility.md):

```csharp
container.Register(
    Component.For<IServiceWithSession>()
        .ImplementedBy<ServiceWithSession>().LifeStyle.Transient
        .Named("Operations")
        .ActAs(new DefaultServiceModel().AddEndpoints(
            WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
                .At("net.tcp://localhost/Operations")
        ))
);
```

Semantically it is identical to `DependsOn` with only difference being, we don't specify the name of the dependency. In the above code we define WCF service implementation, which depends on `DefaultServiceModel` containing all the information WCF needs to run that service, but we don't care about the name of that dependency since it's not actual dependency of the `ServiceWithSession` object per se.

:warning: **Warning:** It is very likely that it will be removed in the future releases.

## See also

* [Fluent Registration API](fluent-registration-api.md)