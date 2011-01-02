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
	using System.Linq;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Releasers;
	using Castle.Windsor.Tests.ClassComponents;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Selectors;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryFacilityTake2TestCase:AbstractContainerTestFixture
	{
		[Test]
		public void Can_Resolve_by_closed_generic_closed_on_arguments_type_with_custom_selector()
		{
			Container.Register(AllTypes.FromAssemblyContaining<TypedFactoryFacilityTake2TestCase>()
			                   	.BasedOn(typeof(GenericComponent<>))
			                   	.WithService.Base().Configure(c => c.LifeStyle.Transient),
			                   Component.For<IObjectFactory>().AsFactory(s => s.SelectedWith<SelectorByClosedArgumentType>()),
			                   Component.For<SelectorByClosedArgumentType>());

			var factory = Container.Resolve<IObjectFactory>();

			var one = factory.GetItemByWithParameter(3);
			var two = factory.GetItemByWithParameter("two");
			Assert.IsInstanceOf<GenericIntComponent>(one);
			Assert.IsInstanceOf<GenericStringComponent>(two);

			Assert.AreEqual(3, ((GenericIntComponent)one).Value);
			Assert.AreEqual("two", ((GenericStringComponent)two).Value);
		}

		[Test]
		public void Can_Resolve_by_name_with_custom_selector()
		{
			Container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component1>()
					.Named("one"),
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.Named("two"),
				Component.For<IFactoryById>().AsFactory(f => f.SelectedWith<SelectorById>()),
				Component.For<SelectorById>());

			var factory = Container.Resolve<IFactoryById>();

			var one = factory.ComponentNamed("one");
			var two = factory.ComponentNamed("two");
			Assert.IsInstanceOf<Component1>(one);
			Assert.IsInstanceOf<Component2>(two);
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_default_selector_list()
		{
			Container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentListFactory>()
					.AsFactory());
			var factory = Container.Resolve<DummyComponentListFactory>();

			var all = factory.All();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Count);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_non_default_selector_array()
		{
			Container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentArrayFactory>()
					.AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<MultipleSelector>());
			var factory = Container.Resolve<DummyComponentArrayFactory>();

			var all = factory.All();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Length);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_non_default_selector_collection()
		{
			Container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentCollectionFactory>()
					.AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<MultipleSelector>());
			var factory = Container.Resolve<DummyComponentCollectionFactory>();

			var all = factory.All().ToArray();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Length);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_non_default_selector_enumerable()
		{
			Container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentEnumerableFactory>()
					.AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<MultipleSelector>());
			var factory = Container.Resolve<DummyComponentEnumerableFactory>();

			var all = factory.All().ToArray();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Length);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_non_default_selector_list()
		{
			Container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentListFactory>()
					.AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<MultipleSelector>());
			var factory = Container.Resolve<DummyComponentListFactory>();

			var all = factory.All();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Count);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_resolve_component()
		{
			Container.Register(Component.For<DummyComponentFactory>().AsFactory());
			var factory = Container.Resolve<DummyComponentFactory>();

			var component = factory.CreateDummyComponent();
			Assert.IsNotNull(component);
		}

		[Test]
		public void Can_resolve_component_by_name_with_default_selector()
		{
			Container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.Named("SecondComponent")
					.LifeStyle.Transient,
				Component.For<DummyComponentFactory>()
					.AsFactory());
			var factory = Container.Resolve<DummyComponentFactory>();

			var component = factory.GetSecondComponent();
			Assert.IsNotNull(component);
			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Can_resolve_open_generic_components()
		{
			Container.Register(
				Component.For<IGenericComponentsFactory>().AsFactory(),
				Component.For(typeof(GenericComponentWithIntArg<>)).LifeStyle.Singleton,
				Component.For(typeof(GenericComponent<>)).LifeStyle.Singleton);

			var factory = Container.Resolve<IGenericComponentsFactory>();

			factory.CreateGeneric<GenericComponent<int>>();
			factory.CreateGeneric<GenericComponent<IDisposable>>();

			var component = factory.CreateGeneric<GenericComponentWithIntArg<string>, int>(667);
			Assert.AreEqual(667, component.Property);
		}

		[Test]
		public void Can_resolve_via_factory_with_generic_method()
		{
			Container.Register(
				Component.For<IGenericComponentsFactory>().AsFactory());

			var factory = Container.Resolve<IGenericComponentsFactory>();
			var component = factory.CreateGeneric<IDummyComponent>();
			Assert.IsInstanceOf<Component1>(component);
		}

		[Test]
		public void Can_resolve_via_generic_factory_with_generic_method()
		{
			Container.Register(
				Component.For(typeof(IDummyComponent<>)).ImplementedBy(typeof(DummyComponent<>)).LifeStyle.Transient,
				Component.For(typeof(IGenericFactoryWithGenericMethod<>)).AsFactory());

			var factory = Container.Resolve<IGenericFactoryWithGenericMethod<A>>();
			factory.Create<IDummyComponent<A>>();
		}

		[Test]
		public void Can_resolve_via_generic_factory()
		{
			Container.Register(Component.For(typeof(IGenericFactory<>)).AsFactory());

			var factory = Container.Resolve<IGenericFactory<IDummyComponent>>();

			var component = factory.Create();
			Assert.IsNotNull(component);
		}

		[Test]
		public void Can_resolve_via_generic_factory_closed()
		{
			Container.Register(Component.For<IGenericFactoryClosed>().AsFactory());

			var factory = Container.Resolve<IGenericFactoryClosed>();

			var component = factory.Create();
			Assert.IsNotNull(component);
		}

		[Test]
		public void Can_resolve_via_generic_factory_closed_doubly()
		{
			Container.Register(Component.For<IGenericFactoryClosedDoubly>().AsFactory());

			var factory = Container.Resolve<IGenericFactoryClosedDoubly>() as IGenericFactory<IDummyComponent>;

			var component = factory.Create();
			Assert.IsNotNull(component);
		}

		[Test]
		public void Can_resolve_via_generic_factory_inherited_semi_closing()
		{
			Container.Register(Component.For(typeof(IGenericFactoryDouble<,>)).AsFactory(),
			                   Component.For<IProtocolHandler>().ImplementedBy<MirandaProtocolHandler>().LifeStyle.Transient);

			var factory = Container.Resolve<IGenericFactoryDouble<IDummyComponent, IProtocolHandler>>();

			factory.Create();
			factory.Create2();
		}

		[Test]
		public void Can_use_non_default_selector()
		{
			Container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.Named("foo")
					.LifeStyle.Transient,
				Component.For<DummyComponentFactory>()
					.AsFactory(f => f.SelectedWith<FooSelector>()),
				Component.For<FooSelector>());
			var factory = Container.Resolve<DummyComponentFactory>();

			var component = factory.GetSecondComponent();
			Assert.IsInstanceOf<Component2>(component);

			component = factory.CreateDummyComponent();
			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Component_released_out_of_band_is_STILL_tracked()
		{
			Container.Register(
				Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);

			var factory = Container.Resolve<INonDisposableFactory>();
			var component = factory.Create();
			var weakComponent = new WeakReference(component);

			Container.Release(component);
			component = null;
			GC.Collect();

			Assert.IsTrue(weakComponent.IsAlive);
		}

		[Test]
		public void Component_released_via_disposing_factory_is_not_tracked()
		{
			Container.Register(
				Component.For<IDisposableFactory>().LifeStyle.Transient.AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);

			var factory = Container.Resolve<IDisposableFactory>();
			var component = factory.Create();
			var weakComponent = new WeakReference(component);

			factory.Dispose();
			component = null;
			GC.Collect();

			Assert.IsFalse(weakComponent.IsAlive);
		}

		[Test]
		public void Component_released_via_factory_is_not_tracked()
		{
			Container.Register(
				Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);

			var factory = Container.Resolve<INonDisposableFactory>();
			var component = factory.Create();
			var weakComponent = new WeakReference(component);

			factory.LetGo(component);
			component = null;
			GC.Collect();

			Assert.IsFalse(weakComponent.IsAlive);
		}

		[Test]
		public void Disposing_factory_destroys_transient_components()
		{
			Container.Register(
				Component.For<IDisposableFactory>().AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);
			var factory = Container.Resolve<IDisposableFactory>();
			var component = factory.Create();
			Assert.IsFalse(component.Disposed);

			factory.Dispose();
			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Disposing_factory_does_not_destroy_singleton_components()
		{
			Container.Register(
				Component.For<IDisposableFactory>().AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Singleton);
			var factory = Container.Resolve<IDisposableFactory>();
			var component = factory.Create();
			Assert.IsFalse(component.Disposed);

			factory.Dispose();
			Assert.IsFalse(component.Disposed);
		}

		[Test]
		public void Factory_interface_can_be_hierarchical()
		{
			Container.Register(
				Component.For<ComponentWithOptionalParameter>()
					.LifeStyle.Transient,
				Component.For<IFactoryWithParametersExtended>()
					.AsFactory());
			var factory = Container.Resolve<IFactoryWithParametersExtended>();

			var one = factory.BuildComponent("one");
			var two = factory.BuildComponent2("two");
			Assert.AreEqual("one", one.Parameter);
			Assert.AreEqual("two", two.Parameter);
		}

		[Test]
		public void Factory_interface_can_be_hierarchical_with_repetitions()
		{
			Container.Register(
				Component.For<ComponentWithOptionalParameter>()
					.LifeStyle.Transient,
				Component.For<IFactoryWithParametersTwoBases>()
					.AsFactory());
			var factory = Container.Resolve<IFactoryWithParametersTwoBases>();

			var one = factory.BuildComponent("one");
			var two = factory.BuildComponent2("two");
			var three = factory.BuildComponent2("three");
			Assert.AreEqual("one", one.Parameter);
			Assert.AreEqual("two", two.Parameter);
			Assert.AreEqual("three", three.Parameter);
		}

		[Test]
		public void Factory_is_tracked_by_the_container()
		{
			Container.Register(Component.For<DummyComponentFactory>().AsFactory());

			var factory = Container.Resolve<DummyComponentFactory>();
			var weak = new WeakReference(factory);
			factory = null;

			GC.Collect();

			Assert.IsTrue(weak.IsAlive);
		}

		[Test]
		public void Releasing_factory_release_components()
		{
			Container.Register(
				Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);
			var factory = Container.Resolve<INonDisposableFactory>();
			var component = factory.Create();
			Assert.IsFalse(component.Disposed);

			Container.Release(factory);
			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Releasing_factory_releases_selector()
		{
			DisposableSelector.InstancesCreated = 0;
			DisposableSelector.InstancesDisposed = 0;
			Container.Register(
				Component.For<DummyComponentFactory>().AsFactory(f => f.SelectedWith<DisposableSelector>()).LifeStyle.Transient,
				Component.For<DisposableSelector>().LifeStyle.Transient);
			var factory = Container.Resolve<DummyComponentFactory>();

			Container.Release(factory);

			Assert.AreEqual(1, DisposableSelector.InstancesDisposed);
		}

		[Test]
		public void Resolve_component_by_name_with_default_selector_falls_back_to_by_type_when_no_name_found()
		{
			Container.Register(
				Component.For<DummyComponentFactory>()
					.AsFactory());
			var factory = Container.Resolve<DummyComponentFactory>();

			var component = factory.GetSecondComponent();
			Assert.IsNotNull(component);
			Assert.IsInstanceOf<Component1>(component);
		}

		[Test]
		public void Resolve_multiple_components_at_once_with_default_selector_collection_unasignable_from_array()
		{
			Container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<InvalidDummyComponentListFactory>()
					.AsFactory());
			var factory = Container.Resolve<InvalidDummyComponentListFactory>();

			Assert.Throws<ComponentNotFoundException>(() => factory.All());
		}

		[Test]
		public void Selector_WILL_NOT_be_picked_implicitly()
		{
			Container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<DummyComponentFactory>().AsFactory(),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>(),
				Component.For<Component2Selector, ITypedFactoryComponentSelector>());

			var factory = Container.Resolve<DummyComponentFactory>();
			var component = factory.CreateDummyComponent();

			Assert.IsInstanceOf<Component1>(component);
		}

		[Test]
		public void Selector_pick_by_instance()
		{
			Container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith(new Component2Selector())));

			var factory = Container.Resolve<DummyComponentFactory>();
			var component = factory.CreateDummyComponent();

			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Selector_pick_by_name()
		{
			Container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith("factoryTwo")),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>().Named("factoryOne"),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("factoryTwo"));

			var factory = Container.Resolve<DummyComponentFactory>();
			var component = factory.CreateDummyComponent();

			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Selector_pick_by_name_multiple_factories()
		{
			Container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith("factoryTwo")).Named("2"),
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith("factoryOne")).Named("1"),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>().Named("factoryOne"),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("factoryTwo"));

			var factory2 = Container.Resolve<DummyComponentFactory>("2");
			var component2 = factory2.CreateDummyComponent();
			Assert.IsInstanceOf<Component2>(component2);

			var factory1 = Container.Resolve<DummyComponentFactory>("1");
			var component1 = factory1.CreateDummyComponent();
			Assert.IsInstanceOf<Component1>(component1);
		}

		[Test]
		public void Selector_pick_by_type()
		{
			Container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith<Component2Selector>()),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>(),
				Component.For<Component2Selector, ITypedFactoryComponentSelector>());

			var factory = Container.Resolve<DummyComponentFactory>();
			var component = factory.CreateDummyComponent();

			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Should_match_arguments_ignoring_case()
		{
			Container.Register(
				Component.For<IFactoryWithParameters>().AsFactory(),
				Component.For<ComponentWithOptionalParameter>());

			var factory = Container.Resolve<IFactoryWithParameters>();
			var component = factory.BuildComponent("foo");

			Assert.AreEqual("foo", component.Parameter);
		}

		[Test]
		public void Typed_factory_lets_go_of_component_reference_on_dispose()
		{
			Container.Register(
				Component.For<IDisposableFactory>().LifeStyle.Transient.AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);
			var factory = Container.Resolve<IDisposableFactory>();
			var component = factory.Create();
			var weakComponentReference = new WeakReference(component);
			factory.Dispose();
			component = null;
			GC.Collect();
			Assert.IsFalse(weakComponentReference.IsAlive);
		}

		[Test]
		public void Typed_factory_lets_go_of_component_reference_on_release()
		{
			Container.Register(
				Component.For<IDisposableFactory>().LifeStyle.Transient.AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);
			var factory = Container.Resolve<IDisposableFactory>();
			var component = factory.Create();
			var weakComponentReference = new WeakReference(component);
			factory.Destroy(component);
			component = null;
			GC.Collect();
			Assert.IsFalse(weakComponentReference.IsAlive);
		}

		[Test]
		public void Typed_factory_obeys_release_policy_non_tracking()
		{
			Container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
			Container.Register(
				Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);

			var factory = Container.Resolve<INonDisposableFactory>();
			var component = factory.Create();

			var weakComponentReference = new WeakReference(component);
			Container.Release(component);
			component = null;
			GC.Collect();

			Assert.IsFalse(weakComponentReference.IsAlive);
		}

		[Test]
		public void Typed_factory_obeys_release_policy_tracking()
		{
			Container.Register(
				Component.For<INonDisposableFactory>().LifeStyle.Transient.AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);
			var factory = Container.Resolve<INonDisposableFactory>();
			var component = factory.Create();
			var weak = new WeakReference(component);
			component = null;
			GC.Collect();
			Assert.IsTrue(weak.IsAlive);
		}

		[Test]
		public void Void_methods_release_components()
		{
			Container.Register(
				Component.For<IDisposableFactory>().AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);
			var factory = Container.Resolve<IDisposableFactory>();
			var component = factory.Create();
			Assert.IsFalse(component.Disposed);

			factory.Destroy(component);
			Assert.IsTrue(component.Disposed);
		}

		protected override WindsorContainer BuildContainer()
		{
			var windsorContainer = new WindsorContainer();
			windsorContainer.AddFacility<TypedFactoryFacility>();
			windsorContainer.Register(Component.For<IDummyComponent>().ImplementedBy<Component1>().LifeStyle.Transient);
			return windsorContainer;
		}
	}
}