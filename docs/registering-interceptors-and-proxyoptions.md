# Registering Interceptors and ProxyOptions

Windsor exposes rich AOP abilities by using Castle DynamicProxy.

## Interceptors

Interceptors are means to inject code around method's invocation.

Using Fluent API you can specify interceptors to use for specified components:

```csharp
container.Register(
    Component.For<ICalcService>()
        .Interceptors(InterceptorReference.ForType<ReturnDefaultInterceptor>()).Last,
    Component.For<ReturnDefaultInterceptor>()
);
```

Notice we register service for an interface without implementation. In this case, interceptor provides implementation for `ICalcService`'s methods. We're using explicit `InterceptorReference` to specify interceptors. By doing so we can specify where we want to put our interceptor in the interception pipeline. If we don't need that, we can use any of other overloads:

## Ordering interceptors

As mentioned above, you can control the order in which interceptors are applied for each component, by using `.Last`, `.First`, or `.AtIndex()` when declaring interceptor references. If you don't care about interceptor ordering, use `.Anywhere`

## Selecting specific interceptors for specific methods

By default all interceptors you specify will be used for each interceptable method. To control which interceptors to use for which method you use `InterceptorSelector` (classes implementing `IInterceptorSelector` interface), which you can specify like this:

```csharp
container.Register(
    Component.For<ICatalog>()
        .ImplementedBy<SimpleCatalog>()
        .Interceptors(InterceptorReference.ForType<DummyInterceptor>())
            .SelectedWith(new FooInterceptorSelector()).Anywhere,
    Component.For<DummyInterceptor>()
);
```

Now `FooInterceptorSelector` will be called upon first call to each method to decide which interceptors should be used for that method.

## Proxy options

In addition to specifying interceptors and interceptor selector, there's a number of other proxy-related options you can specify which are exposed via Proxy property. For example, you can use it to specify objects you want to mix in with your service:

```csharp
container.Register(
    Component.For<ICalcService>()
        .ImplementedBy<CalculatorService>()
        .Proxy.MixIns(new SimpleMixIn())
);
```

:information_source: **Read more about *Castle DynamicProxy*:** Proxies and AOP in Castle is much broader topic than presented here. To learn more about DynamicProxy that underpins it, read the [following tutorial](http://kozmic.net/dynamic-proxy-tutorial/).

## See also

* [Castle DynamicProxy](https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md)
* [Interceptors](interceptors.md)