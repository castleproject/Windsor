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

#if !SILVERLIGHT

namespace Castle.Windsor.Tests.Configuration2
{
	using Config = Castle.Windsor.Installer.Configuration;
	using NUnit.Framework;

	[TestFixture(Description = "Based on http://theburningmonk.com/2010/08/castle-windsor-tips-say-no-to-private-setter/")]
	public class PrivateSettersTestCase
	{
		[Test]
		public void Private_setter_does_not_get_called_when_using_config()
		{
			var container = new WindsorContainer();
			container.Install(
				Config.FromXmlFile(
					ConfigHelper.ResolveConfigPath("Configuration2/class_with_private_setter.xml")));

			var item = container.Resolve<IMyConfiguration>();
			Assert.AreEqual(1234, item.Port);

		}
	}

	public interface IMyConfiguration
	{
		int Port { get; }
	}

	public class MyConfiguration : IMyConfiguration
	{
		public MyConfiguration(int port)
		{
			Port = port;
		}

		public int Port { get; private set; }
	}
}

#endif