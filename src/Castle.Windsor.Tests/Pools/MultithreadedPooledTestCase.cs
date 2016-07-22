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

namespace Castle.MicroKernel.Tests.Pools
{
	using System.Threading;

	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class MultithreadedPooledTestCase
	{
		private readonly ManualResetEvent startEvent = new ManualResetEvent(false);
		private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);
		private IKernel kernel;

		public void ExecuteMethodUntilSignal()
		{
			startEvent.WaitOne(int.MaxValue);

			while (!stopEvent.WaitOne(1))
			{
				var instance = kernel.Resolve<PoolableComponent1>("a");

				Assert.IsNotNull(instance);

				Thread.Sleep(1*500);

				kernel.ReleaseComponent(instance);
			}
		}

		[Test]
		public void Multithreaded()
		{
			kernel = new DefaultKernel();
			kernel.Register(Component.For(typeof(PoolableComponent1)).Named("a"));

			const int threadCount = 15;

			var threads = new Thread[threadCount];

			for (var i = 0; i < threadCount; i++)
			{
				threads[i] = new Thread(ExecuteMethodUntilSignal);
				threads[i].Start();
			}

			startEvent.Set();

			Thread.CurrentThread.Join(3*1000);

			stopEvent.Set();
		}
	}
}