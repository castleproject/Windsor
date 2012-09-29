// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.ProxyInfrastructure;
	using Castle.Windsor.Tests.Interceptors;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ComponentProxyRegistrationTestCase : AbstractContainerTestCase
	{
		private void AssertIsProxy(object o)
		{
			Assert.IsInstanceOf<IProxyTargetAccessor>(o);
		}

		[Test]
		public void AddComponent_WithMixIn_AddsMixin()
		{
			Container.Register(Component.For<ICalcService>()
				                   .ImplementedBy<CalculatorService>()
				                   .Proxy.MixIns(new SimpleMixIn())
				);

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);
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
			AssertIsProxy(calculator);
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void AddComponent_With_named_component_MixIn()
		{
			Container.Register(
				Component.For<ICalcService>().ImplementedBy<CalculatorService>().Proxy.MixIns(m => m.Component("other")),
				Component.For<ISimpleMixIn>().ImplementedBy<SimpleMixIn>().Named("other"));

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);
			Assert.IsInstanceOf(typeof(ISimpleMixIn), calculator);

			var mixin = (ISimpleMixIn)calculator;
			mixin.DoSomething();
		}

		[Test]
		public void AddComponent_With_typed_component_MixIn()
		{
			Container.Register(
				Component.For<ICalcService>().ImplementedBy<CalculatorService>().Proxy.MixIns(m => m.Component<SimpleMixIn>()),
				Component.For<ISimpleMixIn>().ImplementedBy<SimpleMixIn>());

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);
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
					"Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}'{1}' is waiting for the following dependencies:{0}- Component 'Castle.ProxyInfrastructure.ProxyNothingHook' (via override) which was not found. Did you forget to register it or misspelled the name? If the component is registered and override is via type make sure it doesn't have non-default name assigned explicitly or override the dependency via name.{0}",
					Environment.NewLine,
					typeof(CalculatorService).FullName),
				exception.Message);
		}

		[Test]
		public void Missing_dependency_on_mixin_statically_detected()
		{
			Container.Register(Component.For<ICalcService>()
				                   .ImplementedBy<CalculatorService>()
				                   .Proxy.MixIns(m => m.Component<A>()));

			var calc = Container.Kernel.GetHandler(typeof(ICalcService));
			Assert.AreEqual(HandlerState.WaitingDependency, calc.CurrentState);

			var exception =
				Assert.Throws<HandlerException>(() =>
				                                Container.Resolve<ICalcService>());
			var message = string.Format(
				"Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}'{1}' is waiting for the following dependencies:{0}- Component '{2}' (via override) which was not found. Did you forget to register it or misspelled the name? If the component is registered and override is via type make sure it doesn't have non-default name assigned explicitly or override the dependency via name.{0}",
				Environment.NewLine,
				typeof(CalculatorService).FullName,
				typeof(A).FullName);
			Assert.AreEqual(message, exception.Message);
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
			var message = string.Format(
				"Can't create component '{1}' as it has dependencies to be satisfied.{0}{0}'{1}' is waiting for the following dependencies:{0}- Component 'Castle.Windsor.Tests.Interceptors.DummyInterceptorSelector' (via override) which was not found. Did you forget to register it or misspelled the name? If the component is registered and override is via type make sure it doesn't have non-default name assigned explicitly or override the dependency via name.{0}",
				Environment.NewLine,
				typeof(CalculatorService).FullName);

			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Releasing_MixIn_releases_all_parts()
		{
			SimpleServiceDisposable.DisposedCount = 0;
			Container.Register(
				Component.For<ICalcService>()
					.ImplementedBy<CalculatorService>().Proxy.MixIns(m => m.Component<SimpleServiceDisposable>())
					.LifeStyle.Transient,
				Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>()
					.LifeStyle.Transient);

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);
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

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);
			Assert.AreEqual(4, calculator.Sum(2, 2));
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

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);
			Assert.AreEqual(4, calculator.Sum(2, 2));
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

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);
			Assert.AreEqual(4, calculator.Sum(2, 2));
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

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);
			Assert.AreEqual(4, calculator.Sum(2, 2));
		}

		[Test]
		public void can_proxy_interfaces_with_no_impl_given_just_a_hook()
		{
			Container.Register(Component.For<ICalcService>()
				                   .Proxy.Hook(h => h.Instance(new ProxyNothingHook())));

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);
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

			var calculator = Container.Resolve<ICalcService>();
			AssertIsProxy(calculator);

			Assert.AreEqual(1, DisposableHook.InstancesCreated);
			Assert.AreEqual(1, DisposableHook.InstancesDisposed);
		}
	}
}