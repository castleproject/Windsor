# Registering Installers

Default way of registering components in Windsor is its [Fluent Registration API](fluent-registration-api.md) encapsulated in [Installers](installers.md). However to break compile-time dependency on your composition root assembly, you may not have hard reference to some of your assemblies containing your installers. In this case, you can point Windsor to the installers via XML config.

```csharp
container.Install(Configuration.FromAppConfig());
```

:warning: **Installers must have public default constructor:** When installers are instantiated by Windsor, they must have public default constructor. Otherwise an exception will be thrown.

## Installers - registering type

You register installers via `<install/>` element within `<installers/>` section.

```xml
<installers>
   <install type="Acme.Crm.Infrastructure.ServicesInstaller, Acme.Crm.Infrastructure"/>
</installers>
```

## Installers - registering assembly

Alternatively you may point Windsor to an assembly, and it will scan the assembly for installers and use all of them as if they were registered one by one.

```xml
<installers>
   <install assembly="Acme.Crm.Infrastructure"/>
</installers>
```

## Installers - registering multiple assemblies from given directory

Alternatively you may point Windsor to a directory, and it will scan the directory for assemblies and then each assembly for installers and use all of them as if they were registered one by one.

```xml
<installers>
   <install directory="extensions" fileMask="Acme.*.dll" publicKeyToken="b77a5c561934e089" />
</installers>
```

Directory can be absolute or relative. Optionally you can narrow down the scope of assemblies to scan by specifying `fileMask` for assembly file names, and `publicKeyToken` to only scan assemblies signed with key identified by the token.

:information_source: **Duplicates:** Windsor will take care of not installing any installer type you specify via XML more than once.

:information_source: **Ordering of installers:** Windsor will instantiate all installers in the order they were registered in. Specifying installers using `assembly` attribute yields nondeterministic order. However you can designate an installer from given assembly to be installed first, by specifying it explicitly using `type` attribute, and then installing its assembly which will install all remaining installers from that assembly.
