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

namespace Castle.MicroKernel.Tests.Registration
{
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.Lifestyle.Components;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.ClassComponents;
	using Castle.Windsor.Tests.Facilities.FactorySupport;

	using NUnit.Framework;

	[TestFixture]
	public class UsingFactoryMethodTestCase : RegistrationTestCaseBase
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
			var user = new User { FiscalStability = FiscalStability.DirtFarmer };
			Kernel.Register(
				Component.For<ICarProvider>()
					.UsingFactoryMethod(() => new AbstractCarProviderFactory().Create(user))
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
			var user = new User { FiscalStability = FiscalStability.MrMoneyBags };
			Kernel.Register(
				Component.For<User>().Instance(user),
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
	}
}