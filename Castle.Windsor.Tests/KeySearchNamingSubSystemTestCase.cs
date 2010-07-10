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

namespace Castle.MicroKernel.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Naming;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class KeySearchNamingSubSystemTestCase
	{
		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
		}

		private IKernel kernel;

		[Test]
		public void By_default_returns_first_component_for_resolved_type()
		{
			kernel.AddSubSystem(SubSystemConstants.NamingKey, new KeySearchNamingSubSystem());
			kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("1.common"),
			                Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("2.common"));

			var common = kernel.Resolve<ICommon>();

			Assert.IsInstanceOf<CommonImpl1>(common);
		}

		[Test]
		public void Delegate_filters_components_by_key()
		{
			kernel.AddSubSystem(
				SubSystemConstants.NamingKey,
				new KeySearchNamingSubSystem(key => key.StartsWith("castlestronghold.com")));

			kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("castleproject.org.common"),
			                Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("castlestronghold.com.common"));

			var common = kernel.Resolve<ICommon>();

			Assert.IsInstanceOf<CommonImpl2>(common);
		}

		[Test]
		public void Does_not_impact_ability_to_unregister_component()
		{
			kernel.AddSubSystem(SubSystemConstants.NamingKey,
			                    new KeySearchNamingSubSystem(key => key.StartsWith("2")));
			kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("1.common"),
			                Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("2.common"));

			var common = kernel.Resolve<ICommon>();
			Assert.IsInstanceOf<CommonImpl2>(common);

			kernel.RemoveComponent("2.common");

			common = kernel.Resolve<ICommon>();
			Assert.IsInstanceOf<CommonImpl1>(common);

			kernel.RemoveComponent("1.common");

			Assert.AreEqual(0, kernel.GetHandlers(typeof(ICommon)).Length);
		}

		[Test]
		public void Falls_back_to_returning_first_when_delegate_finds_no_match()
		{
			kernel.AddSubSystem(
				SubSystemConstants.NamingKey,
				new KeySearchNamingSubSystem(key => key.StartsWith("3")));

			kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("1.common"),
			                Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("2.common"));

			var common = kernel.Resolve<ICommon>();

			Assert.IsInstanceOf<CommonImpl1>(common);
		}

		[Test]
		public void MultiThreadedAddResolve([Values(100)] int threadCount)
		{
			var locker = new object();
			var list = new List<string>();
			var waitEvent = new ManualResetEvent(false);

			kernel.AddSubSystem(SubSystemConstants.NamingKey, new KeySearchNamingSubSystem());

			kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("common"));

			var count = threadCount;
			WaitCallback resolveThread = delegate
			{
				waitEvent.WaitOne();
				while (count > 0 && list.Count == 0)
				{
					try
					{
						kernel.Resolve<ICommon>();
					}
					catch (Exception e)
					{
						lock (locker)
						{
							list.Add(e.ToString());
						}
					}
				}
			};
			ThreadPool.QueueUserWorkItem(resolveThread);

			WaitCallback addThread = delegate
			{
				waitEvent.WaitOne();
				kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named(Guid.NewGuid() + ".common"));
				Interlocked.Decrement(ref threadCount);
			};
			for (var i = 0; i < threadCount; i++)
			{
				ThreadPool.QueueUserWorkItem(addThread);
			}

			waitEvent.Set();
			while (threadCount > 0 && list.Count == 0)
			{
				Thread.Sleep(15);
			}

			if (list.Count > 0)
			{
				Assert.Fail(list[0]);
			}
		}

		/// <summary>
		/// What I consider an edge case (only consistently reproducable with 1000+ threads):
		///   + Large number of components that implement the same service
		///   + Thread 1 Requests component by service
		///   + Thread n is simultaneously removing component
		///   + Predicate does not match any keys for the requested service OR
		///   + Matching component is removed before the Predicate can match it
		/// </summary>
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		[Ignore("This test needs to be reviewed")]
		public void MultiThreaded_RemoveResolve_Throws_When_LargeRatio_Of_ComponentsToService()
		{
			var threadCount = 1000;
			var list = new List<Exception>();
			var locker = new object();
			var rand = new Random();
			var waitEvent = new ManualResetEvent(false);

			IKernel kernel = new DefaultKernel();
			kernel.AddSubSystem(SubSystemConstants.NamingKey, new KeySearchNamingSubSystem(delegate { return false; }));

			kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named("common"));

			WaitCallback resolveThread = delegate
			{
				waitEvent.WaitOne();
				while (threadCount > 0 && list.Count == 0)
				{
					try
					{
						kernel.Resolve<ICommon>();
					}
					catch (Exception e)
					{
						lock (locker)
						{
							list.Add(e);
						}
					}
				}
			};
			ThreadPool.QueueUserWorkItem(resolveThread);

			WaitCallback removeThread = delegate
			{
				waitEvent.WaitOne();
				kernel.RemoveComponent(threadCount + ".common");
				Interlocked.Decrement(ref threadCount);
			};
			for (var i = 0; i < threadCount; i++)
			{
				kernel.Register(Component.For(typeof(ICommon)).ImplementedBy(typeof(CommonImpl1)).Named(i + ".common"));
				ThreadPool.QueueUserWorkItem(removeThread);
			}

			waitEvent.Set();
			while (threadCount > 0 && list.Count == 0)
			{
				Thread.Sleep(15);
			}

			if (list.Count > 0)
			{
				throw list[0];
			}
		}

		[Test]
		public void On_multiple_matches_returns_first()
		{
			kernel.AddSubSystem(
				SubSystemConstants.NamingKey,
				new KeySearchNamingSubSystem(key => key.StartsWith("1")));

			kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("1.common"),
			                Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("11.common"));

			var common = kernel.Resolve<ICommon>();

			Assert.IsInstanceOf<CommonImpl1>(common);
		}
	}
}