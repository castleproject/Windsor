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

namespace Castle.Windsor.Tests.Proxy
{
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Components;
	using Castle.Windsor.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class ComponentProxyRegistrationTestCase
	{
		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
		}

		private WindsorContainer container;

		[Test]
		public void AddComponent_WithMixIn_AddsMixin()
		{
			container.Register(Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Proxy.MixIns(new SimpleMixIn())
				);

			var calculator = container.Resolve<ICalcService>();
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void AddComponent_With_instance_given_MixIn()
		{
			container.Register(
				Component.For<ICalcService>()
					.ImplementedBy<CalculatorService>()
					.Proxy.MixIns(m => m.Objects(new SimpleMixIn())));

			var calculator = container.Resolve<ICalcService>();
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void AddComponent_With_named_component_MixIn()
		{
			container.Register(
				Component.For<ICalcService>().ImplementedBy<CalculatorService>().Proxy.MixIns(m => m.Service("other")),
				Component.For<ISimpleMixIn>().ImplementedBy<SimpleMixIn>().Named("other"));

			var calculator = container.Resolve<ICalcService>();
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void AddComponent_With_typed_component_MixIn()
		{
			container.Register(
				Component.For<ICalcService>().ImplementedBy<CalculatorService>().Proxy.MixIns(m => m.Service<ISimpleMixIn>()),
				Component.For<ISimpleMixIn>().ImplementedBy<SimpleMixIn>());

			var calculator = container.Resolve<ICalcService>();
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void Releasing_MixIn_releases_all_parts()
		{
			SimpleServiceDisposable.DisposedCount = 0;
			container.Register(
				Component.For<ICalcService>()
					.ImplementedBy<CalculatorService>().Proxy.MixIns(m => m.Service<ISimpleService>())
					.LifeStyle.Transient,
				Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>()
					.LifeStyle.Transient);

			var calculator = container.Resolve<ICalcService>();
			Assert.IsInstanceOf<ISimpleService>(calculator);

			var mixin = (ISimpleService)calculator;
			mixin.Operation();
			container.Release(mixin);
			Assert.AreEqual(1, SimpleServiceDisposable.DisposedCount);
		}

		[Test]
		public void can_atach_hook_as_instance_simple()
		{
			var interceptor = new ResultModifierInterceptor(5);
			var hook = new ProxyNothingHook();
			container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(hook));

			var calc = container.Resolve<ICalcService>();
			Assert.AreEqual(4, calc.Sum(2, 2));
		}

		[Test]
		public void can_atach_hook_as_instance_simple_via_nested_closure()
		{
			var interceptor = new ResultModifierInterceptor(5);
			var hook = new ProxyNothingHook();
			container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(h => h.Instance(hook)));

			var calc = container.Resolve<ICalcService>();
			Assert.AreEqual(4, calc.Sum(2, 2));
		}

		[Test]
		public void can_atach_hook_as_named_service()
		{
			var interceptor = new ResultModifierInterceptor(5);
			container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<ProxyNothingHook>().Named("hook"),
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(h => h.Service("hook")));

			var calc = container.Resolve<ICalcService>();
			Assert.AreEqual(4, calc.Sum(2, 2));
		}

		[Test]
		public void can_atach_hook_as_typed_service()
		{
			var interceptor = new ResultModifierInterceptor(5);
			container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<ProxyNothingHook>(),
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(h => h.Service<ProxyNothingHook>()));

			var calc = container.Resolve<ICalcService>();
			Assert.AreEqual(4, calc.Sum(2, 2));
		}

		[Test]
		public void hook_gets_disposed_after_proxy_is_created()
		{
			DisposableHook.InstancesCreated = 0;
			DisposableHook.InstancesDisposed = 0;
			var interceptor = new ResultModifierInterceptor(5);
			container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<DisposableHook>().Named("hook").LifeStyle.Transient,
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(h => h.Service("hook")));

			container.Resolve<ICalcService>();

			Assert.AreEqual(1, DisposableHook.InstancesCreated);
			Assert.AreEqual(1, DisposableHook.InstancesDisposed);
		}
	}
}