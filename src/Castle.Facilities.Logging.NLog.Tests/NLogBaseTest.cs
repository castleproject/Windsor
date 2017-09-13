// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.Logging.Tests
{
	using System;

	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Windsor;

	public abstract class NLogBaseTest
	{
		protected virtual IWindsorContainer CreateConfiguredContainer(Castle.Facilities.Logging.NLogFacility.LoggerImplementation loggerApi, String custom, string logName)
		{
			IWindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());
			var configFile = GetConfigFile(loggerApi);

			container.AddFacility<Castle.Facilities.Logging.NLogFacility.LoggingFacility>(f => f.LogUsing(loggerApi).WithConfig(configFile).ToLog(logName));

			return container;
		}

		protected IWindsorContainer CreateConfiguredContainerNLog(Castle.Facilities.Logging.NLogFacility.LoggerImplementation loggerApi)
		{
			return CreateConfiguredContainerNLog(loggerApi, String.Empty);
		}

		protected IWindsorContainer CreateConfiguredContainerNLog(Castle.Facilities.Logging.NLogFacility.LoggerImplementation loggerApi, String custom)
		{
			IWindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());
			var configFile = GetConfigFile(loggerApi);

			container.AddFacility<Castle.Facilities.Logging.NLogFacility.LoggingFacility>(f => f.LogUsing(loggerApi).WithConfig(configFile));

			return container;
		}

		protected string GetConfigFile(Castle.Facilities.Logging.NLogFacility.LoggerImplementation loggerApi)
		{
			string configFile = string.Empty;
#if CASTLE_SERVICES_LOGGING
			switch (loggerApi)
			{
				case Castle.Facilities.Logging.NLogFacility.LoggerImplementation.NLog:
				{
					configFile = "NLog.facilities.test.config";
					break;
				}
				case Castle.Facilities.Logging.NLogFacility.LoggerImplementation.ExtendedNLog:
				{
					configFile = "NLog.facilities.test.config";
					break;
				}
			}
#endif
			return configFile;
		}

	}
}