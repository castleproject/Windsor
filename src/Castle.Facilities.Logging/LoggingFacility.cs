// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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
		private readonly string customLoggerFactoryTypeName;
		private string configFileName;

		private ITypeConverter converter;

		private Type loggerFactoryType;
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
		///   Initializes a new instance of the <see cref="LoggingFacility" /> class using a custom LoggerImplementation
		/// </summary>
		/// <param name="customLoggerFactory"> The type name of the type of the custom logger factory. </param>
		/// <param name="configFile"> The configuration file that should be used by the chosen LoggerImplementation </param>
		public LoggingFacility(string customLoggerFactory, string configFile)
		{
			customLoggerFactoryTypeName = customLoggerFactory;
			configFileName = configFile;
		}

		public LoggingFacility LogUsing<TLoggerFactory>()
			where TLoggerFactory : ILoggerFactory
		{
			loggerFactoryType = typeof(TLoggerFactory);
			return this;
		}

		public LoggingFacility LogUsing<TLoggerFactory>(TLoggerFactory loggerFactory)
			where TLoggerFactory : ILoggerFactory
		{
			loggerFactoryType = typeof(TLoggerFactory);
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
			configFileName = configFile ?? throw new ArgumentNullException(nameof(configFile));
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
				ReadConfigurationAndCreateLoggerFactory();
			}
			RegisterLoggerFactory(loggerFactory);
			RegisterDefaultILogger(loggerFactory);
			RegisterSubResolver(loggerFactory);
		}

		private void ReadConfigurationAndCreateLoggerFactory()
		{
			if (loggerFactoryType == null)
			{
				loggerFactoryType = ReadCustomLoggerType();
			}
			EnsureIsValidLoggerFactoryType();
			CreateProperLoggerFactory();
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
			return typeof(NullLogFactory);
		}

		private void EnsureIsValidLoggerFactoryType()
		{
			if (!loggerFactoryType.Is<ILoggerFactory>())
			{
				throw new FacilityException($"The specified type '{loggerFactoryType}' does not implement ILoggerFactory.");
			}
		}

		private void CreateProperLoggerFactory()
		{
			Debug.Assert(loggerFactoryType != null, "loggerFactoryType != null");

			var ctorArgs = GetLoggingFactoryArguments();
			loggerFactory = loggerFactoryType.CreateInstance<ILoggerFactory>(ctorArgs);
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

		private object[] GetLoggingFactoryArguments()
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
			throw new FacilityException($"No support constructor found for logging type '{loggerFactoryType}'");
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
