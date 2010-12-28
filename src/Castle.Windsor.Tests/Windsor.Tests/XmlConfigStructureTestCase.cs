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
namespace Castle.Windsor.Tests
{
	using Castle.Core;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.XmlFiles;

	using NUnit.Framework;

	[TestFixture]
	public class XmlConfigStructureTestCase : AbstractContainerTestFixture
	{

		[Test]
		public void Custom_lifestyle_can_be_specify_via_type_only()
		{
			Container.Install(FromFile("CustomLifestyle.xml"));
			var handler = Kernel.GetHandler(typeof(A));

			Assert.IsNotNull(handler);
			Assert.AreEqual(LifestyleType.Custom, handler.ComponentModel.LifestyleType);
			Assert.AreEqual(typeof(CustomLifestyleManager), handler.ComponentModel.CustomLifestyle);
		}

		private IWindsorInstaller FromFile(string fileName)
		{
			var file = Xml.Embedded(fileName);
			return Castle.Windsor.Installer.Configuration.FromXml(file);
		}

		[Test]
		public void Id_is_not_required_for_component_if_type_is_specified()
		{
			Container.Install(FromFile("componentWithoutId.xml"));
			Kernel.Resolve<A>();
		}
	}
}
#endif