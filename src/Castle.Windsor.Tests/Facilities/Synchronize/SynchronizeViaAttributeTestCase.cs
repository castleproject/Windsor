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
	using System.Reflection;
	using System.Threading;
	using System.Windows.Forms;

	using Castle.Facilities.Synchronize;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;

	using CastleTests.Facilities.Synchronize.Components;

	using NUnit.Framework;

	[TestFixture]
	public class SynchronizeViaAttributeTestCase : AbstractContainerTestCase
	{
		private static readonly MethodInfo preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace",
		                                                                                    BindingFlags.NonPublic | BindingFlags.Instance);

		protected override void AfterContainerCreated()
		{
			Container.AddFacility<SynchronizeFacility>();
		}

		private void ExecuteInThread(ThreadStart run)
		{
			Exception uncaughtException = null;
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
			var invoke = form.BeginInvoke((MethodInvoker)thread.Start);

			Application.Run();
			form.EndInvoke(invoke);
			if (uncaughtException != null)
			{
				preserveStackTrace.Invoke(uncaughtException, null);
				throw uncaughtException;
			}
		}

		[Test]
		public void Component_depending_on_named_and_typed_context_works_corrently_if_both_available()
		{
			Container.Register(Component.For<ClassInContextWithMissingDependency>(),
			                   Component.For<AsynchronousContext>().Named("foo"));
			var exception = Assert.Throws<HandlerException>(() => Container.Resolve<ClassInContextWithMissingDependency>());

			var message =
				string.Format(
					"Can't create component 'CastleTests.Facilities.Synchronize.Components.ClassInContextWithMissingDependency' as it has dependencies to be satisfied.{0}{0}'CastleTests.Facilities.Synchronize.Components.ClassInContextWithMissingDependency' is waiting for the following dependencies:{0}- Service 'System.Threading.SynchronizationContext' which was not registered.{0}",
					Environment.NewLine);

			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Component_depending_on_unregistered_context_via_name_throws()
		{
			Container.Register(Component.For<ClassInContextWithMissingDependency>(),
			                   Component.For<SynchronizationContext>());
			var exception = Assert.Throws<HandlerException>(() => Container.Resolve<ClassInContextWithMissingDependency>());

			var message =
				string.Format(
					"Can't create component 'CastleTests.Facilities.Synchronize.Components.ClassInContextWithMissingDependency' as it has dependencies to be satisfied.{0}{0}'CastleTests.Facilities.Synchronize.Components.ClassInContextWithMissingDependency' is waiting for the following dependencies:{0}- Parameter 'foo' which was not provided. Did you forget to set the dependency?{0}",
					Environment.NewLine);

			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Component_depending_on_unregistered_context_via_type_throws()
		{
			Container.Register(Component.For<AsynchronousWorker>());
			var exception = Assert.Throws<HandlerException>(() => Container.Resolve<AsynchronousWorker>());

			var messasge =
				string.Format(
					"Can't create component 'CastleTests.Facilities.Synchronize.Components.AsynchronousWorker' as it has dependencies to be satisfied.{0}{0}'CastleTests.Facilities.Synchronize.Components.AsynchronousWorker' is waiting for the following dependencies:{0}- Service 'CastleTests.Facilities.Synchronize.Components.AsynchronousContext' which was not registered.{0}",
					Environment.NewLine);

			Assert.AreEqual(messasge, exception.Message);
		}

		[Test]
		public void Component_depending_on_unregistered_context_via_type_throws_even_if_another_is_available()
		{
			Container.Register(Component.For<ClassInContextWithMissingDependency>(),
			                   Component.For<SynchronizationContext>().Named("foo").UsingFactoryMethod(k => k.Resolve<WindowsFormsSynchronizationContext>()));

			var instance = Container.Resolve<ClassInContextWithMissingDependency>();

			var form = new DummyForm();
			ExecuteInThread(() => instance.DoWork(form));
		}
	}
#endif
}