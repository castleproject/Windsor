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

#if !SILVERLIGHT // we do not support xml config on SL

namespace Castle.Windsor.Tests.Configuration2
{
	using System;
	using System.Threading;
	using Castle.Windsor.Configuration.Interpreters;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture, Explicit]
	public class SynchronizationProblemTestCase
	{
		private WindsorContainer container;
		private ManualResetEvent startEvent = new ManualResetEvent(false);
		private ManualResetEvent stopEvent = new ManualResetEvent(false);

		[SetUp]
		public void Init()
		{
			container = new WindsorContainer(new XmlInterpreter(ConfigHelper.ResolveConfigPath("Configuration2/synchtest_config.xml")));

			container.Resolve(typeof(ComponentWithConfigs));
		}

		[TearDown]
		public void Terminate()
		{
			container.Dispose();
		}

		[Test]
		public void ResolveWithConfigTest()
		{
			const int threadCount = 50;

			var threads = new Thread[threadCount];

			for (int i = 0; i < threadCount; i++)
			{
				threads[i] = new Thread(ExecuteMethodUntilSignal);
				threads[i].Start();
			}

			startEvent.Set();

			Thread.CurrentThread.Join(10 * 2000);

			stopEvent.Set();
		}

		private void ExecuteMethodUntilSignal()
		{
			startEvent.WaitOne(int.MaxValue);

			while (!stopEvent.WaitOne(1))
			{
				try
				{
					ComponentWithConfigs comp = (ComponentWithConfigs) container.Resolve(typeof(ComponentWithConfigs));

					Assert.AreEqual(AppContext.BaseDirectory, comp.Name);
					Assert.AreEqual(90, comp.Port);
					Assert.AreEqual(1, comp.Dict.Count);
				}
				catch(Exception ex)
				{
					Console.WriteLine(DateTime.Now.Ticks + " ---------------------------" + Environment.NewLine + ex);
				}
			}
		}
	}
}

#endif