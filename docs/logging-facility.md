# Logging Facility

The logging facility provides a seamless way to add logging capabilities to your application. There are two levels of integration.

* Allow your classes to receive an `ILogger` instance for logging support
* Allow you to ask for an `ILoggerFactory` instance to provide logging support to classes that are not managed by Windsor.

:information_source: **Logger abstraction:** Castle Project does not contain its own logging framework. There are already excellent frameworks out there. Instead `ILogger` and `ILoggerFactory` are abstractions Windsor uses to decouple itself from the framework you decide to use.

## Loggers

Castle Core [provides many logger abstraction implementations](https://github.com/castleproject/Core/blob/master/docs/logging.md), however you can also create your own.

## Registering the facility

### In code

The recommended way of configuring the facility is using code. When specifying custom `ILoggerFactory` or `IExtendedLoggerFactory` you use the following generic overload:

```csharp
container.AddFacility<LoggingFacility>(f => f.LogUsing<CustomLoggerFactory>());
```

For example, using the log4net logger factory with configuration stored in a `log4net.xml` file, the code would look like this:

```csharp
container.AddFacility<LoggingFacility>(f => f.LogUsing<Log4netFactory>().WithConfig("log4net.xml"));
```

#### Built-in logging factories

There are a few helper methods for built-in logging factories:
```csharp
// Null Logger
container.AddFacility<LoggingFacility>(f => f.LogUsingNullLogger());

// Console Logger
container.AddFacility<LoggingFacility>(f => f.LogUsingConsoleLogger());

// Diagnostics Logger
container.AddFacility<LoggingFacility>(f => f.LogUsingDiagnosticsLogger());

// Trace Logger
container.AddFacility<LoggingFacility>(f => f.LogUsingTraceLogger());
```

### Via XML Configuration

It is also possible to configure the facility via XML. For example the same configuration for log4net as above:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
   <facility
      type="Castle.Facilities.Logging.LoggingFacility, Castle.Facilities.Logging"
      customLoggerFactory="Castle.Services.Logging.Log4netIntegration.Log4netFactory, Castle.Services.Logging.Log4netIntegration"
      configFile="log4net.xml" />
</configuration>
```

The full list of configuraation attributes is shown in the following example:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
   <facility
      type="Castle.Facilities.Logging.LoggingFacility, Castle.Facilities.Logging"
      customLoggerFactory="<type of factory>"
      configFile="<path to configuration file (optional attribute)>"
      loggerLevel="<the loggerLevel (optional attribute)>"
      configuredExternally="<boolean value (optional attribute)>" "/>
</configuration>
```

## Best practices

We recommend that you make logging optional on your components/services. This way you maximize the reusability. For example:

```csharp
using Castle.Core.Logging;

public class CustomerService
{
   public CustomerService()
   {
   }

   public ILogger Logger { get; set; } = NullLogger.Instance;

   // ...
}
```

With the approach above, the logger field will never be null. Also, if the logging facility was registered on the container, it will be able to supply a logger instance using the `Logger` property.

## Required Assemblies

* `Castle.Facilities.Logging.dll` (bundled with Windsor)
* `Castle.Core.dll` (contains the `ILogger` and `ILoggerFactory` interfaces; included as a dependency in the Windsor NuGet package)
