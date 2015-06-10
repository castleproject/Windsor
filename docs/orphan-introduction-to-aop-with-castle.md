# Introduction to AOP With Castle

If you look at canonical terms of AOP you can recognize standard ones such as Aspect, JointPoint, PointCut, but if you look at Castle infrastructure you could not find any of these. This does not mean that Castle does not support AOP, but is a clue that Castle handles AOP with different point of view respect to other frameworks.

AOP is based on the concept of weaving, and Castle does this with Castle DynamicProxy, that is used by Castle Windsor to manage crosscutting concerns with the concept of [Interceptors](interceptors.md) and the interface `IInterceptor`. The basic is the ability to create a component that is able to intercept every call to methods and or properties of an interface, and deciding with Windsor configuration where to apply this interceptor. At the most basic level, let's consider this simple interface and its implementation.

## Example

```csharp
public interface ISomething
{
    int Augment(Int32 input);
    void DoSomething(String input);
    int Property { get; set; }
}

class Something : ISomething
{
    public int Augment(int input)
    {
        return input + 1;
    }

    public void DoSomething(string input)
    {
        Console.WriteLine("I'm doing something: " + input);
    }

    public int Property { get; set; }
}
```

Now I want to be able to intercept every call and log it; the solution in castle is to create an interceptor like this.

```csharp
public class DumpInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        Console.WriteLine("DumpInterceptorCalled on method " + invocation.Method.Name);
        invocation.Proceed();
        if (invocation.Method.ReturnType == typeof(int))
        {
            invocation.ReturnValue = (int)invocation.ReturnValue + 1;
        }
        Console.WriteLine("DumpInterceptor returnvalue is " + (invocation.ReturnValue ?? "NULL"));
    }
}
```

As you can see, the `IInterceptor` interface has only a single method, called `Intercept`, that gets called whenever a method or property of the wrapped interface is called. Now we need only to instruct Windsor to intercept an object with this interceptor, here is the solution with standard XML config files.

```csharp
<component id="BasicElement"
		service="TheBasicOfAopWithCastle.TestClasses.ISomething, TheBasicOfAopWithCastle"
		type="TheBasicOfAopWithCastle.TestClasses.Something, TheBasicOfAopWithCastle"
		lifestyle="singleton">
	<interceptors>
		<interceptor>${DumpInterceptor}</interceptor>
	</interceptors>
</component>

<component id="DumpInterceptor"
		service="Castle.Core.Interceptor.IInterceptor, Castle.Core"
		type="TheBasicOfAopWithCastle.Interceptors.DumpInterceptor, TheBasicOfAopWithCastle"
		lifestyle="singleton">
</component>
```

As you can verify you simply need to register the interceptor as any other component, telling castle that it implements the `IInterceptor` interface; once one or more interceptors are configured, you can refer to them in the `<interceptors>` part of other component registration. In the above example I simply registered a concrete implementation for the `ISomething` class, and I specify that I want to use the `DumpInterceptor` registered interceptor to it. Now if you run the following code:

```csharp
Console.WriteLine("Run 1 - configuration with xml file");
using (WindsorContainer container = new WindsorContainer("castle.config"))
{
	ISomething something = container.Resolve<ISomething>();
	something.DoSomething("");
	Console.WriteLine("Augment 10 returns " + something.Augment(10));
}
```

You got this output.

```
Run 1 - configuration with xml file
DumpInterceptorCalled on method DoSomething
I'm doing something:
DumpInterceptor returnvalue is NULL
DumpInterceptorCalled on method Augment
DumpInterceptor returnvalue is 12
Augment 10 returns 12
```

This certifies a couple of interesting things, the first is that every call to the resolved `ISomething` object is correctly intercepted by the interceptor, the other one is that the interceptor is able to modify the return value. As you can see, the original class implements the Augment methods with a simple addition of the value one to its argument, but passing 10 as parameters returns us the value 12, because the interceptors was able to modify it transparently from the real called method.
The conclusion is that Castle is able to do AOP, even if it don't support standard concepts, but this is usually not a big problem, you just need to shift a little bit the AOP paradigm to the concept of interceptor.
Clearly the same can be done using fluent interface, here is the code to register an object with an interceptor:

```csharp
Console.WriteLine("Run 2 - configuration fluent");
using (WindsorContainer container = new WindsorContainer())
{
	container.Register(
		Component.For<IInterceptor>()
		.ImplementedBy<DumpInterceptor>()
		.Named("myinterceptor"));
	container.Register(
		Component.For<ISomething>()
		.ImplementedBy<Something>()
		.Interceptors(InterceptorReference.ForKey("myinterceptor")).Anywhere);
	ISomething something = container.Resolve<ISomething>();
	something.DoSomething("");
	Console.WriteLine("Augment 10 returns " + something.Augment(10));
}
```

Fluent interface has the `Interceptors()` method that you can use to list a series of reference to registered interceptors, and you can verify that the output of this snippet is exactly the same of the previous example.

If you like you can add interceptor to a class with attribute, but this forces you to recompile class if you want to change interceptor used, moreover you can specify an interceptor by name or by registration type, but you always need to register the interceptor into the container. You can request that `Something2` class is intercepted with `DumpInterceptor`

```csharp
[Interceptor(typeof(DumpInterceptor))]
class Something2 : ISomething
{
}
```

Remember that you need to register a DumpInterceptor for the above code to work

```csharp
container.Register(
  Component.For<ISomething>()
    .ImplementedBy<Something2>()
    .Named("Something"));
```

You can also pass a string to the InterceptorAttribute if you like defining interceptor by name

```csharp
[Interceptor("NameOfTheInterceptor")]
class Something3 : ISomething
{
}
```

Now you need to register an `IInterceptor` object named `NameOfTheInterceptor`

```csharp
container.Register(
  Component.For<IInterceptor>()
    .ImplementedBy<DumpInterceptor>()
    .Named("NameOfTheInterceptor"));
```

## Select methods to intercept

Some people, after looking at interceptor concept, are not fully convinced that castle can support all concepts of AOP and the first question usually is: "How can I choose witch method intercept, instead of intercepting calls to all methods, and how can I configure this with XML file or fluent configuration?". This answer can have various solutions, but in my opinion the simplest one is doing a little manual logic on interceptor.

Since the interceptor is resolved by castle it is possible to add a list of valid regular expressions used to select methods to intercept, and simply check the method name to decide if it needs to be intercepted. This approach is the most simple one because it is based only on the basic structure of castle, and it has the advantages of change the list of methods to intercept simply changing the configuration of the interceptor. You can put this code on an interceptor or create a base interceptor class that contains the base logic.

```csharp
public class BetterDumpInterceptor : IInterceptor
{
  public List<String> RegexSelector { get; set; }

  public void Intercept(IInvocation invocation)
  {
    if (!CanIntercept(invocation)) {
      invocation.Proceed();
    return;
  }
}
```

This is a simple example on how you can filter method to intercept, the `CanIntercept()` can be implemented in this way.

```csharp
private Boolean CanIntercept(IInvocation invocation)
{
    if (RegexSelector == null || RegexSelector.Count == 0) return true;

    foreach (var regex in RegexSelector)
    {
      if (Regex.IsMatch(invocation.Method.Name, regex))
        return true;
    }
    return false;
}
```

This is really a simple solution, and you can verify from the output that only selected methods are intercepted.

![](http://www.codewrecks.com/blog/wp-content/uploads/2010/06/image_thumb16.png)

In red I highlighted the calls to regex.Match(), and since they are a little bit expensive, to speedup execution we can add caching using a dictionary, to store result of methods already scanned, just to avoid using too much regex at each call; the basic concept is always the same:

```csharp
public class BetterDumpInterceptor : IInterceptor
{
    public List<String> RegexSelector { get; set; }

    private Dictionary<RuntimeMethodHandle, Boolean> _scanned
      = new Dictionary<RuntimeMethodHandle, bool>();
}
```

I stored the result of the CanIntercept directly in a dictionary, and I use `RuntimeMethodHandle` just to save a little bit of memory not storing the full `MethodInfo` object. Then I modify the CanIntercept function in this way.

```csharp
private Boolean CanIntercept(IInvocation invocation)
{
    if (!_scanned.ContainsKey(invocation.Method.MethodHandle))
    {
        if (RegexSelector == null || RegexSelector.Count == 0) return true;
        foreach (var regex in RegexSelector)
        {
            if (Regex.IsMatch(invocation.Method.Name, regex))
            {
                _scanned[invocation.Method.MethodHandle] = true;
                return true;
            }
        }
        _scanned[invocation.Method.MethodHandle] = false;
    }
    return _scanned[invocation.Method.MethodHandle];
}
```

Now the same configuration produces this output.

![](http://www.codewrecks.com/blog/wp-content/uploads/2010/06/image_thumb17.png)

As you can see the regex are called only for the first call of a method, each subsequent calls does not requires Regex.Match anymore.
And with this little trick I simply end with an interceptor that permits to select methods you want to intercept with configuration.
If you do not like doing this manually, Castle offers a native way to decide which method to intercept, based on the interface `IInterceptorSelector`, that contains a single method with this signature.

```csharp
public IInterceptor[] SelectInterceptors(
  Type type,
  System.Reflection.MethodInfo method,
  IInterceptor[] interceptors)
```

Basically it receives the Type and `MethodInfo` for the method being called, and a list of interceptors configured for that method, the purpose of the selector is to return an array containing only the interceptors that are allowed to be used on that method. A possible implementation is the following one.

```csharp
public Dictionary<Type, List<String>> RegexSelector { get; set; }

public InterceptorSelector(Dictionary<Type, List<string>> regexSelector)
{
    RegexSelector = regexSelector;
}

private Boolean CanIntercept(MethodInfo methodInfo, IInterceptor interceptor)
{
    List<String> regexForInterceptor;
    if (!RegexSelector.TryGetValue(interceptor.GetType(), out regexForInterceptor))
        return false;
    if (regexForInterceptor == null) return false;

    foreach (var regex in regexForInterceptor)
    {
        if (Regex.IsMatch(methodInfo.Name, regex))
        {
            return true;
        }
    }
    return false;
}

#region IInterceptorSelector Members

public IInterceptor[] SelectInterceptors(Type type, System.Reflection.MethodInfo method, IInterceptor[] interceptors)
{
    Utils.ConsoleWriteline(ConsoleColor.Red, "Called interceptor selector for method {0}.{1} and interceptors {2}",
        type.FullName,
        method.Name,
        interceptors
        .Select(i => i.GetType().Name)
        .Aggregate((s1, s2) => s1 + " " + s2));
    return interceptors.Where(i => CanIntercept(method, i)).ToArray();
}

#endregion
```

This selector contains a dictionary of Type and List<String>, basically for each interceptor type it contains a list of regex to identify methods to be intercepted. The rest of the selector is really simple, with a little bit of LINQ we can select only the interceptors that are in the dictionary and have at least one match with the method name. You can use it in this way.

```csharp
Dictionary<Type, List<String>> RegexSelector = new Dictionary<Type, List<string>>();
RegexSelector.Add(typeof(DumpInterceptor), new List<string>() { "DoSomething" });
RegexSelector.Add(typeof(LogInterceptor), new List<string>() { "Augment" });
InterceptorSelector selector = new InterceptorSelector(RegexSelector);
```

I created a selector that permits to select a couple of interceptors, one is the DumpInterceptor, and I want it to intercept the "DoSomething" method, while the LogInterceptor should intercept only the "Augment" method. Once the selector is configured you can simply use it in fluent configuration.

```csharp
container.Register(
    Component.For<IInterceptor>()
    .ImplementedBy<DumpInterceptor>()
    .Named("DumpInterceptor"));

container.Register(
    Component.For<IInterceptor>()
    .ImplementedBy<LogInterceptor>()
    .Named("LogInterceptor"));

container.Register(
    Component.For<ISomething>()
    .ImplementedBy<Something>()
    .Interceptors(
            InterceptorReference.ForKey("DumpInterceptor"),
            InterceptorReference.ForKey("LogInterceptor"))
            .SelectedWith(selector).Anywhere);
```

Now you can run the program and look at the output.

![](http://www.codewrecks.com/blog/wp-content/uploads/2010/06/image_thumb18.png)

With red color I highlight the calls to the SelectInterceptors method, and you can immediately see that Castle does caching internally and does not call the SelectInterceptors() method more than once for each method, so we does not need to cache stuff. As you can see from the output the result is the one I expect, dumpInterceptor is intercepting only the method DoSomething while the LogInterceptor intercepts only the Augment.
You can use the approach you prefer.

## A concrete example - Logging Interceptor

Usually the very first interceptor you can build with an AOP framework is the "logger Interceptor", because it is simple and useful, especially when you expose some services with WPF. Consider this scenario: you expose some services with WPF, sometimes people tell you that your services have bugs or they encountered an exception, or they get wrong result, etc. In this situation you receive information like:

"I got exception from your service", and no more details, so you need to spent time trying to understand what has happened.
Since this is probably the most detailed information you can have from the customer, having a good log system is vital. You need to identify what has happened and you need to build a system to log:

*	every call to service methods
*	all parameters value for the call
*	return value and exception that are raised (if one)

The goal is having a full and configurable log of what has happened in the service to retrace exception and problems experienced by users. Now wait and avoid the temptation to modify every service class in the system adding log calls since you can use AOP to add logging capability to each intercepted service. Such an interceptor is really simple to build, and with few lines of code you can log every call made to a function of intercepted class.

As a requisite, you need to instantiate wcf service classes with CastleIntegration facility and Logging integration facility; thanks to these two facilities you are able to resolve the service class with castle (thus adding interceptor), and use the log4net integration to avoid hardcoding logging to specific target, like file etc. The good point in log4Net castle integration is the ability to declare a dependency to a generic ILogger interface. This feature can be used in the Logging Interceptor.

```csharp
public class LogAspect : IInterceptor
{
  public LogAspect(ILogger logger)
  {
    Logger = logger;
  }

  public ILogger Logger { get; set; }
}
```

Castle [Logging Facility](logging-facility.md) is able to resolve the `ILogger` dependency with an implementation of log4net logger that has a name equal to the name of the class that declares dependency. This is the most important point, because in the above code, the logger will have the name MusicStore.Aspects.Log.LogAspect, and this permits me to change the log setting for each single class in the system. The interceptor will need also a little helper function to create a string that dump every details of the call.

```csharp
public static String CreateInvocationLogString(IInvocation invocation)
{
	StringBuilder sb = new StringBuilder(100);
	sb.AppendFormat("Called: {0}.{1}(", invocation.TargetType.Name, invocation.Method.Name);
	foreach (object argument in invocation.Arguments)
	{
		String argumentDescription = argument == null ? "null" : DumpObject(argument);
		sb.Append(argumentDescription).Append(",");
	}
	if (invocation.Arguments.Count() > 0) sb.Length--;
	sb.Append(")");
	return sb.ToString();
}
```

Since parameters of the service could be complex objects I'm dumping information with an helper function that is able to include every detail of a class.

```csharp
private static string DumpObject(object argument)
{
	Type objtype = argument.GetType();
	if (objtype == typeof(String) || objtype.IsPrimitive || !objtype.IsClass)
		return objtype.ToString();

	return DataContractSerialize(argument, objtype);
}
```

I want to keep the code simple, if the object type is not primitive I use DataContractSerialize to dump the content in XML format. Once everything is in place I need to insert the call to the logger in the appropriate point.

```csharp
public void Intercept(IInvocation invocation)
{
	if (Logger.IsDebugEnabled) Logger.Debug(CreateInvocationLogString(invocation));
	try
	{
		invocation.Proceed();
	}
	catch (Exception ex)
	{
		if (Logger.IsErrorEnabled)  Logger.Error(CreateInvocationLogString(invocation), ex);
		throw;
	}
}
```

Before each call to the logger object I first check if the appropriate level of logging is enabled. This technique is useful to avoid loss of performance when log is not enabled; if the debug level is set to warn, the Logger.Debug will not log anything and the CreateInvocationLogString will build the log string for nothing, losing processor time with no benefit. To avoid this loss you can issue a call to Logger.IsDebugEnabled to avoid entirely the call to logging function.

Now suppose that the caller pass an invalid object to method Save() of MusicStoreService, the user will see a message telling that a service error is occurred, but now I'm able to check the log to understand exactly what is happened. Here is the log.

```
2010-07-24 10:12:28,320 ERROR MusicStore.Aspects.Log.LogAspect - Called: MusicStoreService.Save(<Album xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.datacontract.org/2004/07/MusicStore.Entities">
  <Id>0</Id>
  <Author
    i:nil="true" />
  <Genre
    i:nil="true" />
  <Image
    i:nil="true" />
  <Label
    i:nil="true" />
  <Note
    i:nil="true" />
  <PublicationDate>0001-01-01T00:00:00</PublicationDate>
  <Title
    i:nil="true" />
  <Tracks
    i:nil="true" />
</Album>)
NHibernate.PropertyValueException: not-null property references a null or transient valueMusicStore.Entities.Album.Title
   at NHibernate.Engine.Nullability.CheckNullability(Object[] values, IEntityPersister persister, Boolean isUpdate)
   at NHibernate.Event.Default.AbstractSaveEventListener.PerformSaveOrReplicate(Object entity, EntityKey key, IEntityPersister persister, Boolean useIdentityColumn, Object anything, IEventSource source, Boolean requiresImmediateIdAccess)
   at NHibernate.Event.Default.AbstractSaveEventListener.PerformSave(Object entity, Object id, IEntityPersister persister, Boolean useIdentityColumn, Object anything, IEventSource source, Boolean requiresImmediateIdAccess)
   at NHibernate.Event.Default.AbstractSaveEventListener.SaveWithGeneratedId(Object entity, String entityName, Object anything, IEventSource source, Boolean requiresImmediateIdAccess)
```

From this log I understand that an invalid object is passed to the service, the property Album.Title is required in the database, but the user passed a property with null value. Since log4net is really flexible I'm able to dump this information to a file, to a database, to network or with mail. You can as example send a mail each time an exception occurs, so you are immediately notified if something in the service is not going well.
This logger can be improved a little bit because the name of the logger is always MusicStore.Aspects.Log.LogAspect for each wrapped service. This is not really a problem, but I prefer to have the ability to configure logging differently for each service; in real product with a lot of services, this is a key requiremente. The interceptor can be changed in this way

```csharp
public class LogAspect : IInterceptor
{
	public LogAspect(ILoggerFactory loggerFactory)
	{
		LoggerFactory = loggerFactory;
		Loggers = new Dictionary<Type, ILogger>();
	}

	public ILoggerFactory LoggerFactory { get; set; }

	public Dictionary<Type, ILogger> Loggers { get; set; }
}
```

Now the interceptor declares a dependency to an ILoggerFactory and not to a concrete ILogger, and caches a list of ILogger object based on type. The result is a concrete ILogger object for each wrapped type.

```csharp
public void Intercept(IInvocation invocation)
{
	if (!Loggers.ContainsKey(invocation.TargetType))
	{
		Loggers.Add(invocation.TargetType, LoggerFactory.Create(invocation.TargetType));
	}
	ILogger logger = Loggers[invocation.TargetType];
	if (logger.IsDebugEnabled) logger.Debug(CreateInvocationLogString(invocation));
}
```

instead of using the same logger, we first check if we had already created a logger for a given type, if false we use the ILoggerFactory to create the logger and cache it to an inner dictionary. If we send an invalid object again to the service the head of the log is.

```
2010-07-24 10:27:30,783 DEBUG MusicStore.WebService.MusicStoreService - Called: MusicStoreService.Save(..
```

Now the name of the logger is equal to the name of the concrete service class and you have created a simple logging system that can:

1. Add transparently to each service class without needing a single line of code
1. Change logging level for each concrete class of the service.