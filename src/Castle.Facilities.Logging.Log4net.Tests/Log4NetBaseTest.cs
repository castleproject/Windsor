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

	public abstract class Log4NetBaseTest
	{
		protected virtual IWindsorContainer CreateConfiguredContainer(Castle.Facilities.Logging.Log4netFacility.LoggerImplementation loggerApi, String custom, string logName)
		{
			IWindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());
			var configFile = GetConfigFile(loggerApi);

			container.AddFacility<Castle.Facilities.Logging.Log4netFacility.LoggingFacility>(f => f.LogUsing(loggerApi).WithConfig(configFile).ToLog(logName));

			return container;
		}
		protected virtual IWindsorContainer CreateConfiguredContainerLog4Net(Castle.Facilities.Logging.Log4netFacility.LoggerImplementation loggerApi)
		{
			return CreateConfiguredContainerLog4Net(loggerApi, String.Empty);
		}

		protected virtual IWindsorContainer CreateConfiguredContainerLog4Net(Castle.Facilities.Logging.Log4netFacility.LoggerImplementation loggerApi, String custom)
		{
			IWindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());
			var configFile = GetConfigFile(loggerApi);

			container.AddFacility<Castle.Facilities.Logging.Log4netFacility.LoggingFacility>(f => f.LogUsing(loggerApi).WithConfig(configFile));

			return container;
		}

		protected string GetConfigFile(Castle.Facilities.Logging.Log4netFacility.LoggerImplementation loggerApi)
		{
			string configFile = string.Empty;
#if CASTLE_SERVICES_LOGGING
			switch (loggerApi)
			{
				case Castle.Facilities.Logging.Log4netFacility.LoggerImplementation.Log4Net:
				{
					configFile = "Log4Net.facilities.test.config";
					break;
				}
				case Castle.Facilities.Logging.Log4netFacility.LoggerImplementation.ExtendedLog4Net:
				{
					configFile = "Log4Net.facilities.test.config";
					break;
				}
			}
#endif
			return configFile;
		}
	}
}