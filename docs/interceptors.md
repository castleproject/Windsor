# Interceptors

Windsor can take full advantage of underlying power of [Castle DynamicProxy](https://github.com/castleproject/Core) to offer interesting capabilities.

:information_source: **Learn more about DynamicProxy:** It is very valuable when using features described here, to have good understanding of how DynamicProxy works and its limitations. See [DynamicProxy documentation here](https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md).

## How to create a proxy

You don't have to specify explicitly that you want your component to be a proxy. Windsor will create proxy of the component if any of the following is true:

* There are [interceptors](https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md) specified for that component (that includes interceptors selected dynamically via [model interceptors selectors](model-interceptors-selectors.md))
* There are mixins specified for that component
* There are additional interfaces specified for that component

:information_source: **Other cases:** There may be also other cases when any of these conditions becomes true indirectly. For example [Typed Factory Facility](typed-factory-facility.md) works by creating proxies implicitly. Other facilities are also free to modify your components so that they will be proxied.

## Specify Interceptors

You can specify interceptors and other proxy options for components in three ways:

* Using [fluent registration API](registering-interceptors-and-proxyoptions.md)
* Using [XML configuration](registering-components.md)
* Using Attributes (just for interceptors)

## `InterceptorAttribute`

If you want to explicitly associate interceptor with a component you can do it with `InterceptorAttribute`

```csharp
public interface IOrderRepository
{
   Order GetOrder(Guid id);
}

[Interceptor("cache")]
[Interceptor(typeof(LoggingInterceptor))]
public class OrderRepository: IOrderRepository
{
   Order GetOrder(Guid id)
   {
      // some implementation
   }
}
```

Few things to note about the attribute:

* You put it on component class, not interface.
* You can specify multiple interceptors by putting multiple attributes on the class.
* You can specify interceptors either by type or by name.

:warning: **`InterceptorAttribute` and classes:** The `InterceptorAttribute` is defined in a way that allows you to put it not only on classes, but on any other target where custom attribute is allowed, like interface or a method. However Windsor will ignore the attribute unless it's on component's implementation class. This permissive behavior was introduced to allow people to add support for other targets by building custom extensions.

## Using interceptors

In order to use interceptors you have to register them in the container just like any other services.

```csharp
container.Register(
   Component.For<LoggingInterceptor>().Lifestyle.Transient,
   Component.For<CacheInterceptor>().Lifestyle.Transient.Named("cache"),
   Component.For<IOrderRepository>().ImplementedBy<OrderRepository>());
```

When you then resolve `IOrderRepository` Windsor will first create a proxy, and use the two interceptors to intercept calls to its members.

:information_source: **Make interceptors transient:** It is strongly advised that you always make your interceptors transient. Since interceptors can intercept multiple components with various lifestyles it's best if their own lifespan is no longer than the component they intercept. So unless you have a very good reason not to, always make them transient.

## `IOnBehalfAware` interface

Sometimes in order to function properly interceptor needs not just the information from the `IInvocation` but also from the `ComponentModel` of the component it is intercepting. For example it wants to cache some information about the component upfront before intercepting any call, or it needs some information from extended properties of the component in order to do its job.

In such cases interceptors should implement the `IOnBehalfAware` interface, which has just single method.

```csharp
void SetInterceptedComponentModel(ComponentModel target);
```

When instantiating the interceptor and attaching it to the component, Windsor will then look if any of the interceptors selected for the component implements `IOnBehalfAware`, and for all that do, it will invoke its `SetInterceptedComponentModel` passing `ComponentModel` of the component being created.

## Mixins

You can specify mixins using either fluent API or XML configuration.

:information_source: **Match lifestyles:** When mixing components keep in mind their lifestyle. Same rules apply as when building direct dependencies between components. That's why it's best if lifestyles of mixed components match.

## Additional interfaces

You can specify additional interfaces using either fluent API or XML configuration.