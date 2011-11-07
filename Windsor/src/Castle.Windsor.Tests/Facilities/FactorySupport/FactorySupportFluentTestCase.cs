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

namespace CastleTests.Facilities.FactorySupport
{
	using Castle.Facilities.FactorySupport;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class FactorySupportFluentTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Kernel.AddFacility<FactorySupportFacility>();
		}

		private void RegisterComponentsImplemtedByFerrari(User user)
		{
			Kernel.Register(
				Component.For<User>().Named("currentUser").Instance(user),
				Component.For<AbstractCarProviderFactory>().Named("AbstractCarProviderFactory"),
				Component.For<ICarProvider>()
					.ImplementedBy<FerrariProvider>()
					.Attribute("factoryId").Eq("AbstractCarProviderFactory")
					.Attribute("factoryCreate").Eq("Create")
				);
		}

		[Test]
		public void Can_register_without_providing_an_implementation()
		{
			var user = new User { FiscalStability = FiscalStability.DirtFarmer };
			Kernel.Register(
				Component.For<User>().Named("currentUser").Instance(user),
				Component.For<AbstractCarProviderFactory>().Named("AbstractCarProviderFactory"),
				Component.For<ICarProvider>()
					.Attribute("factoryId").Eq("AbstractCarProviderFactory")
					.Attribute("factoryCreate").Eq("Create")
				);
			Assert.IsInstanceOf(typeof(HondaProvider), Kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void register_ferrari_implementation_get_ferrari_instance()
		{
			RegisterComponentsImplemtedByFerrari(new User { FiscalStability = FiscalStability.MrMoneyBags });
			Assert.IsInstanceOf(typeof(FerrariProvider), Kernel.Resolve<ICarProvider>());
		}

		[Test]
		public void register_ferrari_implementation_get_honda_instance()
		{
			RegisterComponentsImplemtedByFerrari(new User { FiscalStability = FiscalStability.DirtFarmer });
			Assert.IsInstanceOf(typeof(HondaProvider), Kernel.Resolve<ICarProvider>());
		}
	}
}