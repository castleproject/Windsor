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

namespace CastleTests.Registration
{
	using System;

	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.Configuration.Components;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.ClassComponents;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class UsingFactoryMethodTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_dispose_component_on_release_disposable_service()
		{
			Kernel.Register(Component.For<DisposableComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new DisposableComponent()));
			var component = Kernel.Resolve<DisposableComponent>();
			Assert.IsFalse(component.Disposed);

			Kernel.ReleaseComponent(component);

			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Can_dispose_component_on_release_non_disposable_service_and_impl()
		{
			Kernel.Register(Component.For<IComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new ComponentWithDispose()));
			var component = Kernel.Resolve<IComponent>() as ComponentWithDispose;
			Assert.IsFalse(component.Disposed);

			Kernel.ReleaseComponent(component);

			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Can_dispose_component_on_release_non_disposable_service_disposable_impl()
		{
			Kernel.Register(Component.For<IComponent>()
			                	.ImplementedBy<ComponentWithDispose>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new ComponentWithDispose()));
			var component = Kernel.Resolve<IComponent>() as ComponentWithDispose;
			Assert.IsFalse(component.Disposed);

			Kernel.ReleaseComponent(component);

			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Can_opt_out_of_applying_lifetime_concerns_to_factory_component()
		{
			Kernel.Register(Component.For<DisposableComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new DisposableComponent(), managedExternally: true));
			var component = Kernel.Resolve<DisposableComponent>();
			Assert.IsFalse(component.Disposed);

			Kernel.ReleaseComponent(component);

			Assert.IsFalse(component.Disposed);
		}

		[Test]
		public void Can_properly_resolve_component_from_UsingFactory()
		{
			var user = new User { FiscalStability = FiscalStability.DirtFarmer };
			Kernel.Register(
				Component.For<User>().Instance(user),
				Component.For<AbstractCarProviderFactory>(),
				Component.For<ICarProvider>()
					.UsingFactory((AbstractCarProviderFactory f) => f.Create(Kernel.Resolve<User>()))
				);
			Assert.IsInstanceOf<HondaProvider>(Kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void Can_properly_resolve_component_from_UsingFactoryMethod()
		{
			Kernel.Register(
				Component.For<ICarProvider>()
					.UsingFactoryMethod(
						() => new AbstractCarProviderFactory().Create(new User { FiscalStability = FiscalStability.DirtFarmer }))
				);

			Assert.IsInstanceOf<HondaProvider>(Kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void Can_properly_resolve_component_from_UsingFactoryMethod_named()
		{
			Kernel.Register(
				Component.For<ICarProvider>()
					.UsingFactoryMethod(
						() => new AbstractCarProviderFactory().Create(new User { FiscalStability = FiscalStability.MrMoneyBags }))
					.Named("ferrariProvider"),
				Component.For<ICarProvider>()
					.UsingFactoryMethod(
						() => new AbstractCarProviderFactory().Create(new User { FiscalStability = FiscalStability.DirtFarmer }))
					.Named("hondaProvider")
				);

			Assert.IsInstanceOf<HondaProvider>(Kernel.Resolve<ICarProvider>("hondaProvider"));
			Assert.IsInstanceOf<FerrariProvider>(Kernel.Resolve<ICarProvider>("ferrariProvider"));
		}

		[Test]
		public void Can_properly_resolve_component_from_UsingFactoryMethod_with_kernel()
		{
			Kernel.Register(
				Component.For<User>().Instance(new User { FiscalStability = FiscalStability.MrMoneyBags }),
				Component.For<ICarProvider>()
					.UsingFactoryMethod(k => new AbstractCarProviderFactory().Create(k.Resolve<User>()))
				);
			Assert.IsInstanceOf<FerrariProvider>(Kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void Can_properly_resolve_component_from_UsingFactoryMethod_with_kernel_named()
		{
			Kernel.Register(
				Component.For<ICarProvider>()
					.UsingFactoryMethod(
						k => new AbstractCarProviderFactory().Create(new User { FiscalStability = FiscalStability.MrMoneyBags }))
					.Named("ferrariProvider"),
				Component.For<ICarProvider>()
					.UsingFactoryMethod(
						k => new AbstractCarProviderFactory().Create(new User { FiscalStability = FiscalStability.DirtFarmer }))
					.Named("hondaProvider")
				);

			Assert.IsInstanceOf<HondaProvider>(Kernel.Resolve<ICarProvider>("hondaProvider"));
			Assert.IsInstanceOf<FerrariProvider>(Kernel.Resolve<ICarProvider>("ferrariProvider"));
		}

		[Test]
		public void Can_properly_resolve_component_from_UsingFactoryMethod_with_kernel_with_context()
		{
			Kernel.Register(
				Component.For<User>().LifeStyle.Transient,
				Component.For<AbstractCarProviderFactory>(),
				Component.For<ICarProvider>()
					.UsingFactoryMethod((k, ctx) =>
					                    new AbstractCarProviderFactory()
					                    	.Create(k.Resolve<User>(ctx.AdditionalArguments)))
				);
			var carProvider = Kernel.Resolve<ICarProvider>(new Arguments().Insert("FiscalStability", FiscalStability.MrMoneyBags));
			Assert.IsInstanceOf<FerrariProvider>(carProvider);
		}

		[Test]
		public void Can_proxy_component_created_via_factory_using_additional_interfaces()
		{
			Kernel.Register(Component.For<IComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new TrivialComponent())
			                	.Proxy.AdditionalInterfaces(typeof(IEmptyService)));
			var component = Kernel.Resolve<IComponent>();
			Assert.IsInstanceOf<IEmptyService>(component);
		}

		[Test]
		public void Can_proxy_component_created_via_factory_using_interceptors()
		{
			Kernel.Register(
				Component.For<StandardInterceptor>(),
				Component.For<IComponent>()
					.LifeStyle.Transient
					.UsingFactoryMethod(() => new TrivialComponent())
					.Interceptors<StandardInterceptor>());
			var component = Kernel.Resolve<IComponent>();

			var id = component.ID;

			Assert.IsInstanceOf<IProxyTargetAccessor>(component);
		}

		[Test]
		public void Can_proxy_component_created_via_factory_using_interceptors_multiple_services()
		{
			Kernel.Register(
				Component.For<StandardInterceptor>(),
				Component.For<IComponent, TrivialComponent>()
					.LifeStyle.Transient
					.UsingFactoryMethod(() => new TrivialComponent())
					.Interceptors<StandardInterceptor>());
			var component = Kernel.Resolve<IComponent>();

			var id = component.ID;

			Assert.IsInstanceOf<IProxyTargetAccessor>(component);
		}

		[Test]
		public void Can_proxy_component_created_via_factory_using_mixins()
		{
			Kernel.Register(Component.For<IComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new TrivialComponent())
			                	.Proxy.MixIns(new CameraService()));
			var component = Kernel.Resolve<IComponent>();
			Assert.IsInstanceOf<ICameraService>(component);
		}

		[Test(Description = "see issue IOC-ISSUE-207")]
		public void Can_register_more_than_one_with_factory_method()
		{
			Assert.DoesNotThrow(
				() => Kernel.Register(
					Component.For<ClassWithPrimitiveDependency>()
						.UsingFactoryMethod(() => new ClassWithPrimitiveDependency(2)),
					Component.For<ClassWithServiceDependency>()
						.UsingFactoryMethod(() => new ClassWithServiceDependency(null))));
		}

		[Test]
		public void Checks_and_throws_an_exception_when_factory_method_returns_null()
		{
			Kernel.Register(Component.For<IComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => default(IComponent)));

			Assert.Throws<ComponentActivatorException>(() => Kernel.Resolve<IComponent>());
		}

		[Test]
		public void Does_not_try_to_set_properties_on_component_resolved_via_factory_method()
		{
			Kernel.Register(
				Component.For<AProp>().UsingFactoryMethod(() => new AProp()),
				Component.For<A>());

			var aProp = Kernel.Resolve<AProp>();
			Assert.IsNull(aProp.Prop);
		}

		[Test]
		[Bug("IOC-332")]
		public void Factories_returning_proxies_with_no_target_are_not_supported()
		{
			var generator = new ProxyGenerator();
			Container.Register(Component.For<ICameraService>().UsingFactoryMethod(() => generator.CreateInterfaceProxyWithoutTarget<ICameraService>()));

			var exception = Assert.Throws<NotSupportedException>(() => Container.Resolve<ICameraService>());

			Assert.AreEqual(@"Can not apply commission concerns to component Late bound CastleTests.Components.ICameraService because it appears to be a target-less proxy. Currently those are not supported.",
			                exception.Message);
		}

		[Test]
		public void Factory_created_abstract_non_disposable_class_services_are_NOT_tracked()
		{
			Kernel.Register(Component.For<TrivialComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new TrivialComponent()));

			var component = Kernel.Resolve<TrivialComponent>();

			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component));
		}

		[Test]
		public void Factory_created_abstract_non_disposable_interface_services_are_NOT_tracked()
		{
			Kernel.Register(Component.For<IComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new SealedComponent()));

			var component = Kernel.Resolve<IComponent>();

			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component));
		}

		[Test]
		public void Factory_created_abstract_non_disposable_services_with_disposable_dependency_are_tracked()
		{
			Kernel.Register(
				Component.For<IComponent>().LifeStyle.Transient
					.UsingFactoryMethod(k => new TrivialComponentWithDependency(k.Resolve<ISimpleService>())),
				Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>().LifeStyle.Transient);

			var component = Kernel.Resolve<IComponent>() as TrivialComponentWithDependency;

			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(component));
			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(component.Dependency));
		}

		[Test]
		public void Factory_created_abstract_non_disposable_services_with_non_disposable_dependency_are_NOT_tracked()
		{
			Kernel.Register(
				Component.For<IComponent>().LifeStyle.Transient
					.UsingFactoryMethod(k => new TrivialComponentWithDependency(k.Resolve<ISimpleService>())),
				Component.For<ISimpleService>().ImplementedBy<SimpleService>().LifeStyle.Transient);

			var component = Kernel.Resolve<IComponent>() as TrivialComponentWithDependency;

			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component));
			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component.Dependency));
		}

		[Test]
		public void Factory_created_sealed_disposable_services_are_tracked()
		{
			Kernel.Register(Component.For<SealedComponentDisposable>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new SealedComponentDisposable()));

			var component = Kernel.Resolve<SealedComponentDisposable>();

			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(component));

			Kernel.ReleaseComponent(component);

			Assert.IsTrue(component.Disposed);
			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component));
		}

		[Test]
		public void Factory_created_sealed_non_disposable_services_are_not_tracked()
		{
			Kernel.Register(Component.For<SealedComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new SealedComponent()));

			var component = Kernel.Resolve<SealedComponent>();

			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component));
		}

		[Test]
		public void Factory_created_sealed_non_disposable_services_with_disposable_dependency_are_tracked()
		{
			Kernel.Register(
				Component.For<SealedComponentWithDependency>().LifeStyle.Transient
					.UsingFactoryMethod(k => new SealedComponentWithDependency(k.Resolve<ISimpleService>())),
				Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>().LifeStyle.Transient);

			var component = Kernel.Resolve<SealedComponentWithDependency>();

			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(component));
			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(component.Dependency));
		}

		[Test]
		public void Factory_created_sealed_non_disposable_services_with_factory_created_disposable_dependency_are_tracked()
		{
			Kernel.Register(
				Component.For<SealedComponentWithDependency>().LifeStyle.Transient
					.UsingFactoryMethod(k => new SealedComponentWithDependency(k.Resolve<ISimpleService>())),
				Component.For<ISimpleService>().LifeStyle.Transient
					.UsingFactoryMethod(() => new SimpleServiceDisposable()));

			var component = Kernel.Resolve<SealedComponentWithDependency>();

			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(component));
			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(component.Dependency));
		}

		[Test]
		public void
			Factory_created_sealed_non_disposable_services_with_factory_created_non_disposable_dependency_are_NOT_tracked()
		{
			Kernel.Register(
				Component.For<SealedComponentWithDependency>().LifeStyle.Transient
					.UsingFactoryMethod(k => new SealedComponentWithDependency(k.Resolve<ISimpleService>())),
				Component.For<ISimpleService>().LifeStyle.Transient
					.UsingFactoryMethod(() => new SimpleService()));

			var component = Kernel.Resolve<SealedComponentWithDependency>();

			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component));
			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component.Dependency));
		}

		[Test]
		public void Factory_created_sealed_non_disposable_services_with_non_disposable_dependency_are_NOT_tracked()
		{
			Kernel.Register(
				Component.For<SealedComponentWithDependency>().LifeStyle.Transient
					.UsingFactoryMethod(k => new SealedComponentWithDependency(k.Resolve<ISimpleService>())),
				Component.For<ISimpleService>().ImplementedBy<SimpleService>().LifeStyle.Transient);

			var component = Kernel.Resolve<SealedComponentWithDependency>();

			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component));
			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component.Dependency));
		}

		[Test]
		public void Managed_externally_factory_component_transient_is_not_tracked_by_release_policy()
		{
			Kernel.Register(Component.For<DisposableComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new DisposableComponent(), managedExternally: true));

			var component = Kernel.Resolve<DisposableComponent>();

			Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(component));
		}

		[Test]
		public void Managed_externally_factory_component_transient_is_not_tracked_by_the_container()
		{
			Kernel.Register(Component.For<DisposableComponent>()
			                	.LifeStyle.Transient
			                	.UsingFactoryMethod(() => new DisposableComponent(), managedExternally: true));

			var weak = new WeakReference(Kernel.Resolve<DisposableComponent>());
			GC.Collect();

			Assert.IsFalse(weak.IsAlive);
		}

		[Test]
		public void Proxying_type_with_no_default_ctor_throws_helpful_message()
		{
			Kernel.Register(
				Component.For<StandardInterceptor>(),
				Component.For<ClassWithConstructors>()
					.LifeStyle.Transient
					.Interceptors<StandardInterceptor>()
					.UsingFactoryMethod(() => new ClassWithConstructors("something")));

			var exception =
				Assert.Throws<InvalidProxyConstructorArgumentsException>(() => Kernel.Resolve<ClassWithConstructors>());

			var expected =
				@"Can not instantiate proxy of class: Castle.MicroKernel.Tests.Configuration.Components.ClassWithConstructors.
Could not find a parameterless constructor.";

			Assert.AreEqual(expected, exception.Message);
		}
	}
}