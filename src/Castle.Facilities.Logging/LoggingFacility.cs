// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Facilities.Logging
{
	using System;
	using System.Diagnostics;
	using System.Reflection;

	using Castle.Core.Internal;
	using Castle.Core.Logging;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	///   A facility for logging support.
	/// </summary>
	public class LoggingFacility : AbstractFacility
	{
#if CASTLE_SERVICES_LOGGING   //Castle.Services.Logging.Log4netIntegration and Castle.Services.Logging.NLogIntegration are not available for .NET Standard
		private static readonly String ExtendedLog4NetLoggerFactoryTypeName =
			"Castle.Services.Logging.Log4netIntegration.ExtendedLog4netFactory," +
			"Castle.Services.Logging.Log4netIntegration,Version=4.0.0.0, Culture=neutral," +
			"PublicKeyToken=407dd0808d44fbdc";

		private static readonly String ExtendedNLogLoggerFactoryTypeName =
			"Castle.Services.Logging.NLogIntegration.ExtendedNLogFactory," +
			"Castle.Services.Logging.NLogIntegration,Version=4.0.0.0, Culture=neutral," +
			"PublicKeyToken=407dd0808d44fbdc";

		private static readonly String Log4NetLoggerFactoryTypeName =
			"Castle.Services.Logging.Log4netIntegration.Log4netFactory," +
			"Castle.Services.Logging.Log4netIntegration,Version=4.0.0.0, Culture=neutral," +
			"PublicKeyToken=407dd0808d44fbdc";

		private static readonly String NLogLoggerFactoryTypeName =
			"Castle.Services.Logging.NLogIntegration.NLogFactory," +
			"Castle.Services.Logging.NLogIntegration,Version=4.0.0.0, Culture=neutral," +
			"PublicKeyToken=407dd0808d44fbdc";
#endif
		private readonly string customLoggerFactoryTypeName;
		private string configFileName;

		private ITypeConverter converter;

		private LoggerImplementation? loggerImplementation;
		private Type loggingFactoryType;
		private LoggerLevel? loggerLevel;
		private ILoggerFactory loggerFactory;
		private string logName;
		private bool configuredExternally;

		/// <summary>
		///   Initializes a new instance of the <see cref="LoggingFacility" /> class.
		/// </summary>
		public LoggingFacility()
		{
		}

		/// <summary>
		///   Initializes a new instance of the <see cref="LoggingFacility" /> class.
		/// </summary>
		/// <param name="loggingApi"> The LoggerImplementation that should be used </param>
		public LoggingFacility(LoggerImplementation loggingApi) : this(loggingApi, null)
		{
		}

		/// <summary>
		///   Initializes a new instance of the <see cref="LoggingFacility" /> class.
		/// </summary>
		/// <param name="loggingApi"> The LoggerImplementation that should be used </param>
		/// <param name="configFile"> The configuration file that should be used by the chosen LoggerImplementation </param>
		public LoggingFacility(LoggerImplementation loggingApi, string configFile) : this(loggingApi, null, configFile)
		{
		}

		/// <summary>
		///   Initializes a new instance of the <see cref="LoggingFacility" /> class using a custom LoggerImplementation
		/// </summary>
		/// <param name="configFile"> The configuration file that should be used by the chosen LoggerImplementation </param>
		/// <param name="customLoggerFactory"> The type name of the type of the custom logger factory. </param>
		public LoggingFacility(string customLoggerFactory, string configFile)
			: this(LoggerImplementation.Custom, customLoggerFactory, configFile)
		{
		}

		/// <summary>
		///   Initializes a new instance of the <see cref="LoggingFacility" /> class.
		/// </summary>
		/// <param name="loggingApi"> The LoggerImplementation that should be used </param>
		/// <param name="configFile"> The configuration file that should be used by the chosen LoggerImplementation </param>
		/// <param name="customLoggerFactory"> The type name of the type of the custom logger factory. (only used when loggingApi is set to LoggerImplementation.Custom) </param>
		public LoggingFacility(LoggerImplementation loggingApi, string customLoggerFactory, string configFile)
		{
			loggerImplementation = loggingApi;
			customLoggerFactoryTypeName = customLoggerFactory;
			configFileName = configFile;
		}

		public LoggingFacility LogUsing(LoggerImplementation loggingApi)
		{
			if (loggingApi == LoggerImplementation.Custom)
			{
				throw new FacilityException("To use custom logger use LogUsing<TCUstomLoggerFactory>() method.");
			}
			loggerImplementation = loggingApi;
			return this;
		}

		public LoggingFacility LogUsing<TCustomLoggerFactory>() where TCustomLoggerFactory : ILoggerFactory
		{
			loggerImplementation = LoggerImplementation.Custom;
			loggingFactoryType = typeof(TCustomLoggerFactory);
			return this;
		}

		public LoggingFacility LogUsing<TCustomLoggerFactory>(TCustomLoggerFactory loggerFactory) where TCustomLoggerFactory : ILoggerFactory
		{
			loggerImplementation = LoggerImplementation.Custom;
			loggingFactoryType = typeof(TCustomLoggerFactory);
			this.loggerFactory = loggerFactory;
			return this;
		}

		public LoggingFacility ConfiguredExternally()
		{
			configuredExternally = true;
			return this;
		}

		public LoggingFacility WithConfig(string configFile)
		{
			if (configFile == null)
			{
				throw new ArgumentNullException("configFile");
			}

			configFileName = configFile;
			return this;
		}

		public LoggingFacility WithLevel(LoggerLevel level)
		{
			loggerLevel = level;
			return this;
		}

		public LoggingFacility ToLog(string name)
		{
			logName = name;
			return this;
		}

#if CASTLE_SERVICES_LOGGING
		public LoggingFacility UseLog4Net()
		{
			return LogUsing(LoggerImplementation.Log4net);
		}

		public LoggingFacility UseLog4Net(string configFile)
		{
			return UseLog4Net().WithConfig(configFile);
		}

		public LoggingFacility UseNLog()
		{
			return LogUsing(LoggerImplementation.NLog);
		}

		public LoggingFacility UseNLog(string configFile)
		{
			return LogUsing(LoggerImplementation.NLog).WithConfig(configFile);
		}
#endif

#if FEATURE_SYSTEM_CONFIGURATION
		/// <summary>
		///   loads configuration from current AppDomain's config file (aka web.config/app.config)
		/// </summary>
		/// <returns> </returns>
		public LoggingFacility WithAppConfig()
		{
			configFileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			return this;
		}
#endif

		protected override void Init()
		{
			SetUpTypeConverter();
			if (loggerFactory == null)
			{
				loggerFactory = ReadConfigurationAndCreateLoggerFactory();
			}
			RegisterLoggerFactory(loggerFactory);
			RegisterDefaultILogger(loggerFactory);
			RegisterSubResolver(loggerFactory);
		}

		private ILoggerFactory CreateProperLoggerFactory(LoggerImplementation loggerApi)
		{
			var loggerFactoryType = GetLoggingFactoryType(loggerApi);
			Debug.Assert(loggerFactoryType != null, "loggerFactoryType != null");

			var ctorArgs = GetLoggingFactoryArguments(loggerFactoryType);
			return loggerFactoryType.CreateInstance<ILoggerFactory>(ctorArgs);
		}

		private Type EnsureIsValidLoggerFactoryType(Type loggerFactoryType)
		{
			if (loggerFactoryType.Is<ILoggerFactory>() || loggerFactoryType.Is<IExtendedLoggerFactory>())
			{
				return loggerFactoryType;
			}
			throw new FacilityException("The specified type '" + loggerFactoryType +
			                            "' does not implement either ILoggerFactory or IExtendedLoggerFactory.");
		}

		private string GetConfigFile()
		{
			if (configFileName != null)
			{
				return configFileName;
			}

			if (FacilityConfig != null)
			{
				return FacilityConfig.Attributes["configFile"];
			}
			return null;
		}

		private Type GetCustomLoggerType()
		{
			return EnsureIsValidLoggerFactoryType(ReadCustomLoggerType());
		}

		private object[] GetLoggingFactoryArguments(Type loggerFactoryType)
		{
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

			ConstructorInfo ctor;
			if (IsConfiguredExternally())
			{
				ctor = loggerFactoryType.GetConstructor(flags, null, new[] { typeof(bool) }, null);
				if (ctor != null)
				{
					return new object[] { true };
				}
			}
			var configFile = GetConfigFile();
			if (configFile != null)
			{
				ctor = loggerFactoryType.GetConstructor(flags, null, new[] { typeof(string) }, null);
				if (ctor != null)
				{
					return new object[] { configFile };
				}
			}

			var level = GetLoggingLevel();
			if (level != null)
			{
				ctor = loggerFactoryType.GetConstructor(flags, null, new[] { typeof(LoggerLevel) }, null);
				if (ctor != null)
				{
					return new object[] { level.Value };
				}
			}
			ctor = loggerFactoryType.GetConstructor(flags, null, Type.EmptyTypes, null);
			if (ctor != null)
			{
				return new object[0];
			}
			throw new FacilityException("No support constructor found for logging type " + loggerFactoryType);
		}

		private bool IsConfiguredExternally()
		{
			if (configuredExternally)
			{
				return true;
			}
			if (FacilityConfig != null)
			{
				var value = FacilityConfig.Attributes["configuredExternally"];
				if (value != null)
				{
					return converter.PerformConversion<bool>(value);
				}
			}
			return false;
		}

		private LoggerLevel? GetLoggingLevel()
		{
			if (loggerLevel.HasValue)
			{
				return loggerLevel;
			}
			if (FacilityConfig != null)
			{
				var level = FacilityConfig.Attributes["loggerLevel"];
				if (level != null)
				{
					return converter.PerformConversion<LoggerLevel>(level);
				}
			}
			return null;
		}

		private Type GetLoggingFactoryType(LoggerImplementation loggerApi)
		{
			switch (loggerApi)
			{
				case LoggerImplementation.Custom:
					return GetCustomLoggerType();
				case LoggerImplementation.Null:
					return typeof(NullLogFactory);
				case LoggerImplementation.Console:
					return typeof(ConsoleFactory);
#if FEATURE_EVENTLOG   //has dependency on Castle.Core.Logging.DiagnosticsLoggerFactory
				case LoggerImplementation.Diagnostics:
					return typeof(DiagnosticsLoggerFactory);
#endif
				case LoggerImplementation.Trace:
					return typeof(TraceLoggerFactory);
#if CASTLE_SERVICES_LOGGING
				case LoggerImplementation.NLog:
					return converter.PerformConversion<Type>(NLogLoggerFactoryTypeName);
				case LoggerImplementation.Log4net:
					return converter.PerformConversion<Type>(Log4NetLoggerFactoryTypeName);
				case LoggerImplementation.ExtendedLog4net:
					return converter.PerformConversion<Type>(ExtendedLog4NetLoggerFactoryTypeName);
				case LoggerImplementation.ExtendedNLog:
					return converter.PerformConversion<Type>(ExtendedNLogLoggerFactoryTypeName);
#endif
				default:
					{
						throw new FacilityException("An invalid loggingApi was specified: " + loggerApi);
					}
			}
		}

		private ILoggerFactory ReadConfigurationAndCreateLoggerFactory()
		{
			var logApi = ReadLoggingApi();
			var loggerFactory = CreateProperLoggerFactory(logApi);
			return loggerFactory;
		}

		private Type ReadCustomLoggerType()
		{
			if (FacilityConfig != null)
			{
				var customLoggerType = FacilityConfig.Attributes["customLoggerFactory"];
				if (string.IsNullOrEmpty(customLoggerType) == false)
				{
					return converter.PerformConversion<Type>(customLoggerType);
				}
			}
			if (customLoggerFactoryTypeName != null)
			{
				return converter.PerformConversion<Type>(customLoggerFactoryTypeName);
			}
			if (loggingFactoryType != null)
			{
				return loggingFactoryType;
			}
			var message = "If you specify loggingApi='custom' " +
			              "then you must use the attribute customLoggerFactory to inform the " +
			              "type name of the custom logger factory";

			throw new FacilityException(message);
		}

		private LoggerImplementation ReadLoggingApi()
		{
			if (FacilityConfig != null)
			{
				var configLoggingApi = FacilityConfig.Attributes["loggingApi"];
				if (string.IsNullOrEmpty(configLoggingApi) == false)
				{
					return converter.PerformConversion<LoggerImplementation>(configLoggingApi);
				}
			}
			return loggerImplementation.GetValueOrDefault(LoggerImplementation.Console);
		}

		private void RegisterDefaultILogger(ILoggerFactory factory)
		{
			if (factory is IExtendedLoggerFactory)
			{
				var defaultLogger = ((IExtendedLoggerFactory) factory).Create(logName ?? "Default");
				Kernel.Register(Component.For<IExtendedLogger>().NamedAutomatically("ilogger.default").Instance(defaultLogger),
				                Component.For<ILogger>().NamedAutomatically("ilogger.default.base").Instance(defaultLogger));
			}
			else
			{
				Kernel.Register(Component.For<ILogger>().NamedAutomatically("ilogger.default").Instance(factory.Create(logName ?? "Default")));
			}
		}

		private void RegisterLoggerFactory(ILoggerFactory factory)
		{
			if (factory is IExtendedLoggerFactory)
			{
				Kernel.Register(
					Component.For<IExtendedLoggerFactory>().NamedAutomatically("iloggerfactory").Instance((IExtendedLoggerFactory) factory),
					Component.For<ILoggerFactory>().NamedAutomatically("iloggerfactory.base").Instance(factory));
			}
			else
			{
				Kernel.Register(Component.For<ILoggerFactory>().NamedAutomatically("iloggerfactory").Instance(factory));
			}
		}

		private void RegisterSubResolver(ILoggerFactory loggerFactory)
		{
			var extendedLoggerFactory = loggerFactory as IExtendedLoggerFactory;
			if (extendedLoggerFactory == null)
			{
				Kernel.Resolver.AddSubResolver(new LoggerResolver(loggerFactory, logName));
				return;
			}
			Kernel.Resolver.AddSubResolver(new LoggerResolver(extendedLoggerFactory, logName));
		}

		private void SetUpTypeConverter()
		{
			converter = Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey) as IConversionManager;
		}
	}
}