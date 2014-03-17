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
namespace Castle.Windsor.Tests.Configuration2
{
	using NUnit.Framework;

	[TestFixture]
	public class ConfigWithStatementsTestCase
	{
		private IWindsorContainer container;

		[TestCase("debug")]
		[TestCase("prod")]
		[TestCase("qa")]
		[TestCase("default")]
		public void SimpleChoose(string flag)
		{
			var file = ConfigHelper.ResolveConfigPath("Configuration2/config_with_define_{0}.xml", flag);

			container = new WindsorContainer(file);

			var store = container.Kernel.ConfigurationStore;

			Assert.AreEqual(1, store.GetComponents().Length);

			var config = store.GetComponentConfiguration(flag);

			Assert.IsNotNull(config);
		}

		[Test]
		public void SimpleIf()
		{
			container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Configuration2/config_with_if_stmt.xml"));
			var store = container.Kernel.ConfigurationStore;

			Assert.AreEqual(4, store.GetComponents().Length);

			var config = store.GetComponentConfiguration("debug");
			Assert.IsNotNull(config);

			var childItem = config.Children["item"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("some value", childItem.Value);

			childItem = config.Children["item2"];
			Assert.IsNotNull(childItem);
			Assert.AreEqual("some <&> value2", childItem.Value);

			config = store.GetComponentConfiguration("qa");
			Assert.IsNotNull(config);

			config = store.GetComponentConfiguration("default");
			Assert.IsNotNull(config);

			config = store.GetComponentConfiguration("notprod");
			Assert.IsNotNull(config);
		}
	}
}

#endif