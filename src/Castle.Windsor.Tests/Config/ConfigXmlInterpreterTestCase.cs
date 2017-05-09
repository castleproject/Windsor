// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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


#if !SILVERLIGHT
// we do not support xml config on SL

namespace Castle.Windsor.Tests
{
	using System.Linq;

	using Castle.Core.Configuration;
	using Castle.Core.Resource;
	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.Windsor.Configuration.Interpreters.XmlProcessor;
	using Castle.XmlFiles;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ConfigXmlInterpreterTestCase
	{
		[Test]
		public void ComponentIdGetsLoadedFromTheParsedConfiguration()
		{
			var store = new DefaultConfigurationStore();
			var interpreter = new XmlInterpreter(Xml.Embedded("sample_config_with_spaces.xml"));
			IKernel kernel = new DefaultKernel();
			interpreter.ProcessResource(interpreter.Source, store, kernel);

			var container = new WindsorContainer(store);

			var handler = container.Kernel.GetHandler(typeof(ICalcService));
			Assert.AreEqual(Core.LifestyleType.Transient, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void CorrectConfigurationMapping()
		{
			var store = new DefaultConfigurationStore();
			var interpreter = new XmlInterpreter(Xml.Embedded("sample_config.xml"));
			IKernel kernel = new DefaultKernel();
			interpreter.ProcessResource(interpreter.Source, store, kernel);

			var container = new WindsorContainer(store);
			var facility = container.Kernel.GetFacilities().OfType<HiperFacility>().Single();
			Assert.IsTrue(facility.Initialized);
		}

		[Test]
		public void MissingManifestResourceConfiguration()
		{
			var store = new DefaultConfigurationStore();
			var source = new AssemblyResource("assembly://Castle.Windsor.Tests/missing_config.xml");
			IKernel kernel = new DefaultKernel();
			Assert.Throws<ConfigurationProcessingException>(() => new XmlInterpreter(source).ProcessResource(source, store, kernel));
		}

		[Test]
		public void ProperDeserialization()
		{
			var store = new DefaultConfigurationStore();

			var interpreter = new XmlInterpreter(Xml.Embedded("sample_config_complex.xml"));
			IKernel kernel = new DefaultKernel();
			interpreter.ProcessResource(interpreter.Source, store, kernel);

			Assert.AreEqual(2, store.GetFacilities().Length);
			Assert.AreEqual(2, store.GetComponents().Length);
			Assert.AreEqual(2, store.GetConfigurationForChildContainers().Length);

			var config = store.GetFacilityConfiguration(typeof(DummyFacility).FullName);
			var childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("value", childItem.Value);

			config = store.GetFacilityConfiguration(typeof(HiperFacility).FullName);
			Assert.IsNotNull(config);
			Assert.AreEqual("value within CDATA section", config.Value);

			config = store.GetComponentConfiguration("testidcomponent1");
			childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("value1", childItem.Value);

			config = store.GetComponentConfiguration("testidcomponent2");
			childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("value2", childItem.Value);

			config = store.GetChildContainerConfiguration("child1");
			Assert.IsNotNull(config);
			Assert.AreEqual(config.Attributes["name"], "child1");
			Assert.AreEqual("<configuration />", config.Value);

			config = store.GetChildContainerConfiguration("child2");
			Assert.IsNotNull(config);
			Assert.AreEqual(config.Attributes["name"], "child2");
			Assert.AreEqual("<configuration />", config.Value);
		}

		[Test]
		public void ProperManifestDeserialization()
		{
			var store = new DefaultConfigurationStore();
			var interpreter = new XmlInterpreter(Xml.File("sample_config_complex.xml"));
			IKernel kernel = new DefaultKernel();
			interpreter.ProcessResource(interpreter.Source, store, kernel);

			Assert.AreEqual(2, store.GetFacilities().Length);
			Assert.AreEqual(2, store.GetComponents().Length);
			Assert.AreEqual(2, store.GetConfigurationForChildContainers().Length);

			var config = store.GetFacilityConfiguration(typeof(DummyFacility).FullName);
			var childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("value", childItem.Value);

			config = store.GetFacilityConfiguration(typeof(HiperFacility).FullName);
			Assert.IsNotNull(config);
			Assert.AreEqual("value within CDATA section", config.Value);

			config = store.GetComponentConfiguration("testidcomponent1");
			childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("value1", childItem.Value);

			config = store.GetComponentConfiguration("testidcomponent2");
			childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("value2", childItem.Value);

			config = store.GetChildContainerConfiguration("child1");
			Assert.IsNotNull(config);
			Assert.AreEqual(config.Attributes["name"], "child1");
			Assert.AreEqual("<configuration />", config.Value);

			config = store.GetChildContainerConfiguration("child2");
			Assert.IsNotNull(config);
			Assert.AreEqual(config.Attributes["name"], "child2");
			Assert.AreEqual("<configuration />", config.Value);
		}
	}

	public class DummyFacility : IFacility
	{
		public void Init(IKernel kernel, IConfiguration facilityConfig)
		{
			Assert.IsNotNull(facilityConfig);
			var childItem = facilityConfig.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("value", childItem.Value);
		}

		public void Terminate()
		{
		}
	}
}

#endif