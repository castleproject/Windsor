# WCF Facility - Asynchronous Calls

By default WCF performs all calls synchronously, that is it locks the thread calling the operation until result is received (or until server side acknowledges it received the request when operation is marked as one way). This is wasteful and sub-optimal. Unfortunately out of the box if you want to call services asynchoronously you either have to resort to code generation (using `svcutil.exe`) or maintain two versions of your contract as described [here](http://ayende.com/Blog/archive/2008/03/29/WCF-Async-without-proxies.aspx).

## Eat the cake and have it too

WCF Facility lets you perform asynchronous calls on your services while using only single, synchronous contract.

:information_source: **When it makes sense:** Obviously this makes sense only when you develop both the client and the server and you can share binary containing the contract between them. If you're integrating with non-WCF service or a 3rd party service you will still need to obtain the contract somehow first.

## Simple example

Let's assume we have the following simple service contract:

```csharp
[ServiceContract]
public interface IOperations
{
   [OperationContract]
   int GetValue();
}
```

On the client side you register your services as usual. You will also need a namespace using:

```csharp
using Castle.Facilities.WcfIntegration;
```

you can then resolve your client-side proxy from the container:

```csharp
var client = container.Resolve<IOperations>();
```

and invoke the `GetValue` method asynchronously using the following syntax:

```csharp
var call = client.BeginWcfCall(p => p.GetValue());
```

This is important so pay attention. On the client we invoke extension method `BeginWcfCall`. To that method we pass a delegate with invokes the method we want to invoke asynchronously on the service. The call will not block, it will return immediately, but instead of the return value an `IWcfAsyncCall<int>` is returned. This is an object that implements `IAsyncResult` and you use it as if you were using standard .net async pattern.

For example if you wanted to block the thread and wait for the invocation to complete (which you usually would not) you can call

```csharp
var result = call.End();
```

The above is identical to calling the following on generated asynchronous contract:

```csharp
var client = container.Resolve<IOperationsAsync>();
var call = client.BeginGetValue(delegate{}, null);
var result = client.EndGetValue(call);
```

## Fully asynchronous example

While the example above serves well as introduction to the API it hardly offers any advantage over fully synchronous invocation, since we're still blocking the thread waiting for the operation to complete.
Let's change the invocation to fully asynchronous one, to avoid any tread blocking:

```csharp
client.BeginWcfCall(p => p.GetValue(),
   call => Console.WriteLine("Method returned {0} asynchronously", call.End()), null);
```

Here we're not using the returned async result, instead we're passing a delegate that will be invoked (possibly, probably) on another thread when the operation completes. The delegate will receive an`IWcfAsyncCall<int>` and by calling `End` it can obtain the return value from the service call (or any exception that was thrown). For example sake we just print the return value to the console.

Above call is identical to calling the following on generated asynchronous contract:

```csharp
client.BeginGetValue(call => Console.WriteLine("Method returned {0} asynchronously", client.EndGetValue(call)), null);
```

## External Resources

* [Blog post by Krzysztof Koźmic introducing the feature](http://kozmic.pl/2009/08/09/making-asynchronous-wcf-calls-without-svcutil/)