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

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Releasers;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor;
	using Castle.Windsor.Tests.ClassComponents;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Selectors;

	using NUnit.Framework;

	using HasTwoConstructors = Castle.Windsor.Tests.Facilities.TypedFactory.Delegates.HasTwoConstructors;

	[TestFixture]
	public class TypedFactoryDelegatesTestCase
	{
		private WindsorContainer container;

		[Test]
		public void Can_resolve_component_depending_on_delegate_when_inline_argumens_are_provided()
		{
			container.Register(Component.For<Foo>(),
			                   Component.For<UsesFooDelegateAndInt>());

			container.Resolve<UsesFooDelegateAndInt>(new { additionalArgument = 5 });
		}

		[Test]
		public void Can_resolve_delegate_of_generic()
		{
			container.Register(Component.For(typeof(GenericComponent<>)).LifeStyle.Transient);
			var one = container.Resolve<Func<GenericComponent<int>>>();
			var two = container.Resolve<Func<GenericComponent<string>>>();
			one();
			two();
		}

		[Test]
		public void Can_resolve_generic_component_depending_on_delegate_of_generic()
		{
			container.Register(Component.For(typeof(GenericComponent<>)).LifeStyle.Transient,
			                   Component.For(typeof(GenericUsesFuncOfGenerics<>)).LifeStyle.Transient);
			var one = container.Resolve<GenericUsesFuncOfGenerics<int>>();
			var two = container.Resolve<GenericUsesFuncOfGenerics<string>>();
			one.Func();
			two.Func();
		}

		[Test]
		public void Can_resolve_multiple_delegates()
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
		public void Can_resolve_two_services_depending_on_identical_delegates()
		{
			container.Register(Component.For<Foo>().LifeStyle.Transient,
			                   Component.For<UsesFooDelegate>(),
			                   Component.For<UsesFooDelegateAndInt>().DependsOn(new Arguments().Insert(5)));
			var one = container.Resolve<UsesFooDelegate>();
			var two = container.Resolve<UsesFooDelegateAndInt>();
			one.GetFoo();
			two.GetFoo();
		}

		[Test]
		public void Can_resolve_two_services_depending_on_identical_delegates_via_interface_based_factory()
		{
			container.Register(Component.For<Foo>().LifeStyle.Transient,
			                   Component.For<UsesFooDelegate>(),
			                   Component.For<UsesFooDelegateAndInt>().DependsOn(new Arguments().Insert(5)),
			                   Component.For<IGenericComponentsFactory>().AsFactory());

			var factory = container.Resolve<IGenericComponentsFactory>();

			var one = factory.CreateGeneric<UsesFooDelegate>();
			var two = factory.CreateGeneric<UsesFooDelegateAndInt>();

			one.GetFoo();
			two.GetFoo();
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
		public void Explicitly_registered_factory_is_tracked()
		{
			container.Register(Component.For<Func<A>>().AsFactory());

			var factory = container.Resolve<Func<A>>();

			Assert.IsTrue(container.Kernel.ReleasePolicy.HasTrack(factory));
		}

		[Test]
		public void Factory_DOES_NOT_implicitly_pick_registered_selector_explicitly_registered_factory()
		{
			DisposableSelector.InstancesCreated = 0;
			DisposableSelector.InstancesDisposed = 0;
			container.Register(
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<DisposableSelector>().LifeStyle.Transient,
				Component.For<Func<Foo>>().LifeStyle.Transient.AsFactory());

			container.Resolve<Func<Foo>>();

			Assert.AreEqual(0, DisposableSelector.InstancesCreated);
		}

		[Test]
		public void Factory_DOES_NOT_implicitly_pick_registered_selector_implicitly_registered_factory()
		{
			DisposableSelector.InstancesCreated = 0;
			DisposableSelector.InstancesDisposed = 0;
			container.Register(
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<DisposableSelector>().LifeStyle.Transient);

			container.Resolve<Func<Foo>>();

			Assert.AreEqual(0, DisposableSelector.InstancesCreated);
		}

		[Test]
		public void Factory_affects_constructor_resolution()
		{
			container.Register(Component.For<Baz>().Named("baz"));
			container.Register(Component.For<HasTwoConstructors>().Named("fizz"));
			var factory = container.Resolve<Func<string, HasTwoConstructors>>();

			var obj = factory("naaaameee");
			Assert.AreEqual("naaaameee", obj.Name);
		}

		[Test]
		public void Factory_does_not_referece_components_after_theyve_been_released()
		{
			DisposableFoo.DisposedCount = 0;

			container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = container.Resolve<UsesDisposableFooDelegate>();
			var foo = dependsOnFoo.GetFoo();

			Assert.AreEqual(0, DisposableFoo.DisposedCount);
			container.Release(foo);
			Assert.AreEqual(1, DisposableFoo.DisposedCount);

			var weakFoo = new WeakReference(foo);
			foo = null;
			GC.Collect();
			Assert.IsFalse(weakFoo.IsAlive);
		}

		[Test]
		public void Factory_explicitly_pick_registered_selector()
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
		public void Factory_obeys_lifestyle()
		{
			container.Register(Component.For<Foo>().Named("MyFoo").LifeStyle.Singleton);
			container.Register(Component.For<UsesFooDelegate>());
			var dependsOnFoo = container.Resolve<UsesFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
		}

		[Test]
		public void Factory_obeys_release_policy_non_tracking()
		{
			container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
			container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = container.Resolve<UsesDisposableFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.IsFalse(container.Kernel.ReleasePolicy.HasTrack(foo));
		}

		[Test]
		public void Factory_obeys_release_policy_tracking()
		{
			container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = container.Resolve<UsesDisposableFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.IsTrue(container.Kernel.ReleasePolicy.HasTrack(foo));
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Factory_parameters_are_used_in_order_first_ctor_then_properties()
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
		public void Factory_pulls_unspecified_dependencies_from_container()
		{
			container.Register(Component.For(typeof(Baz)).Named("baz"));
			container.Register(Component.For(typeof(Bar)).Named("bar"));
			container.Register(Component.For(typeof(UsesBarDelegate)).Named("uBar"));

			var dependsOnFoo = container.Resolve<UsesBarDelegate>();
			dependsOnFoo.GetBar("aaa", "bbb");
		}

		[Test]
		public void Func_delegate_with_duplicated_Parameter_types_throws_exception()
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

		[Test]
		public void Implicitly_registered_factory_is_tracked()
		{
			var factory = container.Resolve<Func<A>>();

			Assert.IsTrue(container.Kernel.ReleasePolicy.HasTrack(factory));
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
		public void Releasing_component_depending_on_a_factory_releases_what_was_pulled_from_it()
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
		public void Resolution_ShouldNotThrow_When_TwoDelegateFactoriesAreResolvedWithOnePreviouslyLazyLoaded_WithMultipleCtors()
		{
			container.Register(Component.For<SimpleComponent1>(),
			                   Component.For<SimpleComponent2>(),
			                   Component.For<SimpleComponent3>(),
			                   Component.For<ServiceFactory>(),
			                   Component.For<ServiceRedirect>(),
			                   Component.For<ServiceWithMultipleCtors>());

			var factory = container.Resolve<ServiceFactory>();
			factory.Factory();
		}

		[Test]
		public void Selector_pick_by_name()
		{
			container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<Func<IDummyComponent>>().AsFactory(c => c.SelectedWith("factoryTwo")),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>().Named("factoryOne"),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("factoryTwo"));

			Assert.IsTrue(container.Kernel.HasComponent(typeof(Func<IDummyComponent>)));
			var factory = container.Resolve<Func<IDummyComponent>>();
			var component = factory.Invoke();

			Assert.IsInstanceOf<Component2>(component);
		}

		[SetUp]
		public void SetUpTests()
		{
			container = new WindsorContainer();
			container.AddFacility<TypedFactoryFacility>();
		}
	}
}