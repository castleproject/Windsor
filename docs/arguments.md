# Arguments

## The `Arguments` class

The `Arguments` class is used by Windsor to pass arguments [down the invocation pipeline](how-dependencies-are-resolved.md). The class is a simple implementation of non-generic `IDictionary`, but it has some useful capabilities.

:information_source: **Custom `IDictionary` is respected:** When you call `container.Resolve` passing your own custom implementation of `IDictionary` Windsor will respect that and not replace it with `Arguments`. It is advised that you use `Arguments` though.

### Constructors

The class has several constructors:

#### `namedArgumentsAsAnonymousType`

You can pass named arguments as properties on anonymous type:

```csharp
new Arguments(new { logLevel = LogLevel.High });
```

:information_source: **Named arguments are not case sensitive:** You can specify `logLevel`, `LogLevel` or even `LOGLEVEL` as property name. All named arguments are matched in case insensitive manner so in all cases the behavior will be the same.

#### Array of `typedArguments`

When you don't care about names of the dependencies you can pass them as array, in which case they will be matched by their type.

```csharp
new Arguments(new[] { LogLevel.High });
```

:information_source: **Typed arguments are matched exactly:** When you don't specify type of the dependency it's exact type will be used. So if you pass for example `MemoryStream` Windsor will try to match it to dependency of type `MemoryStream`, but not of the base type `Stream`. If you want to match it to `Stream` use `Insert` extension method.

#### Custom dictionary of arguments

You can also pass already populated dictionary to `Arguments` in which case its values will be copied over.

### `Insert` extension method

In addition to `Arguments` class there's also `Insert` extension method that extends `IDictionary`. It has several overloads that let you fluently insert values into the dictionary.

```csharp
args.Insert<Stream>(someMemoryStream)
   .Insert("name", someNamedArgument)
   .Insert(new[] { multiple, typed, arguments})
   .Insert(new { multiple = typed, arguments = also});
```

:information_source: **Insert overwrites:** When item under given key already exists `Insert` will overwrite it.