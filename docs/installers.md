# Windsor Installers

## Introduction

When working with the container, [the first thing you need to do](three-calls-pattern.md#call-one-bootstrapper) is to register all your [components](services-and-components.md). Windsor uses installers (that is types implementing `IWindsorInstaller` interface) to encapsulate and partition your registration logic, as well as some helper types like `Configuration` and `FromAssembly` to make working with installers a breeze.

## `IWindsorInstaller` interface

Installers are simply types that implement the `IWindsorInstaller` interface. The interface has a single method called `Install`. The method gets an instance of the container, which it can then register components with using [fluent registration API](fluent-registration-api.md):

```csharp
public class RepositoriesInstaller : IWindsorInstaller
{
   public void Install(IWindsorContainer container, IConfigurationStore store)
   {
      container.Register(AllTypes.FromAssemblyNamed("Acme.Crm.Data")
                            .Where(type => type.Name.EndsWith("Repository"))
                            .WithService.DefaultInterfaces()
                            .Configure(c => c.LifeStyle.LifestyleTransient));
   }
}
```

:information_source: **Partition your installers:** Usually single installer installs some coherent closed set of related services (like repositories, controllers, etc), and you have separate installer for each of these sets. This helps you keep your installers small and readable, makes it easier to use them in tests and in longer run makes it easier to locate the code that is responsible for registration of any particular component - often overlooked but important effect of well partitioned registration code.

:warning: **By default Installers must be public with public default constructor:** Windsor, when using the default `InstallerFactory` **scan only for public types**, so if your installers aren't public Windsor will not install them. When installers are instantiated by Windsor, they must have public default constructor. Otherwise an exception will be thrown. This is true also about the normal classes.

## Using installers

After you create your installers, you have to install them to the container in your [bootstrapper](three-calls-pattern.md#call-one-bootstrapper). To do this, you use `Install` method on the container:

```csharp
var container = new WindsorContainer();
container.Install(
   new ControllersInstaller(),
   new RepositoriesInstaller(),
   // and all your other installers
);
```

This can be a little tedious, as you will most likely have several or more installers in your app. Also each time you add a new one, you have to remember to come back to your bootstrapper and install it.

To take this tedious manual process away Windsor has some helpers that will automatically take care of that, namely `FromAssembly` static class, and `Configuration` class for using external configuration.

## `FromAssembly` class

Instead of instantiating installers manually you can leave this up to Windsor by using `FromAssembly` class. The class has some methods that select one or more assemblies, and it will then instantiate and install all installer types from that assembly or those assemblies for you. This has the benefit that as you add new installers to these assemblies, they'll be automatically picked up by Windsor, with no additional work from your side.

The type exposes few useful methods for locating the assembly.

```csharp
container.Install(
   FromAssembly.This(),
   FromAssembly.Named("Acme.Crm.Bootstrap"),
   FromAssembly.Containing<ServicesInstaller>(),
   FromAssembly.InDirectory(new AssemblyFilter("Extensions")),
   FromAssembly.Instance(this.GetPluginAssembly())
);
```

:information_source: **Installers are created/installed in non-deterministic order:** When using `FromAssembly` you should not rely on the order in which your installers will be instantiated/installed. It is non-deterministic which means you never know what it's going to be. If you need to install the installers in some specific order, use `InstallerFactory`.

### `This`

Install from assembly calling the method. That is your bootstrapping assembly.

### `Named`

Install from assembly with specified assembly name using standard .NET assembly locating mechanism. You can also provide path to a .dll or .exe file when you have the assembly in some non-standard location.

### `Containing`

Installs from assembly containing designated type. This method is usually used as string-less alternative to `FromAssembly.Named`.

### `InDirectory`

Installs from assemblies located in given directory. This method takes an `AssemblyFilter` object which lets you do all sorts of filtering to narrow down the set of assemblies you're interested in, including filtering by assembly name pattern, public key token or custom predicates.

### `Instance`

Installs from given arbitrary assembly. Use this method as fallback for the other ones, when you have some custom code locating the assembly you want to install.

## `InstallerFactory` class

All of the above methods have an overload that takes an `InstallerFactory` instance. Most of the time you won't care about it and things will just work. However if you need to have tighter control over installers from the assembly (influence order in which they are installed, change how they're instantiated or install just some, not all of them) you can inherit from this class and provide your own implementation to accomplish these goals.

## `Configuration` class

In addition to your own installers that register components in code using [fluent registration API](fluent-registration-api.md), you may have some [XML configuration](xml-registration-reference.md). You can install it via methods exposed on static `Configuration` class.

```csharp
container.Install(
   Configuration.FromAppConfig(),
   Configuration.FromXmlFile("settings.xml"),
   Configuration.FromXml(new AssemblyResource("assembly://Acme.Crm.Data/Configuration/services.xml"))
);
```

You can use it to access configuration in AppDomain configuration file (`app.config`, or `web.config`) or any arbitrary XML file. As shown in the last example the file may be embedded within an assembly (build action set to Embedded Resource).

One useful usage of the `Configuration` class is to use XML configuration file to remove compile-time dependency on some additional assemblies that may, for example, be themselves extensions to your application. You can list these assemblies (or specific installers types contained in them) in the XML file, and have Windsor pick them up and install for you. [Read more here](registering-installers.md).

## See also

* [Fluent registration API](fluent-registration-api.md)
* [XML configuration reference](xml-registration-reference.md)
* [Registering installers via XML configuration](registering-installers.md)

## External resources

* [Blog post by Krzysztof Kozmic (Oct 10, 2010)](http://kozmic.pl/2010/08/10/ioc-patterns-ndash-partitioning-registration/)
