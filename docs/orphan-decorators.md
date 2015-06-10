# Decorators

## Specifying decorators using fluent API (AllTypes)

When registering multiple types at once using `AllTypes.` syntax, it is not possible to rely on order of registration to handle decorated services.

For instance suppose we are planning to run some tasks on application startup:

```csharp
using Castle.MicroKernel.Registration;
using Castle.Windsor;

public void RunStartupTasks()
{
    var container = new WindsorContainer();

    container.Register(AllTypes.FromThisAssembly().BasedOn<ITask>());

    var tasks = container.ResolveAll<ITask>();
    foreach (var task in tasks)
    {
        task.Execute();
    }
}
```

Now since the order of execution of these tasks are indeterministic, some work needs to be done if tasks should run on specific order. Take this as an example where the order of execution matters:

```csharp
public interface ITask
{
    void Execute();
}

public class CreateDatabaseTask : ITask
{
    public void Execute()
    {
        //create the database here
    }
}

public class UpgradeSchemaTask : ITask
{
    public void Execute()
    {
        //update the schema of the
        //existing database here
    }
}
```

To handle this kind of scenarios, we may use an instance of `IHandlerSelector` interface and manually specify the order. The handler selector is first asked if it has any opinion about a service interface, and it does, it is asked to provide proper handler. Here's an implementation that solves the issue in our example:

```csharp
public class TaskSelector : IHandlerSelector
{
    public bool HasOpinionAbout(string key, Type service)
    {
        return service == typeof(ITask);
    }

    public IHandler SelectHandler(string key, Type service, IHandler[] handlers)
    {
    }
}
```