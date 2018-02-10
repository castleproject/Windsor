# Facilities

Facilities are the main way of extending the container. Using facilities you can integrate the container with external frameworks, like WCF or NHibernate, add new capabilities to the container like event wiring, transaction support... or to components (synchronization, startable semantics...).

## How to use them

To start using a facility you need to register it with the container, either in code, as shown below or [using XML configuration](facilities-xml-configuration.md). Usually that's all you need to do as a user. Some facilities, most notably WCF facility may have also additional API, detached from the container object but that's something specific to a given facility.

```csharp
container.AddFacility<TypedFactoryFacility>();
```

Some facilities can also be configured, using an overload of the above method:

```csharp
container.AddFacility<StartableFacility>(f => f.DeferredTryStart());
```

:warning: **Add facilities at the beginning:** In order to work properly most facilities need to be registered before components they affect. Keep that in mind when structuring your registration code, as forgetting to do so may lead to some hard to find issues (like startable components not being started).

## Standard facilities

In container's assembly `Castle.Windsor.dll` you can find the following facilities. Notice that the are many more provided in their own assemblies, but still being part of the Castle project. Many external projects provide their own facilities to integrate with Windsor.

* [Typed Factory Facility](typed-factory-facility.md) - Provides automatic implementation for factory classes, that you can use in your code to create objects on demand without introducing dependency on the container.
* [Startable Facility](startable-facility.md) - Provides ability to 'start' and 'stop' objects. Very useful for things like WCF Services that you may want started as soon as your application starts.

## Other facilities

In addition to the above, as part of Castle Project, some other facilities are provided, mostly for integration between Windsor and other frameworks. They also contain some pretty powerful features and can ease your job significantly.

* [WCF Integration Facility](wcf-facility.md) - Provides integration with Windows Communication Foundation. It dramatically simplifies configuring of your WCF services lets you extend them easily, use non-default constructors, call services asynchronously without having to resort to code generation and more.
* [Logging Facility](logging-facility.md) - Most applications use logging. This facility lets you easily inject loggers into your components. It offers integration with most popular 3rd party logging frameworks like [NLog](http://nlog-project.org/) and [log4net](http://logging.apache.org/log4net/).
* [Factory Support Facility](factory-support-facility.md) - Provides ability for the components to be created by factory objects. You can use it to register things like `HttpContext` in the container.
  * :information_source: **Prefer `UsingFactoryMethod`:** This facility is used mostly for backward compatibility (XML registration) and it is discouraged to use it in new applications. Prefer usage of [`UsingFactoryMethod`](registering-components-one-by-one.md#using-a-delegate-as-component-factory). Factory Support Facility may become obsolete in future release.
* [Event Wiring Facility](event-wiring-facility.md) - Provides ability to wire up classes exposing events to classes consuming them.
* [Remoting Facility](remoting-facility.md) - Provides ability to expose or consume components from another `AppDomain` using .NET Remoting.
* [ActiveRecord Integration Facility](activerecord-integration-facility.md) - The ActiveRecord Integration Facility takes care of configuring and starting [Castle ActiveRecord](https://github.com/castleproject/ActiveRecord) and adds declarative transaction support integration.
* [NHibernate Integration Facility](nhibernate-facility.md) - When you're using bare NHibernate, rather than [Castle ActiveRecord](https://github.com/castleproject/ActiveRecord), this facility offers nice integration between the two frameworks.
* [Synchronize Facility](synchronize-facility.md) - Integrates with synchronization elements of .NET Framework (like `ISynchronizeInvoke` interface, `SynchronizationContext`), ensures components that inherit `Control` get created on UI thread etc.
* [Automatic Transaction Management Facility](atm-facility.md) - This facility manages the creation of Transactions and the associated commit or rollback, depending on whether the method throws an exception or not. Transactions are logical. It is up the other integration to be transaction aware and enlist its resources on it.
* [MonoRail Integration Facility](https://github.com/castleproject/MonoRail/blob/master/MR2/docs/windsor-integration.md) - Provides integration with MonoRail controllers and internal services.
* [System Web Facility](systemweb-facility.md) - Provides system web integration for web projects using `PerWebRequest` lifestyles.
* [ASP.NET Mvc Facility](aspnetmvc-facility.md) - Provides aspnet mvc integration for web projects using Windsor.
* [ASP.NET WebApi Facility](aspnetwebapi-facility.md) - Provides aspnet webapi integration for web projects using Windsor.
* [ASP.NET Core Facility](aspnetcore-facility.md) - Provides aspnet core integration for web projects using Windsor.

## Third Party Facilities

:information_source: **More facilities:** Facilities are primary way of extending and integrating with Windsor container. Multiple other projects, like MVC Contrib, OpenRasta, NServiceBus to name just a few offer their own ready to use facilities that help you use these frameworks with Windsor.

Here's a partial list of facilities for Windsor offered by different other projects.

:information_source: If you know of any other projects offering facilities, go ahead and add them to the list.

* [ASP.NET MVC](http://mvccontrib.codeplex.com/)
* [WCF Session Facility](http://www.sharparchitecture.net/) - Part of [Sharp Architecture project](http://www.sharparchitecture.net/), this facility may be registered within your web application to automatically look for and close WCF connections.  This eliminates all the redundant code for closing the connection and aborting if any appropriate exceptions are encountered.
* [Workflow Foundation](http://using.castleproject.org/display/Contrib/Castle.Facilities.WorkflowIntegration)
* [NServiceBus](http://ayende.com/Blog/archive/2008/07/13/Putting-the-container-to-work-Refactoring-NServiceBus-configuration.aspx)
* [MassTransit](http://code.google.com/p/masstransit/source/browse/tags/0.5/Containers/MassTransit.WindsorIntegration/)
* [re-motion](https://www.re-motion.org/blogs/mix/archive/2009/01/21/we-have-a-facility.aspx)
* [Rhino Service Bus](http://hibernatingrhinos.com/open-source/rhino-service-bus/config)
* [Rhino Security](http://bartreyserhove.blogspot.com/2008/08/rhinosecurity-in-practice.html)
* [Quartz.Net](http://github.com/castleprojectcontrib/QuartzNetIntegration) - Provides integration with [Quartz.Net](http://quartznet.sourceforge.net/) jobs and listeners.
* [SolrNet](http://code.google.com/p/solrnet/wiki/Initialization)
* [SolrSharp](http://bugsquash.blogspot.com/2008/07/castle-facility-for-solrsharp.html)
* [Windows Fax Services](http://bugsquash.blogspot.com/2008/01/castle-facility-for-windows-fax.html)
* [App Config Facility](https://github.com/adamconnelly/WindsorAppConfigFacility)

### See also

* [ComponentModel construction contributors](componentmodel-construction-contributors.md)