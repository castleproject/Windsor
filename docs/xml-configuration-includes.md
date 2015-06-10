# XML configuration includes

You don't have to keep all configuration in a single file. Includes allow you to refer to configuration in other places.

All you need to do is specify an include node with the Uri that will be used to create the proper `Resource`. For example, the following will use the `FileResource`:

```xml
<include uri="file://c:\mydir\services.xml">
```

The file is relative to the configuration file that has the include:

```xml
<include uri="file://Configurations/facilities.xml">
<include uri="file://Configurations/services.xml">
```

The next one uses an assembly resource:

```xml
<include uri="assembly://Castle.Windsor.Tests/Configuration2/include1.xml">
```

The next one will use an section inside the configuration associated with the container.

```xml
<include uri="config://castle">
```

Note that you must include the section declaration, like the following:

```xml
<configSections>
    <section
        name="castle"
        type="Castle.Windsor.Configuration.AppDomain.CastleSectionHandler, Castle.Windsor" />
</configSections>
```

The next one will use a file in a network share

```xml
<include uri="\\mysharedplace\myconfig.xml">
```

You can use multiple includes. Just have in mind that the id cannot repeat. If it does, the last registered id for the facility or for the component will be used and the previous will be discarded without warning or exceptions being thrown.

Any properties that are referenced within an include file must be above the include element like so:

```xml
<properties>
    <Configuration.TimeoutDuration>150</Configuration.TimeoutDuration>
</properties>
<include uri="file://include_file_using_timeoutduration.xml">
```

If you reverse the order and put the include node above any properties that it references, you will get the following exception:

```
failed: Castle.Windsor.Configuration.Interpreters.XmlProcessor.ConfigurationProcessingException : Error processing node configuration, inner content ...
        Castle.Windsor.Configuration.Interpreters.XmlProcessor.XmlProcessorException : Required configuration property Configuration.TimeoutDuration not found
```