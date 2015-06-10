# Changes to lifestyles and release policies in Windsor codename "Wawel"

:warning: **Unreleased code:** Please note that this page talks about future version of Windsor (codenamed Wawel) which is currently not in any stable form. This means that information contained herein can get quickly out of date and final product may expose different behavior/API. Your feedback here is greatly appreciated.

Windsor codename Wawel, introduces some major changes in API and behavior of lifestyles. Those changes are targeted at giving lifestyle managers more control over tracking of the components they... manage.

## Changes

Number of changes in the API was made. Also semantics of some methods have changed.

### The `Track` method

The `ILifestyleManager` has one new method:

```csharp
void Track(Burden burden, IReleasePolicy releasePolicy);
```

The method has default implementation in `AbstractLifestyleManager` which replicates the behavior from previous versions (save for the differences in behavior of `IReleasePolicy` discussed below).

The goal of this method is to fix and properly encapsulate the decision making process that Windsor goes through to