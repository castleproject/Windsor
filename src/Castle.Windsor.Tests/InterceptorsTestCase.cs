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

namespace Castle.Windsor.Tests
{
	using System;
	using System.Linq;
	using System.Threading;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Registration;
	using Castle.ProxyInfrastructure;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests.Interceptors;
#if !SILVERLIGHT
	using Castle.XmlFiles;
#endif
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class InterceptorsTestCase
	{
		private IWindsorContainer container;
		private readonly ManualResetEvent startEvent = new ManualResetEvent(false);
		private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
		private CalculatorService service;

		[SetUp]
		public void Init()
		{
			container = new WindsorContainer();
			container.AddFacility<MyInterceptorGreedyFacility>();
		}

		[TearDown]
		public void Terminate()
		{
			container.Dispose();
		}

		[Test]
		public void InterfaceProxy()
		{
			container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
			container.Register(Component.For(typeof(ICalcService)).ImplementedBy(typeof(CalculatorService)).Named("key"));

			var service = container.Resolve<ICalcService>("key");

			Assert.IsNotNull(service);
			Assert.AreEqual(5, service.Sum(2, 2));
		}

		[Test]
		public void Interface_that_depends_on_service_it_is_intercepting()
		{
			container.Register(Component.For<InterceptorThatCauseStackOverflow>(),
			                   Component.For<ICameraService>().ImplementedBy<CameraService>().Interceptors<InterceptorThatCauseStackOverflow>(),
			                   //because it has no interceptors, it is okay to resolve it...
			                   Component.For<ICameraService>().ImplementedBy<CameraService>().Named("okay to resolve"));

			container.Resolve<ICameraService>();
		}

		[Test]
		public void InterfaceProxyWithLifecycle()
		{
			container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
			container.Register(Component.For(typeof(ICalcService)).ImplementedBy(typeof(CalculatorServiceWithLifecycle)).Named("key"));

			var service = container.Resolve<ICalcService>("key");

			Assert.IsNotNull(service);
			Assert.IsTrue(service.Initialized);
			Assert.AreEqual(5, service.Sum(2, 2));

			Assert.IsFalse(service.Disposed);

			container.Release(service);

			Assert.IsTrue(service.Disposed);
		}

		[Test]
		public void ClassProxy()
		{
			container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
			container.Register(Component.For(typeof(CalculatorService)).Named("key"));

			service = container.Resolve<CalculatorService>("key");

			Assert.IsNotNull(service);
			Assert.AreEqual(5, service.Sum(2, 2));
		}

#if (!SILVERLIGHT)
		//no xml in Silverlight

		[Test]
		public void Xml_validComponent_resolves_correctly()
		{
			container.Install(XmlResource("interceptors.xml"));
			service = container.Resolve<CalculatorService>("ValidComponent");

			Assert.IsNotNull(service);
			Assert.AreEqual(5, service.Sum(2, 2));
		}

		[Test]
		public void Xml_multiple_interceptors_resolves_correctly()
		{
			container.Install(XmlResource("interceptorsMultiple.xml"));
			service = container.Resolve<CalculatorService>("component");

			Assert.IsNotNull(service);
			Assert.AreEqual(10, service.Sum(2, 2));
		}

		[Test]
		public void Xml_Component_With_Non_Existing_Interceptor_throws()
		{
			container.Install(XmlResource("interceptors.xml"));
			Assert.Throws(typeof(HandlerException), () =>
			                                        container.Resolve<CalculatorService>("ComponentWithNonExistingInterceptor"));
		}

		[Test]
		public void Xml_Component_With_Non_invalid_Interceptor_throws()
		{
			Assert.Throws(typeof(Exception), () =>
			                                 container.Install(XmlResource("interceptorsInvalid.xml")));
		}

		[Test]
		public void Xml_mixin()
		{
			container.Install(XmlResource("mixins.xml"));
			service = container.Resolve<CalculatorService>("ValidComponent");

			Assert.IsInstanceOf<ISimpleMixIn>(service);

			((ISimpleMixIn)service).DoSomething();
		}

		[Test]
		public void Xml_additionalInterfaces()
		{
			container.Install(XmlResource("additionalInterfaces.xml"));
			service = container.Resolve<CalculatorService>("ValidComponent");

			Assert.IsInstanceOf<ISimpleMixIn>(service);

			Assert.Throws(typeof(System.NotImplementedException), () =>
			                                                      ((ISimpleMixIn)service).DoSomething());
		}

		[Test]
		public void Xml_hook_and_selector()
		{
			ProxyAllHook.Instances = 0;
			SelectAllSelector.Calls = 0;
			SelectAllSelector.Instances = 0;
			container.Install(XmlResource("interceptorsWithHookAndSelector.xml"));
			var model = container.Kernel.GetHandler("ValidComponent").ComponentModel;
			var options = model.ObtainProxyOptions(false);

			Assert.IsNotNull(options);
			Assert.IsNotNull(options.Selector);
			Assert.IsNotNull(options.Hook);
			Assert.AreEqual(0, SelectAllSelector.Instances);
			Assert.AreEqual(0, ProxyAllHook.Instances);

			service = container.Resolve<CalculatorService>("ValidComponent");

			Assert.AreEqual(1, SelectAllSelector.Instances);
			Assert.AreEqual(0, SelectAllSelector.Calls);
			Assert.AreEqual(1, ProxyAllHook.Instances);

			service.Sum(2, 2);

			Assert.AreEqual(1, SelectAllSelector.Calls);
		}
#endif

		[Test]
		public void OnBehalfOfTest()
		{
			container.Register(Component.For(typeof(InterceptorWithOnBehalf)).Named("interceptor"));
			container.Register(Component.For(typeof(CalculatorService)).Named("key"));

			var service = container.Resolve<CalculatorService>("key");

			Assert.IsNotNull(service);
			Assert.AreEqual(4, service.Sum(2, 2));
			Assert.IsNotNull(InterceptorWithOnBehalf.Model);
			Assert.AreEqual("key", InterceptorWithOnBehalf.Model.Name);
			Assert.AreEqual(typeof(CalculatorService),
			                InterceptorWithOnBehalf.Model.Implementation);
		}

		[Test]
		public void OpenGenericInterceporIsUsedAsClosedGenericInterceptor()
		{
			container.Register(Component.For(typeof(GenericInterceptor<>)));
			container.Register(Component.For(typeof(CalculatorService)).Interceptors<GenericInterceptor<object>>());

			var service = container.Resolve<CalculatorService>();

			Assert.IsNotNull(service);
			Assert.AreEqual(4, service.Sum(2, 2));
		}

		[Test]
		public void ClosedGenericInterceptor()
		{
			container.Register(Component.For(typeof(GenericInterceptor<object>)));
			container.Register(Component.For(typeof(CalculatorService)).Interceptors<GenericInterceptor<object>>());

			var service = container.Resolve<CalculatorService>();

			Assert.IsNotNull(service);
			Assert.AreEqual(4, service.Sum(2, 2));
		}

		[Test]
		public void ClassProxyWithAttributes()
		{
			container = new WindsorContainer(); // So we wont use the facilities

			container.Register(Component.For<ResultModifierInterceptor>(),
			                   Component.For<CalculatorServiceWithAttributes>());

			var service = container.Resolve<CalculatorServiceWithAttributes>();

			Assert.IsNotNull(service);
			Assert.AreEqual(5, service.Sum(2, 2));
		}

		[Test]
		public void Multithreaded()
		{
			container.Register(Component.For(typeof(ResultModifierInterceptor)).Named("interceptor"));
			container.Register(Component.For(typeof(CalculatorService)).Named("key"));

			service = container.Resolve<CalculatorService>("key");

			const int threadCount = 10;

			var threads = new Thread[threadCount];

			for (var i = 0; i < threadCount; i++)
			{
				threads[i] = new Thread(ExecuteMethodUntilSignal);
				threads[i].Start();
			}

			startEvent.Set();

			Thread.CurrentThread.Join(2000);

			stopEvent.Set();
		}

		[Test]
		public void AutomaticallyOmitTarget()
		{
			container.Register(
				Component.For<ICalcService>()
					.Interceptors(InterceptorReference.ForType<ReturnDefaultInterceptor>()).Last,
				Component.For<ReturnDefaultInterceptor>()
				);

			var calcService = container.Resolve<ICalcService>();
			Assert.AreEqual(0, calcService.Sum(1, 2));
		}

		private void ExecuteMethodUntilSignal()
		{
			startEvent.WaitOne(int.MaxValue);

			while (!stopEvent.WaitOne(1))
			{
				Assert.AreEqual(5, service.Sum(2, 2));
				Assert.AreEqual(6, service.Sum(3, 2));
				Assert.AreEqual(8, service.Sum(3, 4));
			}
		}

#if !SILVERLIGHT
		private ConfigurationInstaller XmlResource(string fileName)
		{
			return Configuration.FromXml(Xml.Embedded(fileName));
		}
#endif
	}

	public class MyInterceptorGreedyFacility : IFacility
	{
		public void Init(IKernel kernel, Core.Configuration.IConfiguration facilityConfig)
		{
			kernel.ComponentRegistered += OnComponentRegistered;
		}

		public void Terminate()
		{
		}

		private void OnComponentRegistered(String key, IHandler handler)
		{
			if (key == "key")
			{
				handler.ComponentModel.Interceptors.Add(
					new InterceptorReference("interceptor"));
			}
		}
	}

	public class MyInterceptorGreedyFacility2 : IFacility
	{
		public void Init(IKernel kernel, Core.Configuration.IConfiguration facilityConfig)
		{
			kernel.ComponentRegistered += OnComponentRegistered;
		}

		public void Terminate()
		{
		}

		private void OnComponentRegistered(String key, IHandler handler)
		{
			if (handler.ComponentModel.Services.Any(s => s.Is<IInterceptor>()))
			{
				return;
			}

			handler.ComponentModel.Interceptors.Add(new InterceptorReference("interceptor"));
		}
	}
}