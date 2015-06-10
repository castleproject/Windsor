# How properties are injected

Property injection of dependencies is designed to be done during component activation when a component is created. The responsibility of determining which properties are used for injection is fulfilled by default through `PropertiesDependenciesModelInspector` - a `IContributeComponentModelConstruction` implementation which uses all the following criteria to determine if a property represents a dependency:

* Has 'public' accessible setter
* Is an instance property
* If `ComponentModel.InspectionBehavior` is set to `PropertiesInspectionBehavior.DeclaredOnly`, is not inherited
* Does not have parameters
* Is not annotated with the `Castle.Core.DoNotWireAttribute` attribute

If a property meets all these criteria, a dependency model is created for it, and this is then resolved when the component dependencies are resolved during activation.

## Customization

Property injection can be controlled via a number of mechanisms, the most common are listed below.

### DoNotWireAttribute

A simple method to exempt a property from being chosen for injection is to add a `Castle.Core.DoNotWireAttribute` to the property declaration. This method is best for the occasional exception.

```csharp
public class MyComponent
{
   public IFoo Foo { get; set; }

   [DoNotWire]
   public IBar MyOtherDependency { get; set; }
}
```

In the example above, the `MyOtherDependency` has the attribute, and Windsor will ignore it, never trying to set it when creating instances of this component.

### Encapsulization

Making a property setter `protected` or `internal` will prevent it from chosen as a dependency property. This can be useful in designing a service which is also protected against arbitrary setting of properties after resolution by clients, making a tight, well-focused service interface.

### Removing the PropertiesDependenciesModelInspector

A `DefaultKernel` has a `ComponentModelBuilder` which contains a `PropertiesDependenciesModelInspector` in the `Contributors` list. It can be removed and then no property injection will be performed on any component. To remove it, use code similar to the following:

```csharp
// We don't want to inject properties, only ctors
var propInjector = Kernel.ComponentModelBuilder
                         .Contributors
                         .OfType<PropertiesDependenciesModelInspector>()
                         .Single();
Kernel.ComponentModelBuilder.RemoveContributor(propInjector);
```

### Replacing the PropertiesDependenciesModelInspector

`PropertiesDependenciesModelInspector` can be subclassed, and the method `InspectProperties` can be overridden to accomodate specialized logic to support your needed scenario.