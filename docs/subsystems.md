# SubSystems

SubSystems (`Castle.MicroKernel.ISubSystem`) are used by the container internally to handle something external to it. These are the innermost and high impact extension points. Out of the box container uses the following subsystems:

## Configuration

Configuration subsystems is where configuration of all components and facilities is stored. (implements `IConfigurationStore`)

## Conversion

Conversion subsystem is responsible for converting parameters declared as strings (for example in XML config file) to their actual types. It is itself extensible, so it can be taught how to handle additional types, although out of the box it can handle pretty much most types you'll need. (implements `IConversionManager` and `ITypeConverterContext`)

## Naming

Naming subsystem is where components matching component names / types to actual component handlers happens. (implements `INamingSubSystem`) Out of the box there are three:

### Default

Default naming subsystem uses direct mapping - if you register certain handler with given name, if you query by that name you will get the same handler. This subsystem uses also [handler selectors](handler-selectors.md) which can override that behavior dynamically.

### Key search

Key search naming subsystem extends default naming subsystem, by letting you register a delegate (`Predicate<string>`) to select which component should be used.

:information_source: **Prefer default naming subsystem:** It is recommended to use default subsystem and use [handler selectors](handler-selectors.md) to do the filtering instead.

### Naming parts

Naming parts subsystem is an interesting idea, showing how flexible naming subsystems can be.

```csharp
container.AddComponent("common:key1=true,transportProtocol=MSMQ", typeof(ICommon), typeof(CommonImpl1));
container.AddComponent("common:secure=true,transportProtocol=HTTPS,key1=false", typeof(ICommon), typeof(CommonImpl2));
```

We register component using not just name, but also a micro DSL, containing set of attribute/value pairs. We can just query the container based on these attribute:

```
var any = container.Resolve<ICommon>("common");
var trueKey1 = container.Resolve<ICommon>("common:key1=true");
var secure = container.Resolve<ICommon>("common:secure=true");
var httpsSecure = container.Resolve<ICommon>("common:transportProtocol=HTTPS,secure=true");
```

In the first two cases we'll get a `CommonImpl1`, in last two cases we'll get `CommonImpl2`.

:information_source: This is a nice feature to show the flexibility of NamingSubsystem, but not one you'd use on most projects.

## Resources

Resource subsystem abstracts how resources are provided to various parts of Windsor ecosystem. (implements `IResourceSubSystem`)

While very powerful subsystems are tightly tied to the container and you will want to create your own only to change how the default ones work (if their standard extension points won't suffice) or when you create your own container type extending how default one work, which is pretty much never. Most of the time what you'll need - is to create a [facility](facilities.md).