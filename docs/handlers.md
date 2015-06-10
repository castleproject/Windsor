# Handlers

## What is a handler

Handlers are types implementing `IHandler` interface. Windsor uses handlers to resolve components for given services, and then to release them. Handlers also give access to [`ComponentModel`](componentmodel.md) which allows developers to programatically inspect components.

## See also

* [Services and Components](services-and-components.md)
* [ComponentModel](componentmodel.md)
* [How components are created](how-components-are-created.md)