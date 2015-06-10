# Event Wiring Facility

Facility to allow components to dynamically subscribe to events offered by other components. We call the component that offers events publishers and the components that uses them, subscribers.

With the Event Wiring Facility you can use the configuration to connect component's methods (subscribers) to component's events (publishers).

## Example

Consider the following classes:

```csharp
public class SimplePublisher
{
    public event PublishEventHandler Event;

    public SimplePublisher()
    {
    }

    public void Trigger()
    {
        if (Event != null)
        {
            Event(this, new EventArgs());
        }
    }
}

public class SimpleListener
{
    public SimpleListener()
    {
    }

    public void OnEvent(object sender, EventArgs e)
    {
        ...
    }
}
```

The class `SimplePublisher` exposes the event `Event` and we want to make the method `OnPublishEvent` on `SimpleListener` subscribe to this event.

## Usage from XML

Just install the facility and add the proper configuration. You need to configure only subscribers.

```xml
<configuration>
   <facilities>
      <facility
         id="event.wiring"
         type="Castle.Facilities.EventWiring.EventWiringFacility, Castle.Windsor" />
   </facilities>
</configuration>
```

The to wire the events we use the configuration:

```xml
<configuration>
   <facilities>
      <facility
         id="event.wiring"
         type="Castle.Facilities.EventWiring.EventWiringFacility, Castle.Windsor" />
   </facilities>
   <components>
      <component
         id="SimpleListener"
         type="Castle.Facilities.EventWiring.Tests.Model.SimpleListener, Castle.Facilities.EventWiring.Tests" />
      <component
         id="SimplePublisher"
         type="Castle.Facilities.EventWiring.Tests.Model.SimplePublisher, Castle.Facilities.EventWiring.Tests" >
         <subscribers>
            <subscriber id="SimpleListener" event="Event" handler="OnEvent"/>
         </subscribers>
      </component>
   </components>
</configuration>
```

In the subscribers node, you list all the components you want to subscriber to the event you're publishing (by id), you specify which event you want to publish to that particular subscriber, and which method should handle the event. You can publish multiple events on a component, you can publish single event to multiple subscribers, and single subscriber can be subscribed to multiple events. There's no limitations here.

## Usage from code

Alternatively, in version 2.5 it is (finally!) possible to configure the facility via fluent API.

First you have to add the facility to the container:

```csharp
container.AddFacility<EventWiringFacility>();
```

Also you need to have the following namespace in scope:

```csharp
using Castle.Facilities.EventWiring;
```

When you import the namespace a new extension method called `PublishEvent` (with few overloads) will become available for your registration, and you can use it like this:

```csharp
container.Register(
   Component.For<SimplePublisher>()
      .PublishEvent(p => p.Event += null,
                    x => x.To<SimpleListener>("foo", l => l.OnEvent(null, null))),
   Component.For<SimpleListener>().Named("foo"));
```

:information_source: **How to read that?:** If you're having hard time reading the API, try reading it like this: "publish event named *Event* to component *foo* handled by method *OnEvent*".

The `PublishEvent` method takes two parameters:

* A delegate that performs fake subscription to the event (the `p => p.Event += null` bit). This may look ugly, but given in .NET `+=` and `-=` are only operation we can perform on an event, that's the only way we can tell Windsor which event we're interested in, in a strongly typed, refactoring friendly way. Alternative overload exists that takes the name of the event as string, so we could just pass in `"Event"`.
* A delegate that describes the subscribers of the event. first argument of `To` is the id of the subscriber (which you specify via `Named`) , second is expression pointing to the method that should handle the event. There's also an overload that takes a string instead of expression, which you can use to specify non-public method as handler for the event.

:information_source: **Specifying multiple subscribers:** You can chain multiple calls to `To` in order to specify multiple subscribers of the event.

:warning: **The Event Wiring Facility won't support wiring in the subscribers (even for singleton components) side, only on the publishers:** In other words:

* If your subscriber starts before your publisher it won't be wired.
* If your subscriber is a transient one, only one (the first instantiated) will be wired.}

The wiring will happen once the publisher component is created. We recommend that you use the [Startable Facility](startable-facility.md) together with the Event Wiring so you can start the publishers as soon as the components are ready.

:warning: The Event Wiring facility must be registered/declared before the Startable facility. Otherwise, the order that the components are declared/registered will be relevant.

## External resources

* [Blog post by Mike Hadlow about the facility (Jan 10, 2010)](http://mikehadlow.blogspot.com/2010/01/10-advanced-windsor-tricks-6-event.html)
* [Blog post by Hamilton Verissimo. It is a discussion of the benefits of using Events to decrease coupling in application design. It uses the Event Wiring Facility as a component of the overall design. (Feb 4, 2007)](http://hammett.castleproject.org/?p=115)