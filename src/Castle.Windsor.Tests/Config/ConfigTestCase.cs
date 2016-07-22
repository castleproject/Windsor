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

namespace CastleTests.Config
{
#if !SILVERLIGHT
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Interceptors;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ConfigTestCase : AbstractContainerTestCase
	{
		[Test]
		[Ignore("Not supported. Would be good to have, not sure if in this form or another")]
		public void Can_split_configuration_between_multiple_component_elements()
		{
			// see http://stackoverflow.com/questions/3253975/castle-windsor-with-xml-includes-customization-problem for real life scenario
			Container.Install(Configuration.FromXmlFile(ConfigHelper.ResolveConfigPath("Configuration/OneComponentInTwoPieces.xml")));
			var service = Container.Resolve<ISimpleService>("Foo");
			var interceptor = Container.Resolve<CountingInterceptor>("a");

			service.Operation();

			Assert.AreEqual(1, interceptor.InterceptedCallsCount);
		}
	}
#endif
}