# ComponentModel construction contributors

[ComponentModel](componentmodel.md) construction contributors are objects that implement `IContributeComponentModelConstruction` interface. As their name implies they construct the [`ComponentModel`](componentmodel.md) into its final state right after it was created.

:warning: **Don't modify `ComponentModel` outside of contributors:** It is discouraged to modify component model elsewhere than in construction contributors. After `ComponentModel` is processed by its construction contributors it should be considered read only. Modifying it at any later point may lead to concurrency issues and other kinds of hard to track down issues.

## The `IContributeComponentModelConstruction` interface

ComponentModel construction contributors are required to implement single method:

```csharp
void ProcessModel(IKernel kernel, ComponentModel model);
```

Based on the information provided by other contributors, kernel, model's configuration or its own state they either inspect or modify the `model` parameter. Windsor uses several of built in contributors itself to set up things like proxying, parameters, lifestyles, lifecycle steps, dependencies etc.

### Writing your own

Writing custom contributor is one of the most common ways of extending/customizing Windsor. For the sake of example let's say we want to make all properties of type `ILogger` on all components to be mandatory (By default property dependencies are optional in Windsor). To do that we could write a contributor that looks like the following:

```csharp
public class RequireLoggerProperties : IContributeComponentModelConstruction
{
    public void ProcessModel(IKernel kernel, ComponentModel model)
    {
        model.Properties
            .Where(p => p.Dependency.TargetType == typeof(ILogger))
            .All(p => p.Dependency.IsOptional = false);
    }
}
```

The contributor scans all property dependencies of each component, trying to find ones that have type `ILogger` and marks them as mandatory.

### Plugging the contributor in

When you create your contributor you need to add it to the collection of contributors on container's `ComponentModelBuilder`:

```csharp
container.Kernel.ComponentModelBuilder.AddContributor(new RequireLoggerProperties());
```

### External resources

* [Blog post by Mark Seemann (Apr 26, 2010)](http://blog.ploeh.dk/2010/04/26/ChangingWindsorLifestylesAfterTheFact.aspx)
* [Blog post by Ayende (Mar 11, 2007)](http://ayende.com/Blog/archive/2007/03/11/AOP-With-Windsor-Adding-Caching-to-IRepositoryT-based-on-Ts.aspx)