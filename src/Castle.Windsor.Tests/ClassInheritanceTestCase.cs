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

namespace Castle
{
	using System;
	using System.Linq;

	using Castle.Components;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class ClassInheritanceTestCase : AbstractContainerTestFixture
	{

		// TODO: add tests for generics in the hierarchy (open as well?)
		// TODO: add tests for proxying to make sure we can always cast down
		[Test]
		public void GrandParent_and_Parent_of_impl_can_be_the_service()
		{
			Container.Register(Component.For<JohnGrandparent, JohnParent>().ImplementedBy<JohnChild>());

			var grandparent = Container.Resolve<JohnGrandparent>();
			var parent = Container.Resolve<JohnParent>();
			Assert.AreSame(grandparent, parent);
			Assert.IsInstanceOf<JohnChild>(grandparent);
		}

		[Test]
		public void GrandParent_of_impl_can_be_the_service()
		{
			Container.Register(Component.For<JohnGrandparent>().ImplementedBy<JohnChild>());

			var grandparent = Container.Resolve<JohnGrandparent>();
			Assert.IsInstanceOf<JohnChild>(grandparent);
		}

		[Test(Description = "With some conversion operator or black magic this may actually work for someone... so let them try")]
		public void Not_related_service_and_impl_fail_on_resolve()
		{
			Container.Register(Component.For<A>().ImplementedBy(typeof(A2)));
			var handler = Kernel.GetHandler(typeof(A));
			Assert.AreEqual(typeof(A), handler.Services.Single());
			Assert.AreEqual(typeof(A2), handler.ComponentModel.Implementation);

			// sure, why not - let them do uncompatible types. Who knows - perhaps by some miracul

			Assert.Throws<InvalidCastException>(() => Container.Resolve<A>());
		}

		[Test]
		public void Parent_and_GrandParent_of_impl_can_be_the_service()
		{
			Container.Register(Component.For<JohnParent, JohnGrandparent>().ImplementedBy<JohnChild>());

			var grandparent = Container.Resolve<JohnGrandparent>();
			var parent = Container.Resolve<JohnParent>();
			Assert.AreSame(grandparent, parent);
			Assert.IsInstanceOf<JohnChild>(grandparent);
		}

		[Test]
		public void Parent_of_impl_can_be_the_service()
		{
			Container.Register(Component.For<JohnParent>().ImplementedBy<JohnChild>());

			var parent = Container.Resolve<JohnParent>();
			Assert.IsInstanceOf<JohnChild>(parent);
		}

		[Test]
		public void Same_class_can_be_used_as_service_and_impl_explicitly()
		{
			Container.Register(Component.For<A>().ImplementedBy<A>());
			var handler = Kernel.GetHandler(typeof(A));
			Assert.AreEqual(typeof(A), handler.Services.Single());
			Assert.AreEqual(typeof(A), handler.ComponentModel.Implementation);
		}

		[Test]
		public void Same_class_can_be_used_as_service_and_impl_implicitly()
		{
			Container.Register(Component.For<A>());
			var handler = Kernel.GetHandler(typeof(A));
			Assert.AreEqual(typeof(A), handler.Services.Single());
			Assert.AreEqual(typeof(A), handler.ComponentModel.Implementation);
		}
	}
}