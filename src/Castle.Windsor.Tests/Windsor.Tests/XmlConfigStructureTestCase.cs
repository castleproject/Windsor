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

	using Castle.Core;
	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.Windsor.Installer;
	using Castle.XmlFiles;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class XmlConfigStructureTestCase : AbstractContainerTestCase
	{
		private IWindsorInstaller FromFile(string fileName)
		{
			var file = Xml.Embedded(fileName);
			return Configuration.FromXml(file);
		}

		[Test]
		public void Custom_lifestyle_can_be_specify_via_type_only()
		{
			Container.Install(FromFile("CustomLifestyle.xml"));
			var handler = Kernel.GetHandler(typeof(A));

			Assert.IsNotNull(handler);
			Assert.AreEqual(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
			Assert.AreEqual(typeof(CustomLifestyleManager), handler.ComponentModel.CustomLifestyle);
		}

		[Test]
		public void Id_is_not_required_for_component_if_type_is_specified()
		{
			Container.Install(FromFile("componentWithoutId.xml"));
			Kernel.Resolve<A>();
		}

		[Test]
		public void Id_is_not_required_for_facility_if_type_is_specified()
		{
			Container.Install(FromFile("facilityWithoutId.xml"));
			var facilities = Kernel.GetFacilities();
			Assert.IsNotEmpty(facilities);
			Assert.IsInstanceOf<StartableFacility>(facilities.Single());
		}

		[Test]
		[Bug("IoC-103")]
		public void Invalid_nodes_are_reported_via_exception()
		{
			var e =
				Assert.Throws<ConfigurationProcessingException>(
					() => Container.Install(FromFile("IOC-103.xml")));

			var expected =
				@"Configuration parser encountered <aze>, but it was expecting to find <installers>, <facilities> or <components>. There might be either a typo on <aze> or you might have forgotten to nest it properly.";
			Assert.AreEqual(expected, e.Message);
		}
	}
}

#endif