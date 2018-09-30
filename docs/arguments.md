# Arguments

## The `Arguments` class

The `Arguments` class is used by Windsor to pass arguments [down the invocation pipeline](how-dependencies-are-resolved.md). The class is a simple implementation of non-generic `IDictionary`, but it has some useful capabilities explained further below.

### Constructors

The class has several constructors:

#### `namedArgumentsAsAnonymousType`

You can pass named arguments as properties on an anonymous type:

```csharp
var args = new Arguments().InsertProperties(new { logLevel = LogLevel.High });
```

:information_source: **Named arguments are not case sensitive:** You can specify `logLevel`, `LogLevel` or even `LOGLEVEL` as property name. All named arguments are matched in case insensitive manner so in all cases the behavior will be the same.

#### Array of `typedArguments`

When you don't care about names of the dependencies you can pass them as array, in which case they will be matched by their type.

```csharp
var args = new Arguments().InsertTyped(LogLevel.High);
```

:information_source: **Typed arguments are matched exactly:** When you don't specify type of the dependency it's exact type will be used. So if you pass for example `MemoryStream` Windsor will try to match it to dependency of type `MemoryStream`, but not of the base type `Stream`. If you want to match it to `Stream` use `Insert` extension method.

#### Custom dictionary of arguments

You can also pass a dictionary to `Arguments` where it will be shallow copied.

```csharp
var args = new Arguments().Insert(new Dictionary<string, string> { { "anyString", "anyStringValue" } });
```

#### Custom read-only dictionary of arguments

You can also pass a dictionary to `Arguments` via the `InsertNamed` method which supports `IReadOnlyDictionary<string, object>` and is also shallow copied.

```csharp
var args = new Arguments().InsertNamed(new Dictionary<string, string> { { "anyString", "anyStringValue" } });
```

### `Insert` method

Insert allow you to insert named, typed and Dictionary arguments. Dependencies are matched on name or types. 

```csharp
args.Insert<Stream>(someMemoryStream)
   .Insert("name", someNamedArgument)
   .Insert(new[] { multiple, typed, arguments})
   .Insert(new { multiple = typed, arguments = also});
```

:information_source: **Insert overwrites:** When item under given key already exists `Insert` will overwrite it.

### `InsertTyped` extension method

Inserts a new typed argument where the type of the value being inserted is used as the key. 

```csharp
var args = new Arguments().InsertTyped<MyClass>(new MyClass())
```

:information_source: **InsertTyped overwrites:** When item under given key already exists `InsertTyped` will overwrite it.

### InsertProperties

Inserts a typed set of arguments where the properties and values of the instance are used. This can also be used to 
anonymous types.

```csharp
var args = new Arguments().InsertProperties(new { Value1 = 1 })
```

:information_source: **InsertProperties overwrites:** When item under given key already exists `InsertProperties` will overwrite it.
