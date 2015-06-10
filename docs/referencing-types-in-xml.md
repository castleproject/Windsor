# Referencing types in XML

:information_source: **Experimental feature:** Notice that feature discussed in this section is experimental. It may not always work properly, and may not ship as part of the next release.

Wherever you reference a .NET type in XML you must use it's fully qualified name, as described [in remarks section of this MSDN site](http://msdn.microsoft.com/en-us/library/w3f99sx1.aspx).

## Syntax

For example when you [reference an Installer](registering-installers.md) in Windsor's config:

```xml
<install type="Acme.Crm.Infrastructure.ServicesInstaller, Acme.Crm.Infrastructure"/>
```

Windsor lets you omit parts of the name, as long as the type can be uniquely identified. So instead of the above, you can omit the assembly name...

```xml
<install type="Acme.Crm.Infrastructure.ServicesInstaller"/>
```

...and even the namespace.

```xml
<install type="ServicesInstaller"/>
```

As long as the type can be uniquely identified (there are no other public `ServiceInstaller` types in any namespace in any loaded assembly).

## Scanned assemblies

When the shorthand syntax is used Windsor will try to find the type in all non-BCL assemblies loaded into the AppDomain at given point.
This is often enough, but you may not have loaded some particular needed assembly at given point in time (for example when there's no compile time dependency on that assembly). You may however instruct Windsor to load additional assemblies and scan them too by putting the following element in Windsor's XML configuration:

```xml
<using assembly="Acme.Crm.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=11111111111111" />
```

Instead of assembly name you can also put path to its `.dll` or `.exe` file.

The way it works is similar to `using` statement in C#, only here, instead of namespaces, you're importing entire assemblies.

:information_source: **Location of `using` element:** The `using` element can be placed at any point in the XML configuration. However it is advised to place it at the top, before any reference to types from given assembly.

## Duplicates

When no unique type can be found Windsor will throw an exception. You will then need to expand the information you provide so that the type can be uniquely identified.

## Limitations

By default BCL types (defined in `System.*` assemblies or `mscorlib`) will not be matched when using shorthand syntax. Also arrays and generics containing BCL types will not be matched. You will have to use full type name then

## Generics

Generic types are supported using following syntax:

For simple generic type `IGeneric<ICustomer>`:

```
IGeneric`1[[ICustomer]]
```

For generic type with multiple generic parameters like `IDoubleGeneric<ICustomer, ISpecification>`:

```
IDoubleGeneric`2[[ICustomer],[Acme.Crm.ISpecification]]
```

Notice you can mix and match levels of details provided, specifying some types by just name, while other by namespace and name.

For nested generic type with multiple generic parameters like `IDoubleGeneric<ICustomer, IDoubleGeneric<ICustomer, IClock>>`:

```
IDoubleGeneric`2[[ICustomer],[IDoubleGeneric`2[[ICustomer],[IClock]]]]
```