// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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
	using Castle.Core.Logging;
	using Castle.MicroKernel.SubSystems.Configuration;
#if CASTLE_SERVICES_LOGGING
	using Castle.Services.Logging.Log4netIntegration;
	using Castle.Services.Logging.NLogIntegration;
#endif
	using Castle.Windsor;

	/// <summary>
	/// Summary description for BaseTest.
	/// </summary>
	public abstract class BaseTest
	{
		protected virtual IWindsorContainer CreateConfiguredContainer<TLoggerFactory>()
			where TLoggerFactory : ILoggerFactory
		{
			IWindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());
			var configFile = GetConfigFile<TLoggerFactory>();

			container.AddFacility<LoggingFacility>(f => f.LogUsing<TLoggerFactory>().WithConfig(configFile));

			return container;
		}

		protected string GetConfigFile<TLoggerFactory>()
			where TLoggerFactory : ILoggerFactory
		{
#if CASTLE_SERVICES_LOGGING
			if (typeof(TLoggerFactory) == typeof(Log4netFactory) ||
				typeof(TLoggerFactory) == typeof(ExtendedLog4netFactory))
			{
				return "LoggingFacility\\log4net.facilities.test.config";
			}
			if (typeof(TLoggerFactory) == typeof(NLogFactory) ||
				typeof(TLoggerFactory) == typeof(ExtendedNLogFactory))
			{
				return "LoggingFacility\\NLog.facilities.test.config";
			}
#endif
			return string.Empty;
		}
	}
}
