# Implementing custom scope

Windsor provides a rich set of out of the box [lifestyles](lifestyles.md), that cover a wide range of scenarios. However it is to be expected that in advanced scenarios you will need a custom lifestyle.

In those cases, often you will want an *"instance per something"*, where the *something* may be client application making a request, a message handling request in a messaging application or any other logical or physical *scope*. For those cases Windsor provides easily extensible scoped lifestyle to allow you to quickly build the *instance per your scope* lifestyle into your application.

This might sound complicated, but actually is not, and most of the time, all you'll need to do is to implement single, simple interface - `IScopeAccessor`.

## Accessing the right scope: `IScopeAccessor`

The `IScopeAccessor`'s role is to properly find (and initialize if required) the `ILifetimeScope` appropriate for given context (more on lifetime scope in a moment). The interface is required as follows:

```csharp
public interface IScopeAccessor : IDisposable
{
    ILifetimeScope GetScope(CreationContext context);
}
```

### Example: instance per client

Say you're building a B2B application and you need a particular component to have a per-client-company lifestyle, that is each client gets a reusable instance, for each of their requests.

:information_source: This is not a real-life code so we're going to assume the application runs on a single server.

The scope accessor's job would be to locate and retrieve the scope for the client making current request. One trivial way to implement it might be this:

```csharp
public class PerClientCompanyScopeAccessor : IScopeAccessor
{
    private static readonly ConcurrentDictionary<Guid, ILifetimeScope> collection = new ConcurrentDictionary<Guid, ILifetimeScope>();

    public ILifetimeScope GetScope(Castle.MicroKernel.Context.CreationContext context)
    {
        var companyID = ClientHelper.GetCurrentClientCompanyId();
        return collection.GetOrAdd(companyID, id => new ThreadSafeDefaultLifetimeScope());
    }

    public void Dispose()
    {
        foreach (var scope in collection)
        {
            scope.Value.Dispose();
        }
        collection.Clear();
    }
}
```

Each component with this lifestyle will get its own instance of `PerClientCompanyScopeAccessor`, and they will all have access to the `ILifetimeScope`s through the static `collection` field. Notice that, since we're planning to use the class in multithreaded scenarios, we make the `collection` thread safe.

The `Dispose` method is going to be called by the container, when the container itself is getting `Dispose`d, and the container enforces the method will be only called once for each instance of our scope accessor, and that they will be all called on a single thread, so we don't have to worry about concurrency there.

## The scope abstraction: `ILifetimeScope`

The scope itself, is represented by `ILifetimeScope` interface with each scope instance represents a single client company in our example.

:information_source: **Why two interfaces instead of one?:** It might not be immediately obvious why we need two interfaces; one for the scope and one to abstract how the scope is retrieved, instead of merging the responsibilities into a single abstraction. The reason is longevity. The `IScopeAccessor` lives as long as the container, and each component using it, gets its own instance.
The `ILifetimeScope` is most often shared across multiple components, and its longevity is in no way tied to the components. The per-web-request scope lives as long as a single web request. The call context scope lives from when you call `BeginScope()` to when your `using` block ends and so on. In our per-client-company example the scope may be from when a company becomes a client, to when they decide to no longer be one. Separating those two roles gives you the flexibility most real life scenarios require.

The interface implementors are responsible for keeping track of the instances in the scope. Here's the interface's declaration:

```csharp
public interface ILifetimeScope : IDisposable
{
    Burden GetCachedInstance(ComponentModel instance, ScopedInstanceActivationCallback createInstance);
}
```

:information_source: **The `DefaultLifetimeScope`:** For simple cases, where no additional logic in the scope itself is required, and the scope is not going to be used in a multithreaded scenarios, Windsor provides a non-thread-safe out of the box implementation of the interface called `DefaultLifetimeScope`

### Example: Custom scope

Continuing with our example, we can't use `DefaultLifetimeScope` because it is not thread safe, and we're going to run on a web server which means in an inherently multithreaded environment. Therefore a trivial implementation we might use, could look like this:

```csharp
public class ThreadSafeDefaultLifetimeScope : ILifetimeScope
{
    private static readonly Action<Burden> emptyOnAfterCreated = delegate { };
    private readonly object @lock = new object();
    private readonly Action<Burden> onAfterCreated;
    private IScopeCache scopeCache;

    public ThreadSafeDefaultLifetimeScope(IScopeCache scopeCache = null, Action<Burden> onAfterCreated = null)
    {
        this.scopeCache = scopeCache ?? new ScopeCache();
        this.onAfterCreated = onAfterCreated ?? emptyOnAfterCreated;
    }

    public void Dispose()
    {
        lock (@lock)
        {
            if (scopeCache == null)
            {
                return;
            }
            var disposableCache = scopeCache as IDisposable;
            if (disposableCache != null)
            {
                disposableCache.Dispose();
            }
            scopeCache = null;
        }
    }

    public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
    {
        lock (@lock)
        {
            var burden = scopeCache[model];
            if (burden == null)
            {
                scopeCache[model] = burden = createInstance(onAfterCreated);
            }
            return burden;
        }
    }
}
```

The code is actually `DefaultLifetimeScope` with adding locking.

The `Dispose` method is simpler of the two so let's examine it first. This time we put some locking around it. There are two cases where the method will be called. One, is when the container is being `Dispose`d, by our `PerClientCompanyScopeAccessor`. This is guaranteed to be called only once. However we also `Dispose` the scope where it logically ends. In our example scenario that might be when a client cancels their account. In that case we'll retrieve that client's scope and `Dispose` it, to end the scope. This might be done in a way that allows for threading issues so we put a lock around the method.

The `GetCachedInstance` method retrieves the `Burden` of the component represented by `model` if one exists in the scope already. When we don't yet have an instance of the component in this scope, we delegate the job of creating it to the `createInstance` callback and then cache it for future reuse in the scope.

:information_source: **The `IScopeCache` helper interface:** You might have noticed that the `LockingLifetimeScope` in the example above uses `IScopeCache` to store the tracked objects. The interface is little more than a generic dictionary, with its default implementation `ScopeCache` also providing `IDisposable` implementation to release the instances tracked in the scope. If it suits your scenario, feel free to use it. If not, there's nothing that requires you to.

## See also

* [Lifestyles](lifestyles.md)