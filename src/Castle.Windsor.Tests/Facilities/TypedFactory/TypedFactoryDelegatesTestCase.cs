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

namespace CastleTests.Facilities.TypedFactory
{
	using System;
	using System.Linq;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Releasers;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Facilities.TypedFactory;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Selectors;

	using CastleTests;
	using CastleTests.Components;
	using CastleTests.Facilities.TypedFactory.Delegates;

	using NUnit.Framework;

	using HasTwoConstructors = Castle.Windsor.Tests.Facilities.TypedFactory.Delegates.HasTwoConstructors;
	using ServiceFactory = Castle.Windsor.Tests.Facilities.TypedFactory.ServiceFactory;

	[TestFixture]
	public class TypedFactoryDelegatesTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_register_generic_delegate_factory_explicitly_as_open_generic_optional_dependency()
		{
			Container.Register(Component.For<Foo>().LifeStyle.Transient,
			                   Component.For<Bar>().LifeStyle.Transient,
			                   Component.For<UsesFooAndBarDelegateProperties>(),
			                   Component.For(typeof(Func<>)).AsFactory());

			var instance = Container.Resolve<UsesFooAndBarDelegateProperties>();

			Assert.IsNotNull(instance.FooFactory);
			Assert.IsNotNull(instance.BarFactory);

			var factoryHandler = Kernel.GetHandler(typeof(Func<>));
			Assert.IsNotNull(factoryHandler);

			var allhandlers = Kernel.GetAssignableHandlers(typeof(object));

			Assert.IsFalse(allhandlers.SelectMany(h => h.Services).Any(s => s == typeof(Func<Foo>)));
			Assert.IsFalse(allhandlers.SelectMany(h => h.Services).Any(s => s == typeof(Func<Bar>)));
		}

		[Test]
		public void Can_register_generic_delegate_factory_explicitly_as_open_generic_required_dependency()
		{
			Container.Register(Component.For<Foo>().LifeStyle.Transient,
			                   Component.For<Bar>().LifeStyle.Transient,
			                   Component.For<UsesFooAndBarDelegateCtor>(),
			                   Component.For(typeof(Func<>)).AsFactory());

			var instance = Container.Resolve<UsesFooAndBarDelegateCtor>();

			Assert.IsNotNull(instance.FooFactory);
			Assert.IsNotNull(instance.BarFactory);

			var factoryHandler = Kernel.GetHandler(typeof(Func<>));
			Assert.IsNotNull(factoryHandler);

			var allhandlers = Kernel.GetAssignableHandlers(typeof(object));

			Assert.IsFalse(allhandlers.SelectMany(h => h.Services).Any(s => s == typeof(Func<Foo>)));
			Assert.IsFalse(allhandlers.SelectMany(h => h.Services).Any(s => s == typeof(Func<Bar>)));
		}

		[Test]
		public void Can_resolve_component_depending_on_delegate_when_inline_argumens_are_provided()
		{
			Container.Register(Component.For<Foo>(),
			                   Component.For<UsesFooDelegateAndInt>());

			Container.Resolve<UsesFooDelegateAndInt>(new { additionalArgument = 5 });
		}

		[Test]
		public void Can_resolve_delegate_of_generic()
		{
			Container.Register(Component.For(typeof(GenericComponent<>)).LifeStyle.Transient);
			var one = Container.Resolve<Func<GenericComponent<int>>>();
			var two = Container.Resolve<Func<GenericComponent<string>>>();
			one();
			two();
		}

		[Test]
		public void Can_resolve_generic_component_depending_on_delegate_of_generic()
		{
			Container.Register(Component.For(typeof(GenericComponent<>)).LifeStyle.Transient,
			                   Component.For(typeof(GenericUsesFuncOfGenerics<>)).LifeStyle.Transient);
			var one = Container.Resolve<GenericUsesFuncOfGenerics<int>>();
			var two = Container.Resolve<GenericUsesFuncOfGenerics<string>>();
			one.Func();
			two.Func();
		}

		[Test]
		public void Can_resolve_multiple_delegates()
		{
			Container.Register(Component.For<Baz>());
			Container.Register(Component.For<A>());

			var bazFactory = Container.Resolve<Func<Baz>>();
			var aFactory = Container.Resolve<Func<A>>();

			bazFactory.Invoke();
			aFactory.Invoke();
		}

		[Test]
		public void Can_resolve_service_via_delegate()
		{
			Container.Register(Component.For<Foo>().Named("MyFoo").LifeStyle.Transient);
			Container.Register(Component.For<UsesFooDelegate>());
			var dependsOnFoo = Container.Resolve<UsesFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(2, foo.Number);
		}

		[Test]
		public void Can_resolve_two_services_depending_on_identical_delegates()
		{
			Container.Register(Component.For<Foo>().LifeStyle.Transient,
			                   Component.For<UsesFooDelegate>(),
			                   Component.For<UsesFooDelegateAndInt>().DependsOn(new Arguments().Insert(5)));
			var one = Container.Resolve<UsesFooDelegate>();
			var two = Container.Resolve<UsesFooDelegateAndInt>();
			one.GetFoo();
			two.GetFoo();
		}

		[Test]
		public void Can_resolve_two_services_depending_on_identical_delegates_via_interface_based_factory()
		{
			Container.Register(Component.For<Foo>().LifeStyle.Transient,
			                   Component.For<UsesFooDelegate>(),
			                   Component.For<UsesFooDelegateAndInt>().DependsOn(new Arguments().Insert(5)),
			                   Component.For<IGenericComponentsFactory>().AsFactory());

			var factory = Container.Resolve<IGenericComponentsFactory>();

			var one = factory.CreateGeneric<UsesFooDelegate>();
			var two = factory.CreateGeneric<UsesFooDelegateAndInt>();

			one.GetFoo();
			two.GetFoo();
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Does_not_duplicate_arguments_matching_delegate_parameters()
		{
			Container.Register(Component.For(typeof(HasOnlyOneArgMatchingDelegatesParameter)).Named("fizz"));
			var factory = Container.Resolve<Func<string, string, HasOnlyOneArgMatchingDelegatesParameter>>("fizzFactory");
			var obj = factory("arg1", "name");
			Assert.AreEqual("name", obj.Name);
			Assert.AreEqual("arg1", obj.Arg1);
		}

		[Test]
		public void Explicitly_registered_factory_is_tracked()
		{
			Container.Register(Component.For<Func<A>>().AsFactory());

			var factory = Container.Resolve<Func<A>>();
			var weak = new WeakReference(factory);
			factory = null;
			GC.Collect();

			Assert.IsTrue(weak.IsAlive);
		}

		[Test]
		public void Factory_DOES_NOT_implicitly_pick_registered_selector_explicitly_registered_factory()
		{
			DisposableSelector.InstancesCreated = 0;
			DisposableSelector.InstancesDisposed = 0;
			Container.Register(
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<DisposableSelector>().LifeStyle.Transient,
				Component.For<Func<Foo>>().LifeStyle.Transient.AsFactory());

			Container.Resolve<Func<Foo>>();

			Assert.AreEqual(0, DisposableSelector.InstancesCreated);
		}

		[Test]
		public void Factory_DOES_NOT_implicitly_pick_registered_selector_implicitly_registered_factory()
		{
			DisposableSelector.InstancesCreated = 0;
			DisposableSelector.InstancesDisposed = 0;
			Container.Register(
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<DisposableSelector>().LifeStyle.Transient);

			Container.Resolve<Func<Foo>>();

			Assert.AreEqual(0, DisposableSelector.InstancesCreated);
		}

		[Test]
		public void Factory_affects_constructor_resolution()
		{
			Container.Register(Component.For<Baz>().Named("baz"));
			Container.Register(Component.For<HasTwoConstructors>().Named("fizz"));
			var factory = Container.Resolve<Func<string, HasTwoConstructors>>();

			var obj = factory("naaaameee");
			Assert.AreEqual("naaaameee", obj.Name);
		}

		[Test]
		public void Factory_constructor_dependency_is_satisfied_implicitly_even_if_less_greedy_constructor_is_readily_available()
		{
			Container.Register(Component.For<Bar>().LifeStyle.Transient,
			                   Component.For<UsesBarDelegateTwoConstructors>().LifeStyle.Transient);

			var component = Container.Resolve<UsesBarDelegateTwoConstructors>();

			Assert.IsNotNull(component.BarFactory);
		}

		[Test]
		public void Factory_does_not_referece_components_after_theyve_been_released()
		{
			DisposableFoo.ResetDisposedCount();

			Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();
			var foo = dependsOnFoo.GetFoo();

			Assert.AreEqual(0, DisposableFoo.DisposedCount);
			Container.Release(dependsOnFoo);
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
			Container.Register(
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<DisposableSelector>().Named("1").LifeStyle.Transient,
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<SimpleSelector>().Named("2").LifeStyle.Transient,
				Component.For<Func<Foo>>().LifeStyle.Transient.AsFactory(x => x.SelectedWith("2")));

			Container.Resolve<Func<Foo>>();

			Assert.AreEqual(0, DisposableSelector.InstancesCreated);
			Assert.AreEqual(1, SimpleSelector.InstancesCreated);
		}

		[Test]
		public void Factory_obeys_lifestyle()
		{
			Container.Register(Component.For<Foo>().Named("MyFoo").LifeStyle.Singleton);
			Container.Register(Component.For<UsesFooDelegate>());
			var dependsOnFoo = Container.Resolve<UsesFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
		}

		[Test]
		public void Factory_obeys_release_policy_non_tracking()
		{
			Container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
			Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			var weak = new WeakReference(foo);
			foo = null;
			GC.Collect();

			Assert.IsFalse(weak.IsAlive);
		}

		[Test]
		public void Factory_obeys_release_policy_tracking()
		{
			Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);

			var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			var weak = new WeakReference(foo);
			foo = null;
			GC.Collect();

			Assert.IsTrue(weak.IsAlive);
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Factory_parameters_are_used_in_order_first_ctor_then_properties()
		{
			Container.Register(Component.For(typeof(Baz)).Named("baz"));
			Container.Register(Component.For(typeof(Bar)).Named("bar"));
			Container.Register(Component.For(typeof(UsesBarDelegate)).Named("barBar"));

			var dependsOnFoo = Container.Resolve<UsesBarDelegate>();
			var bar = dependsOnFoo.GetBar("a name", "a description");
			Assert.AreEqual("a name", bar.Name);
			Assert.AreEqual("a description", bar.Description);
		}

		[Test]
		public void Factory_property_dependency_is_satisfied_implicitly()
		{
			Container.Register(Component.For<Bar>().LifeStyle.Transient,
			                   Component.For<UsesBarDelegateProperty>().LifeStyle.Transient);

			var component = Container.Resolve<UsesBarDelegateProperty>();

			Assert.IsNotNull(component.BarFactory);
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Factory_pulls_unspecified_dependencies_from_container()
		{
			Container.Register(Component.For(typeof(Baz)).Named("baz"));
			Container.Register(Component.For(typeof(Bar)).Named("bar"));
			Container.Register(Component.For(typeof(UsesBarDelegate)).Named("uBar"));

			var dependsOnFoo = Container.Resolve<UsesBarDelegate>();
			dependsOnFoo.GetBar("aaa", "bbb");
		}

		[Test]
		public void Func_delegate_with_duplicated_Parameter_types_throws_exception()
		{
			Container.Register(Component.For<Baz>().Named("baz"),
			                   Component.For<Bar>().Named("bar"),
			                   Component.For<UsesBarDelegate>());

			var user = Container.Resolve<UsesBarDelegate>();

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
		public void Implicitly_registered_factory_is_always_tracked()
		{
			var factory = Container.Resolve<Func<A>>();

			Assert.IsTrue(Container.Kernel.ReleasePolicy.HasTrack(factory));
		}

		[Test]
		public void Registered_Delegate_prefered_over_factory()
		{
			var foo = new DisposableFoo();
			Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<Func<int, DisposableFoo>>().Instance(i => foo),
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();
			var otherFoo = dependsOnFoo.GetFoo();
			Assert.AreSame(foo, otherFoo);
		}

		[Test]
		public void Registers_generic_delegate_factories_as_open_generics_optional_dependency()
		{
			Container.Register(Component.For<Foo>().LifeStyle.Transient,
			                   Component.For<Bar>().LifeStyle.Transient,
			                   Component.For<UsesFooAndBarDelegateProperties>());

			var instance = Container.Resolve<UsesFooAndBarDelegateProperties>();

			Assert.IsNotNull(instance.FooFactory);
			Assert.IsNotNull(instance.BarFactory);

			var factoryHandler = Kernel.GetHandler(typeof(Func<>));
			Assert.IsNotNull(factoryHandler);
		}

		[Test]
		public void Registers_generic_delegate_factories_as_open_generics_required_dependency()
		{
			Container.Register(Component.For<Foo>().LifeStyle.Transient,
			                   Component.For<Bar>().LifeStyle.Transient,
			                   Component.For<UsesFooAndBarDelegateCtor>());

			var instance = Container.Resolve<UsesFooAndBarDelegateCtor>();

			Assert.IsNotNull(instance.FooFactory);
			Assert.IsNotNull(instance.BarFactory);

			var factoryHandler = Kernel.GetHandler(typeof(Func<>));
			Assert.IsNotNull(factoryHandler);
		}

		[Test]
		public void Releasing_component_depending_on_a_factory_releases_what_was_pulled_from_it()
		{
			DisposableFoo.ResetDisposedCount();

			Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = Container.Resolve<UsesDisposableFooDelegate>();
			dependsOnFoo.GetFoo();

			Assert.AreEqual(0, DisposableFoo.DisposedCount);
			Container.Release(dependsOnFoo);
			Assert.AreEqual(1, DisposableFoo.DisposedCount);
		}

		[Test]
		public void Releasing_factory_releases_selector()
		{
			DisposableSelector.InstancesCreated = 0;
			DisposableSelector.InstancesDisposed = 0;
			Container.Register(
				Component.For<DisposableSelector>().LifeStyle.Transient,
				Component.For<Func<Foo>>().LifeStyle.Transient.AsFactory(x => x.SelectedWith<DisposableSelector>()));
			var factory = Container.Resolve<Func<Foo>>();

			Assert.AreEqual(1, DisposableSelector.InstancesCreated);

			Container.Release(factory);

			Assert.AreEqual(1, DisposableSelector.InstancesDisposed);
		}

		[Test]
		public void Resolution_ShouldNotThrow_When_TwoDelegateFactoriesAreResolvedWithOnePreviouslyLazyLoaded_WithMultipleCtors()
		{
			Container.Register(Component.For<SimpleComponent1>(),
			                   Component.For<SimpleComponent2>(),
			                   Component.For<SimpleComponent3>(),
			                   Component.For<ServiceFactory>(),
			                   Component.For<ServiceRedirect>(),
			                   Component.For<ServiceWithMultipleCtors>());

			var factory = Container.Resolve<ServiceFactory>();
			factory.Factory();
		}

		[Test]
		public void Selector_pick_by_name()
		{
			Container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<Func<IDummyComponent>>().AsFactory(c => c.SelectedWith("factoryTwo")),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>().Named("factoryOne"),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("factoryTwo"));

			Assert.IsTrue(Container.Kernel.HasComponent(typeof(Func<IDummyComponent>)));
			var factory = Container.Resolve<Func<IDummyComponent>>();
			var component = factory.Invoke();

			Assert.IsInstanceOf<Component2>(component);
		}

		protected override void AfterContainerCreated()
		{
			Container.AddFacility<TypedFactoryFacility>();
		}
	}
}