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


namespace Castle.Windsor.Tests
{
	using System;
	using System.Threading;

	using Castle.Core;
	using Castle.Core.Interceptor;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Components;

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

			container.AddFacility("1", new MyInterceptorGreedyFacility());
			container.AddFacility("2", new MyInterceptorGreedyFacility());
			container.AddFacility("3", new MyInterceptorGreedyFacility());
		}

		[TearDown]
		public void Terminate()
		{
			container.Dispose();
		}

		[Test]
		public void InterfaceProxy()
		{
			container.AddComponent("interceptor", typeof(ResultModifierInterceptor));
			container.AddComponent("key",typeof(ICalcService), typeof(CalculatorService));

			var service = container.Resolve<ICalcService>("key");

			Assert.IsNotNull(service);
			Assert.AreEqual(7, service.Sum(2, 2));
		}

		[Test]
		public void Interface_that_depends_on_service_it_is_intercepting()
		{
			container.AddComponent("interceptor", typeof(InterceptorThatCauseStackOverflow));
			container.Register(
				Component.For<ICameraService>().ImplementedBy<CameraService>()
					.Interceptors(new[] { new InterceptorReference(typeof(InterceptorThatCauseStackOverflow)), }).First,
				//because it has no interceptors, it is okay to resolve it...
				Component.For<ICameraService>().ImplementedBy<CameraService>().Named("okay to resolve")
				);
			container.Resolve<ICameraService>();
		}

		[Test]
		public void InterfaceProxyWithLifecycle()
		{
			container.AddComponent("interceptor", typeof(ResultModifierInterceptor));
			container.AddComponent("key", typeof(ICalcService), typeof(CalculatorServiceWithLifecycle));

			ICalcService service = (ICalcService) container.Resolve("key");

			Assert.IsNotNull(service);
			Assert.IsTrue(service.Initialized);
			Assert.AreEqual(7, service.Sum(2, 2));

			Assert.IsFalse(service.Disposed);

			container.Release(service);

			Assert.IsTrue(service.Disposed);
		}

		[Test]
		public void ClassProxy()
		{
			container.AddComponent("interceptor", typeof(ResultModifierInterceptor));
			container.AddComponent("key", typeof(CalculatorService));

			service = container.Resolve<CalculatorService>("key");

			Assert.IsNotNull(service);
			Assert.AreEqual(7, service.Sum(2, 2));
		}

#if (!SILVERLIGHT) //no xml in Silverlight

		[Test]
		public void Xml_validComponent_resolves_correctly()
		{
			container.Install(Windsor.Installer.Configuration.FromXmlFile(ConfigHelper.ResolveConfigPath("Interceptors.config")));
			service = container.Resolve<CalculatorService>("ValidComponent");

			Assert.IsNotNull(service);
			Assert.AreEqual(5, service.Sum(2, 2));
		}

		[Test]
		public void Xml_multiple_interceptors_resolves_correctly()
		{
			container.Install(Windsor.Installer.Configuration.FromXmlFile(ConfigHelper.ResolveConfigPath("InterceptorsMultiple.config")));
			service = container.Resolve<CalculatorService>("component");

			Assert.IsNotNull(service);
			Assert.AreEqual(10, service.Sum(2, 2));
		}

		[Test]
		public void Xml_Component_With_Non_Existing_Interceptor_throws()
		{
			container.Install(Windsor.Installer.Configuration.FromXmlFile(ConfigHelper.ResolveConfigPath("Interceptors.config")));
			Assert.Throws(typeof(HandlerException), () =>
				container.Resolve<CalculatorService>("ComponentWithNonExistingInterceptor"));
		}

		[Test]
		public void Xml_Component_With_Non_invalid_Interceptor_throws()
		{
			Assert.Throws(typeof(Exception), () =>
				container.Install(
					Windsor.Installer.Configuration.FromXmlFile(
						ConfigHelper.ResolveConfigPath("InterceptorsInvalid.config"))));
		}
#endif
		[Test]
		public void OnBehalfOfTest()
		{
			container.AddComponent("interceptor", typeof(InterceptorWithOnBehalf));
			container.AddComponent("key", typeof(CalculatorService));

			CalculatorService service =
				(CalculatorService) container.Resolve("key");

			Assert.IsNotNull(service);
			Assert.AreEqual(4, service.Sum(2, 2));
			Assert.IsNotNull(InterceptorWithOnBehalf.Model);
			Assert.AreEqual("key", InterceptorWithOnBehalf.Model.Name);
			Assert.AreEqual(typeof(CalculatorService),
			                InterceptorWithOnBehalf.Model.Implementation);
		}

		[Test]
		public void ClassProxyWithAttributes()
		{
			container = new WindsorContainer(); // So we wont use the facilities

			container.AddComponent("interceptor", typeof(ResultModifierInterceptor));
			container.AddComponent("key", typeof(CalculatorServiceWithAttributes));

			CalculatorServiceWithAttributes service =
				(CalculatorServiceWithAttributes) container.Resolve("key");

			Assert.IsNotNull(service);
			Assert.AreEqual(5, service.Sum(2, 2));
		}

		[Test]
		public void Multithreaded()
		{
			container.AddComponent("interceptor", typeof(ResultModifierInterceptor));
			container.AddComponent("key", typeof(CalculatorService));

			service = (CalculatorService) container.Resolve("key");

			const int threadCount = 10;

			Thread[] threads = new Thread[threadCount];

			for(int i = 0; i < threadCount; i++)
			{
				threads[i] = new Thread(new ThreadStart(ExecuteMethodUntilSignal));
				threads[i].Start();
			}

			startEvent.Set();

			Thread.CurrentThread.Join(1 * 2000);

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

			ICalcService calcService = container.Resolve<ICalcService>();
			Assert.AreEqual(0, calcService.Sum(1, 2));
		}

		public void ExecuteMethodUntilSignal()
		{
			startEvent.WaitOne(int.MaxValue);

			while(!stopEvent.WaitOne(1))
			{
				Assert.AreEqual(7, service.Sum(2, 2));
				Assert.AreEqual(8, service.Sum(3, 2));
				Assert.AreEqual(10, service.Sum(3, 4));
			}
		}
	}

	public class MyInterceptorGreedyFacility : IFacility
	{
		#region IFacility Members

		public void Init(IKernel kernel, Core.Configuration.IConfiguration facilityConfig)
		{
			kernel.ComponentRegistered += new ComponentDataDelegate(OnComponentRegistered);
		}

		public void Terminate()
		{
		}

		#endregion

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
		#region IFacility Members

		public void Init(IKernel kernel, Core.Configuration.IConfiguration facilityConfig)
		{
			kernel.ComponentRegistered += new ComponentDataDelegate(OnComponentRegistered);
		}

		public void Terminate()
		{
		}

		#endregion

		private void OnComponentRegistered(String key, IHandler handler)
		{
			if (typeof(IInterceptor).IsAssignableFrom(handler.ComponentModel.Service))
			{
				return;
			}

			handler.ComponentModel.Interceptors.Add(new InterceptorReference("interceptor"));
		}
	}

	public class InterceptorThatCauseStackOverflow : IInterceptor
	{

		public InterceptorThatCauseStackOverflow(ICameraService service)
		{
		}

		public void Intercept(IInvocation invocation)
		{

		}
	}

	public class ResultModifierInterceptor : IInterceptor
	{
		private readonly int? returnValue;

		public ResultModifierInterceptor()
		{
		}

		public ResultModifierInterceptor(int returnValue)
		{
			this.returnValue = returnValue;
		}

		public void Intercept(IInvocation invocation)
		{
			if (invocation.Method.Name.Equals("Sum"))
			{
				invocation.Proceed();
				object result = invocation.ReturnValue;
				if (!returnValue.HasValue)
				{
					invocation.ReturnValue = ((int)result) + 1;
					return;
				}
				invocation.ReturnValue = returnValue.Value;
				return;
			}

			invocation.Proceed();
		}
	}

	public class InterceptorWithOnBehalf : IInterceptor, IOnBehalfAware
	{
		private static ComponentModel _model;

		#region IMethodInterceptor Members

		public void Intercept(IInvocation invocation)
		{
			invocation.Proceed();
		}

		#endregion

		public void SetInterceptedComponentModel(ComponentModel target)
		{
			_model = target;
		}

		public static ComponentModel Model
		{
			get { return _model; }
		}
	}

	public class ReturnDefaultInterceptor : IInterceptor
	{
		public void Intercept(IInvocation invocation)
		{
			if (invocation.Method.ReturnType.IsValueType)
			{
				invocation.ReturnValue = Activator.CreateInstance(invocation.Method.ReturnType);
			}
		}
	}
}