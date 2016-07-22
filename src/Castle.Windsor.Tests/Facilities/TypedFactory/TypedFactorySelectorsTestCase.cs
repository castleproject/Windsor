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
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

	using CastleTests.Components;
	using CastleTests.Facilities.TypedFactory.Selectors;
	using NUnit.Framework;

	[TestFixture]
	public class TypedFactorySelectorsTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.AddFacility<TypedFactoryFacility>();
		}

		[Test]
		public void Explicitly_specified_name_fails_if_not_present()
		{

			Container.Register(Component.For<A>(),
			                   Component.For<A>().Named("name"),
			                   Component.For<IGenericFactory<A>>().AsFactory(x => x.SelectedWith(new WithNameSelector("non existing name"))));

			var factory = Container.Resolve<IGenericFactory<A>>();

			Assert.Throws<ComponentNotFoundException>(() => factory.Create());

		}

		[Test]
		public void Implicitly_specified_name_falls_back_if_not_present()
		{

			Container.Register(Component.For<A>(),
							   Component.For<A>().Named("name"),
							   Component.For<IGenericFactory<A>>().AsFactory(x => x.SelectedWith(new WithNameSelector("non existing name"))));

			var factory = Container.Resolve<IGenericFactory<A>>();

			Assert.Throws<ComponentNotFoundException>(() => factory.Create());

		}
	}
}