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

namespace CastleTests
{
	using System;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;
	using Castle.MicroKernel.Tests;

	using CastleTests.Components;

	using NUnit.Framework;

	public class DependencyCyclesTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_detect_and_report_cycle_via_factory_method()
		{
			Container.Register(
				Component.For<A>().UsingFactoryMethod(k =>
				{
					var thisCreatesCycle = k.Resolve<C>();
					Assert.NotNull(thisCreatesCycle, "just so that the variable is not optimized away");
					return new A();
				}),
				Component.For<B>(),
				Component.For<C>());

			var exception = Assert.Throws<CircularDependencyException>(() => Container.Resolve<C>());
			var message =
				string.Format(
					"Dependency cycle has been detected when trying to resolve component 'CastleTests.Components.C'.{0}The resolution tree that resulted in the cycle is the following:{0}Component 'CastleTests.Components.C' resolved as dependency of{0}\tcomponent 'Late bound CastleTests.Components.A' resolved as dependency of{0}\tcomponent 'CastleTests.Components.B' resolved as dependency of{0}\tcomponent 'CastleTests.Components.C' which is the root component being resolved.{0}",
					Environment.NewLine);

			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Can_detect_and_report_cycle_where_container_has_lazy_loaders()
		{
			Container.Register(
				Component.For<ILazyComponentLoader>().ImplementedBy<ABLoader>(),
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>());

			var exception = Assert.Throws<CircularDependencyException>(() => Container.Resolve<IEmptyService>());
			var message =
				string.Format(
					"Dependency cycle has been detected when trying to resolve component 'CastleTests.Components.EmptyServiceDecorator'.{0}The resolution tree that resulted in the cycle is the following:{0}Component 'CastleTests.Components.EmptyServiceDecorator' resolved as dependency of{0}	component 'CastleTests.Components.EmptyServiceDecorator' which is the root component being resolved.{0}",
					Environment.NewLine);

			Assert.AreEqual(message, exception.Message);
		}
	}
}