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
	using System;
	using System.Reflection;

	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Installer;

	using NUnit.Framework;

	[TestFixture]
	public class FromAssemblyInstallersTestCase
	{
		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
		}

		private IWindsorContainer container;

		[Test]
		public void Can_install_from_assembly_by_assembly()
		{
			container.Install(FromAssembly.Instance(Assembly.GetExecutingAssembly()));
			container.Resolve("Customer-by-CustomerInstaller");
		}

		[Test]
		public void Can_install_from_assembly_by_directory_simple()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			container.Install(FromAssembly.InDirectory(new AssemblyFilter(location)));
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
		public void Can_install_from_calling_assembly()
		{
			container.Install(FromAssembly.This());

			container.Resolve("Customer-by-CustomerInstaller");
		}

		[Test]
		public void Install_from_assembly_by_directory_executes_assembly_condition()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			var called = false;
			container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByAssembly(a =>
			{
				called = true;
				return true;
			})));

			Assert.IsTrue(called);
			Assert.IsTrue(container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_executes_name_condition()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			var byNameCalled = false;
			container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByName(a =>
			{
				byNameCalled = true;
				return true;
			})));

			Assert.IsTrue(byNameCalled);
			Assert.IsTrue(container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_obeys_assembly_condition()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			var called = false;
			container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByAssembly(a =>
			{
				called = true;
				return false;
			})));

			Assert.IsTrue(called);
			Assert.IsFalse(container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_obeys_name_condition()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			var byNameCalled = false;
			container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByName(a =>
			{
				byNameCalled = true;
				return false;
			})));

			Assert.IsTrue(byNameCalled);
			Assert.IsFalse(container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_with_fake_key_as_string_does_not_install()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;

			container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken("1234123412341234")));
			Assert.IsFalse(container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_with_key_as_string_installs()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;

			var fullName = GetType().Assembly.FullName;
			var index = fullName.IndexOf("PublicKeyToken=");
			if (index == -1)
			{
				Assert.Ignore("Assembly is not signed so no way to test this.");
			}
			var publicKeyToken = fullName.Substring(index + "PublicKeyToken=".Length, 16);
			container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken(publicKeyToken)));
			Assert.IsTrue(container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_with_key_installs()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;

			var publicKeyToken = GetType().Assembly.GetName().GetPublicKeyToken();
			if (publicKeyToken == null || publicKeyToken.Length == 0)
			{
				Assert.Ignore("Assembly is not signed so no way to test this.");
			}

			container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken(GetType())));
			Assert.IsTrue(container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_with_mscorlib_key_does_not_install()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;

			var publicKeyToken = GetType().Assembly.GetName().GetPublicKeyToken();
			if (publicKeyToken == null || publicKeyToken.Length == 0)
			{
				Assert.Ignore("Assembly is not signed so no way to test this.");
			}

			container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken<object>()));
			Assert.IsFalse(container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}
	}
}