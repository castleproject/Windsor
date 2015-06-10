# Remoting Facility

By using this facility, the container is able to configure components (publishing and consuming) on the .net remoting runtime. Its usage and configuration may vary depending on what your requirements are. Please read this documentation carefully to see if it fits for your needs.

We also assume that you know at least the basics of .NET remoting support.

## Using it

The facility works on both server and client side, and you can even have a situation where the application serves as both endpoints.

The remoting configuration (channels, formatters) is  external and it is the same file that is accepted by .NET remoting. Please check its official documentation.

For each component that is remote (client or server) you must supply the remoting strategy. We currently support:

* `Singleton`: which maps to a wellknown service in mode singleton
* `SingleCall`: which maps to a wellknown service in mode singlecall
* `ClientActivated`: which maps to a client activated service (one server instance per client request)
* `Component`: activation goes through the communication between the client container and the server container
* `RecoverableComponent`: activation goes through the communication between the client container  and the server container, but the server can be restarted without any problem. The component must be use the `Singleton` lifestyle.

## Configuration

:information_source: **XML configuration:** Currently there's no programatic configuration for the facility and XML is the only way to use it.

The following depicts the full configuration scheme for the facility.  Most of the attributes are optional or only required for some specific  scenarios. The scenarios will be discussed further in this document.

```xml
<facility
   id="remote.facility"
   isServer="true|false"
   isClient="true|false"
   kernelUri="a valid identifier"
   remoteKernelUri="a full uri including protocol, host, path and resource"
   remotingConfigurationFile="absolute or relative path to the remoting configuration file" />
```

The following depicts the attributes which are available on the component nodes:

```xml
<component
   id=""
   remoteclient="singleton|singlecall|clientactivated|component|recoverableComponent "
   uri="required for singleton, singlecall and recoverableComponent but if not present, the component id is used" />
```

## Supported Scenarios

There are various situations where the Remoting Facility can become handy. The following sections exemplifies each one and presents any caveat that they might have.

* Using the container on just one endpoint
* Using the container on both endpoints but just to configure remoting
* Using the container on both endpoints and use the container components

## External resources

[Blog post by Gojko Adzic about the facility (Jul 09, 2008)](http://gojko.net/2008/07/09/really-simple-net-remoting-with-castle/)