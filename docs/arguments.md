# Arguments

## The `Arguments` class

The `Arguments` class is used by Windsor to pass arguments [down the invocation pipeline](how-dependencies-are-resolved.md). The class is a simple dictionary-like class, but it has some useful capabilities.

Resolution arguments are key/value pairs, keyed either by name (`string`) or type (`System.Type`). How the arguments are added to the `Arguments` collections determines how they are bound to dependencies during resolution.

### Constructors

The class has several constructors:
```csharp
new Arguments(); // Empty collection
new Arguments(new Arguments())); // Copy constructor
```

Collection initializers are supported, both named and typed arguments can be provided:

```csharp
new Arguments {
	{ "Name", "John" },
	{ "Age", 18 },
	{ typeof(IService), new MyService() }
};
```

### Fluent Interface

#### Named Arguments

Arguments will be matched by a string key in a case insensitive manner. For example, `logLevel`, `LogLevel` and even `LOGLEVEL` as property names are all treated as one key.

```csharp
new Arguments()
	.AddNamed("key", 123456)
	.AddNamed(new Dictionary<string, string> { { "string-key", "string-value" } });
```

Named arguments can also be added from a plain old C# object or from properties of an anonymous type:
```csharp
new Arguments()
	.AddProperties(myPOCO) // plain old C# object with public properties
	.AddProperties(new { logLevel = LogLevel.High }); // anonymous type
```

#### Typed Arguments

Arguments can be matched by type as dependencies:

```csharp
new Arguments()
	.AddTyped(LogLevel.High, new AppConfig()) // params array
	.AddTyped(typeof(MyClass), new MyClass())
	.AddTyped<IService>(new MyService());
```

:information_source: **Typed arguments are matched exactly:** When you don't specify the type of the argument, its concrete type will be used. For example, if you pass a `MemoryStream` it will only match to a dependency of type `MemoryStream`, but not of the base type `Stream`. If you want to match it to `Stream` specify the type explicitly.

#### Named and/or Typed Arguments Collection

A collection implementing the generic `IDictionary<TKey, TValue>` containing named and/or typed arguments can be added to an `Arguments` instance:
```csharp
var map = new Dictionary<object, object>();
map.Add("string-key", 123456);
map.Add(typeof(MyType), 123456);

new Arguments()
 	.Add(map);
```
