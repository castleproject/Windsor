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

namespace Castle.Windsor.Tests.Facilities.Startable
{
	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Facilities.Startable.Components;

	using NUnit.Framework;

	[TestFixture]
	public class OptimizedForSingleInstallTestCase
	{
		[SetUp]
		public void SetUp()
		{
			windsorContainer = new WindsorContainer();
			windsorContainer.AddFacility<StartableFacility>(f => f.DeferredStart());
			Startable.Started = false;
		}

		private IWindsorContainer windsorContainer;

		[Test]
		public void Appearing_missing_dependencies_dont_cause_component_to_be_started_before_the_end_of_Install()
		{
			windsorContainer.Install(new ActionBasedInstaller(c => c.Register(Component.For<Startable>())),
			                         new ActionBasedInstaller(c =>
			                         {
			                         	c.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>());
			                         	Assert.IsFalse(Startable.Started);
			                         }));
			Assert.IsTrue(Startable.Started);
		}

		[Test]
		public void Facility_wont_try_to_start_anything_before_the_end_of_Install()
		{
			windsorContainer.Install(
				new ActionBasedInstaller(c => c.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>())),
				new ActionBasedInstaller(c =>
				{
					c.Register(Component.For<Startable>());
					Assert.IsFalse(Startable.Started);
				}));
			Assert.IsTrue(Startable.Started);
		}

		[Test]
		public void Missing_dependencies_after_the_end_of_Install_cause_exception()
		{
			Assert.Throws<HandlerException>(() =>
			windsorContainer.Install(
			    new ActionBasedInstaller(c => c.Register(Component.For<Startable>()))));
		}


		[Test]
		public void Missing_dependencies_after_the_end_of_Install_no_exception_when_tryStart_true()
		{
			var container = new WindsorContainer();
			container.AddFacility<StartableFacility>(f => f.DeferredTryStart());

			container.Install(new ActionBasedInstaller(c => c.Register(Component.For<Startable>())));

			Assert.IsFalse(Startable.Started);
		}

		[Test]
		public void Missing_dependencies_after_the_end_of_Install_starts_after_adding_missing_dependency_after_Install()
		{
			var container = new WindsorContainer();
			container.AddFacility<StartableFacility>(f => f.DeferredTryStart());

			container.Install(new ActionBasedInstaller(c => c.Register(Component.For<Startable>())));

			container.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>());
			Assert.IsTrue(Startable.Started);
		}
	}
}