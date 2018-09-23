# Castle Windsor

<img align="right" src="docs/images/windsor-logo.png">

Castle Windsor is a best of breed, mature Inversion of Control container available for .NET.

See the [documentation](docs/README.md).

## Releases

See the [releases](https://github.com/castleproject/Windsor/releases).

## License

Castle Windsor is &copy; 2004-2018 Castle Project. It is free software, and may be redistributed under the terms of the [Apache 2.0](http://opensource.org/licenses/Apache-2.0) license.

## NuGet Preview Feed

If you would like to use preview NuGet's from our CI builds on AppVeyor, you can add the following NuGet source to your project:

```
https://ci.appveyor.com/nuget/windsor-qkry8n2r6yak
```

## Building

### Conditional Compilation Symbols

The following conditional compilation symbols are currently defined for Windsor:

Symbol                              | .NET 4.5           | .NET Standard
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
`FEATURE_REMOTING`                  | :white_check_mark: | :no_entry_sign:
`FEATURE_SECURITY_PERMISSIONS`      | :white_check_mark: | :no_entry_sign:
`FEATURE_SERIALIZATION`             | :white_check_mark: | :no_entry_sign:
`FEATURE_SYSTEM_CONFIGURATION`      | :white_check_mark: | :no_entry_sign:
`FEATURE_URIMEMBERS`                | :white_check_mark: | :no_entry_sign:

* `CASTLE_SERVICES_LOGGING` - enables access to `Castle.Services.Logging.log4netIntegration` and `Castle.Services.Logging.NLogIntegration` in the logging facility.
* `FEATURE_APPDOMAIN` - enables support for features that make use of an AppDomain in the host.
* `FEATURE_ASSEMBLIES` - uses `AssemblyName.GetAssemblyName()` and `Assembly.LoadFile()`.
* `FEATURE_EVENTLOG` - uses Castle Core APIs that are based on the Windows Event Log.
* `FEATURE_GAC` - enables support for obtaining assemblies using an assembly's long form name.
* `FEATURE_GETCALLINGASSEMBLY` - enables code that uses `System.Reflection.Assembly.GetCallingAssembly()`.
* `FEATURE_ISUPPORTINITIALIZE` - enables support for features that make use of `System.ComponentModel.ISupportInitialize`.
* `FEATURE_NETCORE_REFLECTION_API` - provides shims to implement missing functionality in .NET Core that have no alternatives.
* `FEATURE_PERFCOUNTERS` - enables code that uses Windows Performance Counters.
* `FEATURE_REMOTING` - supports remoting on various types including inheriting from `MarshalByRefObject`.
* `FEATURE_SECURITY_PERMISSIONS` - enables the use of CAS and `Security[Critical|SafeCritical|Transparent]`.
* `FEATURE_SERIALIZATION` - enables support for serialization of dynamic proxies and other types.
* `FEATURE_SYSTEM_CONFIGURATION` - enables features that use `System.Configuration` and the `ConfigurationManager`.
* `FEATURE_URIMEMBERS` - enables code that uses `Uri.SchemeDelimiter`.

The following conditional compilation symbols are defined for tests only under .NET 4.5:
* `FEATURE_CODEDOM` - enables code that uses `System.CodeDom`.
* `FEATURE_CONSOLETRACELISTENER` - enables code that requires `System.Diagnostics.ConsoleTraceListener`.
* `FEATURE_THREADABORT` - enables code that uses `Thread.Abort()`.
* `FEATURE_WPF` - enables code that uses `PresentationCore.dll`.
* `NUNIT_SETCULTUREATTRIBUTE` - uses `NUnit.Framework.SetCultureAttribute`.
* `NUNIT_TIMEOUTATTRIBUTE` - uses `NUnit.Framework.TimeoutAttribute`.
