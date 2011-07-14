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

namespace CastleTests.Lifestyle
{
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ScopedLifestyleExplicitAndTypedFactoriesTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.AddFacility<TypedFactoryFacility>();
		}

		[Test]
		public void Can_obtain_scoped_component_via_factory()
		{
			Container.Register(Component.For<UsesDisposableFooDelegate>().LifestyleTransient(),
			                   Component.For<DisposableFoo>().LifestyleScoped());

			var instance = Container.Resolve<UsesDisposableFooDelegate>();
			using (Container.BeginScope())
			{
				instance.GetFoo();
			}
		}

		[Test]
		public void Scoped_component_via_factory_and_outsideinstances_reused_properly()
		{
			Container.Register(Component.For<UsesFooAndDelegate>().LifeStyle.Transient,
			                   Component.For<Foo>().LifeStyle.Scoped
			                   	.DependsOn(Parameter.ForKey("number").Eq("1")));
			using (Container.BeginScope())
			{
				var instance = Container.Resolve<UsesFooAndDelegate>();

				var one = instance.Foo;
				var two = instance.GetFoo();
				Assert.AreSame(one, two);
			}
		}

		[Test]
		public void Scoped_component_via_factory_instances_reused_properly()
		{
			Container.Register(Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient,
			                   Component.For<DisposableFoo>().LifeStyle.Scoped);
			using (Container.BeginScope())
			{
				var instance = Container.Resolve<UsesDisposableFooDelegate>();

				var one = instance.GetFoo();
				var two = instance.GetFoo();

				Assert.AreSame(one, two);
			}
		}

		[Test]
		public void Scoped_component_via_factory_reused_properly_across_factories()
		{
			Container.Register(Component.For<UsesTwoFooDelegates>().LifeStyle.Transient,
			                   Component.For<Foo>().LifestyleScoped());

			var instance = Container.Resolve<UsesTwoFooDelegates>();
			using (Container.BeginScope())
			{
				var one = instance.GetFooOne();
				var two = instance.GetFooTwo();

				Assert.AreSame(one, two);
			}
		}
	}
}