# XML Inline Parameters

When using [XML configuration](xml-registration-reference.md) it is possible to specify various dependencies for the components.

## Simple parameters

:information_source: **Parameter names are not case sensitive:** All names of the parameters are **not** case sensitive so no matter if you specify `foo`, `FOO` or `Foo` Windsor will process them the same.

```xml
<component
   id="ping"
   type="Acme.Crm.Services.PingService, Acme.Crm">
   <parameters>
      <pingServer>http://acme.org</pingServer>
      <pingInterval>00:00:30</pingInterval>
      <pingRetries>3</pingRetries>
      <pingNotificator>${emailSender}</pingNotificator>
   </parameters>
</component>
```

Simple parameters are parameters of simple types, like primitives, `Uri`s, `DateTime`s, `TimeSpan`s, `Type`s, `enum`s or service overrides. These are parameters that have their value expressed as simple content of the enclosing name tag.

:information_source: **How parameter types are matched:** Windsor does not require you to specify type of the parameter you provide. Instead, when resolving the component it will try to match the dependency on the component by name (case insensitive), and see if it can convert the parameter value to the the type of the dependency.

## Complex parameters

Complex parameters are ones that consist of more than one element. For example if you want to specify in XML parameter, named `pingServerInfo` of the following type:

```csharp
[Convertible]
public class ServerInfo
{
   private readonly Uri address;

   public ServerInfo(Uri address)
   {
      this.address = address;
   }

   public Uri Address
   {
      get { return address; }
   }

   public int Port { get; set; }
}
```

you can do it thusly:

```xml
<parameters>
   <pingServerInfo>
      <parameters>
         <address>http://localhost</address>
         <port>80</port>
      </parameters>
   </pingServerInfo>
</parameters>
```

:information_source: **The `ConvertibleAttribute` attribute:** You may have noticed the `[Convertible]` attribute on `ServerInfo` class. Currently when you're using complex parameter it is required that you tell Windsor explicitly, that this type is parameter, not a service.

## Lists

Assuming you have the component:

```csharp
public class MyComponent
{
   public MyComponent(IEnumerable<Uri> info)
   {
      Info = info;
   }

   public IEnumerable<Uri> Info { get; private set; }
}
```

You can pass the `info` collection as list, specifying it in config:

```xml
<parameters>
   <info>
      <list>
         <item>http://localhost:80</item>
         <item>http://castleproject.org</item>
      </list>
   </info>
</parameters>
```

Notice we specified the parameter as `IEnumerable<Uri>` passing in a list. We also didn't have to specify any types. Windsor inferred correct type from generic parameter of the `IEnumerable<>`.

:information_source: **Non-generic collections:** If we would use non-generic collection (`IList` or `IEnumerable`) we'd get collection of `string`s, instead of `Uri`s, since then Windsor would not have enough information to correctly guess the type of the items in the collection. In this case, we have to be explicit:

```xml
<list type="System.Uri, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
```

## Arrays

Arrays are almost identical to lists. If instead of list, in the above example we wanted an array, we'd have to configure it as follows:

```xml
<parameters>
   <info>
      <array>
         <item>http://localhost:80</item>
         <item>http://castleproject.org</item>
      </array>
   </info>
</parameters>
```

## Dictionaries

In addition to lists and arrays, also dictionaries are supported. If we specified `info` to be `IDictionary<string, Uri>` we would use the following configuration:

```xml
<parameters>
   <info>
      <dictionary>
         <entry key="local">http://localhost:80</entry>
         <entry key="castle">http://castleproject.org</entry>
      </dictionary>
   </info>
</parameters>
```

:information_source: **Untyped dictionary:** If you used untyped, non-generic dictionary Windsor would default to using `string`s for key and value types. If that is not what you want, you can specify their types explicitly via `keyType` and `valueType` attributes respectively, similarly to how you'd do it for list. You can specify these attributes at `dictionary` level, or at `entry` level.

## Custom parameter types

If built in converters don't suit your needs, you can always supply your own implementation. To do so, you need to create type implementing `ITypeConverter` (you can inherit abstract base implementation `AbstractTypeConverter`) and plug it into conversion subsystem of the container:

```csharp
var manager = (IConversionManager)container.Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);
manager.Add(new MyConfigConverter());
```