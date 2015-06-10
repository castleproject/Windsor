# Container Events

Windsor container uses .NET events to notify external subscribers when something interesting happens. You can subscribe to these events and perform some logic as they occur.

:information_source: **Use facilities:** Primary usage of these events is for [facilities](facilities.md). While there's no limitation for who can subscribe it is considered a good practice to encapsulate this logic within a facility.

## The events

The events are defined on `IKernelEvents` interface that is exposed by container's `Kernel` property. The following events are provided.

### Container events

Events regarding lifetime of the container.

#### `AddedAsChildKernel`

Raised when current container was added as child container of some other container.

#### `RemovedAsChildKernel`

Opposite of the above.

#### `RegistrationCompleted`

:information_source: This event is new in Windsor 3

Raised when registration / installation process is completed, that is right before the container exist `Install` or `Register` (whichever is the outermost).

### `ComponentModel` events

Events regarding lifetime of a [`ComponentModel`](componentmodel.md) in the container.

#### `ComponentRegistered`

Raised when a new component gets registered with the container.

#### `ComponentUnregistered`

Raised when a component gets removed from the container.

#### `ComponentModelCreated`

Raised when a [`ComponentModel`](componentmodel.md) gets created, but after [ComponentModel construction contributors](componentModel-construction-contributors.md) finish their job.

:information_source: **Don't modify the [`ComponentModel`](componentmodel.md) when handling this event:** You should never perform any modification to the [`ComponentModel`](componentmodel.md) passed to this event. Use [ComponentModel construction contributors](componentmodel-construction-contributors.md) for that.

:warning: **Obsolete warning:** It is best if you try not to use the events mentioned above. In future version they may become obsolete and eventually be removed from the API.

### Handler events

Events regarding lifetime of a `IHandler`.

#### `HandlerRegistered`

Raised when a new handler is registered (it might be in a valid or waiting dependency state).

#### `HandlersChanged`

Raised when a new handler (or group of handlers) is registered (they might be in a valid or waiting dependency state). The difference from `HandlerRegistered` is the intent - while the former is concentrated on the newly registered handler, this one is simply a notification that state of the container has changed. Windsor, for example, uses it internally to check if newly registered handlers satisfy missing dependencies of other handlers in `WaitingDependency` state.

#### Component events

Events regarding lifetime of a component instance.

#### `ComponentCreated`

Raised right after component was created.

#### `ComponentDestroyed`

Raised right after component was destroyed.

#### `DependencyResolving`

Raised right after a dependency was resolved. This event is useful to perform some modification on resolved component, similar to [`OnCreate`](registering-components-one-by-one.md#oncreate) method, but globally.

#### `EmptyCollectionResolving`

:information_source: This event is new in Windsor 3

Raised when a collection is being resolved (via `IKernel.ResolveAll` method, or indirectly - via collection resolver) and the collection is empty. Implementors would usually log that fact or potentially throw an exception (especially in development).