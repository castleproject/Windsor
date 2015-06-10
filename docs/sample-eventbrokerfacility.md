# Extensibility Sample App - EventBrokerFacility

The EventBroker sample shows how you would go about extending and customising Windsor. The app contains a simple [facility](facilities.md) that internally uses [ComponentModel construction contributors](componentmodel-construction-contributors.md) and [lifecycle concerns](lifecycle.md) to provide its functionality.

:information_source: **Get the code:** You can get the code for the sample [here](http://github.com/kkozmic/Castle.Samples.Extensibility).

:information_source: The sample is based on a code contributed by Jason Meckley

## What it does

The app provides a simple implementation of the event broker pattern revolving around two interfaces:

```csharp
public interface IEventPublisher
{
    void Publish<T>(T message);
}

public interface IListener<T>
{
    void Handle(T message);
}
```

Objects wishing to publish an event consume an `IEventPublisher` and use it to broadcast the event. Objects wishing to be notified of certain events implement the `IListener<T>` interface where `T` is the type of message that they are interested in.

Routing of messages to listeners is done (unsurprisingly) by the `EventBroker` class which is the implementation of `IEventPublisher` interface.

### `EventBrokerFacility`

By implementing `IListener<T>` listeners expose enough information for Windsor to discover them, subscribe to the message when they are created and unsubscribe when their lifetime ends. However Windsor obviously does not know about our EventBroker so we need to extend it with this knowledge.

We do this via a facility which will encapsulate all of our extension logic. The following snippet is the full implementation of the facility:

:warning: **Remember - this is sample code:** Remember that this is sample code which has its sole purpose of showcasing how you would go about extending Windsor yourself. It does not meet robustness, security, performance and other requirements of a production quality code.

```csharp
public class EventBrokerFacility : AbstractFacility
{
    protected override void Init()
    {
        Kernel.Register(
            Component
                .For<SynchronizationContext>()
                .ImplementedBy<WindowsFormsSynchronizationContext>()
                .LifeStyle.Singleton,
            Component
                .For<IEventPublisher, IEventRegister>()
                .ImplementedBy<EventBroker>()
                .DependsOn(new { listeners = new List<object>() })
                .LifeStyle.Singleton
        );

        Kernel.ComponentModelBuilder.AddContributor(new EventBrokerContributor());
    }
}
```

All the facility does is register two components, including our `EventBroker` and add a [custom ComponentModel construction contributors](componentmodel-construction-contributors.md) which we'll discuss shortly. Notice that `EventBroker` is registered as both `IEventPublisher` as we mentioned previously and `IEventRegister` which we'll also discuss in a moment.

### `EventBrokerContributor`

The contributor analyses components being registered looking for listeners and when it finds one it attaches custom lifestyle concerns for registering/unregistering the component's instances with the EventBroker. Here's its entire code

```csharp
public class EventBrokerContributor : IContributeComponentModelConstruction
{
    public void ProcessModel(IKernel kernel, ComponentModel model)
    {
        if (model.ImplementationIsAListener() == false)
        {
            return;
        }

        var broker = kernel.Resolve<IEventRegister>();
        model.Lifecycle.Add(new RegisterWithEventBroker(broker));
        model.Lifecycle.Add(new UnregisterWithEventBroker(broker));
    }
}
```

### Lifestyle concerns

The two [lifecycle concerns](lifecycle.md) subscribe component instance to receive messages right after it is created, and unsubscribe it when its lifetime ends. They also are very simple.

```csharp
public class RegisterWithEventBroker : ICommissionConcern
{
    private readonly IEventRegister broker;

    public RegisterWithEventBroker(IEventRegister broker)
    {
        this.broker = broker;
    }

    public void Apply(ComponentModel model, object component)
    {
        broker.Register(component);
    }
}

public class UnregisterWithEventBroker : IDecommissionConcern
{
    private readonly IEventRegister broker;

    public UnregisterWithEventBroker(IEventRegister broker)
    {
        this.broker = broker;
    }

    public void Apply(ComponentModel model, object component)
    {
        broker.Unregister(component);
    }
}
```

### Summary

This sample demonstrates how easy it is to extend Windsor using various extensibility mechanisms and how to encapsulate them together using a facility.