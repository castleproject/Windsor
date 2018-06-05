﻿// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Facilities.Startable
{
	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Facilities.Startable.Components;

	using NUnit.Framework;

	public class ManuallyTriggeredStartTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_manually_trigger_start()
		{
			var flag = new StartFlag();
			Startable.Started = false;
			Container.AddFacility<StartableFacility>(f => f.DeferredStart(flag));
			Container.Register(Component.For<Startable>(),
				Component.For<ICustomer>().ImplementedBy<CustomerImpl>());

			Assert.IsFalse(Startable.Started);

			flag.Signal();

			Assert.IsTrue(Startable.Started);
		}

		[Test]
		public void Can_manually_trigger_start_only_once()
		{
			var flag = new StartFlag();
			Startable.Started = false;
			Container.AddFacility<StartableFacility>(f => f.DeferredStart(flag));
			Container.Register(Component.For<Startable>().LifestyleTransient(),
				Component.For<ICustomer>().ImplementedBy<CustomerImpl>());

			flag.Signal();
			Startable.Started = false;
			flag.Signal();
			Assert.IsFalse(Startable.Started);
		}

		[Test]
		public void Can_manually_trigger_start_when_using_Install()
		{
			var flag = new StartFlag();
			Startable.Started = false;
			Container.AddFacility<StartableFacility>(f => f.DeferredStart(flag));
			Container.Install(
				new ActionBasedInstaller(c => c.Register(Component.For<Startable>(),
					Component.For<ICustomer>().ImplementedBy<CustomerImpl>()))
				);

			Assert.IsFalse(Startable.Started);

			flag.Signal();

			Assert.IsTrue(Startable.Started);
		}

		[Test]
		public void Manually_triggered_start_throws_on_missing_dependencies()
		{
			var flag = new StartFlag();
			Startable.Started = false;
			Container.AddFacility<StartableFacility>(f => f.DeferredStart(flag));
			Container.Register(Component.For<Startable>());

			Assert.Throws<HandlerException>(() =>
				flag.Signal()
				);
		}
	}
}