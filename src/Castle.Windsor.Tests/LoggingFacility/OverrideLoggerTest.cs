namespace Castle.Facilities.Logging.Tests
{
	using System;

	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Windsor;

	public abstract class OverrideLoggerTest : BaseTest
	{		
		protected virtual IWindsorContainer CreateConfiguredContainer(LoggerImplementation loggerApi, String custom, string logName)
		{
			IWindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());
			var configFile = GetConfigFile(loggerApi);

			container.AddFacility<LoggingFacility>(f => f.LogUsing(loggerApi).WithConfig(configFile).ToLog(logName));

			return container;
		}

	}
}