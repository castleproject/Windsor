# WCF Facility - Channel Action Policy

WCF Facility replaces standard WCF remoting proxy with Windsor's dynamic proxy. This lets you [intercept](interceptors.md) the call to the channel proxy at a early stage just like with any other Windsor component. You can also intercept it at a later stage, when it's about to reach WCF execution pipeline by using `IChannelActionPolicy`.

:information_source: **So what's the difference?:** Difference between `IChannelActionPolicy` and standard `IInterceptor` is that the former gives you access to some information specific to the WCF action call, whereas the latter is generic purpose interception mechanism.

## The `IChannelActionPolicy` interface

(This section is outdated, `IChannelActionPolicy` is no longer exists, look for `IWcfPolicy` instead)

The channel action policy is one of standard [WCF Facility policies](wcf-facility-policies.md) that lets you execute some logic before the call is made, and optionally cancel it. The interface looks like this:

```csharp
public interface IChannelActionPolicy : IWcfChannelPolicy
{
    /// <summary>
    /// Performs the action using the policies quality of service.
    /// </summary>
    /// <param name="channelHolder">The channel holder.</param>
    /// <param name="method">The method executing.</param>
    /// <param name="action">The action to perform.</param>
    /// <returns>true if the policy was applied.</returns>
    bool Perform(IWcfChannelHolder channelHolder, MethodInfo method, Action action);
}
```

The method `Perform` is called by the facility when a call to one of your WCF proxy methods is about to be invoked.

### Example implementation

Below is a sample, trivial implementation of the interface that retries the call in case of timeout.

```csharp
public class ChannelReconnectOnTimeoutPolicy : AbstractWcfPolicy, IChannelActionPolicy
{
    public bool Perform(IWcfChannelHolder channelHolder, MethodInfo method, Action action)
    {
        try
        {
            action();
        }
        catch (TimeoutException)
        {
            action();
        }
        finally
        {
            return true;
        }
    }
}
```

### Attaching the policy

There are a couple of ways you can attach a channel action policy to your client side proxies.

#### As a component

To attach the policy all you need to do is to register it in the container. It will be then picked up and attached to every client proxy managed by Windsor and WCF Facility.

```csharp
container.Register(Component.For<MyPolicy>());
```

#### Explicitly

If you have multiple WCF client proxies and want to have fine grained, per endpoint control over policies you apply you can add the policy as a endpoint extension:

```csharp
container.Register(
   Component.For<IOperationsEx>()
      .Named("operations")
      .AsWcfClient(WcfEndpoint
         .BoundTo(new NetTcpBinding { PortSharingEnabled = true })
         .At("net.tcp://localhost/Operations1/Ex")
         .AddExtensions(new ChannelReconnectPolicy())));
```

#### Default policy

When no custom policy is specified or none of them is interested applies in given invocation (all return `false` from `Perform` method) the default policy will be used. The policy is specified at a facility level, when you configure the facility:

```csharp
container.AddFacility<WcfFacility>(f =>
{
    f.Clients.DefaultChannelPolicy = new MyPolicy();
});
```

:information_source: **Default `DefaultChannelPolicy`:** If you don't specify `DefaultChannelPolicy` WCF Facility will use `ChannelReconnectPolicy`, which will retry call once if `ChannelTerminatedException`, `CommunicationObjectFaultedException`, `CommunicationObjectAbortedException`, `MessageSecurityException` or `CommunicationException` is thrown.

## See also

* [WCF Facility Policies](wcf-facility-policies.md)
* [Refresh Channel Policy](wcf-facility-refesh-channel-policy.md)