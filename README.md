# Castle Windsor

<img align="right" src="docs/images/windsor-logo.png">

Castle Windsor is a best of breed, mature Inversion of Control container available for .NET.

See the [documentation](docs/README.md).

## Releases

See the [releases](https://github.com/castleproject/Windsor/releases).

## Copyright

Copyright 2004-2017 Castle Project

## License

Castle Windsor is licensed under the [Apache 2.0](http://opensource.org/licenses/Apache-2.0) license. Refer to LICENSE for more information.


### Conditional Compilation Symbols

The following conditional compilation symbols are currently defined for Castle.Windsor:

Symbol                              | NET45              | .NET Standard
----------------------------------- | ------------------ | ------------------
`CASTLE_SERVICES_LOGGING`           | :white_check_mark: | :no_entry_sign:
`FEATURE_APPDOMAIN`                 | :white_check_mark: | :no_entry_sign:
`FEATURE_ASSEMBLIES`                | :white_check_mark: | :no_entry_sign:
`FEATURE_EVENTLOG`                  | :white_check_mark: | :no_entry_sign:
`FEATURE_GAC`                       | :white_check_mark: | :no_entry_sign:
`FEATURE_GETCALLINGASSEMBLY`        | :white_check_mark: | :no_entry_sign:
`FEATURE_ISUPPORTINITIALIZE`        | :white_check_mark: | :no_entry_sign:
`FEATURE_NETCORE_REFLECTION_API`    | :no_entry_sign:    | :white_check_mark:
`FEATURE_PERFCOUNTERS`              | :white_check_mark: | :no_entry_sign:
`FEATURE_REFLECTION_METHODBODY`     | :white_check_mark: | :no_entry_sign:
`FEATURE_REMOTING`                  | :white_check_mark: | :no_entry_sign:
`FEATURE_SECURITY_PERMISSIONS`      | :white_check_mark: | :no_entry_sign:
`FEATURE_SERIALIZATION`             | :white_check_mark: | :no_entry_sign:
`FEATURE_SYSTEM_CONFIGURATION`      | :white_check_mark: | :no_entry_sign:
`FEATURE_SYSTEM_WEB`                | :white_check_mark: | :no_entry_sign:
`FEATURE_URIMEMBERS`                | :white_check_mark: | :no_entry_sign:
`FEATURE_WINFORMS`                  | :white_check_mark: | :no_entry_sign:

* `CASTLE_SERVICES_LOGGING` - uses Castle.Services.Logging.Log4netIntegration or Castle.Services.Logging.NLogIntegration
* `FEATURE_APPDOMAIN` - enables support for features that make use of an AppDomain in the host.
* `FEATURE_ASSEMBLIES` - uses AssemblyName.GetAssemblyName() and Assembly.LoadFile() .
* `FEATURE_EVENTLOG` - uses Castle.Core API's that are based on Windows Event Log.
* `FEATURE_GAC` - enables support for obtaining assemblies using an assembly long form name.
* `FEATURE_GETCALLINGASSEMBLY` - enables code that uses System.Reflection.Assembly.GetCallingAssembly().
* `FEATURE_ISUPPORTINITIALIZE` - enables support for features that make use of System.ComponentModel.ISupportInitialize.
* `FEATURE_NETCORE_REFLECTION_API` - provides shims to implement missing functionality in .NET Core that has no alternatives.
* `FEATURE_PERFCOUNTERS` - enables code that uses Windows Performance Counters.
* `FEATURE_REFLECTION_METHODBODY` - enables code that System.Reflection.Methodbase.GetMethodBody() to get access to the MSIL for a method
* `FEATURE_REMOTING` - supports remoting on various types including inheriting from MarshalByRefObject.
* `FEATURE_SECURITY_PERMISSIONS` - enables the use of CAS and Security[Critical|SafeCritical|Transparent].
* `FEATURE_SERIALIZATION` - enables support for serialization of dynamic proxies and other types.
* `FEATURE_SYSTEM_CONFIGURATION` - enables features that use System.Configuration and the ConfigurationManager.
* `FEATURE_SYSTEM_WEB` - enables code that uses System.Web and System.Web.UI.
* `FEATURE_URIMEMBERS` - enables code that uses Uri.SchemeDelimiter.
* `FEATURE_WINFORMS` - enables code that uses Windows Forms.

The following conditional compilation symbols are defined for tests only, for .NET45
* `FEATURE_CODEDOM` - enables code that uses System.CodeDom.
* `FEATURE_CONSOLETRACELISTENER` - enables code that require System.Diagnostics.ConsoleTraceListener.
* `FEATURE_THREADABORT` - enables code that uses Thread.Abort().
* `FEATURE_WPF` - enables code that uses PresentationCore.dll.
* `NUNIT_SETCULTUREATTRIBUTE` - uses NUnit.Framework.SetCultureAttribute.
* `NUNIT_TIMEOUTATTRIBUTE` - uses NUnit.Framework.TimeoutAttribute.

