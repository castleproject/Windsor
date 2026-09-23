# Castle.Windsor.Extensions.DependencyInjection

This package adapts Castle Windsor to the Microsoft.Extensions.DependencyInjection abstractions.
It lets a Microsoft host build an `IServiceProvider` backed by a Windsor container while preserving the most important Microsoft DI behaviors around lifetimes, scopes, `IEnumerable<T>`, and "last registration wins" resolution.

## Typical Usage

Use the hosting extension from `Castle.Windsor.Extensions.Hosting`:

```csharp
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
	.UseWindsorContainerServiceProvider()
	.ConfigureServices(services =>
	{
		services.AddSingleton<IMyService, MyService>();
	})
	.Build();
```

You can also pass an existing Windsor container:

```csharp
var container = new WindsorContainer();

var host = Host.CreateDefaultBuilder(args)
	.UseWindsorContainerServiceProvider(container)
	.ConfigureServices(services =>
	{
		services.AddScoped<IUnitOfWork, UnitOfWork>();
	})
	.Build();
```

The entry point is `WindsorServiceProviderFactory`, which implements:

```csharp
IServiceProviderFactory<IWindsorContainer>
```

Microsoft hosting calls this factory during host construction:

1. `CreateBuilder(IServiceCollection services)` creates or reuses the Windsor container and registers the Microsoft service collection into it.
2. `CreateServiceProvider(IWindsorContainer container)` resolves the root `IServiceProvider` from Windsor.

## High-Level Flow

The main implementation is `WindsorServiceProviderFactoryBase`.

When `CreateBuilder` is called, it delegates to `BuildContainer`. That method:

1. Ensures a root Windsor container exists.
2. Registers Windsor itself as `IWindsorContainer`.
3. Registers `WindsorScopedServiceProvider` as the implementation for `IServiceProvider` and related Microsoft service provider interfaces.
4. Registers `WindsorScopeFactory` as `IServiceScopeFactory`.
5. Registers the factory itself as `IServiceProviderFactory<IWindsorContainer>`.
6. Adds sub-resolvers required for Microsoft DI behavior.
7. Converts every `ServiceDescriptor` from the `IServiceCollection` into Windsor registrations.

The root service provider is a Windsor component. Calling `CreateServiceProvider` simply does:

```csharp
return container.Resolve<IServiceProvider>();
```

## How ServiceDescriptor Is Converted

The conversion happens through `ServiceDescriptorExtensions.CreateWindsorRegistration`.

For each Microsoft `ServiceDescriptor`, the adapter creates a Windsor `IRegistration` using `RegistrationAdapter`.

The adapter supports:

- `ImplementationType`
- `ImplementationInstance`
- `ImplementationFactory`
- open generic service registrations
- keyed services on target frameworks where Microsoft keyed services are available

The adapter then maps the Microsoft lifetime to a Windsor lifestyle:

| Microsoft lifetime | Windsor mapping |
| --- | --- |
| `Singleton` | `NetStatic()` |
| `Scoped` | `ScopedToNetServiceScope()` |
| `Transient` | `LifestyleNetTransient()` |

Each registration is tagged with an extended property:

```csharp
"microsoft-di-registered"
```

That marker is important when resolving services with multiple registrations. It lets the provider distinguish services registered through Microsoft DI from services registered directly in Castle Windsor.

## Registration Order And "Last Wins"

Castle Windsor and Microsoft DI have different default behavior when more than one component is registered for the same service.

Castle Windsor normally resolves the first matching registration.

Microsoft DI resolves the last registration for `GetService<T>()`, while `IEnumerable<T>` returns all matching registrations in registration order.

This package adjusts behavior for registrations imported from `IServiceCollection`:

- Every Microsoft DI registration is marked as adapter-created.
- Adapter-created registrations are configured with `.IsDefault()`.
- `WindsorScopedServiceProvider` checks whether any candidate was registered through Microsoft DI.
- If Microsoft DI registrations are involved, it uses Microsoft-style resolution rules.

For non-keyed single service resolution, when multiple handlers exist:

1. If none of them came from Microsoft DI, normal Windsor behavior is preserved.
2. If at least one came from Microsoft DI, the provider chooses the last matching non-open-generic registration.
3. If only open generic registrations are available, the provider chooses the last matching registration.

This keeps normal Windsor applications working while making `IServiceProvider.GetService<T>()` behave like Microsoft DI for services imported from `IServiceCollection`.

## Naming Subsystem

The package replaces Windsor's default naming subsystem with `DependencyInjectionNamingSubsystem`.

The custom subsystem exists because Microsoft DI needs registration-order behavior in places where Windsor's default selection rules are different.

`DependencyInjectionNamingSubsystem.GetHandlers(Type service)` returns handlers in registration order. This is important for:

- `IEnumerable<T>` resolution
- selecting the last Microsoft DI registration
- preserving Microsoft service collection ordering

For direct `GetHandler(Type service)` calls, the subsystem still handles generic service lookup and falls back to base Windsor behavior where appropriate.

## IServiceProvider Resolution

The implementation of `IServiceProvider` is `WindsorScopedServiceProvider`.

It implements:

- `IServiceProvider`
- `ISupportRequiredService`
- `IDisposable`
- `IServiceProviderIsService` on supported target frameworks
- keyed service provider interfaces on supported target frameworks

When `GetService` or `GetRequiredService` is called, it resolves through the Windsor container, but inside the current Microsoft DI scope:

```csharp
using (_ = new ForcedScope(scope))
{
	return ResolveInstanceOrNull(serviceType, isOptional);
}
```

That forced scope makes Windsor lifestyle resolution use the current Microsoft scope instead of an unrelated ambient Windsor scope.

If the requested service is `IEnumerable<T>`, the provider resolves all non-keyed handlers for `T` and returns a typed list. If all registered services are non-keyed, it can use `container.ResolveAll(typeToResolve)`. If keyed and non-keyed services are mixed, it resolves only the non-keyed handlers one by one.

## Scope Model

The package uses custom Windsor scope accessors to model Microsoft DI scopes.

Important types:

- `ExtensionContainerRootScope`
- `ExtensionContainerScope`
- `ExtensionContainerScopeBase`
- `ExtensionContainerScopeAccessor`
- `ExtensionContainerRootScopeAccessor`
- `WindsorScopeFactory`
- `ServiceScope`
- `ForcedScope`

The factory creates one root scope when it is constructed. `WindsorScopeFactory.CreateScope()` creates child scopes for Microsoft `IServiceScope`.

Scoped services are mapped through:

```csharp
ScopedToNetServiceScope()
```

which uses:

```csharp
ExtensionContainerScopeAccessor
```

Singleton services are mapped through:

```csharp
NetStatic()
```

By default, `NetStatic()` can either map to regular Windsor singleton behavior or to the root Microsoft scope depending on `WindsorDependencyInjectionOptions.MapNetStaticToSingleton`.

Transient services are mapped through:

```csharp
LifestyleNetTransient()
```

This is intentionally not just Windsor transient. Microsoft DI transient disposables are released when the owning service scope is disposed. To model that, `LifestyleNetTransient()` marks the component as transient and scopes disposal to the Microsoft service scope.

## Sub-Resolvers

`WindsorServiceProviderFactoryBase.AddSubResolvers` adds these resolvers:

- `RegisteredCollectionResolver`
- `OptionsSubResolver`
- `LoggerDependencyResolver`

`RegisteredCollectionResolver` helps resolve registered collections in a Microsoft DI compatible way.

`OptionsSubResolver` handles Microsoft options patterns.

`LoggerDependencyResolver` resolves loggers and uses `RegistrationAdapter.OriginalComponentName` to strip generated suffixes from adapter-created component names.

On target frameworks with keyed service support, the factory also adds `KeyedServicesSubDependencyResolver`.

## Keyed Services

On supported target frameworks, the adapter supports Microsoft keyed services.

Keyed registrations are tracked through `KeyedRegistrationHelper`. The helper creates stable Windsor component names for keyed services and keeps metadata that allows:

- resolving a single keyed service
- resolving keyed `IEnumerable<T>`
- answering `IServiceProviderIsKeyedService`

Non-keyed resolution deliberately ignores keyed handlers. This matches Microsoft DI behavior: requesting a non-keyed service should not accidentally return a keyed registration.

## Performance And Batch Registration

Windsor registration has an important internal cost: after handlers are registered, Windsor may need to raise `HandlersChanged` and re-check components that were waiting for dependencies.

Calling `container.Register(...)` once per component is therefore slower than registering many components in one call.

The adapter now avoids the worst case in two ways.

First, `RegisterServiceCollection` converts all Microsoft service descriptors into an array of Windsor registrations and calls `Register` once:

```csharp
var registrations = serviceCollection
	.Select(service => service.CreateWindsorRegistration(rootContainer))
	.ToArray();

rootContainer.Register(registrations);
```

Second, `BuildContainer` wraps the whole adapter population phase in one Windsor optimized registration scope:

```csharp
var token = (rootContainer.Kernel as IKernelInternal)?.OptimizeDependencyResolution();
try
{
	RegisterContainer(rootContainer);
	RegisterProviders(rootContainer);
	RegisterFactories(rootContainer);

	AddSubResolvers();

	RegisterServiceCollection(serviceCollection);
}
finally
{
	token?.Dispose();
}
```

This matters because `RegisterContainer`, `RegisterProviders`, `RegisterFactories`, and `RegisterServiceCollection` all call `Register` internally. Without an outer optimized scope, each one may cause its own `HandlersChanged` flush.

With the outer optimized scope, nested `Register` calls still register their handlers immediately, but Windsor defers the expensive `HandlersChanged` and `RegistrationCompleted` flush until the outer token is disposed.

## Are Registrations Buffered?

No. The actual component registrations are not delayed until `Dispose`.

This is the key distinction:

- The adapter buffers the list of `IRegistration` objects before calling `rootContainer.Register(registrations)`.
- Windsor then executes those registrations immediately inside `Register`.
- While the optimized scope is active, Windsor defers only the expensive container-change notifications.

In other words, this code:

```csharp
rootContainer.Register(registrations);
```

really registers the components at that point.

What is deferred is the end-of-registration work:

- `HandlersChanged`
- dependency re-checks for handlers in `WaitingDependency`
- `RegistrationCompleted`

That deferred work runs when the outer optimize token is disposed.

So the performance fix does not mean "register everything later." It means "register everything now, but flush the global dependency-change notification once."

## Why `handlersChanged` Is Usually True During Adapter Registration

Inside Windsor, `handlersChanged` becomes true whenever a handler is registered while optimized dependency resolution is active.

That does not mean the handler is wrong or slow by itself. It means at least one handler was registered and Windsor must eventually notify its internal dependency graph that the set of handlers changed.

Before batching, the adapter registered each Microsoft `ServiceDescriptor` separately. With thousands of services, that created thousands of optimized scopes and thousands of possible `HandlersChanged` flushes.

After batching, `handlersChanged` should still become true during service collection population, but it should become true for one outer operation and flush once at the end.

## Direct Windsor Registrations After Build

The batching described above applies to the factory's build phase.

If application code later calls:

```csharp
container.Register(Component.For<IMyService>().ImplementedBy<MyService>());
```

that registration is outside the adapter's batch. It will use Windsor's normal registration behavior.

For high-volume direct Windsor registrations, prefer:

```csharp
container.Register(
	Component.For<A>().ImplementedBy<A>(),
	Component.For<B>().ImplementedBy<B>(),
	Component.For<C>().ImplementedBy<C>());
```

or convention-based registration:

```csharp
container.Register(
	Classes.FromAssemblyContaining<SomeType>()
		.Pick()
		.WithServiceDefaultInterfaces()
		.LifestyleTransient());
```

Avoid calling `container.Register(...)` repeatedly inside large loops unless there is a real need for event boundaries between registrations.

## Extending The Factory

`WindsorServiceProviderFactoryBase` is designed for customization.

The main extension points are:

- `CreateRootContainer`
- `SetRootContainer`
- `AddSubSystemToContainer`
- `BuildContainer`
- `RegisterContainer`
- `RegisterProviders`
- `RegisterFactories`
- `RegisterServiceCollection`
- `AddSubResolvers`

If you override registration methods and add many components, keep batching in mind. Prefer one `Register` call with many `IRegistration` instances. If you need to perform a larger custom registration phase, wrap it in `IKernelInternal.OptimizeDependencyResolution()` when you know the kernel is a Windsor kernel.

## Disposal

Disposing `WindsorServiceProviderFactoryBase` removes the kernel-to-factory mapping used to find the root Microsoft scope for a Windsor kernel.

Disposing `WindsorScopedServiceProvider` disposes the root scope only when the provider owns the root scope. The container itself is intentionally not disposed from `WindsorScopedServiceProvider.Dispose`, because some hosts or frameworks can dispose the service provider while the caller still owns the container lifetime.

Dispose the Windsor container explicitly when your application owns it and no longer needs it.
