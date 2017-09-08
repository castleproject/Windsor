# Castle Windsor Documentation

<img align="right" src="images/windsor-logo.png">

Castle Windsor is best of breed, mature [Inversion of Control container](ioc.md) available for .NET.

* See [the release notes](https://github.com/castleproject/Windsor/releases/tag/v3.3)
* [Download it](https://github.com/castleproject/Windsor/releases/tag/v3.3)
* Get official builds from [NuGet](http://nuget.org/packages/Castle.Windsor): `PM> Install-Package Castle.Windsor`
* Or [get pre-release packages as they're built](https://github.com/castleproject/Home/blob/master/prerelease-packages.md)

## Show me the code already

Windsor is very simple to use. Code below is not just *hello world* - that's how many big real life applications use Windsor. See the full documentation for more details on the API, features, patterns, and practices.

```csharp
// application starts...
var container = new WindsorContainer();

// adds and configures all components using WindsorInstallers from executing assembly
container.Install(FromAssembly.This());

// instantiate and configure root component and all its dependencies and their dependencies and...
var king = container.Resolve<IKing>();
king.RuleTheCastle();

// clean up, application exits
container.Dispose();
```

So what about those [installers](installers.md)? Here's one.

```csharp
public class RepositoriesInstaller : IWindsorInstaller
{
	public void Install(IWindsorContainer container, IConfigurationStore store)
	{
		container.Register(Classes.FromThisAssembly()
			                .Where(Component.IsInSameNamespaceAs<King>())
			                .WithService.DefaultInterfaces()
			                .LifestyleTransient());
	}
}
```

For more in-depth sample try the section below, or dive right into API documentation on the right.

## Samples and tutorials

Learn Windsor by example by completing step-by-step tutorials. See Windsor in action by exploring sample applications showcasing its capabilities:

* [Basic tutorial](basic-tutorial.md)
* [Simple ASP.NET MVC 3 application (To be Seen)](mvc-tutorial-intro.md) - built step by step from the ground up. This tutorial will help you get up to speed with Windsor quickly while keeping an eye on both the usage of the container API as well as patterns that will help you get the most out of using the container.

## Documentation

* [What's new in Windsor 3.2](whats-new-3.2.md)
* [What's new in Windsor 3.1](whats-new-3.1.md)

### Concepts

* [Inversion of Control and Inversion of Control Container](ioc.md)
* [Services, Components and Dependencies](services-and-components.md)
* [How components are created](how-components-are-created.md)
* [How dependencies are resolved](how-dependencies-are-resolved.md)

### Using the Container

* [Using the container - how and where to call it](three-calls-pattern.md)
* [Windsor installers - this is how you tell Windsor about your components](installers.md)
* [Registration API reference](fluent-registration-api.md)
* [Using XML configuration](xml-registration-reference.md)
* [Passing arguments to the container](passing-arguments.md)
* [AOP, Proxies, and Interceptors](interceptors.md)
* [Child Containers](child-containers.md)
* [Windsor's support for debugger views and diagnostics](debugger-views.md)
* [Windsor's support for performance counters](performance-counters.md)

### Customizing the container

* [Extension Points Overview](extension-points.md)
* [Lifestyles](lifestyles.md)
* [Lifecycle](lifecycle.md)
* [Release Policy](release-policy.md)
* [ComponentModel construction contributors](componentmodel-construction-contributors.md)

### Extending the container

* [Facilities](facilities.md)

### Know another container

* [Castle Windsor for Autofac users](windsor-for-autofac-users.md)
* [Castle Windsor for StructureMap users](windsor-for-structuremap-users.md)

## Resources

* [External Resources](external-resources.md) - screencasts, podcasts, etc
* [FAQ](faq.md)
* [Roadmap](roadmap.md)
