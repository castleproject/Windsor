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

namespace Castle.Facilities.Logging.Tests
{
	
#if !SILVERLIGHT
	using System;
	using System.Diagnostics;
	using System.IO;

	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Facilities.Logging.Tests.Classes;

	using NUnit.Framework;

	[TestFixture]
	public class TraceFacilityTest : BaseTest
	{
		[SetUp]
		public void Setup()
		{
			container = base.CreateConfiguredContainer(LoggerImplementation.Trace);
			consoleWriter.GetStringBuilder().Length = 0;

			var source = new TraceSource("Default");
			foreach (TraceListener listener in source.Listeners)
			{
				var consoleListener = listener as ConsoleTraceListener;
				if (consoleListener != null)
				{
					consoleListener.Writer = consoleWriter;
				}
			}
		}

		[TearDown]
		public void Teardown()
		{
			if (container != null)
			{
				container.Dispose();
			}
		}

		private IWindsorContainer container;
		private readonly StringWriter consoleWriter = new StringWriter();

		[Test]
		public void SimpleTest()
		{
			container.Register(Component.For(typeof(SimpleLoggingComponent)).Named("component"));
			var test = container.Resolve<SimpleLoggingComponent>("component");

			var expectedLogOutput = String.Format("{0} Information: 0 : Hello world" + Environment.NewLine,
			                                      typeof(SimpleLoggingComponent).FullName);

			if (test != null)
			{
				test.DoSomething();
			}

			var actualLogOutput = consoleWriter.GetStringBuilder().ToString();
			Assert.AreEqual(expectedLogOutput, actualLogOutput);
		}
	}
#endif
}