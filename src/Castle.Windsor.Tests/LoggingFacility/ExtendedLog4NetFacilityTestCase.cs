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

namespace Castle.Facilities.Logging.Tests
{
#if !SILVERLIGHT && !CLIENTPROFILE
	using System;
	using System.IO;
	using Castle.Facilities.Logging.Tests.Classes;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using log4net;
	using log4net.Appender;
	using log4net.Layout;
	using log4net.Repository.Hierarchy;
	using NUnit.Framework;

	/// <summary>
	/// Summary description for ExtendedLog4NetFacilityTests.
	/// </summary>
	[TestFixture]
	public class ExtendedLog4NetFacilityTestCase : BaseTest
	{
		private IWindsorContainer container;

		[SetUp]
		public void Setup()
		{
			container = base.CreateConfiguredContainer(LoggerImplementation.ExtendedLog4net);
		}

		[TearDown]
		public void Teardown()
		{
			container.Dispose();
		}

		[Test]
		public void SimpleTest()
		{
			container.Register(Component.For(typeof(SimpleLoggingComponent)).Named("component1"));
			SimpleLoggingComponent test = container.Resolve<SimpleLoggingComponent>("component1");

			test.DoSomething();

			String expectedLogOutput = String.Format("[INFO ] [{0}] - Hello world" + Environment.NewLine, typeof(SimpleLoggingComponent).FullName);
			MemoryAppender memoryAppender = ((Hierarchy)LogManager.GetRepository()).Root.GetAppender("memory") as MemoryAppender;
			TextWriter actualLogOutput = new StringWriter();
			PatternLayout patternLayout = new PatternLayout("[%-5level] [%logger] - %message%newline");
			patternLayout.Format(actualLogOutput, memoryAppender.GetEvents()[0]);

			Assert.AreEqual(expectedLogOutput, actualLogOutput.ToString());

			container.Register(Component.For(typeof(SmtpServer)).Named("component2"));
			ISmtpServer smtpServer = container.Resolve<ISmtpServer>("component2");

			smtpServer.Start();
			smtpServer.InternalSend("rbellamy@pteradigm.com", "jobs@castlestronghold.com", "We're looking for a few good porgrammars.");
			smtpServer.Stop();

			expectedLogOutput = String.Format("[DEBUG] [Castle.Facilities.Logging.Tests.Classes.SmtpServer] - Stopped" + Environment.NewLine, typeof(SimpleLoggingComponent).FullName);
			memoryAppender = ((Hierarchy)LogManager.GetRepository()).Root.GetAppender("memory") as MemoryAppender;
			actualLogOutput = new StringWriter();
			patternLayout = new PatternLayout("[%-5level] [%logger] - %message%newline");

			Assert.AreEqual(memoryAppender.GetEvents().Length, 4);

			patternLayout.Format(actualLogOutput, memoryAppender.GetEvents()[3]);

			Assert.AreEqual(expectedLogOutput, actualLogOutput.ToString());
		}

		[Test]
		public void ContextTest()
		{
			container.Register(Component.For<ComplexLoggingComponent>().Named("component1"));
			var complexLoggingComponent = container.Resolve<ComplexLoggingComponent>("component1");

			complexLoggingComponent.DoSomeContextual();

			var expectedLogOutput = String.Format("[DEBUG] [Castle.Facilities.Logging.Tests.Classes.ComplexLoggingComponent] [Outside Inside0] [bar] [flam] - Bim, bam boom." + Environment.NewLine, typeof(SimpleLoggingComponent).FullName);
			var memoryAppender = ((Hierarchy)LogManager.GetRepository()).Root.GetAppender("memory") as MemoryAppender;
			var actualLogOutput = new StringWriter();
			var patternLayout = new PatternLayout("[%-5level] [%logger] [%properties{NDC}] [%properties{foo}] [%properties{flim}] - %message%newline");
			patternLayout.Format(actualLogOutput, memoryAppender.GetEvents()[0]);

			Assert.AreEqual(expectedLogOutput, actualLogOutput.ToString());
		}
	}
#endif
}
