# WCF Integration Facility

WCF Integration facility enables integration with Windows Communication Foundation. It makes services and WCF proxies available as services in your application, lets you use non-default constructor and inject dependencies into your services, adds ability to easily set up your services with extensions, call services asynchronously without needing to use code generation and much more.

## Installing the facility

WCF Integration Facility lives in `Castle.Facilities.WcfIntegration.dll` file distributed as part of Windsor binary package.

If you're using Nuget, you'll have to download it separately using the following command:

`Install-Package Castle.WcfIntegrationFacility`

After adding reference to the facility, You can add it to the container:

```csharp
using Castle.Facilities.WcfIntegration;

Container.AddFacility<WcfFacility>();
```

## On the client

Some of the functionality exposed by the facility on the client side of WCF application.

:information_source: Notice that the documentation is still being created and the list is incomplete.

* Windsor replaces standard WCF client side remoting proxy with its own Dynamic Proxy. This lets you add [interceptors](interceptors.md) to it like to any other Windsor component.
* [Ability to perform asynchronous calls without need to code-generate client-side proxy](wcf-facility-async-calls.md)
* [Transparently recycle the channel when it gets closed/faulted](wcf-facility-refresh-channel-policy.md)

## On the server

Some of the functionality exposed by the facility on the service side of WCF application.

* [WCF Facility lifestyles - PerWcfSession and PerWcfOperation](wcf-facility-lifestyles.md)

## On both

Some of the functionality exposed by the facility on both - client and service side of WCF application.

* [Registering WCF components](wcf-facility-registration.md)

## See also

* [Other Facilities](facilities.md)