#region License

//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

#endregion

namespace Castle.Facilities.NHibernateIntegration.Tests
{
	using System.Configuration;
	using Builders;
	using Core.Configuration;
	using Configuration = NHibernate.Cfg.Configuration;

	public class TestConfigurationBuilder : IConfigurationBuilder
	{
		private readonly IConfigurationBuilder defaultConfigurationBuilder;

	    public TestConfigurationBuilder()
		{
			defaultConfigurationBuilder = new DefaultConfigurationBuilder();
		}

		#region IConfigurationBuilder Members

		public Configuration GetConfiguration(IConfiguration config)
		{
			Configuration nhConfig = defaultConfigurationBuilder.GetConfiguration(config);
			nhConfig.Properties["dialect"] = ConfigurationManager.AppSettings["nhf.dialect"];
			nhConfig.Properties["connection.driver_class"] = ConfigurationManager.AppSettings["nhf.connection.driver_class"];
			nhConfig.Properties["connection.provider"] = ConfigurationManager.AppSettings["nhf.connection.provider"];
			nhConfig.Properties["connection.connection_string"] =
				ConfigurationManager.AppSettings["nhf.connection.connection_string.1"];
			if (config.Attributes["id"] != "sessionFactory1")
			{
				nhConfig.Properties["connection.connection_string"] =
					ConfigurationManager.AppSettings["nhf.connection.connection_string.2"];
			}
			return nhConfig;
		}

		#endregion
	}
}