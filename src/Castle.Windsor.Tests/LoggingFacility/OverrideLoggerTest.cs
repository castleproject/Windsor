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
	using Castle.Windsor;

	public abstract class OverrideLoggerTest : BaseTest
	{
		protected virtual IWindsorContainer CreateConfiguredContainer<TLoggerFactory>(string logName)
			where TLoggerFactory : ILoggerFactory
		{
			IWindsorContainer container = new WindsorContainer(new DefaultConfigurationStore());
			var configFile = GetConfigFile<TLoggerFactory>();

			container.AddFacility<LoggingFacility>(f => f.LogUsing<TLoggerFactory>().WithConfig(configFile).ToLog(logName));

			return container;
		}
	}
}