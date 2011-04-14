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

namespace Castle.Core.Tests
{
	using Castle.DynamicProxy;
	using Castle.MicroKernel.Registration;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	public class InterceptorAttributeTestCase : AbstractContainerTestCase
	{
		private bool IsProxy(object instance)
		{
			return instance is IProxyTargetAccessor;
		}

		[Test]
		public void Can_set_interceptor_via_attribute_named()
		{
			Container.Register(
				Component.For<StandardInterceptor>().Named("FooInterceptor"),
				Component.For<ICalcService>().ImplementedBy<CalculatorServiceWithFooInterceptorNamed>());
			var calcService = Container.Resolve<ICalcService>();
			Assert.IsTrue(IsProxy(calcService));
		}

		[Test]
		public void Can_set_interceptor_via_attribute_typed()
		{
			Container.Register(
				Component.For<StandardInterceptor>(),
				Component.For<ICalcService>().ImplementedBy<CalculatorServiceWithStandartInterceptorTyped>());
			var calcService = Container.Resolve<ICalcService>();
			Assert.IsTrue(IsProxy(calcService));
		}

		[Test]
		public void Can_set_interceptor_via_inherited_attribute()
		{
			Container.Register(
				Component.For<StandardInterceptor>(),
				Component.For<CalculatorServiceWithStandardInterceptor>());
			var calcService = Container.Resolve<CalculatorServiceWithStandardInterceptor>();
			Assert.IsTrue(IsProxy(calcService));
		}
	}
}
