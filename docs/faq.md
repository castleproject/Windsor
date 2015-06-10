# Frequently Asked Questions

## How do I interact with the container. I mean how and where do I actually **call** it?

Windsor is an Inversion of Control Container, that means you generally don't call it, and most of your app should be unaware of/oblivious to its presence. Interaction with the container (actually calling any methods on the container) is limited to three points in your application lifetime:

* When the app starts (`Main`, `Application_Start` etc) you create the container, and call its `Install` method. Once. Notice you should only have one instance of the container.
* Then at single point (in `Main`, `ControllerFactory` etc) you are allowed to call `Resolve`. If you need to callback to the container to pull some additional dependencies later on, use [typed factories](typed-factory-facility.md).
* When your application ends you call `Dispose` on the container to let it clean up and release all the components.

### See also

* [More in-depth discussion of how to interact with the container](three-calls-pattern.md)
* [Inversion of Control](ioc.md)
* [Windsor Installers](installers.md)
* [Fluent Registration API](fluent-registration-api.md)
* [Typed Factory Facility](typed-factory-facility.md)

### External resources

* [Blog post by Nicholas Blumhardt explaining thought process behind this usage (Dec 26, 2008)](http://blogs.msdn.com/b/nblumhardt/archive/2008/12/27/container-managed-application-design-prelude-where-does-the-container-belong.aspx)
* [Blog post by Krzysztof Koźmic introducing container usage (in push scenarios) (Jun 20, 2010](http://kozmic.pl/2010/06/20/how-i-use-inversion-of-control-containers/)
* [Blog post by Krzysztof Koźmic explaining container usage (in pull scenarios) (Jun 22, 2010](http://kozmic.pl/2010/06/22/how-i-use-inversion-of-control-containers-ndash-pulling-from/)

## Why won't Windsor inject itself (`IWindsorContainer`) into my components?

Because your components aren't supposed to be calling Windsor. This goes against the very principle of Inversion of Control. Or from practical point of view - will cause you pain and is a decision you'll regret.

### So what should I do instead

See the first question.

## Why is Windsor keeping reference to my transient components?

Windsor, by default tracks all components to ensure proper [lifecycle](lifecycle.md) management, in particular ensure that all `IDisposable` components and their dependencies will be properly disposed. You can tell Windsor to stop tracking components by setting its [release policy](release-policy.md) to `NoTrackingReleasePolicy` but be aware that this is discouraged, and you're giving up proper lifecycle management by doing so.

## Why is Windsor not able to inject array or list of components?

Windsor, by default when you have dependency on `IFoo[]`, `IEnumerable<IFoo>` or `IList<IFoo>` will check if you have a component registered for that exact type (array or list of `IFoo`), not if you have any components registered for `IFoo` (array of components, is not the same as a component which is an array). You can change the behavior to say *"When you see array or list of `IFoo` just give me all `IFoo`s you can get"* you use `CollectionResolver`. See [Resolvers](resolvers.md).

## Can I register a component with more than one service?

Yes, you can. This ability is called [forwarded types](forwarded-types.md).

## Can I register more than one component for any given service?

Yes, you can. However you will have to give the components unique names.

## Why can't Windsor resolve concrete types without registering them first?

Because that leads to more problems than it solves. There's no good default for how such components should be configured. Also doing this might actually mask problems in your registration - you might have wanted to register a type, forget to do so, and then get mis-configured objects.

Having said that - there is a way to resolve components without registering them by using [Lazy Component Loaders](lazy-component-loaders.md), and Windsor (some facilities to be precise, like [WCF Integration Facility](wcf-facility.md) and [Typed Factory Facility](typed-factory-facility.md)) take advantage of that. You can too, but it's you who sets the rules for how that objects should be configured, not the container.

## Can Windsor inject properties into existing objects?

No, it can't.

## Is `null` a valid value for a dependency?

No, it's not. `null` means *no value* and is ignored by Windsor. Be explicit - if the value is optional provide overloaded constructor that does not include it. Alternatively specify explicitly `null` as the default value in the signature of the constructor. This is the only scenario where Windsor will allow passing null.