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
	using Castle.Facilities.TypedFactory.Tests.Components;
	using Castle.Facilities.TypedFactory.Tests.Factories;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Selectors;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryFacilityTake2TestCase
	{
		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
			container.AddFacility<TypedFactoryFacility>();
			container.Register(Component.For<IDummyComponent>().ImplementedBy<Component1>().LifeStyle.Transient);
		}

		private WindsorContainer container;

		[Test]
		public void Can_Resolve_by_closed_generic_closed_on_arguments_type_with_custom_selector()
		{
			container.Register(AllTypes.FromAssemblyContaining<TypedFactoryFacilityTake2TestCase>()
			                   	.BasedOn(typeof(GenericComponent<>))
			                   	.WithService.Base().Configure(c => c.LifeStyle.Transient),
			                   Component.For<IGenericFactory>().AsFactory(),
			                   Component.For<ITypedFactoryComponentSelector>()
			                   	.ImplementedBy<SelectorByClosedArgumentType>());

			var factory = container.Resolve<IGenericFactory>();

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
			container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component1>()
					.Named("one"),
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.Named("two"),
				Component.For<IFactoryById>().AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<SelectorById>());

			var factory = container.Resolve<IFactoryById>();

			var one = factory.ComponentNamed("one");
			var two = factory.ComponentNamed("two");
			Assert.IsInstanceOf<Component1>(one);
			Assert.IsInstanceOf<Component2>(two);
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_default_selector_list()
		{
			container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentListFactory>()
					.AsFactory());
			var factory = container.Resolve<DummyComponentListFactory>();

			var all = factory.All();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Count);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_non_default_selector_array()
		{
			container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentArrayFactory>()
					.AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<MultipleSelector>());
			var factory = container.Resolve<DummyComponentArrayFactory>();

			var all = factory.All();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Length);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_non_default_selector_collection()
		{
			container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentCollectionFactory>()
					.AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<MultipleSelector>());
			var factory = container.Resolve<DummyComponentCollectionFactory>();

			var all = factory.All().ToArray();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Length);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_non_default_selector_enumerable()
		{
			container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentEnumerableFactory>()
					.AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<MultipleSelector>());
			var factory = container.Resolve<DummyComponentEnumerableFactory>();

			var all = factory.All().ToArray();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Length);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_Resolve_multiple_components_at_once_with_non_default_selector_list()
		{
			container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<DummyComponentListFactory>()
					.AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<MultipleSelector>());
			var factory = container.Resolve<DummyComponentListFactory>();

			var all = factory.All();
			Assert.IsNotNull(all);
			Assert.AreEqual(2, all.Count);
			Assert.That(all.Any(c => c is Component1));
			Assert.That(all.Any(c => c is Component2));
		}

		[Test]
		public void Can_pick_non_default_selector_by_instance()
		{
			container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith(new Component2Selector())));

			var factory = container.Resolve<DummyComponentFactory>();
			var component = factory.CreateDummyComponent();

			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Can_pick_non_default_selector_by_name()
		{
			container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith("factoryTwo")),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>().Named("factoryOne"),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("factoryTwo"));

			var factory = container.Resolve<DummyComponentFactory>();
			var component = factory.CreateDummyComponent();

			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Can_pick_non_default_selector_by_name_multiple_factories()
		{
			container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith("factoryTwo")).Named("2"),
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith("factoryOne")).Named("1"),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>().Named("factoryOne"),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component2Selector>().Named("factoryTwo"));

			var factory2 = container.Resolve<DummyComponentFactory>("2");
			var component2 = factory2.CreateDummyComponent();
			Assert.IsInstanceOf<Component2>(component2);

			var factory1 = container.Resolve<DummyComponentFactory>("1");
			var component1 = factory1.CreateDummyComponent();
			Assert.IsInstanceOf<Component1>(component1);
		}

		[Test]
		public void Can_pick_non_default_selector_by_type()
		{
			container.Register(
				Component.For<IDummyComponent>().ImplementedBy<Component1>().Named("one").LifeStyle.Transient,
				Component.For<IDummyComponent>().ImplementedBy<Component2>().Named("two").LifeStyle.Transient,
				Component.For<DummyComponentFactory>().AsFactory(c => c.SelectedWith<Component2Selector>()),
				Component.For<ITypedFactoryComponentSelector>().ImplementedBy<Component1Selector>(),
				Component.For<Component2Selector, ITypedFactoryComponentSelector>());

			var factory = container.Resolve<DummyComponentFactory>();
			var component = factory.CreateDummyComponent();

			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Can_resolve_component()
		{
			container.Register(Component.For<DummyComponentFactory>().AsFactory());
			var factory = container.Resolve<DummyComponentFactory>();

			var component = factory.CreateDummyComponent();
			Assert.IsNotNull(component);
		}

		[Test]
		public void Can_resolve_component_by_name_with_default_selector()
		{
			container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.Named("SecondComponent")
					.LifeStyle.Transient,
				Component.For<DummyComponentFactory>()
					.AsFactory());
			var factory = container.Resolve<DummyComponentFactory>();

			var component = factory.GetSecondComponent();
			Assert.IsNotNull(component);
			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Can_resolve_open_generic_components()
		{
			container.Register(
				Component.For<IGenericComponentsFactory>().AsFactory(),
				Component.For(typeof(GenericComponentWithIntArg<>)).LifeStyle.Singleton,
				Component.For(typeof(GenericComponent<>)).LifeStyle.Singleton);

			var factory = container.Resolve<IGenericComponentsFactory>();

			factory.CreateGeneric<GenericComponent<int>>();
			factory.CreateGeneric<GenericComponent<IDisposable>>();

			var component = factory.CreateGeneric<GenericComponentWithIntArg<string>, int>(667);
			Assert.AreEqual(667, component.Property);
		}

		[Test]
		public void Can_resolve_via_generic_factory()
		{
			container.Register(
				Component.For<IGenericComponentsFactory>().AsFactory());

			var factory = container.Resolve<IGenericComponentsFactory>();
			var component = factory.CreateGeneric<IDummyComponent>();
			Assert.IsInstanceOf<Component1>(component);
		}

		[Test]
		public void Can_use_non_default_selector()
		{
			container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.Named("foo")
					.LifeStyle.Transient,
				Component.For<DummyComponentFactory>()
					.AsFactory(),
				Component.For<ITypedFactoryComponentSelector>()
					.ImplementedBy<FooSelector>());
			var factory = container.Resolve<DummyComponentFactory>();

			var component = factory.GetSecondComponent();
			Assert.IsInstanceOf<Component2>(component);

			component = factory.CreateDummyComponent();
			Assert.IsInstanceOf<Component2>(component);
		}

		[Test]
		public void Disposing_factory_destroys_transient_components()
		{
			container.Register(
				Component.For<IDisposableFactory>().AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);
			var factory = container.Resolve<IDisposableFactory>();
			var component = factory.Create();
			Assert.IsFalse(component.Disposed);

			factory.Dispose();
			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Disposing_factory_does_not_destroy_singleton_components()
		{
			container.Register(
				Component.For<IDisposableFactory>().AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Singleton);
			var factory = container.Resolve<IDisposableFactory>();
			var component = factory.Create();
			Assert.IsFalse(component.Disposed);

			factory.Dispose();
			Assert.IsFalse(component.Disposed);
		}

		[Test]
		public void Resolve_component_by_name_with_default_selector_falls_back_to_by_type_when_no_name_found()
		{
			container.Register(
				Component.For<DummyComponentFactory>()
					.AsFactory());
			var factory = container.Resolve<DummyComponentFactory>();

			var component = factory.GetSecondComponent();
			Assert.IsNotNull(component);
			Assert.IsInstanceOf<Component1>(component);
		}

		[Test]
		public void Resolve_multiple_components_at_once_with_default_selector_collection_unasignable_from_array()
		{
			container.Register(
				Component.For<IDummyComponent>()
					.ImplementedBy<Component2>()
					.LifeStyle.Transient,
				Component.For<InvalidDummyComponentListFactory>()
					.AsFactory());
			var factory = container.Resolve<InvalidDummyComponentListFactory>();

			Assert.Throws<ComponentNotFoundException>(() => factory.All());
		}

		[Test]
		public void Should_match_arguments_ignoring_case()
		{
			container.Register(
				Component.For<IFactoryWithParameters>().AsFactory(),
				Component.For<ComponentWithOptionalParameter>());

			var factory = container.Resolve<IFactoryWithParameters>();
			var component = factory.BuildComponent("foo");

			Assert.AreEqual("foo", component.Parameter);
		}

		[Test]
		public void Void_methods_release_components()
		{
			container.Register(
				Component.For<IDisposableFactory>().AsFactory(),
				Component.For<DisposableComponent>().LifeStyle.Transient);
			var factory = container.Resolve<IDisposableFactory>();
			var component = factory.Create();
			Assert.IsFalse(component.Disposed);

			factory.Destroy(component);
			Assert.IsTrue(component.Disposed);
		}
	}
}