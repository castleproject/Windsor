# Model Interceptors Selectors

Model interceptors selectors are types implementing `IModelInterceptorsSelector` interface. They provide custom logic that can modify default set of [interceptors](interceptors.md) associated with the component.

:warning: **Breaking changes in Windsor 2.5:** In Windsor 2.5 `IModelInterceptorsSelector` method's signatures and how it is used have changed.

## `IModelInterceptorsSelector` interface

The `IModelInterceptorsSelector` interface exposes two methods:

### `HasInterceptors` method

The method is invoked by the container to learn if given selector is interested in given component. It has the following signature:

```csharp
bool HasInterceptors(ComponentModel model)
```

### `SelectInterceptors` method

When using the selectors, Windsor will first invoke `HasInterceptors` for each of its selectors, and for these of them that return `true` it will invoke `SelectInterceptors` which performs the actual selection. The method has the following signature:

```csharp
InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
```

The second argument contains the array of interceptor references returned by previous interceptor in the pipeline. The first interceptor, will receive the default set of interceptors from `model.Interceptors` collection. The selector usually will reorder passed interceptors, filter out some of them, or append some more to the collection.

## Attaching selectors

Selectors are attached to ProxyFactory of the kernel, using the following method:

```csharp
var selector = new MyInterceptorSelector();
container.Kernel.ProxyFactory.AddInterceptorSelector(selector);
```