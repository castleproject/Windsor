# Passing Arguments

While most components in your application will depend on other components it's not always the case. Also the default rule in Windsor for finding the right component to use to satisfy a dependency has to be tweaked sometimes.

How you do it depends on where the value comes from and where you obtain the value.

## Composition root - `container.Resolve`

The `container.Resolve` method has several overloads that let you pass in arguments as `IDictionary` (in which case it is advised to use `[Arguments]` class), or named arguments as anonymous object.

:warning: **Don't reference the container directly:** It is advised to resist the temptation and use this approach everywhere passing the container around. Use this approach only in your composition root to resolve the root components (see [Three Calls Pattern](three-calls-pattern.md)). In other cases, try to go with one of the other approaches.

:information_source: **Inline dependencies don't get propagated:** Whatever arguments you pass to `Resolve` method will only be available to the root component you're trying to resolve, and its [Interceptors](interceptors.md). All the components further down (root's dependencies, and their dependencies and so on) will not have access to them.

This approach is useful for parameters that are available in the composition root, like your `Program.Main` method. While it may look simple at the beginning, and is in fact often used by people new to Windsor, it's applicability is generally quite limited, and the other two ways are used much more often.

### Example - command line parameter in a Console application

```csharp
// rest of the code omitted for brevity.
public void Main(string[] args)
{
   var serverAddress = GetServerAddress(args);
   var container = BootstrapContainer();

   var serverMonitor = container.Resolve<IServerMonitor>(new Arguments(new { serverAddress }));

   DoSomethingUseful(serverMonitor);
}
```

## Registration time - `DependsOn` and `DynamicParameters`

When the value of the dependency can be known at registration time, or at registration time you have all the information required to obtain it later at call time you can plug directly into registration API and provide the value from there, without any explicit knowledge of the call site. See [here](inline-dependencies.md) for more details. This approach can be used for both, non-service (primitive) dependencies, as well as specifying non-default component to satisfy a service dependency. Find more details and description of other methods (the API can be used for much more than just appSettings) [here](inline-dependencies.md).

### Example - dependency value coming for a config file

```csharp
// inside an Installer class registering IServiceMonitor
public void Install(IWindsorContainer container, IConfigurationStore store)
{
   container.Register(Component.For<IServerMonitor>()
      .ImplementedBy<PingServerMonitor>()
      .LifestyleTransient()
      // value for 'serverAddress' dependency comes from .config file's appSettings value also named 'serverAddress'
      .DependsOn(Dependency.OnAppSettingsValue("serverAddress"))
}
```

## Resolution time - typed factories

This third option is basically used when the other two are not applicable. That is when a value is not known upfront. For example (using our server monitor sample) when the address of the server to monitor is typed in by the user in our application UI.
In that scenario you use a [typed factory](typed-factory-facility.md) to pull a new server monitor, passing the argument through the factory.

### Example - typed factory

```csharp
//our factory interface
public interface IServerMonitorFactory
{
   IServerMonitor Open(string serverAddress);
   void Close(IServerMonitor monitor);
}
//inside another component, like ServerMonitorController. Rest of the code omitted for brevity.
IServerMonitorFactory factory;

void StartMonitoring(string addressToMonitor)
{
   var monitor = factory.Open(addressToMonitor);
   DoSomethingUseful(monitor);
}
```

## See also

* [How dependencies are resolved](how-dependencies-are-resolved.md)
* [Inline dependencies (via registration API)](inline-dependencies.md)
* [XML Inline Parameters](xml-inline-parameters.md)
* [Arguments](arguments.md)
* [Typed Factories](typed-factory-facility.md)