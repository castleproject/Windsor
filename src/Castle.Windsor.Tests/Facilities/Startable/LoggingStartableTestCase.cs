// Copyright 2004-2026 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Facilities.Startable
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class LoggingStartableTestCase
	{
		[SetUp]
		public void ClearLog()
		{
			LoggingStartable.Log.Clear();
		}

		[Test]
		public void Transient_startable_is_stopped_and_disposed_with_container()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>();
				container.Register(Component.For<LoggingStartable>().LifestyleTransient());
			}

			CollectionAssert.AreEqual(new[] { "start", "stop", "dispose" }, LoggingStartable.Log);
		}

		[Test]
		public void Transient_startable_is_stopped_and_disposed_with_container_when_start_is_deferred()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart());
				container.Register(Component.For<LoggingStartable>().LifestyleTransient());
			}

			CollectionAssert.AreEqual(new[] { "start", "stop", "dispose" }, LoggingStartable.Log);
		}

		[Test]
		public void Transient_startable_is_stopped_and_disposed_with_container_when_start_is_deferred_and_tried()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredTryStart());
				container.Register(Component.For<LoggingStartable>().LifestyleTransient());
			}

			CollectionAssert.AreEqual(new[] { "start", "stop", "dispose" }, LoggingStartable.Log);
		}

		[Test]
		public void Transient_startable_is_stopped_and_disposed_with_container_when_start_is_triggered_manually()
		{
			var flag = new StartFlag();
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>(f => f.DeferredStart(flag));
				container.Register(Component.For<LoggingStartable>().LifestyleTransient());

				flag.Signal();
			}

			CollectionAssert.AreEqual(new[] { "start", "stop", "dispose" }, LoggingStartable.Log);
		}

		[Test]
		public void Singleton_startable_is_stopped_and_disposed_once_with_container()
		{
			using (var container = new WindsorContainer())
			{
				container.AddFacility<StartableFacility>();
				container.Register(Component.For<LoggingStartable>().LifestyleSingleton());
			}

			CollectionAssert.AreEqual(new[] { "start", "stop", "dispose" }, LoggingStartable.Log);
		}

		public class LoggingStartable : IStartable, IDisposable
		{
			public static readonly List<string> Log = new List<string>();

			public void Start()
			{
				Log.Add("start");
			}

			public void Stop()
			{
				Log.Add("stop");
			}

			public void Dispose()
			{
				Log.Add("dispose");
			}
		}
	}
}
