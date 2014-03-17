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

namespace CastleTests
{
	using Castle.Core.Resource;
	using Castle.Windsor.Installer;

	using CastleTests.Components;

	using NUnit.Framework;

#if !SILVERLIGHT
	[TestFixture]
	public class RegistrationWithAttributeAndXmlTestCase : AbstractContainerTestCase
	{
		[Test]
		[Bug("IOC-295")]
		public void Registration_via_xml_no_service_specified_uses_service_from_attribute()
		{
			var xml = @"<configuration>
  <components>
    <component type=""HasType"" />
  </components>
</configuration>";

			Container.Install(Configuration.FromXml(new StaticContentResource(xml)));
			Assert.IsTrue(Kernel.HasComponent(typeof(ISimpleService)));
		}
	}
#endif
}