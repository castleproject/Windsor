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

	using Castle.Facilities.Logging.Tests.Classes;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NLog;
	using NLog.Targets;

	using NUnit.Framework;

	/// <summary>
	/// Summary description for NLogFacilityTestts.
	/// </summary>
	[TestFixture]
	public class NLogFacilityTests : BaseTest
	{
		[SetUp]
		public void Setup()
		{
			container = base.CreateConfiguredContainer(LoggerImplementation.NLog);
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

		[Test]
		public void SimpleTest()
		{
			container.Register(Component.For(typeof(SimpleLoggingComponent)).Named("component"));
			var test = container.Resolve<SimpleLoggingComponent>("component");

			test.DoSomething();

			var expectedLogOutput = String.Format("|INFO|{0}|Hello world", typeof(SimpleLoggingComponent).FullName);
			var actualLogOutput = (LogManager.Configuration.FindTargetByName("memory") as MemoryTarget).Logs[0].ToString();
			actualLogOutput = actualLogOutput.Substring(actualLogOutput.IndexOf('|'));
			Assert.AreEqual(expectedLogOutput, actualLogOutput);
		}
	}
#endif
}