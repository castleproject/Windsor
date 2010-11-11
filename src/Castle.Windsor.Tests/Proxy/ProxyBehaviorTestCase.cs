// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	using System;

	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Tests.Components;
	using NUnit.Framework;

	[TestFixture]
	public class ProxyBehaviorTestCase
	{

#if !SILVERLIGHT // we do not support xml config on SL
		[Test]
		public void DefaultProxyBehaviorFromConfiguration()
		{
			var container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Proxy/proxyBehavior.xml"));

			var calcService = container.Resolve<ICalcService>("default");
			Assert.IsNotNull(calcService);
			Assert.IsTrue(calcService is IDisposable, "Service proxy should expose the IDisposable interface");
		}

		[Test]
		public void NoSingleInterfaceProxyBehaviorFromConfiguration()
		{
			var container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Proxy/proxyBehavior.xml"));

			var calcService = container.Resolve<ICalcService>("noSingle");
			Assert.IsNotNull(calcService);
			Assert.IsTrue(calcService is IDisposable, "Service proxy should expose the IDisposable interface");
		}
#endif

		[Test]
		public void ProxyGenarationHook_can_be_OnBehalfAware()
		{
			OnBehalfAwareProxyGenerationHook.target = null;
			var container = new WindsorContainer();
			container.Register(Component.For<OnBehalfAwareProxyGenerationHook>().LifeStyle.Transient,
			                   Component.For<StandardInterceptor>().LifeStyle.Transient,
			                   Component.For<ISimpleService>()
			                   	.ImplementedBy<SimpleService>()
			                   	.LifeStyle.Transient
			                   	.Interceptors<StandardInterceptor>()
			                   	.Proxy.Hook(h => h.Service<OnBehalfAwareProxyGenerationHook>()));

			var service = container.Resolve<ISimpleService>();

			Assert.IsTrue(ProxyServices.IsDynamicProxy(service.GetType()));
			Assert.IsNotNull(OnBehalfAwareProxyGenerationHook.target);
			Assert.AreEqual(typeof(ISimpleService), OnBehalfAwareProxyGenerationHook.target.Service);
		}

		[Test]
		public void OnBehalfAware_ProxyGenarationHook_works_on_dependencies()
		{
			OnBehalfAwareProxyGenerationHook.target = null;
			var container = new WindsorContainer();
			container.Register(Component.For<OnBehalfAwareProxyGenerationHook>().LifeStyle.Transient,
							   Component.For<StandardInterceptor>().LifeStyle.Transient,
							   Component.For<UsesSimpleComponent1>().LifeStyle.Transient,
							   Component.For<SimpleComponent1>().LifeStyle.Transient
								.Interceptors<StandardInterceptor>()
								.Proxy.Hook(h => h.Service<OnBehalfAwareProxyGenerationHook>()));

			var service = container.Resolve<UsesSimpleComponent1>();

			Assert.IsTrue(ProxyServices.IsDynamicProxy(service.Dependency.GetType()));
			Assert.IsNotNull(OnBehalfAwareProxyGenerationHook.target);
			Assert.AreEqual(typeof(SimpleComponent1), OnBehalfAwareProxyGenerationHook.target.Service);
		}

		[Test]
		public void InterceptorSelector_can_be_OnBehalfAware()
		{
			OnBehalfAwareInterceptorSelector.target = null;
			var container = new WindsorContainer();
			container.Register(Component.For<OnBehalfAwareInterceptorSelector>().LifeStyle.Transient,
			                   Component.For<StandardInterceptor>().LifeStyle.Transient,
			                   Component.For<ISimpleService>()
			                   	.ImplementedBy<SimpleService>()
			                   	.LifeStyle.Transient
			                   	.Interceptors<StandardInterceptor>()
			                   	.SelectInterceptorsWith(s => s.Service<OnBehalfAwareInterceptorSelector>()));

			var service = container.Resolve<ISimpleService>();

			Assert.IsTrue(ProxyServices.IsDynamicProxy(service.GetType()));
			Assert.IsNotNull(OnBehalfAwareInterceptorSelector.target);
			Assert.AreEqual(typeof(ISimpleService), OnBehalfAwareInterceptorSelector.target.Service);
		}
		[Test]
		public void OnBehalfAware_InterceptorSelector_works_on_dependencies()
		{
			OnBehalfAwareInterceptorSelector.target = null;
			var container = new WindsorContainer();
			container.Register(Component.For<OnBehalfAwareInterceptorSelector>().LifeStyle.Transient,
							   Component.For<StandardInterceptor>().LifeStyle.Transient,
							   Component.For<UsesSimpleComponent1>().LifeStyle.Transient,
							   Component.For<SimpleComponent1>().LifeStyle.Transient
								.Interceptors<StandardInterceptor>()
								.SelectInterceptorsWith(s => s.Service<OnBehalfAwareInterceptorSelector>()));

			var service = container.Resolve<UsesSimpleComponent1>();

			Assert.IsTrue(ProxyServices.IsDynamicProxy(service.Dependency.GetType()));
			Assert.IsNotNull(OnBehalfAwareInterceptorSelector.target);
			Assert.AreEqual(typeof(SimpleComponent1), OnBehalfAwareInterceptorSelector.target.Service);
		}

		[Test]
		public void NoSingleInterfaceProxyWithAttribute()
		{
			var container = new WindsorContainer();

			container.Register(Component.For<StandardInterceptor>(),
			                   Component.For<ICalcService>().ImplementedBy<CalculatorServiceWithMultipleInterfaces>());

			var calcService = container.Resolve<ICalcService>();
			Assert.IsNotNull(calcService);
			Assert.IsTrue(calcService is IDisposable, "Service proxy should expose the IDisposable interface");
		}

		[Test]
		public void Forwarded_type_proxy_does_what_questionMark()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<StandardInterceptor>()
			                   	.Named("a")
			                   	.LifeStyle.Transient,
			                   Component.For<ICommon, ICommon2>()
			                   	.ImplementedBy<TwoInterfacesImpl>()
			                   	.Interceptors("a")
			                   	.LifeStyle.Transient);

			var common = container.Resolve<ICommon>();
			var common2 = container.Resolve<ICommon2>();
			
			Assert.IsInstanceOf<ICommon>(common2);
			Assert.IsInstanceOf<ICommon2>(common);
		}

#if !SILVERLIGHT
		[Test]
		public void RequestMarshalByRefProxyWithAttribute()
		{
			var container = new WindsorContainer();

			container.Register(Component.For<StandardInterceptor>().Named("standard.interceptor"),
							   Component.For<ICalcService>().ImplementedBy<CalculatorServiceWithMarshalByRefProxyBehavior>()
								.Named("useMarshal"));

			var calcService = container.Resolve<ICalcService>("useMarshal");
			Assert.IsNotNull(calcService);
			Assert.IsFalse(calcService is CalculatorServiceWithMarshalByRefProxyBehavior, "Service proxy should not expose CalculatorServiceWithMarshalByRefProxyBehavior");
			Assert.IsTrue(calcService is MarshalByRefObject, "Service proxy should expose MarshalByRefObject");
			Assert.IsTrue(calcService is IDisposable, "Service proxy should expose the IDisposable interface");
		}

		[Test]
		public void InternalInterfaceIgnoredByProxy()
		{
			var container = new WindsorContainer(ConfigHelper.ResolveConfigPath("Proxy/proxyBehavior.xml"));
			Assert.DoesNotThrow(() => container.Resolve<object>("hasInternalInterface"));
		}
#endif
	}
}