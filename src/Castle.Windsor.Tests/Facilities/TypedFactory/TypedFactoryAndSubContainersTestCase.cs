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

namespace Castle.Windsor.Tests.Facilities.TypedFactory.Components
{
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

	using CastleTests;
	using CastleTests.Components;
	using CastleTests.Facilities.TypedFactory.Factories;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryAndSubContainersTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Facility_When_added_to_a_child_container_wher_parent_has_facility_pulls_from_child()
		{
			var childContainer = new WindsorContainer();

			// NOTE: this has to happen in this order
			Container.AddChildContainer(childContainer);
			Container.AddFacility<TypedFactoryFacility>();
			childContainer.AddFacility<TypedFactoryFacility>();

			Container.Register(Component.For<IDummyComponent>().ImplementedBy<Component1>());
			childContainer.Register(Component.For<IDummyComponentFactory>().AsFactory(),
			                        Component.For<IDummyComponent>().ImplementedBy<Component2>());

			var fromParent = Container.Resolve<IDummyComponent>();
			var fromFactory = childContainer.Resolve<IDummyComponentFactory>().CreateDummyComponent();
			var fromChild = childContainer.Resolve<IDummyComponent>();

			Assert.AreSame(fromFactory, fromChild);
			Assert.AreNotSame(fromChild, fromParent);
			Assert.AreNotSame(fromFactory, fromParent);
		}

		[Test]
		[Bug("IOC-345")]
		public void Resolve_SingletonAndDisposeChildContainer_ShouldNotDisposeSingleton()
		{
			Container.AddFacility<TypedFactoryFacility>();
			Container.Register(Component.For<IGenericFactory<A>>().AsFactory(),
			                   Component.For<A>());

			// uncomment the line below and the test will not fail
			//container.Resolve<ISomeFactory>();

			var childContainer = new WindsorContainer();
			Container.AddChildContainer(childContainer);

			var factory = childContainer.Resolve<IGenericFactory<A>>();
			Container.RemoveChildContainer(childContainer);
			childContainer.Dispose();

			factory = Container.Resolve<IGenericFactory<A>>();


			factory.Create();
		}
	}
}