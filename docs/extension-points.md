# Extension Points

Windsor does not try to support every scenario and every capability out of the box. Instead it exposes a rich set of extension points that you can plug your own logic into to extend or modify how Windsor behaves:

* [SubSystems](subsystems.md) - innermost of Windsor's extension points. Very rarely extended/swapped.
* [Facilities](facilities.md) - primary extension point. They usually encompass one or more of extension points listed below.
* [ComponentModel construction contributors](componentmodel-construction-contributors.md) - inspect or modify [ComponentModel](componentmodel.md).
* [Handler Selectors](handler-selectors.md) - custom logic overriding how components are selected. Often used in multi tenant applications.
* [Model Interceptors Selectors](model-interceptors-selectors.md) - custom logic dynamically selecting interceptors for given component.
* [Lazy Component Loaders](lazy-component-loaders.md) - just in time component registration. Especially targeted at pulling component information from other frameworks, like MEF or WCF or resolving un-pre-registered concrete types.
* [Lifecycle concerns](lifecycle.md) - execute logic when component instance gets created / decommissioned.
* [Lifestyle managers](lifestyles.md) - control when object instances should be created / reused and when to end their lifetime.
* [Component Activators](component-activators.md) - control how components are instantiated.
* [Release Policy](release-policy.md) - handle tracking and releasing of components.
* [Resolvers](resolvers.md) - override component resolution logic
* [Container Events](container-events.md) - notify of events in the container
* MORE