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

	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.ProxyInfrastructure;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Components;
	using Castle.Windsor.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class ComponentProxyRegistrationTestCase : AbstractContainerTestFixture
	{
		[Test]
		public void AddComponent_WithMixIn_AddsMixin()
		{
			Container.Register(Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Proxy.MixIns(new SimpleMixIn())
				);

			var calculator = Container.Resolve<ICalcService>();
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void AddComponent_With_instance_given_MixIn()
		{
			Container.Register(
				Component.For<ICalcService>()
					.ImplementedBy<CalculatorService>()
					.Proxy.MixIns(m => m.Objects(new SimpleMixIn())));

			var calculator = Container.Resolve<ICalcService>();
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void AddComponent_With_named_component_MixIn()
		{
			Container.Register(
				Component.For<ICalcService>().ImplementedBy<CalculatorService>().Proxy.MixIns(m => m.Service("other")),
				Component.For<ISimpleMixIn>().ImplementedBy<SimpleMixIn>().Named("other"));

			var calculator = Container.Resolve<ICalcService>();
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void AddComponent_With_typed_component_MixIn()
		{
			Container.Register(
				Component.For<ICalcService>().ImplementedBy<CalculatorService>().Proxy.MixIns(m => m.Service<ISimpleMixIn>()),
				Component.For<ISimpleMixIn>().ImplementedBy<SimpleMixIn>());

			var calculator = Container.Resolve<ICalcService>();
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void Missing_dependency_on_hook_statically_detected()
		{
			Container.Register(Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Proxy.Hook(h => h.Service<ProxyNothingHook>()));

			var calc = Container.Kernel.GetHandler(typeof(ICalcService));
			Assert.AreEqual(HandlerState.WaitingDependency, calc.CurrentState);

			var exception =
				Assert.Throws<HandlerException>(() =>
				                                Container.Resolve<ICalcService>());
			Assert.AreEqual(
				string.Format(
					"Can't create component 'Castle.Windsor.Tests.Components.CalculatorService' as it has dependencies to be satisfied. {0}Castle.Windsor.Tests.Components.CalculatorService is waiting for the following dependencies: {0}{0}Services: {0}- Castle.ProxyInfrastructure.ProxyNothingHook which was not registered. {0}",
					Environment.NewLine),
				exception.Message);
		}

		[Test]
		public void Missing_dependency_on_mixin_statically_detected()
		{
			Container.Register(Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Proxy.MixIns(m => m.Service<A>()));

			var calc = Container.Kernel.GetHandler(typeof(ICalcService));
			Assert.AreEqual(HandlerState.WaitingDependency, calc.CurrentState);

			var exception =
				Assert.Throws<HandlerException>(() =>
				                                Container.Resolve<ICalcService>());
			Assert.AreEqual(
				string.Format(
					"Can't create component 'Castle.Windsor.Tests.Components.CalculatorService' as it has dependencies to be satisfied. {0}Castle.Windsor.Tests.Components.CalculatorService is waiting for the following dependencies: {0}{0}Services: {0}- Castle.Windsor.Tests.A which was not registered. {0}",
					Environment.NewLine),
				exception.Message);
		}

		[Test]
		public void Missing_dependency_on_selector_statically_detected()
		{
			Container.Register(Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.SelectInterceptorsWith(s => s.Service<DummyInterceptorSelector>()));

			var calc = Container.Kernel.GetHandler(typeof(ICalcService));
			Assert.AreEqual(HandlerState.WaitingDependency, calc.CurrentState);

			var exception =
				Assert.Throws<HandlerException>(() =>
				                                Container.Resolve<ICalcService>());
			Assert.AreEqual(
				string.Format(
					"Can't create component 'Castle.Windsor.Tests.Components.CalculatorService' as it has dependencies to be satisfied. {0}Castle.Windsor.Tests.Components.CalculatorService is waiting for the following dependencies: {0}{0}Services: {0}- Castle.Windsor.Tests.Interceptors.DummyInterceptorSelector which was not registered. {0}",
					Environment.NewLine),
				exception.Message);
		}

		[Test]
		public void Releasing_MixIn_releases_all_parts()
		{
			SimpleServiceDisposable.DisposedCount = 0;
			Container.Register(
				Component.For<ICalcService>()
					.ImplementedBy<CalculatorService>().Proxy.MixIns(m => m.Service<ISimpleService>())
					.LifeStyle.Transient,
				Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>()
					.LifeStyle.Transient);

			var calculator = Container.Resolve<ICalcService>();
			Assert.IsInstanceOf<ISimpleService>(calculator);

			var mixin = (ISimpleService)calculator;
			mixin.Operation();
			Container.Release(mixin);
			Assert.AreEqual(1, SimpleServiceDisposable.DisposedCount);
		}

		[Test]
		public void can_atach_hook_as_instance_simple()
		{
			var interceptor = new ResultModifierInterceptor(5);
			var hook = new ProxyNothingHook();
			Container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(hook));

			var calc = Container.Resolve<ICalcService>();
			Assert.AreEqual(4, calc.Sum(2, 2));
		}

		[Test]
		public void can_atach_hook_as_instance_simple_via_nested_closure()
		{
			var interceptor = new ResultModifierInterceptor(5);
			var hook = new ProxyNothingHook();
			Container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(h => h.Instance(hook)));

			var calc = Container.Resolve<ICalcService>();
			Assert.AreEqual(4, calc.Sum(2, 2));
		}

		[Test]
		public void can_atach_hook_as_named_service()
		{
			var interceptor = new ResultModifierInterceptor(5);
			Container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<ProxyNothingHook>().Named("hook"),
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(h => h.Service("hook")));

			var calc = Container.Resolve<ICalcService>();
			Assert.AreEqual(4, calc.Sum(2, 2));
		}

		[Test]
		public void can_atach_hook_as_typed_service()
		{
			var interceptor = new ResultModifierInterceptor(5);
			Container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<ProxyNothingHook>(),
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(h => h.Service<ProxyNothingHook>()));

			var calc = Container.Resolve<ICalcService>();
			Assert.AreEqual(4, calc.Sum(2, 2));
		}

		[Test]
		public void hook_gets_disposed_after_proxy_is_created()
		{
			DisposableHook.InstancesCreated = 0;
			DisposableHook.InstancesDisposed = 0;
			var interceptor = new ResultModifierInterceptor(5);
			Container.Register(Component.For<ResultModifierInterceptor>().Instance(interceptor),
			                   Component.For<DisposableHook>().Named("hook").LifeStyle.Transient,
			                   Component.For<ICalcService>()
			                   	.ImplementedBy<CalculatorService>()
			                   	.Interceptors<ResultModifierInterceptor>()
			                   	.Proxy.Hook(h => h.Service("hook")));

			Container.Resolve<ICalcService>();

			Assert.AreEqual(1, DisposableHook.InstancesCreated);
			Assert.AreEqual(1, DisposableHook.InstancesDisposed);
		}
	}
}