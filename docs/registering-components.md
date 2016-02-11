# Registering components in XML

Components are defined by tag `<component/>` within `<components/>` section.

```xml
<components>
  <component ...attributes>
    ...elements
  </component>
</components>
```

## Simple components

To register a component you need to specify at least two things - id of the component, and its type)

```xml
<components>
  <component
      id="notification"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm">
  </component>
</components>
```

:information_source: In Windsor 3 `id` attribute is optional, so you need to only specify `type`.

This is identical to specifying `Component.For<EmailNotificationService>().Named("notification");` with [Fluent Registration API](fluent-registration-api.md).

### Component with abstraction

More often you will want the component type to be an abstract base type, or interface, implemented by the type you're exposing

```xml
<components>
  <component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm">
  </component>
</components>
```

This is identical to specifying `Component.For<INotificationService>().ImplementedBy<EmailNotificationService>().Named("notification");`.

### Component with lifestyle

It is possible to specify a lifestyle via the XML configuration

```xml
<components>
  <component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm"
      lifestyle="transient">
  </component>
</components>
```

The list of valid values for the `lifestyle` attribute (mapped to appropriate [`LifestyleManager`](lifestyles.md) enum values):

Lifestyle | Notes
--------- | -----
singleton | This is the default lifestyle in Windsor
transient | [Windsor keeps references to transient components](release-policy.md)!
pooled | For pooled lifestyle two additional attributes need to be defined: `initialPoolSize` and `maxPoolSize` both of which accept positive integer values
thread |
custom | For custom lifestyle additional attribute needs to be defined: `customLifestyleType` which points to the type implementing the lifestyle

### Component with parameters

It is possible to specify parameters via XML configuration.

```xml
<components>
  <component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm"
      lifestyle="transient">
    <parameters>
      <smtpServer>localhost:667</smtpServer>
      <senderEmail>#{admin.email}</senderEmail>
      <emailFormatter>${emailFormatter}</emailFormatter>
    </parameters>
  </component>
</components>
```

As shown in the example parameters can be defined in three distinct ways:

#### Inline parameters

In the above example `smtpServer` is an inline parameter: its value - localhost:667, is provided right on the spot. Windsor's XML format lets you specify not just primitive types, but also lists, arrays, dictionaries or any other complex types. ([read more](xml-inline-parameters.md))

#### Property references

In the above example `senderEmail` is a property reference parameter: its value is defined in the `<properties>` section of the configuration file (not shown here) under name `admin.email`. You specify property references using `#{property.id}` notation.

#### Service overrides (other service as parameter)

In the above example `emailFormatter` is a service override parameter: as its value another component, registered with id equals `emailFormatter` will be used. You specify service overrides using `${service.id}` notation.

#### Component with multiple services (forwarded types)

Single component can be used as multiple services. This feature, called [forwarded types](forwarded-types.md) in Windsor, is also exposed via XML.

```xml
<components>
  <component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm">
    <forwardedTypes>
      <add service="Acme.Crm.Services.IEmailSender, Acme.Crm" />
    </forwardedTypes>
  </component>
</components>
```

#### Setting properties

By default when Windsor creates component it will try to set all its settable properties it can provide value for. When using XML configuration this behavior can be adjusted.

```xml
<component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm"
      inspectionBehavior="none">
</component>
```

Inspection behavior | Notes
------------------- | -----
all | This is the default behavior
none | Windsor will not try to set any properties
declaredonly | Only properties declared on the component's type (and not ones declared on base types) will be set

### Proxy components

All power of underlying DynamicProxy library is exposed via XML configuration.

:information_source: **Read more:** This guide only discusses the XML syntax. To learn more about how DynamicProxy works and how and where it can be helpful, [read this tutorial](http://kozmic.pl/archive/2009/04/27/castle-dynamic-proxy-tutorial.aspx).

#### Interceptors

Registering interceptors is very similar to service overrides:

```xml
<component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm">
    <interceptors>
      <interceptor>${loggingInterceptor}</interceptor>
    </interceptors>
</component>
```

You reference interceptors via key, using standard reference notation.

#### Interceptors selector

You reference interceptor selector using `selector` attribute on `<interceptors>` element:

```xml
<component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm">
    <interceptors selector="${interceptorsSelectorId}">
      <interceptor>${loggingInterceptor}</interceptor>
    </interceptors>
  </component>
```

#### Proxy hook

You reference proxy hook using `hook` attribute on `<interceptors>` element:

```xml
<component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm">
  <interceptors hook="${proxyHookId}">
    <interceptor>${loggingInterceptor}</interceptor>
  </interceptors>
</component>
```

#### Mixins

Windsor lets you mix your service with additional services into one object:

```xml
<component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm">
    <mixins>
      <mixin>${otherServiceId}</mixin>
    </mixins>
</component>
```

You reference other services you want to mix in using Windsor's standard reference notation.

#### Additional interfaces

Windsor lets you specify additional interfaces to be implemented by your proxy. Implementation for these interfaces should be provided by interceptors:

```xml
<component
      id="notification"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm">
    <interceptors>
      <interceptor>${metadataInterceptor}</interceptor>
    </interceptors>
    <additionalInterfaces>
      <add interface="Acme.Crm.Services.IMetadataService, Acme.Crm" />
    </additionalInterfaces>
</component>
```
