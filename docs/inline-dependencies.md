# Inline dependencies

## Supplying inline dependencies

Most components in your application will depend on services exposed by other components or have no dependencies at all.
Some components however require dependencies which would either be illegal as services, or make no sense as components in the container. Those may be `int` dependencies, for example retry-count when attempting to query a third party REST API (say Twitter or GitHub), a `string` representing API key for the API, a `TimeSpan` for timeout, or a `TwitterApiConfiguration` object encapsulating all of the above.

Windsor obviously supports these scenarios. The registration API has a `.DependsOn()` method for that. The method takes a value returned by one of the static methods of `Dependency` class. We'll look at those methods, why there are multiple, and when each of them becomes useful.

### Supplying *static* dependencies, that are known upfront and don't change: `Dependency.OnValue()`

By static dependencies we mean those who have their value determined on the spot where we register them. The value does not change afterward and is used every time a new instance of that component gets created.
The configuration of Twitter API integration would be an example of that. You normally want to hardcode it somewhere, so that a developer can easily change it if needed, yet you don't want to put it in the component itself, so that multiple components can use the same value, or simply to keep the component from depending on particular hardcoded values.

You do that by using `Dependency.OnValue()` method (one of the overloads).

```csharp
var twitterApiKey = @"the key goes here";

container.Register(
    Component.For<ITwitterCaller>().ImplementedBy<MyTwitterCaller>()
        .DependsOn(Dependency.OnValue("APIKey", twitterApiKey))
);
```

In this case we used overload that will match the dependency by name, so it will supply the value we specified to a property or constructor parameter named *"APIKey"* (not case sensitive) in `MyTwitterCaller` class.

:information_source: **Dependency by name is not case sensitive:** In the example above all: `ApiKey` property, `apiKey` constructor parameter or `ApIkEy` parameter (not that we're encouraging you to adapt this naming convention) would be matched by the dependency specified, since the matching Windsor uses is not case sensitive. This is a great feature, as it allows you to follow *normal* naming conventions for your properties and parameters and yet have either of them matched by your dependency, decoupling your registration code from the implementation details of your class.

For primitive dependencies specifying the dependency by name makes sense, since if there happens to be another `string` dependency in `MyTwitterCaller` we might end up in some really confusing state.

For cases where we're using higher level types where risk of duplicated dependency is zero, it might be a better option to decouple ourselves from the name of the dependency, and specify it by type.

:information_source: **Dependency and refactoring:** Additional benefit of specifying dependency by type, rather than by name, is that we mitigate the risk of rename refactoring breaking our registration.

```csharp
var config = new TwitterApiConfiguration {
    // set all the properties here...
};

container.Register(
    Component.For<ITwitterCaller>().ImplementedBy<MyTwitterCaller>()
        .DependsOn(Dependency.OnValue<TwitterApiConfiguration>(config))
);
```

### Setting up properties: `Property.ForKey()`

```csharp
container.Register(
    Component.For<ICustomer>().ImplementedBy<CustomerImpl>()
        .DependsOn(Property.ForKey("Name").Eq("Caption Hook"), Property.ForKey("Age").Eq(45)));
```

Another way to initialize properties is to set them inside [OnCreate delegate](registering-components-one-by-one.md#oncreate).

### Explicit service dependencies: `Dependency.OnComponent()`

What if you have two implementation of a service (let's say `ILogger`) and you want to use the default logger for all components except for `TransactionProcessingEngine` service, which requires `SecureLogger` that does additional processing to protect personal information of your clients. This ability to explicitly specify which of many components providing given service you want to use you utilize is called *service override* and you specify it using `Dependency.OnComponent()` method.

```csharp
container.Register(
    Component.For<ITransactionProcessingEngine>().ImplementedBy<TransactionProcessingEngine>()
        .DependsOn(Dependency.OnComponent("Logger", "secureLogger"))
);
```

This way we specifies that for named dependency "Logger" we want Windsor to use component that's registered with "secureLogger" name.

There are a few overloads of `Dependency.OnComponent()` that allow you to specify the dependency as well as the service either by name or by type.

The code above could be rewritten as:

```csharp
container.Register(
    Component.For<ITransactionProcessingEngine>().ImplementedBy<TransactionProcessingEngine>()
        .DependsOn(Dependency.OnComponent(typeof(ILogger), "secureLogger"))
);
```

Also if we don't name the logger explicitly when registering, we can refer to it by its implementation type and rewrite the registration again to:

```csharp
container.Register(
    Component.For<ITransactionProcessingEngine>().ImplementedBy<TransactionProcessingEngine>()
        .DependsOn(Dependency.OnComponent<ILogger, SecureLogger>())
);
```

### appSettings dependencies: `Dependency.OnAppSettingsValue()`

Sometimes you want the value that your service depends on to reside in the configuration file, in the `appSettings` section. Going back to the very first example on this page, we might want to keep the timeout value for the Twitter API calls in the config file. In that case your registration code would look like this:

```csharp
container.Register(
    Component.For<ITwitterCaller>().ImplementedBy<MyTwitterCaller>()
        .DependsOn(Dependency.OnAppSettingsValue("timeout", "twitterApiTimeout"))
);
```

The dependency called "timeout" on our components will be supplied with the value coming from "twitterApiTimeout" element of appSettings section of the configuration file.

:information_source: There is another overload that takes just one parameter, when the name of the dependency and the name of the appSettings element are the same. However this introduced unnecessary coupling and should be avoided.

:information_source: **appSettings and conversion:** Values in the config file are stored as text, yet the dependencies in your code may be of other types (`TimeSpan` in this example). Windsor has got you covered, and for most cases will perform the appropriate conversion for you.

### Embedded resource dependencies: `Dependency.OnResource()`

:information_source: This option is new in Windsor 3.2

There are cases where you want to take a dependency on an embedded resource. For example you may want your view model to take its `DisplayName` from a (localizable) resource, so that you can leverage the localization support that .NET offers and display window text to the user in their native language. The registration code (for a single view model) would look like this:

```csharp
container.Register(
    Component.For<MainViewModel>()
        .DependsOn(Dependency.OnResource<MyApp.Properties.Resources>("DisplayName", "MainWindowTitle"))
);
```

This will get the value for the "DisplayName" property or parameter of `MainViewModel` from "MainWindowTitle" resource (appropriate for the culture of the current user) from `ResourceManager` encapsulated by the `MyApp.Properties.Resources` auto generated class.

Alternatively you can pass the ResourceManager explicitly:

```csharp
container.Register(
    Component.For<MainViewModel>()
        .DependsOn(Dependency.OnResource("DisplayName", new ResourceManager(/* appropriate values here */), "MainWindowTitle"))
);
```

### Supplying dynamic dependencies

There are times where you need to supply a dependency, which will not be known until the creation time of the component. For example, say you need a creation timestamp for your service. You know how to obtain it at the time of registration, but you don't know what its specific value will be (and indeed it will be different each time you create a new instance). In this scenarios you use DynamicParameters method.

```csharp
container.Register(
    Component.For<ClassWithArguments>()
        .LifestyleTransient()
        .DynamicParameters((k, d) => d["createdTimestamp"] = DateTime.Now)
);
```

`DynamicParameters` works with a delegate which is invoked at the very beginning of component resolution pipeline. It has two parameters: IKernel instance, and a dictionary with contains parameters supplied from the call site. Usually it will be empty. It is that dictionary that you can now populate with dependencies which will be passed further to the resolution pipeline.

Optionally you can also return a value from this method, being another delegate, which will be called at the end of your component's lifetime. You can use it to clean up the dependencies you supplied.

```csharp
container.Register(
    Component.For<ClassWithArguments>()
        .LifestyleTransient()
        .DynamicParameters((k, d) =>
        {
            d["arg1"] = "foo";
            return kernel => ++releaseCalled;
        })
);
```

The other delegate takes an `IKernel` which can be useful in some advanced scenarios. Here, we just ignore it.

## See also

* [XML configuration inline parameters](xml-inline-parameters.md)
* [Registering components one-by-one](registering-components-one-by-one.md)