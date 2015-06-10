# How constructor is selected

When deciding which constructor to invoke in order to instantiate a component, Windsor will use the following strategy.

* It will look at the component's constructors and see which of them it can satisfy (for which of them it can provide all required parameters).
* It will then see how many parameters each satisfiable constructor has, and pick the one with most parameters (the greediest one).
* If there is more than one greediest constructor, it will use any of them. It is undefined which one and you should not depend on Windsor always picking the same.
* (As of Windsor 3.2.x) If the attribute `Castle.Core.DoNotSelectAttribute` is applied to a constructor, it will not be selected, notwithstanding any other criteria.

## See also

* [How components are created](how-components-are-created.md)
* [How properties are injected](how-properties-are-injected.md)