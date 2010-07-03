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

namespace Castle.Windsor.Tests.Facilities.TypedFactory
{
	using System;

	using Castle.Core;
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Releasers;
	using Castle.Windsor;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Selectors;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryDelegatesTestCase
	{
		[SetUp]
		public void SetUpTests()
		{
			container = new WindsorContainer();
			container.AddFacility<TypedFactoryFacility>();
		}

		private WindsorContainer container;

		[Test]
		public void Affects_constructor_resolution()
		{
			container.Register(Component.For<Baz>().Named("baz"));
			container.Register(Component.For<HasTwoConstructors>().Named("fizz"));
			var factory = container.Resolve<Func<string, HasTwoConstructors>>();

			var obj = factory("naaaameee");
			Assert.AreEqual("naaaameee", obj.Name);
		}

		[Test]
		public void Can_resolve_multiple_delegates_just_fine()
		{
			container.Register(Component.For<Baz>());
			container.Register(Component.For<A>());

			var bazFactory = container.Resolve<Func<Baz>>();
			var aFactory = container.Resolve<Func<A>>();

			bazFactory.Invoke();
			aFactory.Invoke();
		}

		[Test]
		public void Can_resolve_service_via_delegate()
		{
			container.Register(Component.For<Foo>().Named("MyFoo").LifeStyle.Transient);
			container.Register(Component.For<UsesFooDelegate>());
			var dependsOnFoo = container.Resolve<UsesFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(2, foo.Number);
		}

		[Test]
		public void Delegate_factory_obeys_release_policy_non_tracking()
		{
			container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
			container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = container.Resolve<UsesDisposableFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.IsFalse(container.Kernel.ReleasePolicy.HasTrack(foo));
		}

		[Test]
		public void Delegate_factory_obeys_release_policy_tracking()
		{
			container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = container.Resolve<UsesDisposableFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.IsTrue(container.Kernel.ReleasePolicy.HasTrack(foo));
		}

		[Test]
		public void Delegate_obeys_lifestyle()
		{
			container.Register(Component.For<Foo>().Named("MyFoo").LifeStyle.Is(LifestyleType.Singleton));
			container.Register(Component.For<UsesFooDelegate>());
			var dependsOnFoo = container.Resolve<UsesFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
		}

		[Test]
		public void Delegate_based_factories_explicitly_pick_registered_selector()
		{
			DisposableSelector.InstancesCreated = 0;
			SimpleSelector.InstancesCreated = 0;
			container.Register(
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<DisposableSelector>().Named("1").LifeStyle.Transient,
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<SimpleSelector>().Named("2").LifeStyle.Transient,
				Component.For<Func<Foo>>().LifeStyle.Transient.AsFactory(x => x.SelectedWith("2")));

			container.Resolve<Func<Foo>>();

			Assert.AreEqual(0, DisposableSelector.InstancesCreated);
			Assert.AreEqual(1, SimpleSelector.InstancesCreated);
		}

		[Test]
		public void Delegate_based_factories_explicitly_pick_registered_selector_implicitly_registered_factory()
		{
			DisposableSelector.InstancesCreated = 0;
			DisposableSelector.InstancesDisposed = 0;
			container.Register(
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<DisposableSelector>().LifeStyle.Transient);

			container.Resolve<Func<Foo>>();

			Assert.AreEqual(1, DisposableSelector.InstancesCreated);
		}

		[Test]
		public void Delegate_based_factories_implicitly_pick_registered_selector_explicitly_registered_factory()
		{
			DisposableSelector.InstancesCreated = 0;
			DisposableSelector.InstancesDisposed = 0;
			container.Register(
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<DisposableSelector>().LifeStyle.Transient,
				Component.For<Func<Foo>>().LifeStyle.Transient.AsFactory());

			container.Resolve<Func<Foo>>();

			Assert.AreEqual(1, DisposableSelector.InstancesCreated);
		}

		[Test]
		public void Releasing_factory_releases_selector()
		{
			DisposableSelector.InstancesCreated = 0;
			DisposableSelector.InstancesDisposed = 0;
			container.Register(
				Component.For<DisposableSelector>().LifeStyle.Transient,
				Component.For<Func<Foo>>().LifeStyle.Transient.AsFactory(x => x.SelectedWith<DisposableSelector>()));
			var factory = container.Resolve<Func<Foo>>();

			Assert.AreEqual(1, DisposableSelector.InstancesCreated);

			container.Release(factory);

			Assert.AreEqual(1, DisposableSelector.InstancesDisposed);
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Delegate_parameters_are_used_in_order_first_ctor_then_properties()
		{
			container.Register(Component.For(typeof(Baz)).Named("baz"));
			container.Register(Component.For(typeof(Bar)).Named("bar"));
			container.Register(Component.For(typeof(UsesBarDelegate)).Named("barBar"));

			var dependsOnFoo = container.Resolve<UsesBarDelegate>();
			var bar = dependsOnFoo.GetBar("a name", "a description");
			Assert.AreEqual("a name", bar.Name);
			Assert.AreEqual("a description", bar.Description);
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Delegate_pulls_unspecified_dependencies_from_container()
		{
			container.Register(Component.For(typeof(Baz)).Named("baz"));
			container.Register(Component.For(typeof(Bar)).Named("bar"));
			container.Register(Component.For(typeof(UsesBarDelegate)).Named("uBar"));

			var dependsOnFoo = container.Resolve<UsesBarDelegate>();
			dependsOnFoo.GetBar("aaa", "bbb");
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Does_not_duplicate_arguments_matching_delegate_parameters()
		{
			container.Register(Component.For(typeof(HasOnlyOneArgMatchingDelegatesParameter)).Named("fizz"));
			var factory = container.Resolve<Func<string, string, HasOnlyOneArgMatchingDelegatesParameter>>("fizzFactory");
			var obj = factory("arg1", "name");
			Assert.AreEqual("name", obj.Name);
			Assert.AreEqual("arg1", obj.Arg1);
		}

		[Test]
		public void Registered_Delegate_prefered_over_factory()
		{
			var foo = new DisposableFoo();
			container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<Func<int, DisposableFoo>>().Instance(i => foo),
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = container.Resolve<UsesDisposableFooDelegate>();
			var otherFoo = dependsOnFoo.GetFoo();
			Assert.AreSame(foo, otherFoo);
		}

		[Test]
		public void Releasing_component_depending_on_the_delegate_factory_releases_what_was_pulled_from_it()
		{
			DisposableFoo.DisposedCount = 0;

			container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = container.Resolve<UsesDisposableFooDelegate>();
			dependsOnFoo.GetFoo();

			Assert.AreEqual(0, DisposableFoo.DisposedCount);
			container.Release(dependsOnFoo);
			Assert.AreEqual(1, DisposableFoo.DisposedCount);
		}

		[Test]
		public void Using_Func_delegate_with_duplicated_Parameter_types_throws_exception()
		{
			container.Register(Component.For<Baz>().Named("baz"),
			                   Component.For<Bar>().Named("bar"),
			                   Component.For<UsesBarDelegate>());

			var user = container.Resolve<UsesBarDelegate>();

			var exception =
				Assert.Throws<ArgumentException>(() =>
				                                 user.GetBar("aaa", "bbb"));

			Assert.AreEqual(
				"Factory delegate System.Func`3[System.String,System.String,Castle.Windsor.Tests.Facilities.TypedFactory.Delegates.Bar] has duplicated arguments of type System.String. " +
				"Using generic purpose delegates with duplicated argument types is unsupported, because then it is not possible to match arguments properly. " +
				"Use some custom delegate with meaningful argument names or interface based factory instead.",
				exception.Message);
		}
	}
}