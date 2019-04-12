# Using XML configuration

Most often you use [Fluent Registration API](fluent-registration-api.md) to register and configure components. However that's not the only way and Windsor has comprehensive support for XML configuration to accomplish some of the container related tasks.

:information_source: **Where the configuration can live:** You can keep Windsor's configuration in your app.config/web.config file, custom dedicated file or distribute it across several files if you really need to. Moreover the files can live on the disk, or be embedded in the assembly if you don't want to expose them after deployment to the users.

## What to use it for

XML configuration can be used to accomplish the following goals:

* [provide configuration-time properties to your components](xml-configuration-properties.md) (things like connection strings, addresses of services to talk to, admin email address)
* [register installers](registering-installers.md)
* [register and configure components](registering-components.md) (using registration in code is recommended for doing that if possible)
* [register and configure facilities](facilities-xml-configuration.md) (using registration in code is recommended for doing that if possible)

:information_source: **Registering components in XML:** Ability to register components in XML is mostly a leftover from early days of Windsor before [Fluent Registration API](fluent-registration-api.md) was created. It is much less powerful than registration in code and many tasks can be only accomplished from code.

To make working with XML configuration easier it [can be distributed among several files](xml-configuration-includes.md) if you need to partition it.

:information_source: **XML schema:** This documentation discusses only the default elements, provided out of the box. However Windsor's schema is not rigid and various extensions, like facilities, may (and often do) provide additional elements that extend the default set.

## XML config at a glance

:information_source: This section is only focused on the format, not on the code that uses or how they can be externalized.

:information_source: **Referencing types in XML:** Windsor allows you to omit parts of assembly qualified type name when referencing types in XML. Read [Referencing types in XML](referencing-types-in-xml.md) for details.

The reference below presents all nodes and attributes that the container uses by default. The section above contains links leading to pages exploring them in greater details.

```xml
<configuration>
  <!--lets you reference types from that assembly by specifying just their name, instead of assembly qualified full name.-->
  <using assembly="Acme.Crm.Services, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1987352536523" />

  <include uri="file://Configurations/services.xml" />
  <include uri="assembly://Acme.Crm.Data/Configuration/DataConfiguration.xml" />

  <installers>
    <install type="Acme.Crm.Infrastructure.ServicesInstaller, Acme.Crm.Infrastructure"/>
    <install assembly="Acme.Crm.Infrastructure"/>
  </installers>

  <properties>
    <connection_string>value here</connection_string>
  </properties>

  <facilities>
    <facility type="Acme.Common.Windsor.AcmeFacility, Acme.Common" />
  </facilities>

  <components>
    <component
      id="uniqueId"
      service="Acme.Crm.Services.INotificationService, Acme.Crm"
      type="Acme.Crm.Services.EmailNotificationService, Acme.Crm"
      inspectionBehavior="all|declaredonly|none"
      lifestyle="singleton|thread|transient|pooled|scoped|bound|custom"
      customLifestyleType="type that implements ILifestyleManager"
      componentActivatorType="type that implements IComponentActivator"
      initialPoolSize="2" maxPoolSize="6">

      <forwardedTypes>
        <add service="Acme.Crm.Services.IEmailSender, Acme.Crm" />
      </forwardedTypes>

      <additionalInterfaces>
        <add interface="Acme.Crm.Services.IMetadataService, Acme.Crm" />
      </additionalInterfaces>

      <parameters>
        <paramtername>value</paramtername>
        <otherparameter>#{connection_string}</otherparameter>
      </parameters>

      <interceptors selector="${interceptorsSelector.id}" hook="${generationHook.id}">
        <interceptor>${interceptor.id}</interceptor>
      </interceptors>

      <mixins>
        <mixin>${mixin.id}</mixin>
      </mixins>

    </component>
  </components>
</configuration>
```

## Loading XML configuration

There are two ways to install XML configuration into the container:

### Using static `Configuration` class

You can install configuration from XML just like any other installer via `Configuration` class ([read more](installers.md#configuration-class))

### Using constructor

Using `WindsorContainer`'s constructor:

```csharp
public WindsorContainer(IConfigurationInterpreter interpreter)
```

you can pass reference to XML configuration file right when the container is created.

```csharp
IResource resource = new AssemblyResource("assembly://Acme.Crm.Data/Configuration/services.xml");
container = new WindsorContainer(new XmlInterpreter(resource));
```

In this case XML file embedded in `Acme.Crm.Data` assembly will be used.

You can also use parameterless constructor of `XmlInterpreter` in which case `AppDomain` config file will be used as source of configuration:

```csharp
container = new WindsorContainer(new XmlInterpreter());
```

:information_source: **Prefer using `Configuration` class:** It is advised to use the other approach, mentioned above. It's not only more flexible, but also future versions of Windsor may be optimized for that usage.
