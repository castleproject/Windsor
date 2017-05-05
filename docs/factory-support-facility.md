# Factory Support Facility

Factory Support Facility allows using factories to create components. This is beneficial when you want to make available as services components that do not have accessible constructor, or that you don't instantiate, like `HttpContext`.

:information_source: **Prefer `UsingFactoryMethod` over this facility:** while the facility provides programmatic API it is deprecated and its usage is discouraged and won't be discussed here. Recommended approach is to use [`UsingFactoryMethod`](registering-components-one-by-one.md#using-a-delegate-as-component-factory) method of [fluent registration API](fluent-registration-api.md) to create components. This limits the usefulness of the facility to XML-driven and legacy scenarios.

:information_source: **`UsingFactoryMethod` does not require this facility anymore:** In older versions of Windsor (up to and including version 2.1) `UsingFactoryMethod` method in the fluent API discussed above required this facility to be active in the container. That was later changed and there's no such dependency anymore.

## Using factories from configuration

In addition to code, the facility uses XML configuration. You can register the facility in the standard facilities section of Windsor's config:

Just install the facility and add the proper configuration.

```xml
<configuration>
   <facilities>
      <facility
         id="factory.support"
         type="Castle.Facilities.FactorySupport.FactorySupportFacility, Castle.Facilities.FactorySupport" />
   </facilities>
</configuration>
```

### Configuration Schema

Broadly speaking facility exposes the following scheme, with two kinds of supported factories: accessors and methods

```xml
<components>
   <component id="mycomp1" instance-accessor="Static accessor name" />
   <component id="factory1" />
   <component id="mycomp2" factoryId="factory1" factoryCreate="Create" />
</components>
```

### Accessor example

Given the following singleton class:

```csharp
public class SingletonWithAccessor
{
    private static readonly SingletonWithAccessor instance = new SingletonWithAccessor();

    private SingletonWithAccessor()
    {
    }

    public static SingletonWithAccessor Instance
    {
        get { return instance; }
    }
}
```

You may expose its instance to the container through the following configuration:

```xml
<components>
   <component id="mycomp1"
   type="Company.Components.SingletonWithAccessor, Company.Components"
   instance-accessor="Instance" />
</components>
```

Using it:

```csharp
var comp = container.Resolve<SingletonWithAccessor>("mycomp1");
```

### Factory example

Given the following component and factory classes:

```csharp
public class MyComp
{
    internal MyComp()
    {
    }

    ...
}

public class MyCompFactory
{
    public MyComp Create()
    {
        return new MyComp();
    }
}
```

You may expose its instance to the container through the following configuration:

```xml
<components>
   <component id="mycompfactory"
      type="Company.Components.MyCompFactory, Company.Components"/>
   <component id="mycomp"
      type="Company.Components.MyComp, Company.Components"
      factoryId="mycompfactory" factoryCreate="Create" />
</components>
```

Using it:

```csharp
var comp = container.Resolve<MyComp>("mycomp");
```

### Factory with parameters example

Given the following component and factory classes:

```csharp
public class MyComp
{
    internal MyComp(String storeName, IDictionary props)
    {
    }

    ...
}

public class MyCompFactory
{
    public MyComp Create(String storeName, IDictionary props)
    {
        return new MyComp(storeName, props);
    }
}
```

You may expose its instance to the container through the following configuration:

```xml
<components>
   <component id="mycompfactory"
      type="Company.Components.MyCompFactory, Company.Components"/>
   <component id="mycomp"
      type="Company.Components.MyComp, Company.Components"
      factoryId="mycompfactory" factoryCreate="Create">
      <parameters>
          <storeName>MyStore</storeName>
          <props>
             <dictionary>
                <entry key="key1">item1</entry>
                <entry key="key2">item2</entry>
             </dictionary>
          </props>
       </parameters>
   </component>
</components>
```

Using it:

```csharp
var comp = container.Resolve<MyComp>("mycomp");
```

### Factory using auto-wire example

If your factory request as parameter some other component instance, this facility will be able to resolve it without your aid:

```csharp
public class MyComp
{
    internal MyComp(IMyService serv)
    {
    }

    ...
}

public class MyCompFactory
{
    public MyComp Create(IMyService service)
    {
        return new MyComp(service);
    }
}
```

You may expose its instance to the container through the following configuration:

```xml
<facilities>
   <facility
      id="factorysupport"
      type="Castle.Facilities.FactorySupport.FactorySupportFacility, Castle.Facilities.FactorySupport"/>
</facilities>

<components>
   <component id="myservice"
      service="SomethingElse.IMyService"
      type="Company.Components.MyServiceImpl, Company.Components" />
   <component id="mycompfactory"
      type="Company.Components.MyCompFactory, Company.Components" />
   <component id="mycomp"
      type="Company.Components.MyComp, Company.Components"
      factoryId="mycompfactory" factoryCreate="Create" />
</components>
```

Using it:

```csharp
var comp = container.Resolve<MyComp>("mycomp");
```

## External resources

[Blog post by David Hayden about the facility (Dec 13, 2007)](http://codebetter.com/blogs/david.hayden/archive/2007/12/13/factory-method-support-in-castle-windsor-and-spring-net.aspx)
