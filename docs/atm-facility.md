# Automatic Transaction Management Facility

This facility manages the creation of Transactions and the associated commit or rollback, depending on whether the method throws an exception or not.

Transactions are logical. It is up the other integration to be transaction aware and enlist its resources on it.

## Quick start

This facility usually works together with others facilities, as it requires an implementation of `ITransactionManager`. Currently the [ActiveRecord Integration Facility](activerecord-integration-facility.md) and [NHibernate facility](nhibernate-facility.md) implement the `ITransactionManager`.

### Defining transaction behavior on components

You can use attributes or the configuration file to associate transaction information with your components.

### Using attributes

```csharp
using Castle.Services.Transaction;

[Transactional]
public class BusinessClass
{
   public void Load(int id)
   {
      ...
   }

   // note the "virtual"
   [Transaction(TransactionMode.Requires)]
   public virtual void Save(Data data)
   {
      ...
   }
}
```

#### Using configuration

```xml
<configuration>
   <components>
      <component
         id="mycomp"
         type="Namespace.BusinessClass, AssemblyName"
         isTransactional="true">
         <transaction>
            <method name="Save" />
            <method name="Create" />
         </transaction>
      </component>
   </components>
</configuration>
```

:warning: **Class methods must be virtual:** If you are registering the component without an interface as a service you must make the methods virtual in order to being intercepted. Please refer to [DynamicProxy documentation](https://github.com/castleproject/Core/blob/master/docs/README.md) for more information about it.

### TransactionMode

* `NotSupported`: Transaction context will be created but no transaction is started
* `Requires`: Transaction context will be created if not present
* `RequiresNew`: A new transaction context will be created (not supported at the moment)
* `Supported`: An existing appropriate transaction context will be joined if present (not supported at the moment)

### Required Assemblies

* `Castle.Facilities.AutomaticTransactionManagement.dll`
* `Castle.Services.Transactions.dll`
* `Castle.Core.dll`