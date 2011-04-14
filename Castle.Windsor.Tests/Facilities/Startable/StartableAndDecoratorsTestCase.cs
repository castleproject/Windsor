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

namespace Castle.Windsor.Tests.Facilities.Startable
{
	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class StartableAndDecoratorsTestCase
	{
		private class AllInstaller : IWindsorInstaller
		{
			public void Install(IWindsorContainer container,
			                    IConfigurationStore store)
			{
				container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
				                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());
				container.Register(Component.For<UsesIEmptyService>().Start());
			}
		}

		private class DependenciesInstaller : IWindsorInstaller
		{
			public void Install(IWindsorContainer container,
			                    IConfigurationStore store)
			{
				container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
				                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());
			}
		}

		private class StartableInstaller : IWindsorInstaller
		{
			public void Install(IWindsorContainer container,
			                    IConfigurationStore store)
			{
				container.Register(Component.For<UsesIEmptyService>().Start());
			}
		}

		[Test]
		public void No_startable_explicit_Resolve_resolves_with_no_issues()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());
				container.Register(
					Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
					Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
					Component.For<UsesIEmptyService>()
					);
				container.Resolve<UsesIEmptyService>();
			}
		}

		[Test]
		public void Startable_and_components_in_separate_Install_Resolve_Startable_last_works()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());
				container.Install(new DependenciesInstaller());
				container.Register(Component.For<UsesIEmptyService>().Start());
			}
		}

		[Test]
		public void Startable_and_components_in_separate_Install_Startable_first_throws()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());

				Assert.Throws<HandlerException>(() =>
				{
					container.Install(new StartableInstaller());
					container.Install(new DependenciesInstaller());
				});
			}
		}

		[Test]
		public void Startable_and_components_in_separate_Install_Startable_last_works()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());
				container.Install(new DependenciesInstaller());
				container.Install(new StartableInstaller());
			}
		}

		[Test]
		public void Startable_and_components_in_single_Install_works()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());
				container.Install(new DependenciesInstaller(),
				                  new StartableInstaller());
			}
		}

		[Test]
		public void Startable_and_components_in_single_Installer_works()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());
				container.Install(new AllInstaller());
			}
		}

		[Test]
		public void Startable_and_components_in_single_Register_works()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());
				container.Register(
					Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
					Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
					Component.For<UsesIEmptyService>().Start()
					);
			}
		}

		[Test]
		public void Startable_and_components_separate_Register_Startable_first_throws()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());

				Assert.Throws<HandlerException>(() =>
				{
					container.Register(Component.For<UsesIEmptyService>().Start());
					container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
					                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());
				});
			}
		}

		[Test]
		public void Startable_and_components_separate_Register_Startable_last_works()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());
				container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
				                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());
				container.Register(Component.For<UsesIEmptyService>().Start());
			}
		}
	}
}