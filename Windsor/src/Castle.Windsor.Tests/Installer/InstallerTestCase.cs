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

namespace CastleTests.Installer
{
	using System;

	using Castle.Windsor;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.Windsor.Tests;
	using Castle.XmlFiles;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class InstallerTestCase : AbstractContainerTestCase
	{

		[Test]
		public void InstallCalcService()
		{
			var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("installerconfig.xml")));

			Assert.IsTrue(container.Kernel.HasComponent(typeof(ICalcService)));
			Assert.IsTrue(container.Kernel.HasComponent("calcservice"));
		}

		[Test]
		public void InstallChildContainer()
		{
			var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("installerconfig.xml")));
			var child1 = container.GetChildContainer("child1");

			Assert.IsNotNull(child1);
			Assert.AreEqual(child1.Parent, container);
			Assert.IsTrue(child1.Kernel.HasComponent(typeof(ICalcService)));
			Assert.IsTrue(child1.Kernel.HasComponent("child_calcservice"));

			var calcservice = container.Resolve<ICalcService>("calcservice");
			var child_calcservice = child1.Resolve<ICalcService>();
			Assert.AreNotEqual(calcservice, child_calcservice);
		}
	}
}

#endif