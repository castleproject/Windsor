// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests.Installer
{
	using System;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests.Components;
	using NUnit.Framework;

	[TestFixture]
	public class ConfigurationInstallerTestCase
	{
		private IWindsorContainer container;

		[SetUp]
		public void Init()
		{
			container = new WindsorContainer();
		}

		[Test]
		public void InstallComponents_FromAppConfig_ComponentsInstalled()
		{
			container.Install(Configuration.FromAppConfig());

			Assert.IsTrue(container.Kernel.HasComponent(typeof(ICalcService)));
			Assert.IsTrue(container.Kernel.HasComponent("calcservice"));
		}

		[Test]
		public void InstallComponents_FromXmlFile_ComponentsInstalled()
		{
			container.Install(
				Configuration.FromXmlFile(
					ConfigHelper.ResolveConfigPath("installerconfig.xml")));

			Assert.IsTrue(container.Kernel.HasComponent(typeof(ICalcService)));
			Assert.IsTrue(container.Kernel.HasComponent("calcservice"));
		}

		[Test]
		public void InstallComponents_FromXmlFile_first_and_from_code()
		{
			container.Install(
				Configuration.FromXmlFile(ConfigHelper.ResolveConfigPath("JustConfiguration.xml")),
				new Installer(c => c.Register(Component.For<ICamera>()
				                              	.ImplementedBy<Camera>()
				                              	.Named("camera"))));

			var camera = container.Resolve<ICamera>();
			Assert.AreEqual("from configuration", camera.Name);
		}

		[Test,Ignore("This does not work. Would be cool if it did, but we need deeper restructuring first.")]
		public void InstallComponents_from_code_first_and_FromXmlFile()
		{
			container.Install(
				new Installer(c => c.Register(Component.For<ICamera>()
				                              	.ImplementedBy<Camera>()
				                              	.Named("camera"))),
				Configuration.FromXmlFile(ConfigHelper.ResolveConfigPath("JustConfiguration.xml"))
				);

			var camera = container.Resolve<ICamera>();
			Assert.AreEqual("from configuration", camera.Name);
		}

		[Test]
		public void InstallComponents_FromMultiple_ComponentsInstalled()
		{
			container.Install(
				Configuration.FromAppConfig(),
				Configuration.FromXmlFile(
					ConfigHelper.ResolveConfigPath("ignoreprop.xml")),
				Configuration.FromXmlFile(
					ConfigHelper.ResolveConfigPath("robotwireconfig.xml"))
				);

			Assert.IsTrue(container.Kernel.HasComponent(typeof(ICalcService)));
			Assert.IsTrue(container.Kernel.HasComponent("calcservice"));
			Assert.IsTrue(container.Kernel.HasComponent(typeof(ClassWithDoNotWireProperties)));
			Assert.IsTrue(container.Kernel.HasComponent("server"));
			Assert.IsTrue(container.Kernel.HasComponent(typeof(Robot)));
			Assert.IsTrue(container.Kernel.HasComponent("robot"));
		}

		[Test]
		public void InstallComponents_FromXmlFileWithEnvironment_ComponentsInstalled()
		{
			container.Install(
				Configuration.FromXmlFile(
					ConfigHelper.ResolveConfigPath("Configuration2/env_config.xml"))
					.Environment("devel")
					);

			var prop = container.Resolve<ComponentWithStringProperty>("component");

			Assert.AreEqual("John Doe", prop.Name);
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
