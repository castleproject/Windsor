# Handler Selectors

Handler selectors let you dynamically choose a component to satisfy the requested service and override Windsor's default behavior. This is particularly useful in multi-tenant applications.

:information_source: **Custom naming subsystem:** Only `DefaultNamingSubSystem` works with selectors. All other implementations of `INamingSubSystem` provided out of the box with Windsor ignore selectors.

## How it works

Selectors are types implementing `IHandlerSelector` interface which exposes two methods:

```csharp
bool HasOpinionAbout(string key, Type service);

IHandler SelectHandler(string key, Type service, IHandler[] handlers);
```

The `HasOpinionAbout` method is called to ask the selector if it would like to select a handler for the given service (described by key or/and type). If the call returns `true` then `SelectHandler` will be called which should select a single handler from the given array. The selector can also return `null` in which case the next selector in turn will be asked.

:warning: **The array contains all assignable handlers:** Notice that the `handlers` array passed to the `SelectHandler` method contains all handlers that expose services assignable to `service` (which may mean, in case when `service` is not provided, all handlers in the entire container). This gives you ability to select handler that was not explicitly registered with the `service`. Also take note Windsor does not do any filtering of the handlers it gives you, so some of them may not be resolvable (may have some required dependencies missing), which is something you should take into the account when implementing your selectors.

### The selection algorithm

When more than one selector is registered, Windsor will call `HasOpinionAbout` on each of them in the order in which they were registered until one returns `true`. Windsor will then invoke the `SelectHandler` method, and if non-`null` handler is returned, that handler will be used to satisfy the service. If the call to `SelectHandler` returns `null` Windsor will continue asking the remaining selectors following the same algorithm. If none of the selectors return a handler Windsor will fall back to its default behavior.

### How to use it

You attach selectors to the container using the following method:

```csharp
container.Kernel.AddHandlerSelector(new MySelector());
```

## External resources

* [(05 Oct, 2008) blogpost by Ayende, discussing the feature](http://ayende.com/Blog/archive/2008/10/05/windsor-ihandlerselector.aspx)