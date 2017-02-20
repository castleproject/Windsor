# Three Calls Pattern

Inversion of Control (called IoC) frameworks are different from all other kinds of frameworks in that you don't see many calls to the framework in your code. In fact - in most applications (regardless of their size and complexity) you will only call the container directly in three places. That's the most common pattern of usage and Windsor supports it fully.

## The Three Container Calls Pattern

The pattern is called Three Calls. Sometimes it's referred to as RRR - Register, Resolve, Release - name used in [Mark Seemann's book about containers](http://www.manning.com/seemann/). As the name implies the container is only called in three places in the application, or more precisely; in your entry project.

:information_source: **What is entry project?** Entry project means either `Program.Main` method in `.exe` of your client application, `Application_Start` and `Application_End` in your web application or corresponding places in other kinds of applications.

:information_source: **Yes, that also means in *Enterprise* application:** Newcomers to IoC find it hard to believe that you can build an "enterprise" "big", "real-world" application with the container and hardly explicitly use it. Yes, absolutely, you can. It's been done multiple times and works beautifully. As noted in the first paragraph - that's what inversion of control is all about.

That also implies one more very important aspects of dealing with the container - there is one container instance in your application. You only ever need a single, root container instance of a container. In some rare advanced scenarios you may create "child" containers, but they are tied to, as children to the one single ubiquitous root application container.

Let's now have a look at what the three calls are.

### Call one - bootstrapper

Bootstrapper is the place where you create and configure your container. It usually is just a single method that looks somewhat like this:

```csharp
public IWindsorContainer BootstrapContainer()
{
    return new WindsorContainer()
        .Install(
            Configuration.FromAppConfig(),
            FromAssembly.This()
            //perhaps pass other installers here if needed
        );
}
```

In bootstrapper you do the following things:

* Create the instance of the container that we'll be using.
* Customise the container if needed. This is something you won't have to do most of the time (and that usually means never) since the default behavior and configuration of the container should suffice for 95% of applications. By customising, I mean replacing container's `HandlerFactory`, `ReleasePolicy`, `DependencyResolver`, subsystems. Things that container uses internally to perform its job. You may also want to add some extensions to the container here as well, for example [facilities](facilities.md) that need to be registered before any components.
* Register all your [components](services-and-components.md) that the container will manage. That's the call to `Install` method you see in the code above. Here's where you pass your [installers](installers.md) which encapsulate all information about your specific component in the application. That's where the bulk of the work happens as you'll see later.

:information_source: **Prefer calling `Install` just once:** It is recommended to install all your installers in a single call to `Install`. While currently the container will work correctly even if call `Install` multiple times, or configure components outside of this method, Windsor is optimised for this scenario, and it performs better if you do it like this. In future versions it will be additionally optimised for usage of single `Install`.

### Call two - `Resolve`

In the first step we fully configure the container, and now we can actually use it. The important part is we use it just once (remember the *inversion of control* part). Every application has a root component. In a MonoRail or ASP.NET MVC application this would be your controller, WPF or WinForms application your main window, in WCF service your service, etc. You may have more than one root component, like in a console application where you'd have two - one for parsing command line parameters and second to perform some actual work, how ever there will always be very few of them and they will be the root of your component graph.

Those are the only components you explicitly `Resolve` from the container. The container then constructs the entire graph of their dependencies and dependencies of their dependencies and so on, doing all sorts of work for you so that you can write code that's as simple as this:

```csharp
var container = BootstrapContainer();
var shell = container.Resolve<IShell>();
shell.Display();
```

:information_source: **Resolving by type vs resolving by name:** The `Resolve` method has several overloads that can be split into two groups - with name and without name. If name is not provided Windsor will use type to locate the service (as in the example above). If name is provided the name will be used and type will be used as a convenience for you (so that you don't have to cast from `object` or as a hint to the container, when you're resolving open generic component, how Windsor should close the open generic type). **Unless you have a good reason to resolve by name, use type.**

By the time the second line of the code above finishes executing you will have a fully configured instance of your `IShell` service that you can then pass control to in the following line. If you're new to container's that should be the part that is really impressive. The container created entire complicated (containing potentially hundreds of objects, each configured differently) graph of objects for you, in a single line of code. This is profound, trust me.

:information_source: **What about components I can't/don't want to obtain at root resolution time/place?** For cases where you need to pull some components from the container at some later point in time, not when resolving root component(s) use [typed factories](typed-factory-facility.md).

:information_source: **What about components I want to instantiate at root resolution time/place but I don't really want to do anything with them, like background tasks?** For cases where you have components that are not dependencies of your root, but need to start as the application starts (like background tasks) use [Startable Facility](startable-facility.md).

### Call three - `Dispose`

That's the part that many people (especially those insisting that container does "dependency injection") forget about. Container manages entire lifetime of the components, and before we shutdown our application we need to shutdown the container, which will in turn decommission all the components it manages (for example Dispose them). That's why it is really important to call `container.Dispose()` before you close your application.

## Additional Resources

* [Windsor Installers](installers.md)
* [Fluent Registration API](fluent-registration-api.md)
* [Typed Factory Facility](typed-factory-facility.md)
* [Startable Facility](startable-facility.md)
* [Lifecycle](lifecycle.md)
* [Extensibility Sample App - EventBrokerFacility](sample-eventbrokerfacility.md)
