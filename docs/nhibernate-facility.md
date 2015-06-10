# NHibernate Facility

The NHibernate facility provides two levels of integration with NHibernate. You should pick the one you feel more comfortable with.

* You can just receive the `ISessionFactory` and Configuration on your components and use it as you wish.
* You can use `ISessionManager` component to manage sessions.

## Integration level 1

This example illustrates how you can have fine grained control over `ISessionFactory` and thus, `ISession`.

```csharp
using NHibernate;
using NHibernate.Cfg;

public class MyDataAccessClass
{
    private ISessionFactory _sessFactory;
    private Configuration _cfg;

    public MyDataAccessClass(ISessionFactory sessFactory, Configuration cfg)
    {
        _sessFactory = sessFactory;
        _cfg = cfg;
    }

    public void SaveSomething(object item)
    {
        ISession session = _sessFactory.OpenSession();

        // .. do whatever you want

        session.Close();
    }
}
```

With this scenario you have to control transactions and share sessions instances manually.

## Integration level 2

The second level of integration introduce the ISessionManager interface so you can use session and leave to the ISessionManager implementation the responsability of sharing compatible session within an invocation chain and be aware of transactions.

## Configuration

The configuration serves two purposes: configure NHibernate and configure the facility for the environment it is running in.

### Configuration Schema

```xml
<facilities>
  <facility id="nhibernate" isWeb="true|false" customStore="typename for a class that implements ISessionStore" type="implementation type of facility">
    <factory id="nhibernate.factory">
      <settings>
        <item key="nhibernate config key 1">value</item>
        <item key="nhibernate config key 2">value</item>
      </settings>
      <resources>
        <resource name="hbm.xml file location" />
        <resource assembly="assembly name" name="hbm.xml file name" />
      </resources>
      <assemblies>
        <assembly>assembly name</assembly>
      </assemblies>
    </factory>
  </facility>
</facilities>
```

You can register more than one factory if you are accessing more than one database. In this case, you must provide an alias for the factory:

```xml
<facilities>
  <facility id="nhibernate" ...>
    <factory id="nhibernate.factory">
      ...
    </factory>

    <factory id="nhibernate.factory" alias="oracle2">
      ...
    </factory>
  </facility>
</facilities>
```

The alias is used to obtain an ISession instance through ISessionManager. More on that below.

The attribute isWeb allows the facility to switch the implementation for ISessionStore. You can provide your own implementation using the attribute customStore.

### Configuration Sample

```xml
<configuration>

<configSections>
  <section
      name="castle"
      type="Castle.Windsor.Configuration.AppDomain.CastleSectionHandler, Castle.Windsor" />
</configSections>

<castle>
  <facilities>
    <facility id="nhibernate" type="Castle.Facilities.NHibernateIntegration.NHibernateFacility, Castle.Facilities.NHibernateIntegration">
      <factory id="nhibernate.factory">
        <settings>
          <item key="connection.provider">
            NHibernate.Connection.DriverConnectionProvider
          </item>
          <item key="connection.driver_class">
            NHibernate.Driver.MySqlDataDriver
          </item>
          <item key="connection.connection_string">
            Database=minddump;Data Source=localhost
          </item>
          <item key="dialect">
            NHibernate.Dialect.MySQLDialect
          </item>
        </settings>
        <resources>
          <resource name="..\bin\Author.hbm.xml" />
          <resource name="..\bin\Blog.hbm.xml" />
          <resource name="..\bin\Post.hbm.xml" />
        </resources>
      </factory>
    </facility>
  </facilities>
</castle>

</configuration>
```

If you wish to use a connection string from a section of your configuration file then you need to use the NHibernate hibernate.connection.connection_string_name property rather than the hibernate.connection.connection_string property. This is documented in the [NHibernate documentation](http://www.hibernate.org/hib_docs/nhibernate/1.2/reference/en/html/session-configuration.html#d0e771).

## Using it

To use the NHibernate facility you just need to register it and provide the configuration. If you want to use the integration level 2 approach, you can make your data access component require the ISessionManager.

### ISessionManager

The following code exemplifies a common data access component:

```csharp
public class BlogDao
{
    private ISessionManager sessionManager;

    public BlogDao(ISessionManager sessionManager)
    {
        this.sessionManager = sessionManager;
    }

    public Blog CreateBlog(String name)
    {
        using(ISession session = sessionManager.OpenSession())
        {
            Blog blog = new Blog();
            blog.Name = name;

            session.Save(blog);

            return blog;
        }
    }
}
```

When OpenSession is called without arguments, the first configured Session Factory is used. If you have more than one database you have to specify the alias:

```csharp
public Blog CreateBlog(String name)
{
    using (ISession session = sessionManager.OpenSession("oracle2"))
    {
        Blog blog = new Blog();
        blog.Name = name;

        session.Save(blog);

        return blog;
    }
}
```

If your component access another component, or just call another method that will use a session, within an opened session, the SessionManager will use the same session. For example:

```csharp
public class BlogDao
{
    private ISessionManager sessionManager;

    public BlogDao(ISessionManager sessionManager)
    {
        this.sessionManager = sessionManager;
    }

    public Blog CreateBlog(String name)
    {
        using (ISession session = sessionManager.OpenSession())
        {
            // make sure there are not blogs with this name

            Blog existing = FindByName(name);

            if (existing != null)
            {
                throw new DaoLayerException("Duplicated blog name");
            }

            Blog blog = new Blog();
            blog.Name = name;

            session.Save(blog);

            return blog;
        }
    }

    public Blog FindByName(String name)
    {
        // If the previous call had opened a session
        // this one will reuse it
        using (ISession session = sessionManager.OpenSession())
        {
            ...
        }
    }
}
```

### Web applications

If you make use of NHibernate's Lazy loading, then, for a web application, you must provide a session instance for the request lifetime. NHibernate facility provides a Http Module to do that.

The following sections illustrates the steps to make everything work as expected.

#### Configuring the facility

First of all, do not forget to use the attribute isWeb="true" on the facility configuration node:

```xml
<facilities>
  <facility id="nhibernate" isWeb="true" ... >
    ...
  </facility>
</facilities>
```

This enables a different strategy to keep session instances.

#### Global.asax

You must make the your container available to the web application. The best place for it is the global.asax:

```
<%@ Application Inherits="YourApp.Web.MyGlobalApplication" %>
```

MyGlobalApplication.cs:

```csharp
namespace YourApp.Web
{
    using System;
    using System.Web;

    using Castle.Windsor;

    public class MyGlobalApplication : HttpApplication, IContainerAccessor
    {
        private static WebAppContainer container;

        public void Application_OnStart()
        {
            container = new WebAppContainer();
        }

        public void Application_OnEnd()
        {
            container.Dispose();
        }

        public IWindsorContainer Container
        {
            get { return container; }
        }
    }
}
```

#### Enabling the http module

In your web.config, register the module SessionWebModule:

```xml
<configuration>
  <system.web>
    <httpModules>
      <add name="NHibernateSessionWebModule"
        type="Castle.Facilities.NHibernateIntegration.Components.Web.SessionWebModule, Castle.Facilities.NHibernateIntegration"/>
    </httpModules>
  </system.web>
</configuration>
```

## Generic DAO

A fellow Castle user, Steve Degosserie, has shared his implementation of NHibernateGenericDao. Follows a list of operations it implements:

```csharp
Array FindAll(Type type)
Array FindAll(Type type, int firstRow, int maxRows)
object FindById(Type type, object id)
object Create(object instance)
object Update(object instance)
object Delete(object instance)
void Save(object instance
void DeleteAll(Type type)
Array FindAll(Type type, ICriterion[] criterias)
Array FindAll(Type type, ICriterion[] criterias, int firstRow, int maxRows)
Array FindAll(Type type, ICriterion[] criterias, Order[] sortItems)
Array FindAll(Type type, ICriterion[] criterias, Order[] sortItems, int firstRow, int maxRows)
Array FindAllWithCustomQuery(string queryString)
Array FindAllWithCustomQuery(string queryString, int firstRow, int maxRows)
Array FindAllWithNamedQuery(string namedQuery)
Array FindAllWithNamedQuery(string namedQuery, int firstRow, int maxRows)
void InitializeLazyProperties(object instance)
void InitializeLazyProperty(object instance, string propertyName)
```

## Transaction Support

When you use the ISessionManager you can use declarative transaction management, together with the Automatic Transaction Management Facility.

You can use the Transactional and Transaction attributes to associate transaction boundaries with your DAO class or business classes.

```csharp
[Transactional]
public class BlogDao
{
    [Transaction(TransactionMode.Requires)]
    public virtual Blog CreateBlog(String name)
    {
        using (ISession session = SessionManager.OpenSession())
        {
            // This session is going to have a transaction associated

            Blog blog = new Blog();
            blog.Name = name;

            session.Save(blog);

            return blog;
        }
    }
}
```

## Required Assemblies==

* [Castle.Facilities.NHibernateIntegration.dll](http://github.com/castleproject/Castle.Facilities.NHibernateIntegration)
* [NHibernate assemblies](http://www.nhforge.org)