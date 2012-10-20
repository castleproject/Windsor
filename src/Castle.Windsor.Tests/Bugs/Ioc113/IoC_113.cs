// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Bugs.Ioc113
{
	using System.Collections.Generic;

	using Castle.Facilities.Startable;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.Bugs.Ioc113;

	using NUnit.Framework;

	[TestFixture]
	public class IoC_113_When_resolving_initializable_disposable_and_startable_component
	{
		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();

			kernel.AddFacility<StartableFacility>();

			kernel.Register(
				Component.For<StartableDisposableAndInitializableComponent>()
					.LifeStyle.Transient
				);

			component = kernel.Resolve<StartableDisposableAndInitializableComponent>();
			component.DoSomething();
			kernel.ReleaseComponent(component);

			calledMethods = component.calledMethods;
		}

		private IKernel kernel;
		private StartableDisposableAndInitializableComponent component;
		private IList<SdiComponentMethods> calledMethods;

		[Test]
		public void Should_call_DoSomething_between_start_and_stop()
		{
			Assert.AreEqual(SdiComponentMethods.DoSomething, calledMethods[2]);
		}

		[Test]
		public void Should_call_all_methods_once()
		{
			Assert.AreEqual(5, component.calledMethods.Count);
		}

		[Test]
		public void Should_call_initialize_before_start()
		{
			Assert.AreEqual(SdiComponentMethods.Initialize, calledMethods[0]);
			Assert.AreEqual(SdiComponentMethods.Start, calledMethods[1]);
		}

		[Test]
		public void Should_call_stop_before_dispose()
		{
			Assert.AreEqual(SdiComponentMethods.Stop, calledMethods[3]);
			Assert.AreEqual(SdiComponentMethods.Dispose, calledMethods[4]);
		}
	}
}