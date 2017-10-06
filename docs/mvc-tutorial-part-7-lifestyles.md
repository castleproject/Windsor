# Windsor Tutorial - Part Seven - Lifestyles

We noted in a [previous part](mvc-tutorial-part-6-persistence-layer.md) the `ISession` object from NHibernate is our [Unit of Work](http://martinfowler.com/eaaCatalog/unitOfWork.html). What that means in practical terms, is that in the context of a web request we want to have just one. Also every web request will have a different instance. Then when the web request ends we will dispose of the session thus flushing all the changes to the database.

## Lifestyles

In Windsor the *sharing* of objects is called [lifestyle](lifestyles.md). If you recall, when we were registering controllers we specified that their lifestyle is Transient. That means that the instance is never shared, and is not bound to any context. Every time we request a transient component we get a new instance. Also transient components that were explicitly `Resolve`d have to be explicitly `Release`d, since Windsor has no idea when their lifetime should end.

In a previous part when we registered `ISessionFactory` and `ISession` we didn't specify any lifestyle. If the lifestyle is not defined, the default of singleton will be used. Singleton means that there is going to be just a single instance in the entire container, for as long as the container is alive (which means - entire lifetime of the application, since that's how long we use the container).

For singleton components an instance is created upon the first request for the component, and then reused for every subsequent request. It is destroyed only when the container gets `Disposed`. This is exactly what we want for `ISessionFactory`. That's one of the first things you learn about NHibernate - `ISessionFactory` is big, heavyweight, thread-safe, and you only should ever have one per database.

That's great - all fine there.

That's however as far as we can get for what we want from `ISession`. When it's a singleton our changes will never be flushed, hence they'll get lost, instead of getting persisted at the end of request. Also `ISession` is not thread safe so we'll be exposed to multiple bugs if multiple requests try to access it. Also as we get more and more requests, its internal cache will grow and grow, getting slower with every request until we run out of memory. To fix this we have to change its lifestyle to `PerWebRequest`.

### The `PerWebRequest` lifestyle

Please install the `Castle.Facilities.AspNet.SystemWeb` NuGet first. To change the `ISession` lifestyle to be per web request, we need to specify that in the registration. So we need to change it to the following:

```csharp
Kernel.Register(
	Component.For<ISession>()
		.UsingFactoryMethod(k => k.Resolve<ISessionFactory>().OpenSession())
		.LifestylePerWebRequest()
);
```

Now Windsor will call our factory method once in every web request where it needs an `ISession` instance and then reuse this instance in the scope of that web request. When a new web request comes along, the factory method will be called again, asking `ISessionFactory` to `OpenSession` for that new web request, and so on.

:warning: **Releasing components:** Some contexts, like web request have a well defined ends that Windsor can detect. Therefore Windsor knows when to release the per web request objects and it will do it without requiring any action from you. That's not the case for all lifestyles however. This brings us to the `Release` method.

A rather detailed discussion about releasing components can be found in [this blog post](http://kozmic.pl/2010/08/27/must-i-release-everything-when-using-windsor). It is highly recommended that you read it

:information_source: **Importance of lifestyles:** Getting lifestyles *right* when using Windsor (or any other container) is one of the most important aspects of working with a container, and one that has has big impact on your application. Pay attention to what lifestyle you assign to your components. There's no hard and fast rule on which one you should chose so think how the components will be used.

## Summary

One of the most important concepts regarding Inversion of Control containers is the lifetime or lifestyle of the components that are registered within them, which this section discussed. Ensure you understand the concepts, because they are fundamental to using Windsor. Another important part of any IoC container is managing dependencies, which you can read about in [Part Eight - Satisfying Dependencies](mvc-tutorial-part-8-satisfying-dependencies.md).