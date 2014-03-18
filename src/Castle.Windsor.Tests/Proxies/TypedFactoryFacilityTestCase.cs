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

#if !SILVERLIGHT
// we do not support xml config on SL

namespace CastleTests.Proxies
{
	using Castle.DynamicProxy;
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
	}
}

#endif