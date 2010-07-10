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

namespace Castle.MicroKernel.Tests.Lifecycle
{
	using System;

	using Castle.Core;
	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.Lifecycle.Components;
	using NUnit.Framework;

	/// <summary>
	/// Summary description for LifecycleTestCase.
	/// </summary>
	[TestFixture]
	public class LifecycleTestCase
	{
		private IKernel kernel;

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[Test]
		public void InitializeLifecycle()
		{
			kernel.Register(Component.For<HttpFakeServer>());

			var server = kernel.Resolve<HttpFakeServer>();

			Assert.IsTrue(server.IsInitialized);
		}

		[Test]
		public void DisposableLifecycle()
		{
			kernel.Register(Component.For<HttpFakeServer>());
			
			var handler = kernel.GetHandler(typeof(HttpFakeServer));
			var server = (HttpFakeServer) handler.Resolve(CreationContext.Empty);

			handler.Release(server);

			Assert.IsTrue(server.IsDisposed);
		}

		[Test]
		public void Works_when_method_has_overloads()
		{
			kernel.AddFacility<StartableFacility>();
			kernel.Register(Component.For<WithOverloads>()
			                	.StartUsingMethod("Start")
			                	.StopUsingMethod("Stop"));
			var c = kernel.Resolve<WithOverloads>();
			Assert.IsTrue(c.StartCalled);
			kernel.ReleaseComponent(c);
			Assert.IsTrue(c.StopCalled);
		}

	}

	[Transient]
	public class WithOverloads
	{
		public void Start()
		{
			StartCalled = true;
		}

		public bool StartCalled { get; set; }

		public void Start(int fake)
		{
			
		}
		public void Stop()
		{
			StopCalled = true;
		}

		public bool StopCalled { get; set; }

		public void Stop(string fake)
		{
			
		}
	}
}
