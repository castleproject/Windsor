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
	using Castle.MicroKernel.Tests.Facilities.FactorySupport;
	using Castle.MicroKernel.Tests.Lifestyle.Components;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class UsingFactoryMethodTestCase : RegistrationTestCaseBase
	{

		[Test]
		public void RegisterWithFluentFactory()
		{
			var user = new User { FiscalStability = FiscalStability.DirtFarmer };
			Kernel.Register(
				Component.For<User>().Instance(user),
				Component.For<AbstractCarProviderFactory>(),
				Component.For<ICarProvider>()
					.UsingFactory((AbstractCarProviderFactory f) => f.Create(Kernel.Resolve<User>()))
				);
			Assert.IsInstanceOf(typeof(HondaProvider), Kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void RegisterWithFactoryMethod()
		{
			var user = new User { FiscalStability = FiscalStability.DirtFarmer };
			Kernel.Register(
				Component.For<AbstractCarProviderFactory>(),
				Component.For<ICarProvider>()
					.UsingFactoryMethod(() => new AbstractCarProviderFactory().Create(user))
				);
			Assert.IsInstanceOf(typeof(HondaProvider), Kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void RegisterWithFactoryMethodAndKernel()
		{
			var user = new User { FiscalStability = FiscalStability.MrMoneyBags };
			Kernel.Register(
				Component.For<User>().Instance(user),
				Component.For<AbstractCarProviderFactory>(),
				Component.For<ICarProvider>()
					.UsingFactoryMethod(k => new AbstractCarProviderFactory().Create(k.Resolve<User>()))
				);
			Assert.IsInstanceOf(typeof(FerrariProvider), Kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void RegisterWithFactoryMethodNamed()
		{
			Kernel.Register(
				Component.For<ICarProvider>()
					.UsingFactoryMethod(() => new AbstractCarProviderFactory().Create(new User { FiscalStability = FiscalStability.MrMoneyBags }))
					.Named("ferrariProvider"),
				Component.For<ICarProvider>()
					.UsingFactoryMethod(() => new AbstractCarProviderFactory().Create(new User { FiscalStability = FiscalStability.DirtFarmer }))
					.Named("hondaProvider")
				);

			Assert.IsInstanceOf(typeof(HondaProvider), Kernel.Resolve<ICarProvider>("hondaProvider"));
			Assert.IsInstanceOf(typeof(FerrariProvider), Kernel.Resolve<ICarProvider>("ferrariProvider"));
		}

		[Test]
		public void RegisterWithFactoryMethodAndKernelNamed()
		{
			Kernel.Register(
				Component.For<ICarProvider>()
					.UsingFactoryMethod(k => new AbstractCarProviderFactory().Create(new User { FiscalStability = FiscalStability.MrMoneyBags }))
					.Named("ferrariProvider"),
				Component.For<ICarProvider>()
					.UsingFactoryMethod(k => new AbstractCarProviderFactory().Create(new User { FiscalStability = FiscalStability.DirtFarmer }))
					.Named("hondaProvider")
				);

			Assert.IsInstanceOf(typeof(HondaProvider), Kernel.Resolve<ICarProvider>("hondaProvider"));
			Assert.IsInstanceOf(typeof(FerrariProvider), Kernel.Resolve<ICarProvider>("ferrariProvider"));
		}

		[Test]
		public void RegisterWithFactoryMethodKernelAndContext()
		{
			Kernel.Register(
				Component.For<User>().LifeStyle.Transient,
				Component.For<AbstractCarProviderFactory>(),
				Component.For<ICarProvider>()
					.UsingFactoryMethod((k, ctx) => new AbstractCarProviderFactory().Create(
						k.Resolve<User>(ctx.AdditionalParameters)))
				);
			Assert.IsInstanceOf(typeof(FerrariProvider), Kernel.Resolve<ICarProvider>(
				new { FiscalStability = FiscalStability.MrMoneyBags }));
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

		[Test, Ignore("This does not work yet")]
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
	}
}