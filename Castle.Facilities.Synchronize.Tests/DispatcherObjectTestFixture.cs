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

[assembly: NUnit.Framework.RequiresSTA]

namespace Castle.Facilities.Synchronize.Tests
{
	using System;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Threading;
	using Castle.Core.Configuration;
	using Castle.Facilities.Synchronize.Tests.Components;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using NUnit.Framework;

	[TestFixture, Ignore("Need to support multiple Applications")]
	public class DispatcherObjectTestFixture
	{
		private Application application;
		private IWindsorContainer container;
		private Exception uncaughtException;

		[SetUp]
		public void SetUp()
		{
			application = new Application();

			uncaughtException = null;
			container = new WindsorContainer();

			container.AddFacility<SynchronizeFacility>()
				.Register(Component.For<SynchronizationContext>(),
						  Component.For<AsynchronousContext>(),
						  Component.For<DispatcherSynchronizationContext>(),
						  Component.For<DummyWindow>().Named("Dummy")
							.Activator<DummyFormActivator>(),
						  Component.For<IDummyWindow>().ImplementedBy<DummyWindow>(),
						  Component.For<ClassUsingWindowInWindowsContext>(),
						  Component.For<ClassUsingWindowInAmbientContext>(),
						  Component.For(typeof(IClassUsingDepedenecyContext<>)).ImplementedBy(typeof(ClassUsingDispatcherContext<>)),
						  Component.For<IWorker>().ImplementedBy<SimpleWorker>(),
						  Component.For<IWorkerWithOuts>().ImplementedBy<AsynchronousWorker>(),
						  Component.For<ManualWorker>()
						  );

			var componentNode = new MutableConfiguration("component");
			componentNode.Attributes[Constants.SynchronizedAttrib] = "true";
			var synchronizeNode = new MutableConfiguration("synchronize");
			synchronizeNode.Attributes["contextType"] = typeof(DispatcherSynchronizationContext).AssemblyQualifiedName;
			var doWorkMethod = new MutableConfiguration("method");
			doWorkMethod.Attributes["name"] = "DoWork";
			doWorkMethod.Attributes["contextType"] = typeof(DispatcherSynchronizationContext).AssemblyQualifiedName;
			synchronizeNode.Children.Add(doWorkMethod);
			componentNode.Children.Add(synchronizeNode);

			container.Kernel.ConfigurationStore.AddComponentConfiguration("class.needing.context", componentNode);
			container.Register(Component.For<ClassUsingWindow>().Named("class.needing.context"));
		}

		[TearDown]
		public void TearDown()
		{
			application.Shutdown();
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void AddControl_DifferentThread_ThrowsException()
		{
			DummyWindow window = null;
			ExecuteInThread(() => window = new DummyWindow());
			window.AddControl(new Button());
		}

		[Test]
		public void AddControl_DifferentThread_Using_Context_ThrowsException()
		{
			var window = new DummyWindow();
			var classInCtx = new ClassUsingWindowInWindowsContext();
			ExecuteInThread(() => classInCtx.DoWork(window.Panel));
			Assert.IsNotNull(uncaughtException, "Expected an exception");
		}

		[Test]
		public void AddControl_DifferentThreadUsingClass_WorksFine()
		{
			DummyWindow window = null;
			ExecuteInThread(() =>
			{
				window = container.Resolve<DummyWindow>();
			});
			Assert.AreEqual(0, window.Panel.Children.Count);
			var count = window.AddControl(new Button());
			Assert.AreEqual(1, count);
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void AddControl_DifferentThreadUsingService_WorksFine()
		{
			IDummyWindow window = null;
			ExecuteInThread(() =>
			{
				window = container.Resolve<IDummyWindow>();
			});
			var count = window.AddControl(new Button());
			Assert.AreEqual(1, count);
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void AddControl_DifferentThreadInContext_WorksFine()
		{
			var window = new DummyWindow();
			var client = container.Resolve<ClassUsingWindowInWindowsContext>();
			ExecuteInThread(() => { client.DoWork(window.Panel); });
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void AddControl_DifferentThreadInContextUsingConfiguration_WorksFine()
		{
			var window = new DummyWindow();
			var client = container.Resolve<ClassUsingWindow>();
			ExecuteInThread(() => client.DoWork(window.Panel));
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void DoWorkGeneric_DifferentThreadInContext_WorksFine()
		{
			var window = new DummyWindow();
			var client = container.Resolve<IClassUsingDepedenecyContext<Panel>>();
			ExecuteInThread(() => Assert.AreEqual(window, client.DoWork(window.Panel)));
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void AddControl_DifferentThreadInContextUsingConfiguration_UsesAmbientContext()
		{
			var window = new DummyWindow();
			var client = container.Resolve<ClassUsingWindowInAmbientContext>();
			ExecuteInThread(() =>
			{
				var dispatcherCtx = new DispatcherSynchronizationContext();
				SynchronizationContext.SetSynchronizationContext(dispatcherCtx);
				client.DoWork(window.Panel);
			});
			Assert.IsNull(uncaughtException, "Expected no exception");
		}

		[Test]
		public void GetResultOf_UsingSynchronousContext_WorksFine()
		{
			var worker = container.Resolve<IWorker>();
			var remaining = Result.Of(worker.DoWork(2));
			Assert.AreEqual(4, remaining.End());
		}

		[Test]
		public void GetResultOf_UsingSynchronousContext_CallsCallback()
		{
			bool called = false;
			var worker = container.Resolve<IWorker>();
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
		public void GetResultOf_UsingAsynchronousContext_WorksFine()
		{
			var worker = container.Resolve<IWorkerWithOuts>();
			var remaining = Result.Of(worker.DoWork(5));
			Assert.AreEqual(10, remaining.End());
			Assert.IsFalse(remaining.CompletedSynchronously);
		}

		[Test]
		public void GetResultOf_WaitingForResult_WorksFine()
		{
			var worker = container.Resolve<ManualWorker>();
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
		public void GetResultOf_UsingAsyncSynchronousContextWithOuts_CallsCallback()
		{
			int passed;
			string batch = "bar";
			var called = new ManualResetEvent(false);
			var worker = container.Resolve<ManualWorker>();
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
			string batch = "bar";
			var called = new ManualResetEvent(false);
			var worker = container.Resolve<ManualWorker>();
			var result = Result.Of(worker.DoWork(20, ref batch, out passed));
			Assert.AreEqual(40, result.End(out batch));
			Assert.AreEqual("foo", batch);
			Assert.AreEqual(2, result.OutValues.Length);
			Assert.AreEqual(1, result.UnboundOutValues.Length);
			Assert.AreEqual("foo", result.GetOutArg<string>(0));
			Assert.AreEqual(10, result.GetOutArg<int>(1));
			Assert.AreEqual(10, result.GetUnboundOutArg<int>(0));
		}

		[Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Bad Bad Bad...")]
		public void GetResultOf_WaitingForResultThrowsException_WorksFine()
		{
			var worker = container.Resolve<ManualWorker>();
			var remaining = Result.Of(worker.DoWork(5));
			ExecuteInThread(() => worker.Failed(new ArgumentException("Bad Bad Bad...")));
			Assert.AreEqual(10, remaining.End());
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void GetResultOf_NotWithAnySynchronizationContext_ThrowsInvalidOperationException()
		{
			var worker = new AsynchronousWorker();
			var remaining = Result.Of(worker.DoWork(2));
			Assert.AreEqual(4, remaining);
		}

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

				application.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
				application.Dispatcher.Invoke(new Action(() => application.Shutdown()));
			});
			thread.SetApartmentState(ApartmentState.STA);

			application.Dispatcher.BeginInvoke(new ThreadStart(() => thread.Start()));
			application.Run();
		}
	}
}