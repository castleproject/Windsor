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

namespace Castle.Windsor.Tests
{
	using System.Runtime.Remoting;

	using Castle.DynamicProxy;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Components;
	using Castle.Windsor.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class SmartProxyTestCase
	{
		private IWindsorContainer container;

		[Test]
		public void ConcreteClassProxy()
		{
			container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
			container.Register(Component.For(typeof(CalculatorService)).Named("key"));

			var service = container.Resolve<CalculatorService>("key");

			Assert.IsNotNull(service);
#if (!SILVERLIGHT)
			Assert.IsFalse(RemotingServices.IsTransparentProxy(service));
#endif
			Assert.AreEqual(5, service.Sum(2, 2));
		}

		[SetUp]
		public void Init()
		{
			container = new WindsorContainer();

			container.AddFacility<MyInterceptorGreedyFacility>();
		}

		[Test]
		public void InterfaceInheritance()
		{
			container.Register(Component.For<StandardInterceptor>().Named("interceptor"));
			container.Register(Component.For<ICameraService>().ImplementedBy<CameraService>());

			var service = container.Resolve<ICameraService>();

			Assert.IsNotNull(service);
		}

		[Test]
		public void InterfaceProxy()
		{
			container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
			container.Register(Component.For(typeof(ICalcService)).ImplementedBy(typeof(CalculatorService)).Named("key"));

			var service = container.Resolve<ICalcService>("key");

			Assert.IsNotNull(service);
#if (!SILVERLIGHT)
			Assert.IsFalse(RemotingServices.IsTransparentProxy(service));
#endif
			Assert.AreEqual(5, service.Sum(2, 2));
		}

		[TearDown]
		public void Terminate()
		{
			container.Dispose();
		}
	}
}