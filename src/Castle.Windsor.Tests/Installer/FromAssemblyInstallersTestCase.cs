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

namespace Castle.Windsor.Tests.Installer
{
	using System.Reflection;

	using Castle.Windsor.Installer;

	using NUnit.Framework;

	[TestFixture]
	public class FromAssemblyInstallersTestCase
	{
		public IWindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
		}

		[Test]
		public void Can_install_from_calling_assembly()
		{
			container.Install(FromAssembly.This());

			container.Resolve("Customer-by-CustomerInstaller");
		}

		[Test]
		public void Can_install_from_assembly_by_name()
		{
			container.Install(FromAssembly.Named("Castle.Windsor.Tests"));

			container.Resolve("Customer-by-CustomerInstaller");
		}

		[Test]
		public void Can_install_from_assembly_by_type()
		{
			container.Install(FromAssembly.Containing(GetType()));

			container.Resolve("Customer-by-CustomerInstaller");
		}

		[Test]
		public void Can_install_from_assembly_by_type_generic()
		{
			container.Install(FromAssembly.Containing<FromAssemblyInstallersTestCase>());

			container.Resolve("Customer-by-CustomerInstaller");
		}

		[Test]
		public void Can_install_from_assembly_by_assembly()
		{
			container.Install(FromAssembly.Given(Assembly.GetExecutingAssembly()));

			container.Resolve("Customer-by-CustomerInstaller");
		}
	}
}