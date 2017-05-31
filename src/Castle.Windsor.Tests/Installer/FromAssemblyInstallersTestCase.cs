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

namespace CastleTests.Installer
{
	using System;
	using System.IO;
	using System.Reflection;

	using Castle.Core.Internal;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Installer;

	using NUnit.Framework;

	[TestFixture]
	public class FromAssemblyInstallersTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_install_from_assembly_by_assembly()
		{
			Container.Install(FromAssembly.Instance(Assembly.GetExecutingAssembly()));
			Container.Resolve<object>("Customer-by-CustomerInstaller");
		}

#if !SILVERLIGHT
		[Test]
		public void Can_install_from_assembly_by_directory_simple()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location)));
			Container.Resolve<object>("Customer-by-CustomerInstaller");
		}

		[Test]
		public void Can_install_from_assembly_by_name()
		{
			Container.Install(FromAssembly.Named("Castle.Windsor.Tests"));
		}
#endif

		[Test]
		public void Can_install_from_assembly_by_type()
		{
			Container.Install(FromAssembly.Containing(GetType()));
		}

		[Test]
		public void Can_install_from_assembly_by_application()
		{
			Container.Install(FromAssembly.InThisApplication(GetCurrentAssembly(), new FilterAssembliesInstallerFactory(t => t.Assembly != typeof(IWindsorInstaller).Assembly)));
		}

		[Test]
		public void Can_install_from_assembly_by_type_generic()
		{
			Container.Install(FromAssembly.Containing<FromAssemblyInstallersTestCase>());
		}

		[Test]
		public void Can_install_from_calling_assembly1()
		{
			Container.Install(FromAssembly.Instance(GetCurrentAssembly()));
		}

#if FEATURE_GETCALLINGASSEMBLY
		[Test]
		public void Can_install_from_calling_assembly2()
		{
			Container.Install(FromAssembly.This());
		}
#endif

#if !SILVERLIGHT

		[Test]
		public void Install_from_assembly_by_directory_ignores_non_existing_path()
		{
			var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString("N"));

			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location)));

			Assert.AreEqual(0, Container.Kernel.GraphNodes.Length);
		}

		[Test]
		public void Install_from_assembly_by_directory_executes_assembly_condition()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			var called = false;
			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByAssembly(a =>
			{
				called = true;
				return true;
			})));

			Assert.IsTrue(called);
			Assert.IsTrue(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_empty_name_searches_currentDirectory()
		{
			var called = false;
			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(string.Empty).FilterByAssembly(a =>
			{
				called = true;
				return true;
			})));

			Assert.IsTrue(called);
			Assert.IsTrue(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_executes_name_condition()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			var byNameCalled = false;
			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByName(a =>
			{
				byNameCalled = true;
				return true;
			})));

			Assert.IsTrue(byNameCalled);
			Assert.IsTrue(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_obeys_assembly_condition()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			var called = false;
			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByAssembly(a =>
			{
				called = true;
				return false;
			})));

			Assert.IsTrue(called);
			Assert.IsFalse(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_obeys_name_condition()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;
			var byNameCalled = false;
			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).FilterByName(a =>
			{
				byNameCalled = true;
				return false;
			})));

			Assert.IsTrue(byNameCalled);
			Assert.IsFalse(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}

		[Test]
		public void Install_from_assembly_by_directory_with_fake_key_as_string_does_not_install()
		{
			var location = AppDomain.CurrentDomain.BaseDirectory;

			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken("1234123412341234")));
			Assert.IsFalse(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
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
			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken(publicKeyToken)));
			Assert.IsTrue(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
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

			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken(GetType())));
			Assert.IsTrue(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
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

			Container.Install(FromAssembly.InDirectory(new AssemblyFilter(location).WithKeyToken<object>()));
			Assert.IsFalse(Container.Kernel.HasComponent("Customer-by-CustomerInstaller"));
		}
#endif
	}
}