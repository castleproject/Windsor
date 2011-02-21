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

namespace Castle.Proxies
{
	using System;

	using Castle.Core.Configuration;
	using Castle.DynamicProxy;
	using Castle.Facilities.FactorySupport;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class FactorySupportTestCase : AbstractContainerTestCase
	{
		private void AddComponent(string key, Type service, Type type, string factoryMethod)
		{
			var config = new MutableConfiguration(key);
			config.Attributes["factoryId"] = "factory";
			config.Attributes["factoryCreate"] = factoryMethod;
			Container.Kernel.ConfigurationStore.AddComponentConfiguration(key, config);
			Container.Kernel.Register(Component.For(service).ImplementedBy(type).Named(key));
		}

		[Test]
		public void FactorySupport_UsingProxiedFactory_WorksFine()
		{
			Container.AddFacility<FactorySupportFacility>();
			Container.Register(Component.For<StandardInterceptor>(),
			                   Component.For<CalulcatorFactory>().Named("factory"));

			AddComponent("calculator", typeof(ICalcService), typeof(CalculatorService), "Create");

			var service = Container.Resolve<ICalcService>("calculator");

			Assert.IsNotNull(service);
		}
	}
}