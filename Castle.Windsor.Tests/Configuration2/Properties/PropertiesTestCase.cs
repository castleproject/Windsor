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

#if !SILVERLIGHT // we do not support xml config on SL

namespace Castle.Windsor.Tests.Configuration2.Properties
{
	using Castle.Core.Configuration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Windsor.Configuration.Interpreters.XmlProcessor;
	using NUnit.Framework;

	[TestFixture]
	public class PropertiesTestCase
	{
		private IWindsorContainer container;

		[Test]
		public void CorrectEval()
		{
			container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_properties.xml"));

			AssertConfiguration();
		}

		[Test]
		public void SilentProperties()
		{
			container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_silent_properties.xml"));

			IConfigurationStore store = container.Kernel.ConfigurationStore;

			Assert.AreEqual(1, store.GetFacilities().Length, "Diff num of facilities");
			Assert.AreEqual(1, store.GetComponents().Length, "Diff num of components");

			IConfiguration config = store.GetFacilityConfiguration("facility1");
			IConfiguration childItem = config.Children["param1"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("prop1 value", childItem.Value);
			Assert.AreEqual("", childItem.Attributes["attr"]);

			config = store.GetComponentConfiguration("component1");
			childItem = config.Children["param1"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual(null, childItem.Value);
			Assert.AreEqual("prop1 value", childItem.Attributes["attr"]);
		}

		[Test, ExpectedException(typeof(ConfigurationProcessingException))]
		public void MissingProperties()
		{
			container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_missing_properties.xml"));
		}

		[Test]
		public void PropertiesWithinProperties()
		{
			container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/properties_using_properties.xml"));

			AssertConfiguration();
		}

		[Test]
		public void PropertiesAndIncludes()
		{
			container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_properties_and_includes.xml"));

			AssertConfiguration();
		}

		[Test]
		public void PropertiesAndDefines()
		{
			container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_properties_and_defines.xml"));

			AssertConfiguration();
		}

		[Test]
		public void PropertiesAndDefines2()
		{
			container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/Properties/config_with_properties_and_defines2.xml"));

			AssertConfiguration();
		}

		private void AssertConfiguration()
		{
			IConfigurationStore store = container.Kernel.ConfigurationStore;

			Assert.AreEqual(3, store.GetFacilities().Length, "Diff num of facilities");
			Assert.AreEqual(2, store.GetComponents().Length, "Diff num of components");

			IConfiguration config = store.GetFacilityConfiguration("facility1");
			IConfiguration childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("prop1 value", childItem.Value);

			config = store.GetFacilityConfiguration("facility2");
			Assert.IsNotNull(config);
			childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("prop2 value", childItem.Attributes["value"]);
			Assert.IsNull(childItem.Value);

			config = store.GetFacilityConfiguration("facility3");
			Assert.IsNotNull(config);
			Assert.AreEqual(3, config.Children.Count, "facility3 should have 3 children");

			childItem = config.Children["param1"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("prop2 value", childItem.Value);
			Assert.AreEqual("prop1 value", childItem.Attributes["attr"]);

			childItem = config.Children["param2"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("prop1 value", childItem.Value);
			Assert.AreEqual("prop2 value", childItem.Attributes["attr"]);

			childItem = config.Children["param3"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("param3 attr", childItem.Attributes["attr"]);

			childItem = childItem.Children["value"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("param3 value", childItem.Value);
			Assert.AreEqual("param3 value attr", childItem.Attributes["attr"]);

			config = store.GetComponentConfiguration("component1");
			childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("prop1 value", childItem.Value);

			config = store.GetComponentConfiguration("component2");
			childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("prop2 value", childItem.Attributes["value"]);
		}
	}
}

#endif