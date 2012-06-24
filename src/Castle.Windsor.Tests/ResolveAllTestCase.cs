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

namespace CastleTests
{
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ResolveAllTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_resolve_more_than_single_component_for_service()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());
			var clocks = Container.ResolveAll<IEmptyService>();
			Assert.AreEqual(2, clocks.Length);
		}

		[Test]
		public void Can_use_mutliResolve_with_generic_Specialization()
		{
			Container.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(DemoRepository<>)),
			                   Component.For(typeof(IRepository<>)).ImplementedBy(typeof(TransientRepository<>)));

			Container.Resolve<IRepository<IEmptyService>>();
			var repositories = Container.ResolveAll<IRepository<EmptyServiceA>>();

			Assert.AreEqual(2, repositories.Length);
		}

		[Test]
		public void Exception_on_generic_constraint_violation_of_dependency_is_propagated_not_ignored()
		{
			Container.Register(
				Component.For(typeof(ICache<>)).ImplementedBy(typeof(CacheWithClassConstraint<>)),
				Component.For(typeof(IRepository<>)).ImplementedBy(typeof(CachingRepository<>)));

			var exception = Assert.Throws<HandlerException>(() => Container.ResolveAll<IRepository<int>>());

			var expectedMessage =
				string.Format(
					"Generic component {0} has some generic dependencies which were not successfully closed. This often happens when generic implementation has some additional generic constraints. See inner exception for more details.",
					typeof(CachingRepository<>).FullName);

			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		public void ResolveAll_honors_order_and_kinf_of_registration()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().IsFallback(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceC>().IsDefault());

			var clocks = Container.ResolveAll<IEmptyService>();

			Assert.IsInstanceOf<EmptyServiceC>(clocks[0]);
			Assert.IsInstanceOf<EmptyServiceA>(clocks[1]);
			Assert.IsInstanceOf<EmptyServiceB>(clocks[2]);

			//reversing order
			ResetContainer();
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>().IsFallback(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().IsDefault(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceC>().IsDefault());

			clocks = Container.ResolveAll<IEmptyService>();

			Assert.IsInstanceOf<EmptyServiceC>(clocks[0]);
			Assert.IsInstanceOf<EmptyServiceB>(clocks[1]);
			Assert.IsInstanceOf<EmptyServiceA>(clocks[2]);
		}

		[Test]
		public void ResolveAll_honors_order_of_registration()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());

			var clocks = Container.ResolveAll<IEmptyService>();

			Assert.IsInstanceOf<EmptyServiceA>(clocks[0]);
			Assert.IsInstanceOf<EmptyServiceB>(clocks[1]);

			//reversing order
			ResetContainer();
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());

			clocks = Container.ResolveAll<IEmptyService>();

			Assert.IsInstanceOf<EmptyServiceB>(clocks[0]);
			Assert.IsInstanceOf<EmptyServiceA>(clocks[1]);
		}
	}
}