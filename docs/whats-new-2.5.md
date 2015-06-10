# What's new in Windsor 2.5

## Introduction

Windsor 2.5 is a transition release, highly compatible with previous 2.x releases, at the same time making room for some major improvements in 3rd generation of the container. This release also contains Dynamic Proxy 2.5, Dictionary Adapter and few standalone facilities. Full changelog and list of breaking changes are included in the package. Below are the most important changes in the release (in no particular order).

## Five versions

The release comes in five different flavors, targeting five frameworks/versions.

* .NET 3.5
* .NET 4.0 Client Profile
* .NET 4.0 full version
* Silverlight 3
* Silverlight 4

The different versions have varying feature sets depending on the API and features exposed by the underlying platform.

## Two assemblies

Castle Windsor framework now requires just two assemblies to run as opposed to four previously.

* `Castle.Core.dll` which now in addition to what was provided in previous versions contains DynamicProxy framework and Dictionary Adapter.
* `Castle.Windsor.dll` - which now in addition to what was provided in previous versions contains all the types that previously lived in `Castle.MicroKernel.dll` as well as several container specific types that were moved out of `Castle.Core.dll`. This simplifies management and alleviates false perception that some people shared, that Windsor was bloated because it had four assemblies, while some other containers were distributed with smaller number of assemblies.

Notice that additional facilities still reside in their own assemblies.

## Installers

One of major improvements in this release is making it easier to work with [Windsor Installers](installers.md). It is now possible to automatically discover and install multiple installers in one go, [using FromAssembly class](installers.md#fromassembly-class). Also support for [discovering installers from XML config](registering-installers.md) was added.

## Typed Factory Facility improvements

A lot of work went into the [Typed Factory Facility](typed-factory-facility.md). It now performs better, is easier to extend easier to customize and supports [`delegate`-based factories](typed-factory-facility-delegate-based.md), in addition to [`interface`-based factories](typed-factory-facility-interface-based.md). It also now supports resolving of collections of objects, out of the box (arrays, lists and enumerables).

## More flexible proxying

Elements of proxy building pipeline can now be specified in a more flexible manner. You can specify mixins, proxy generation hooks, interceptor selectors not just as instances, but you can designate other components from the container to satisfy these roles. This is also supported from XML configuration as well.

## Improved working with arguments

Multiple small improvements were made around working with in-line arguments and dependencies. It is now possible to specify them via type of the argument/dependency in addition to name. This works for service overrides, dynamic parameters, static dependencies, and arguments passed from the call site. Also new `Arguments` class and `Insert` extension method were introduced to streamline working with dependencies.

## Improvements in fluent API

Multiple improvements were made in fluent registration API. Methods like `If`, `Unless` or `WithService` are now cumulative. `UsingFactoryMethod` method no longer requires Factory Support Facility to be registered. New fluent API for Event Wiring Facility was introduced. Configuration for other facilities was also streamlined. The API now contains also several new methods to cover common scenarios, like registering types from multiple assemblies located in a location, selecting default service for a component, matching and configuring components via attributes etc.