// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.TypedFactory.Tests
{
	using System;
	using System.Reflection;

	using Castle.Core;
	using Castle.Facilities.TypedFactory.Tests.Components;
	using Castle.Facilities.TypedFactory.Tests.Factories;
	using Castle.MicroKernel.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers.SpecializedResolvers;
	using Castle.Windsor;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryFacilityTake2TestCase
	{
		private WindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
			container.AddFacility<TypedFactoryFacility>();
			container.AddComponentLifeStyle<IDummyComponent, Component1>(LifestyleType.Transient);
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
	}

	public class FooSelector:ITypedFactoryComponentSelector
	{
		public TypedFactoryComponent SelectComponent(MethodInfo method, Type type, object[] arguments)
		{
			return new TypedFactoryComponent("foo", null, null);
		}
	}
}