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

namespace CastleTests.Facilities.Synchronize
{
#if !SILVERLIGHT
	using System;
	using System.Configuration;
	using System.Threading;
	using System.Windows.Forms;

	using Castle.Core.Configuration;
	using Castle.Facilities.Synchronize;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using CastleTests.Facilities.Synchronize.Components;

	using NUnit.Framework;

	[TestFixture]
	public class SynchronizeFacilityTestCase : AbstractContainerTestCase
	{
		protected override WindsorContainer BuildContainer()
		{
			uncaughtException = null;
			var container = new WindsorContainer();

			container.AddFacility<SynchronizeFacility>()
				.Register(Component.For<SynchronizationContext>(),
				          Component.For<AsynchronousContext>(),
				          Component.For<DummyForm>().Named("Dummy").Activator<DummyFormActivator>(),
				          Component.For<IDummyForm>().ImplementedBy<DummyForm>(),
				          Component.For<ClassUsingFormInWindowsContext>(),
				          Component.For<ClassUsingFormInAmbientContext>(),
				          Component.For<SyncClassWithoutContext>(),
				          Component.For<SyncClassOverrideContext>(),
				          Component.For(typeof(IClassUsingContext<>)).ImplementedBy(typeof(ClassUsingContext<>)),
				          Component.For<IWorker>().ImplementedBy<SimpleWorker>(),
				          Component.For<IWorkerWithOuts>().ImplementedBy<AsynchronousWorker>(),
				          Component.For<ManualWorker>()
				);

			var componentNode = new MutableConfiguration("component");
			componentNode.Attributes[Constants.SynchronizedAttrib] = "true";
			var synchronizeNode = new MutableConfiguration("synchronize");
			synchronizeNode.Attributes["contextType"] = typeof(WindowsFormsSynchronizationContext).AssemblyQualifiedName;
			var doWorkMethod = new MutableConfiguration("method");
			doWorkMethod.Attributes["name"] = "DoWork";
			doWorkMethod.Attributes["contextType"] = typeof(WindowsFormsSynchronizationContext).AssemblyQualifiedName;
			synchronizeNode.Children.Add(doWorkMethod);
			componentNode.Children.Add(synchronizeNode);

			container.Kernel.ConfigurationStore.AddComponentConfiguration("class.needing.context", componentNode);
			container.Register(Component.For<ClassUsingForm>().Named("class.needing.context"));
			return container;
		}

		private Exception uncaughtException;

		private void ExecuteInThread(ThreadStart run)
		{
			var thread = new Thread(() =>
			{
				try
				{
					run();
				}
				catch (Exception e)
				{
					uncaughtException = e;
				}

				Application.DoEvents();
				Application.Exit();
			});

			var form = new Form();
			if (form.Handle == IntPtr.Zero)
			{
				throw new InvalidOperationException("Control handle could not be obtained");
			}
			form.BeginInvoke((MethodInvoker)delegate { thread.Start(); });

			Application.Run();
		}

		[Test]
		[ExpectedException(typeof(FacilityException))]
		public void AddContextComponent_WithoutVirtualMethod_ThrowsFacilityException()
		{
			Container.Register(Component.For<ClassInContextWithoutVirtualMethod>().Named("class.in.context.bad"));
		}

		[Test]
		public void AddControl_DifferentThreadInContextUsingConfiguration_UsesAmbientContext()
		{
			var form = new DummyForm();
			var client = Container.Resolve<ClassUsingFormInAmbientContext>();
			ExecuteInThread(() =>
			{
				using (var winCtx = new WindowsFormsSynchronizationContext())
				{
					SynchronizationContext.SetSynchronizationContext(winCtx);
					client.DoWork(form);
				}
			});
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void AddControl_DifferentThreadInContextUsingConfiguration_WorksFine()
		{
			var form = new DummyForm();
			var client = Container.Resolve<ClassUsingForm>();
			ExecuteInThread(() => client.DoWork(form));
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void AddControl_DifferentThreadInContext_WorksFine()
		{
			var form = new DummyForm();
			var client = Container.Resolve<ClassUsingFormInWindowsContext>();
			ExecuteInThread(() => { client.DoWork(form); });
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void AddControl_DifferentThreadUsingClass_WorksFine()
		{
			var form = Container.Resolve<DummyForm>();
			Assert.AreEqual(0, form.Controls.Count);
			ExecuteInThread(() =>
			{
				var count = form.AddControl(new Button());
				Assert.AreEqual(1, count);
			});
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void AddControl_DifferentThreadUsingService_WorksFine()
		{
			var form = Container.Resolve<IDummyForm>();
			ExecuteInThread(() =>
			{
				var count = form.AddControl(new Button());
				Assert.AreEqual(1, count);
			});
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void AddControl_DifferentThread_ThrowsException()
		{
			var form = new DummyForm();
			ExecuteInThread(() => form.AddControl(new Button()));
			Assert.IsNotNull(uncaughtException, "Expected an exception");

			uncaughtException = null;

			var classInCtx = new ClassUsingFormInWindowsContext();
			ExecuteInThread(() => classInCtx.DoWork(form));
			Assert.IsNotNull(uncaughtException, "Expected an exception");
		}

		[Test]
		public void DoWorkGeneric_DifferentThreadInContext_WorksFine()
		{
			var form = new DummyForm();
			var client = Container.Resolve<IClassUsingContext<DummyForm>>();
			ExecuteInThread(() => Assert.AreEqual(form, client.DoWork(form)));
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetResultOf_NotWithAnySynchronizationContext_ThrowsInvalidOperationException()
		{
			var sync = Container.Resolve<SyncClassOverrideContext>();
			sync.DoWork();

			var worker = new AsynchronousWorker();
			var remaining = Result.Of(worker.DoWork(2));
			Assert.AreEqual(4, remaining);
		}

		[Test]
		public void GetResultOf_UsingAsyncSynchronousContextWithOuts_CallsCallback()
		{
			int passed;
			var batch = "bar";
			var called = new ManualResetEvent(false);
			var worker = Container.Resolve<ManualWorker>();
			var result = Result.Of(worker.DoWork(20, ref batch, out passed),
			                       (Result<int> remaining) =>
			                       {
			                       	Assert.AreEqual(40, remaining.End(out batch, out passed));
			                       	Assert.AreEqual(10, passed);
			                       	Assert.AreEqual("foo", batch);
			                       	Assert.AreSame(worker, remaining.AsyncState);
			                       	Assert.IsFalse(remaining.CompletedSynchronously);
			                       	called.Set();
			                       }, worker);
			Assert.IsTrue(called.WaitOne(5000, false));
		}

		[Test]
		public void GetResultOf_UsingAsyncSynchronousContextWithOuts_CanAccessOuts()
		{
			int passed;
			var batch = "bar";
			var called = new ManualResetEvent(false);
			var worker = Container.Resolve<ManualWorker>();
			var result = Result.Of(worker.DoWork(20, ref batch, out passed));
			Assert.AreEqual(40, result.End(out batch));
			Assert.AreEqual("foo", batch);
			Assert.AreEqual(2, result.OutValues.Length);
			Assert.AreEqual(1, result.UnboundOutValues.Length);
			Assert.AreEqual("foo", result.GetOutArg<string>(0));
			Assert.AreEqual(10, result.GetOutArg<int>(1));
			Assert.AreEqual(10, result.GetUnboundOutArg<int>(0));
		}

		[Test]
		public void GetResultOf_UsingAsynchronousContext_WorksFine()
		{
			var worker = Container.Resolve<IWorkerWithOuts>();
			var remaining = Result.Of(worker.DoWork(5));
			Assert.AreEqual(10, remaining.End());
			Assert.IsFalse(remaining.CompletedSynchronously);
		}

		[Test]
		public void GetResultOf_UsingSynchronousContext_CallsCallback()
		{
			var called = false;
			var worker = Container.Resolve<IWorker>();
			var result = Result.Of(worker.DoWork(2), (Result<int> remaining) =>
			{
				Assert.AreEqual(4, remaining.End());
				Assert.AreSame(worker, remaining.AsyncState);
				Assert.IsTrue(remaining.CompletedSynchronously);
				called = true;
			}, worker);
			Assert.IsTrue(called);
		}

		[Test]
		public void GetResultOf_UsingSynchronousContext_WorksFine()
		{
			var worker = Container.Resolve<IWorker>();
			var remaining = Result.Of(worker.DoWork(2));
			Assert.AreEqual(4, remaining.End());
		}

		[Test]
		[ExpectedException(typeof(ArgumentException), ExpectedMessage = "Bad Bad Bad...")]
		public void GetResultOf_WaitingForResultThrowsException_WorksFine()
		{
			var worker = Container.Resolve<ManualWorker>();
			var remaining = Result.Of(worker.DoWork(5));
			ExecuteInThread(() => worker.Failed(new ArgumentException("Bad Bad Bad...")));
			Assert.AreEqual(10, remaining.End());
		}

		[Test]
		public void GetResultOf_WaitingForResult_WorksFine()
		{
			var worker = Container.Resolve<ManualWorker>();
			var remaining = Result.Of(worker.DoWork(5));
			ExecuteInThread(() =>
			{
				Thread.Sleep(1000);
				worker.Ready();
			});
			Assert.AreEqual(10, remaining.End());
			Assert.IsFalse(remaining.CompletedSynchronously);
		}

		[Test]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void RegisterFacility_WithBadControlProxyHook_ThrowsConfigurationException()
		{
			var type = typeof(SynchronizeFacility).FullName;
			var container2 = new WindsorContainer();
			var facNode = new MutableConfiguration("facility");
			facNode.Attributes["type"] = type;
			facNode.Attributes[Constants.ControlProxyHookAttrib] = typeof(string).AssemblyQualifiedName;
			container2.Kernel.ConfigurationStore.AddFacilityConfiguration(type, facNode);
			container2.AddFacility(new SynchronizeFacility());
		}

		[Test]
		public void RegisterFacility_WithControlProxyHook_WorksFine()
		{
			var type = typeof(SynchronizeFacility).FullName;
			var container2 = new WindsorContainer();
			var facNode = new MutableConfiguration("facility");
			facNode.Attributes["type"] = type;
			facNode.Attributes[Constants.ControlProxyHookAttrib] = typeof(DummyProxyHook).AssemblyQualifiedName;
			container2.Kernel.ConfigurationStore.AddFacilityConfiguration(type, facNode);
			container2.AddFacility(new SynchronizeFacility());

			container2.Register(Component.For<DummyForm>().Named("dummy.form.class"));
			var model = container2.Kernel.GetHandler("dummy.form.class").ComponentModel;
			var options = model.ObtainProxyOptions(false);
			Assert.IsNotNull(options, "Proxy options should not be null");
			Assert.IsTrue(options.Hook.Resolve(container2.Kernel, CreationContext.CreateEmpty()) is DummyProxyHook,
			              "Proxy hook should be a DummyProxyHook");
		}

		[Test]
		public void RegisterFacility_With_missing_ControlProxyHook_ThrowsConfigurationException()
		{
			var type = typeof(SynchronizeFacility).FullName;
			var container2 = new WindsorContainer();
			var facNode = new MutableConfiguration("facility");
			facNode.Attributes["type"] = type;
			facNode.Attributes[Constants.ControlProxyHookAttrib] = "${missing.component}";
			container2.Kernel.ConfigurationStore.AddFacilityConfiguration(type, facNode);
			container2.AddFacility(new SynchronizeFacility());
			container2.Register(Component.For<DummyForm>());

			var exception = Assert.Throws<HandlerException>(() => container2.Resolve<DummyForm>());

			var expected =
				string.Format(
					"Can't create component 'CastleTests.Facilities.Synchronize.Components.DummyForm' as it has dependencies to be satisfied.{0}{0}'CastleTests.Facilities.Synchronize.Components.DummyForm' is waiting for the following dependencies:{0}- Component 'missing.component' (via override) which was not found. Did you forget to register it or misspelled the name? If the component is registered and override is via type make sure it doesn't have non-default name assigned explicitly or override the dependency via name.{0}",
					Environment.NewLine);
			Assert.AreEqual(expected, exception.Message);
		}

		[Test]
		[ExpectedException(typeof(HandlerException))]
		public void ResolveContextComponent_WithMissingDependency_ThrowsHandlerException()
		{
			Container.Register(Component.For<ClassInContextWithMissingDependency>().Named("class.in.context.bad"));
			Container.Resolve<ClassInContextWithMissingDependency>("class.in.context.bad");
		}

		[Test]
		public void SynchronizeContextReference_DifferentInstanceDifferentKey_AreNotEqual()
		{
			var sync1 = new SynchronizeContextReference("key1");
			var sync2 = new SynchronizeContextReference("key2");

			Assert.AreNotEqual(sync1, sync2);
			Assert.AreNotEqual(sync1.GetHashCode(), sync2.GetHashCode());
		}

		[Test]
		public void SynchronizeContextReference_DifferentInstanceDifferentType_AreNotEqual()
		{
			var sync1 = new SynchronizeContextReference(typeof(string));
			var sync2 = new SynchronizeContextReference(typeof(float));

			Assert.AreNotEqual(sync1, sync2);
			Assert.AreNotEqual(sync1.GetHashCode(), sync2.GetHashCode());
		}

		[Test]
		public void SynchronizeContextReference_DifferentInstanceSameKey_AreEqual()
		{
			var sync1 = new SynchronizeContextReference("key1");
			var sync2 = new SynchronizeContextReference("key1");

			Assert.AreEqual(sync1, sync2);
			Assert.AreEqual(sync1.GetHashCode(), sync2.GetHashCode());
		}

		[Test]
		public void SynchronizeContextReference_DifferentInstanceSameType_AreEqual()
		{
			var sync1 = new SynchronizeContextReference(typeof(string));
			var sync2 = new SynchronizeContextReference(typeof(string));

			Assert.AreEqual(sync1, sync2);
			Assert.AreEqual(sync1.GetHashCode(), sync2.GetHashCode());
		}

		[Test]
		public void Synchronize_NoContextSpecified_DoesNotUseContext()
		{
			var sync = Container.Resolve<SyncClassWithoutContext>();
			sync.DoWork();
		}

		[Test]
		public void Synchronize_OverrideContext_UsesContext()
		{
			var sync = Container.Resolve<SyncClassOverrideContext>();
			sync.DoWork();
		}
	}
#endif
}