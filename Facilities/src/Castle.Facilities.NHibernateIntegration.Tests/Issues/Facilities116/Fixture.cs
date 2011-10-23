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

namespace Castle.Facilities.NHibernateIntegration.Tests.Issues.Facilities116
{
	using System;
	using System.Configuration;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading;
	using Builders;
	using Core.Configuration;
	using Core.Resource;
	using MicroKernel.SubSystems.Configuration;
	using NUnit.Framework;
	using Windsor.Configuration.Interpreters;
	using Configuration = NHibernate.Cfg.Configuration;

	[TestFixture]
	public class Fixture : IssueTestCase
	{
		protected override string ConfigurationFile
		{
			get { return "EmptyConfiguration.xml"; }
		}

		private const string filename = "myconfig.dat";
		private IConfiguration configuration;
		private IConfigurationBuilder configurationBuilder;

		public override void OnSetUp()
		{
			var configurationStore = new DefaultConfigurationStore();
			var resource = new AssemblyResource("Castle.Facilities.NHibernateIntegration.Tests/Issues/Facilities116/facility.xml");
			var xmlInterpreter = new XmlInterpreter(resource);
			xmlInterpreter.ProcessResource(resource, configurationStore);
			configuration = configurationStore.GetFacilityConfiguration("nhibernatefacility").Children["factory"];
			configurationBuilder = new PersistentConfigurationBuilder();
		}

		public override void OnTearDown()
		{
			File.Delete(filename);
		}

		[Test]
		public void Can_create_serialized_file_in_the_disk()
		{
			Assert.IsFalse(File.Exists(filename));
			Configuration cfg = configurationBuilder.GetConfiguration(configuration);
			Assert.IsTrue(File.Exists(filename));
			BinaryFormatter bf = new BinaryFormatter();
			Configuration nhConfig;
			using (var fileStream = new FileStream(filename, FileMode.Open))
			{
				nhConfig = bf.Deserialize(fileStream) as Configuration;
			}
			Assert.IsNotNull(nhConfig);

			ConfigureConnectionSettings(nhConfig);

			nhConfig.BuildSessionFactory();
		}

		[Test]
		public void Can_deserialize_file_from_the_disk_if_new_enough()
		{
			Assert.IsFalse(File.Exists(filename));
			Configuration nhConfig = configurationBuilder.GetConfiguration(configuration);
			Assert.IsTrue(File.Exists(filename));
			DateTime dateTime = File.GetLastWriteTime(filename);
			Thread.Sleep(1000);
			nhConfig = configurationBuilder.GetConfiguration(configuration);
			Assert.AreEqual(File.GetLastWriteTime(filename), dateTime);
			Assert.IsNotNull(configuration);

			ConfigureConnectionSettings(nhConfig);

			nhConfig.BuildSessionFactory();
		}

		[Test]
		public void Can_deserialize_file_from_the_disk_if_one_of_the_dependencies_is_newer()
		{
			Assert.IsFalse(File.Exists(filename));
			Configuration nhConfig = configurationBuilder.GetConfiguration(configuration);
			Assert.IsTrue(File.Exists(filename));
			DateTime dateTime = File.GetLastWriteTime(filename);
			Thread.Sleep(1000);
			DateTime dateTime2 = DateTime.Now;
			File.Create("SampleDllFile").Dispose();
			File.SetLastWriteTime("SampleDllFile", dateTime2);
			nhConfig = configurationBuilder.GetConfiguration(configuration);
			Assert.Greater(File.GetLastWriteTime(filename), dateTime2);
			Assert.IsNotNull(configuration);

			ConfigureConnectionSettings(nhConfig);

			nhConfig.BuildSessionFactory();
		}

		private static void ConfigureConnectionSettings(Configuration nhConfig)
		{
			nhConfig.Properties["dialect"] = ConfigurationManager.AppSettings["nhf.dialect"];
			nhConfig.Properties["connection.driver_class"] = ConfigurationManager.AppSettings["nhf.connection.driver_class"];
			nhConfig.Properties["connection.provider"] = ConfigurationManager.AppSettings["nhf.connection.provider"];
			nhConfig.Properties["connection.connection_string"] =
				ConfigurationManager.AppSettings["nhf.connection.connection_string.1"];
		}
	}
}