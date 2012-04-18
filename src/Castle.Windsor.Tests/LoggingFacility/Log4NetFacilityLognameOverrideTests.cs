namespace Castle.Facilities.Logging.Tests
{
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

	[TestFixture]
	public class Log4NetFacilityLognameOverrideTests : OverrideLoggerTest
	{
		private IWindsorContainer container;

		[SetUp]
		public void Setup()
		{
			container = base.CreateConfiguredContainer(LoggerImplementation.ExtendedLog4net, string.Empty, "Override");
		}

		[TearDown]
		public void Teardown()
		{
			container.Dispose();
		}

		[Test]
		public void OverrideTest()
		{
			container.Register(Component.For(typeof(SimpleLoggingComponent)).Named("component"));
			SimpleLoggingComponent test = container.Resolve<SimpleLoggingComponent>("component");

			test.DoSomething();

			String expectedLogOutput = String.Format("[INFO ] [Override.{0}] - Hello world" + Environment.NewLine, typeof(SimpleLoggingComponent).FullName);
			MemoryAppender memoryAppender = ((Hierarchy)LogManager.GetRepository()).Root.GetAppender("memory") as MemoryAppender;
			TextWriter actualLogOutput = new StringWriter();
			PatternLayout patternLayout = new PatternLayout("[%-5level] [%logger] - %message%newline");
			patternLayout.Format(actualLogOutput, memoryAppender.GetEvents()[0]);

			Assert.AreEqual(expectedLogOutput, actualLogOutput.ToString());
		}
	}
}