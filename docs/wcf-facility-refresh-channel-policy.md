# WCF Facility - Refresh Channel Policy

In bare WCF application when the channel becomes unusable (gets faulted, closed or aborted) you have to explicitly go to every place in your code and explicitly replace the reference to a new, fresh, working channel.

When using Windsor WCF Facility you can do it transparently - refresh the channel without any of its users being aware of that if you wish so. This behavior is controlled by `IRefreshChannelPolicy` interface.

:information_source: **By default channel is always refreshed:** If you don't specify any custom `IRefreshChannelPolicy` WCF Facility will refresh the channel before every call that requires that.

## The `IRefreshChannelPolicy` interface

The refresh channel policy is one of standard [WCF Facility policies](wcf-facility-policies.md) that contains decision logic as to whether or not the channel should be refreshed. The interface looks like this:

```csharp
public interface IRefreshChannelPolicy : IWcfChannelPolicy
{
	/// <summary>
	/// Called when an attempt is made to use a channel in an invalid state
	/// 	i.e. closed or faulted, or aborted./>
	/// </summary>
	/// <param name="channelHolder">The channel holder.</param>
	/// <param name="method">The attempted method.</param>
	void WantsToUseUnusableChannel(IWcfChannelHolder channelHolder, MethodInfo method);
}
```

The method `WantsToUseUnusableChannel` is called by the facility when a call to one of your WCF proxy methods is about to be invoked and the channel is not in usable state.

### Example implementation

Below is a sample, trivial implementation of the interface that refreshes the channel based on the value of its own property, so that the behavior can be changed externally.

```csharp
public class RefreshChannelPolicy : AbstractWcfPolicy, IRefreshChannelPolicy
{
    public bool Refresh { get; set; }

    public void WantsToUseUnusableChannel(IWcfChannelHolder channelHolder, MethodInfo method)
    {
        if (Refresh)
        {
            channelHolder.RefreshChannel();
        }
    }
}
```

## Attaching custom policy

To attach the policy all you need to do is to register it in the container. It will be then picked up and attached to every client proxy managed by Windsor and WCF Facility.

```csharp
container.Register(Component.For<RefreshChannelPolicy>());
```

:information_source: **What if I have more than one?:** It you have more than one refresh policy Windsor will call all of them giving each chance to have a say, ordered by their priority

## See also

* [WCF Facility policies](wcf-facility-policies.md)
* [Channel Action Policy](wcf-facility-channel-action-policy.md)