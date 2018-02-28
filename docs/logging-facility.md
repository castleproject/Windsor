# Logging Facility

The logging facility provides a seamless way to add logging capabilities to your application. There are two levels of integration.

* Allow your classes to receive an `ILogger` instance for logging support
* Allow you to ask for an `ILoggerFactory` instance to provide logging support to classes that are not managed by Windsor.

:information_source: **Logger abstraction:** Castle Project does not contain its own logging framework. There are already excellent frameworks out there. Instead `ILogger` and `ILoggerFactory` are abstractions Windsor uses to decouple itself from the framework you decide to use.

## Loggers

Castle Core [provides many logger abstraction implementations](https://github.com/castleproject/Core/blob/master/docs/logging.md), however you can also create your own.

## Registering the facility

:warning: **`LoggerImplementation` enum and the `loggingApi` XML property are deprecated:** Usage of `LogUsing` and `customLoggerFactory` are highly recommended even for Castle Core provided implementations.

### Via XML Configuration

Logging facility exposes minimalistic configuration:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
   <facility
      type="Castle.Facilities.Logging.LoggingFacility, Castle.Facilities.Logging"
      loggingApi="null|console|diagnostics|web|nlog|log4net|custom"
      customLoggerFactory="type name that implements ILoggerFactory"
      configFile="optional config file location" />
</configuration>
```

For example to use log4net with logger configuration stored in `log4net.xml` file, you would configure the facility like this:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
   <facility
      type="Castle.Facilities.Logging.LoggingFacility, Castle.Facilities.Logging"
      loggingApi="log4net"
      configFile="log4net.xml" />
</configuration>
```

### In code

Recommended way of configuring the facility however, is using code. The facility exposes the same options like via XML.
For example the same configuration for log4net as above, from code would look like this:

```csharp
container.AddFacility<LoggingFacility>(f => f.LogUsing<Log4netFactory>().WithConfig("log4net.xml"));
```

When specifying custom `ILoggerFactory` or `IExtendedLoggerFactory` you use the following generic overload:

```csharp
container.AddFacility<LoggingFacility>(f => f.LogUsing<CustomLoggerFactory>());
```

## Best practices

We recommend that you make logging optional on your components/services. This way you maximize the reusability. For example:

```csharp
using Castle.Core.Logging;

public class CustomerService
{
   private ILogger logger = NullLogger.Instance;

   public CustomerService()
   {
   }

   public ILogger Logger
   {
      get { return logger; }
      set { logger = value; }
   }

   // ...
}
```

With the approach above, the logger field will never be null. Also, if the logging facility was registered on the container, it will be able to supply a logger instance using the `Logger` property.

## Required Assemblies

* `Castle.Facilities.Logging.dll` (bundled with Windsor)
* `Castle.Core.dll` (contains the `ILogger` and `ILoggerFactory`)
