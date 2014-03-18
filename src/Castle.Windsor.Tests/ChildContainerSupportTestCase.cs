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
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ChildContainerSupportTestCase : AbstractContainerTestCase
	{
		[Test]
		[Bug("IOC-127")]
		public void AddComponentInstanceAndChildContainers()
		{
			var child = new WindsorContainer();
			Container.AddChildContainer(child);

			var clock1 = new EmptyServiceA();
			var clock2 = new EmptyServiceB();

			Container.Register(Component.For<IEmptyService>().Instance(clock2));
			child.Register(Component.For<IEmptyService>().Instance(clock1));

			Assert.AreSame(clock2, Container.Resolve<IEmptyService>());
			Assert.AreSame(clock1, child.Resolve<IEmptyService>());
		}

		[Test]
		public void AddAndRemoveChildContainer()
		{
			IWindsorContainer childcontainer = new WindsorContainer();
			Container.AddChildContainer(childcontainer);
			Assert.AreEqual(Container, childcontainer.Parent);

			Container.RemoveChildContainer(childcontainer);
			Assert.IsNull(childcontainer.Parent);

			Container.AddChildContainer(childcontainer);
			Assert.AreEqual(Container, childcontainer.Parent);
		}

		[Test]
		public void AddAndRemoveChildContainerWithProperty()
		{
			IWindsorContainer childcontainer = new WindsorContainer();
			childcontainer.Parent = Container;
			Assert.AreEqual(Container, childcontainer.Parent);

			childcontainer.Parent = null;
			Assert.IsNull(childcontainer.Parent);

			childcontainer.Parent = Container;
			Assert.AreEqual(Container, childcontainer.Parent);
		}

		[Test]
		[ExpectedException(typeof(KernelException))]
		public void AddingToTwoParentContainsThrowsKernelException()
		{
			IWindsorContainer container3 = new WindsorContainer();
			IWindsorContainer childcontainer = new WindsorContainer();
			Container.AddChildContainer(childcontainer);
			container3.AddChildContainer(childcontainer);
		}

		[Test]
		[ExpectedException(typeof(KernelException))]
		public void AddingToTwoParentWithPropertyContainsThrowsKernelException()
		{
			IWindsorContainer container3 = new WindsorContainer();
			IWindsorContainer childcontainer = new WindsorContainer();
			childcontainer.Parent = Container;
			childcontainer.Parent = container3;
		}

		protected override void AfterContainerCreated()
		{
			Container.Register(Component.For(typeof(A)).Named("A"));
		}

		[Test]
		public void ResolveAgainstParentContainer()
		{
			IWindsorContainer childcontainer = new WindsorContainer();
			Container.AddChildContainer(childcontainer);

			Assert.AreEqual(Container, childcontainer.Parent);

			childcontainer.Register(Component.For(typeof(B)).Named("B"));
			var b = childcontainer.Resolve<B>("B");
			Assert.IsNotNull(b);
		}

		[Test]
		public void ResolveAgainstParentContainerWithProperty()
		{
			IWindsorContainer childcontainer = new WindsorContainer { Parent = Container };

			Assert.AreEqual(Container, childcontainer.Parent);

			childcontainer.Register(Component.For(typeof(B)).Named("B"));
			var b = childcontainer.Resolve<B>("B");

			Assert.IsNotNull(b);
		}

#if !SILVERLIGHT
		[Test]
		public void StartWithParentContainer()
		{
			IWindsorContainer childcontainer = new WindsorContainer(Container, new XmlInterpreter());

			Assert.AreEqual(Container, childcontainer.Parent);

			childcontainer.Register(Component.For(typeof(B)).Named("B"));
			var b = childcontainer.Resolve<B>("B");

			Assert.IsNotNull(b);
		}
#endif
	}
}