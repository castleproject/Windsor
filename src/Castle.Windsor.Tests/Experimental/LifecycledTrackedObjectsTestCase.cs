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

namespace Castle.Windsor.Tests.Experimental
{
#if !SILVERLIGHT
	using System;
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Releasers;
	using Castle.Windsor.Experimental.Debugging;
	using Castle.Windsor.Experimental.Debugging.Extensions;
	using Castle.Windsor.Experimental.Debugging.Primitives;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	public class LifecycledTrackedObjectsTestCase : AbstractContainerTestFixture
	{
		private DefaultDebuggingSubSystem subSystem;

		[Test]
		public void List_tracked_alive_instances()
		{
			Register<DisposableFoo>();
			var foo1 = Container.Resolve<DisposableFoo>();
			var foo2 = Container.Resolve<DisposableFoo>();

			var objects = GetTrackedObjects();
			var values = (Burden[])objects.Value;
			Assert.AreEqual(2, values.Length);
		}

		[Test]
		public void NoTrackingReleasePolicy_has_special_message()
		{
			Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
			Register<DisposableFoo>();
			var foo1 = Container.Resolve<DisposableFoo>();
			var objects = GetTrackedObjects();
			Assert.AreEqual("No objects are ever tracked", objects.Key);
			Assert.IsNull(objects.Value);
		}

		[Test]
		public void custom_ReleasePolicy_has_special_message()
		{
			Kernel.ReleasePolicy = new MyCustomReleasePolicy();
			Register<DisposableFoo>();
			var foo1 = Container.Resolve<DisposableFoo>();
			var objects = GetTrackedObjects();
			Assert.AreEqual("Not supported with MyCustomReleasePolicy", objects.Key);
			Assert.IsNull(objects.Value);
		}

		[Test]
		public void List_tracked_alive_instances_only()
		{
			Register<DisposableFoo>();
			var foo1 = Container.Resolve<DisposableFoo>();
			var foo2 = Container.Resolve<DisposableFoo>();
			Container.Release(foo1);

			var objects = GetTrackedObjects();
			var values = (Burden[])objects.Value;
			Assert.AreEqual(1, values.Length);
			Assert.AreSame(foo2, values.Single().Instance);
		}

		[Test]
		public void Present_even_when_no_objects_were_created()
		{
			var objects = GetTrackedObjects();
			Assert.IsNotNull(objects);
		}

		[SetUp]
		public void SetSubSystem()
		{
			subSystem = new DefaultDebuggingSubSystem();
			Kernel.AddSubSystem(SubSystemConstants.DebuggingKey, subSystem);
		}

		private DebuggerViewItem GetTrackedObjects()
		{
			return subSystem.SelectMany(e => e.Attach()).SingleOrDefault(i => i.Name == LifecycledTrackedObjects.Title);
		}

		private void Register<T>()
		{
			Container.Register(Component.For<T>().LifeStyle.Transient);
		}
	}

	public class MyCustomReleasePolicy : IReleasePolicy
	{
		public void Dispose()
		{
			
		}

		public IReleasePolicy CreateSubPolicy()
		{
			return this;
		}

		public bool HasTrack(object instance)
		{
			return false;
		}

		public void Release(object instance)
		{
		}

		public void Track(object instance, Burden burden)
		{
		}
	}
#endif
}