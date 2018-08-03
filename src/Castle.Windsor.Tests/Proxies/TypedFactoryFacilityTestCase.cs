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

namespace CastleTests.Proxies
{
	using System;

	using Castle.DynamicProxy;
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.TypedFactoryInterfaces;
	using Castle.Windsor;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.XmlFiles;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryFacilityTestCase
	{
		[Test]
		public void TypedFactory_CreateMethodHasNoId_WorksFine()
		{
			var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("typedFactoryCreateWithoutId.xml")));

			var calcFactory = container.Resolve<ICalculatorFactoryCreateWithoutId>();
			Assert.IsNotNull(calcFactory);

			var calculator = calcFactory.Create();
			Assert.IsNotNull(calculator);
			Assert.AreEqual(3, calculator.Sum(1, 2));
		}

		[Test]
		public void TypedFactory_WithProxies_WorksFine()
		{
			var container = new WindsorContainer(new XmlInterpreter(Xml.Embedded("typedFactory.xml")));

			var calcFactory = container.Resolve<ICalculatorFactory>();
			Assert.IsNotNull(calcFactory);

			var calculator = calcFactory.Create("default");
			Assert.IsInstanceOf<IProxyTargetAccessor>(calculator);
			Assert.AreEqual(3, calculator.Sum(1, 2));

			calcFactory.Release(calculator);
		}

		public interface IInterface { }
		public class C1 : IInterface { }
		public class C2 : IInterface { }
		public class Root
		{
			private readonly IInterface i;
			public Root(Func<IInterface> factory)
			{
				// Works if `Func<TIInterface>` is changed to `IInterface`
				i = factory();
				Assert.That(i, Is.TypeOf<C2>());
			}
		}

		[Test]
		public void TypedFactory_WithServiceOverride_Picks_Correct_Dependency()
		{
			var container = new WindsorContainer();
			container.AddFacility<TypedFactoryFacility>();

			container.Register(Component.For<IInterface>().ImplementedBy<C1>());
			container.Register(Component.For<IInterface>().ImplementedBy<C2>());
			container.Register(Component.For<Root>().DependsOn(Dependency.OnComponent<IInterface, C2>()));

			var t = container.Resolve<Root>();
		}

	}
}
