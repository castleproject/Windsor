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

namespace CastleTests.Lifestyle
{
	using System;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Scoping;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	[Ignore("This althouth initially looked as a good idea quickly gets out of hand and we can't really support it.")]
	public class ScopedLifestyleAndTypedFactoriesTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Kernel.AddSubSystem("scope", new ScopingSubsystem(new ThreadScopeAccessor()));
			Container.AddFacility<TypedFactoryFacility>();
		}

		[Test]
		public void Can_obtain_scoped_component_via_factory()
		{
			Container.Register(Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient,
			                   Component.For<DisposableFoo>().LifeStyle.ScopedPer<UsesDisposableFooDelegate>());

			var instance = Container.Resolve<UsesDisposableFooDelegate>();

			instance.GetFoo();
		}

		[Test]
		public void Scoped_component_via_factory_and_outsideinstances_reused_properly()
		{
			Container.Register(Component.For<UsesFooAndDelegate>().LifeStyle.Transient,
			                   Component.For<Foo>().LifeStyle.ScopedPer<UsesFooAndDelegate>()
			                   	.DependsOn(Parameter.ForKey("number").Eq("1")));

			var instance = Container.Resolve<UsesFooAndDelegate>();

			var one = instance.Foo;
			var two = instance.GetFoo();

			Assert.AreSame(one, two);
		}

		[Test]
		public void Scoped_component_via_factory_instances_reused_properly()
		{
			Container.Register(Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient,
			                   Component.For<DisposableFoo>().LifeStyle.ScopedPer<UsesDisposableFooDelegate>());

			var instance = Container.Resolve<UsesDisposableFooDelegate>();

			var one = instance.GetFoo();
			var two = instance.GetFoo();

			Assert.AreSame(one, two);
		}

		[Test]
		public void Scoped_component_via_factory_reused_properly_across_factories()
		{
			Container.Register(Component.For<UsesTwoFooDelegates>().LifeStyle.Transient,
			                   Component.For<Foo>().LifeStyle.ScopedPer<UsesTwoFooDelegates>());

			var instance = Container.Resolve<UsesTwoFooDelegates>();

			Assert.AreNotSame(instance.One, instance.Two);

			var one = instance.GetFooOne();
			var two = instance.GetFooTwo();

			Assert.AreSame(one, two);
		}

		[Test]
		public void Scoped_component_via_factory_two_roots_but_factory_invoked_by_yet_another_object()
		{
			Container.Register(Component.For<UsesFooDelegate>().LifeStyle.Transient,
			                   Component.For<Foo>().LifeStyle.ScopedPer<UsesFooDelegate>(),
			                   Component.For<Func<Foo>>().LifeStyle.Singleton);

			var rootOne = Container.Resolve<UsesFooDelegate>();

			var rootTwo = Container.Resolve<UsesFooDelegate>();

			var fooOne = rootOne.GetFoo();
			var fooTwo = rootTwo.GetFoo();

			var oneFactory = rootOne.Factory;
			var anotherFoo = oneFactory(4);

			Assert.Inconclusive("Not even sure what to assert here... What a sane person might expect other than this not being supported");
		}

		[Test]
		public void Scoped_component_via_factory_two_roots_each_gets_its_own_instance()
		{
			Container.Register(Component.For<UsesFooDelegate>().LifeStyle.Transient,
			                   Component.For<Foo>().LifeStyle.ScopedPer<UsesFooDelegate>(),
			                   Component.For<Func<Foo>>().LifeStyle.Singleton);

			var rootOne = Container.Resolve<UsesFooDelegate>();

			var rootTwo = Container.Resolve<UsesFooDelegate>();

			Assert.AreSame(rootOne.GetFoo(), rootOne.GetFoo());
			Assert.AreNotSame(rootOne.GetFoo(), rootTwo.GetFoo());
		}
	}
}