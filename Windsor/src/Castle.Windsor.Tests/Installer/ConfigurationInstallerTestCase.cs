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

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Windsor;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests;
	using Castle.XmlFiles;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ConfigurationInstallerTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_reference_components_from_app_config_in_component_node()
		{
			Container.Install(Configuration.FromAppConfig());

			var item = Container.Resolve<ClassWithArguments>();
			Assert.AreEqual("a string", item.Arg1);
			Assert.AreEqual(42, item.Arg2);
		}

		[Test]
		public void InstallComponents_FromAppConfig_ComponentsInstalled()
		{
			Container.Install(Configuration.FromAppConfig());

			Assert.IsTrue(Container.Kernel.HasComponent(typeof(ICalcService)));
			Assert.IsTrue(Container.Kernel.HasComponent("calcservice"));
		}

		[Test]
		public void InstallComponents_FromMultiple_ComponentsInstalled()
		{
			Container.Install(
				Configuration.FromAppConfig(),
				Configuration.FromXml(Xml.Embedded("ignoreprop.xml")),
				Configuration.FromXml(Xml.Embedded("robotwireconfig.xml"))
				);

			Assert.IsTrue(Container.Kernel.HasComponent(typeof(ICalcService)));
			Assert.IsTrue(Container.Kernel.HasComponent("calcservice"));
			Assert.IsTrue(Container.Kernel.HasComponent(typeof(ClassWithDoNotWireProperties)));
			Assert.IsTrue(Container.Kernel.HasComponent("server"));
			Assert.IsTrue(Container.Kernel.HasComponent(typeof(Robot)));
			Assert.IsTrue(Container.Kernel.HasComponent("robot"));
		}

		[Test]
		public void InstallComponents_FromXmlFileWithEnvironment_ComponentsInstalled()
		{
			Container.Install(
				Configuration.FromXmlFile(
					ConfigHelper.ResolveConfigPath("Configuration2/env_config.xml"))
					.Environment("devel")
				);

			var prop = Container.Resolve<ComponentWithStringProperty>("component");

			Assert.AreEqual("John Doe", prop.Name);
		}

		[Test]
		public void InstallComponents_FromXmlFile_ComponentsInstalled()
		{
			Container.Install(
				Configuration.FromXml(Xml.Embedded("installerconfig.xml")));

			Assert.IsTrue(Container.Kernel.HasComponent(typeof(ICalcService)));
			Assert.IsTrue(Container.Kernel.HasComponent("calcservice"));
		}

		[Test]
		public void InstallComponents_FromXmlFile_first_and_from_code()
		{
			Container.Install(
				Configuration.FromXml(Xml.Embedded("justConfiguration.xml")),
				new Installer(c => c.Register(Component.For<ICamera>()
				                              	.ImplementedBy<Camera>()
				                              	.Named("camera"))));

			var camera = Container.Resolve<ICamera>();
			Assert.AreEqual("from configuration", camera.Name);
		}

		[Test]
		[Ignore("This does not work. Would be cool if it did, but we need deeper restructuring first.")]
		public void InstallComponents_from_code_first_and_FromXmlFile()
		{
			Container.Install(
				new Installer(c => c.Register(Component.For<ICamera>()
				                              	.ImplementedBy<Camera>()
				                              	.Named("camera"))),
				Configuration.FromXml(Xml.Embedded("justConfiguration.xml"))
				);

			var camera = Container.Resolve<ICamera>();
			Assert.AreEqual("from configuration", camera.Name);
		}
	}

	internal class Installer : IWindsorInstaller
	{
		private readonly Action<IWindsorContainer> install;

		public Installer(Action<IWindsorContainer> install)
		{
			this.install = install;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			install(container);
		}
	}
}

#endif