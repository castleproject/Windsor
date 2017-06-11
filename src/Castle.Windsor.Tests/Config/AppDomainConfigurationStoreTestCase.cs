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


#if FEATURE_SYSTEM_CONFIGURATION

namespace Castle.Windsor.Tests.Config
{
	using Castle.Core.Resource;
	using Castle.MicroKernel;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Configuration.Interpreters;

	using NUnit.Framework;

	/// <summary>
	///   Summary description for AppDomainConfigSourceTestCase.
	/// </summary>
	[TestFixture]
	public class AppDomainConfigSourceTestCase
	{
		[Test]
		public void ProperDeserialization()
		{
			var store = new DefaultConfigurationStore();
			var interpreter = new XmlInterpreter(new ConfigResource());
			IKernel kernel = new DefaultKernel();
			interpreter.ProcessResource(interpreter.Source, store, kernel);

			Assert.AreEqual(2, store.GetFacilities().Length);
			Assert.AreEqual(4, store.GetComponents().Length);

			var config = store.GetFacilityConfiguration(typeof(NoopFacility).FullName);
			var childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("value", childItem.Value);

			config = store.GetFacilityConfiguration(typeof(Noop2Facility).FullName);
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
		}
	}
}

#endif