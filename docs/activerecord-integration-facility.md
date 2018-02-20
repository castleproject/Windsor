# ActiveRecord Integration Facility

Use the ActiveRecord Integration facility if you want to benefit from the transaction management offered by the [Automatic Transaction Management Facility](atm-facility.md) and if you want to have the Container configuring the [ActiveRecord framework](https://github.com/castleproject/ActiveRecord) (this keeps the configuration centralized).

The facility does not change in any way how ActiveRecord types are managed, loaded, saved and so on.

## Quick start

Just install the facility and add the proper configuration

```xml
<configuration>
   <facilities>
      <facility
         type="Castle.Facilities.ActiveRecordIntegration.ActiveRecordFacility, Castle.Facilities.ActiveRecordIntegration"
         isDebug="false"
         isWeb="false">
         <!-- Configure the namespaces for the models using Active Record Intergration -->
         <assemblies>
            <item>Company.Project.Model</item>
         </assemblies>
         <config>
            <add key="hibernate.connection.driver_class"      value="NHibernate.Driver.SqlClientDriver" />
            <add key="hibernate.dialect"                      value="NHibernate.Dialect.MsSql2000Dialect" />
            <add key="hibernate.connection.provider"          value="NHibernate.Connection.DriverConnectionProvider" />
            <add key="hibernate.connection.connection_string" value="Data Source=.;Initial Catalog=appdb;Integrated Security=SSPI" />
         </config>
      </facility>
   </facilities>
</configuration>
```

The configuration is basically the same as ActiveRecord. The exception is the assemblies node which you use to specify the assembly name that has the ActiveRecord types.

### Auto wiring

The facility also adds `ISessionFactoryHolder` and `ISessionFactory` as components, so you can gain access to NHibernate's `ISession` from any component. For example:

```csharp
public class CustomerService
{
   private readonly ISessionFactory sessionFactory;

   public CustomerService(ISessionFactory sessionFactory)
   {
      this.sessionFactory = sessionFactory;
   }

   public void PerformSomeComplexQuery()
   {
      ISession session = sessionFactory.OpenSession();
      ...
      session.Close();
   }
}
```

:information_source: **`OpenSession`:** You can only invoke `OpenSession` on the `ISessionFactory`, and also do not forget to close the Session as it is being managed by ActiveRecord.

### Transaction support

This facility is aware of transactions, for more information check [Automatic Transaction Management Facility](atm-facility.md).

### Required Assemblies

* `Castle.ActiveRecord.dll`
* `Castle.Services.Transaction.dll`
* `Castle.Facilities.ActiveRecordIntegration.dll`
* `Castle.Facilities.AutomaticTransactionManagement.dll`