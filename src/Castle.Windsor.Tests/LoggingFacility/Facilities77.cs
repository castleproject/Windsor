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
	using Castle.Core.Logging;
	using Castle.Facilities.Logging.Tests.Classes;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class Facilities77 : BaseTest
	{
		public class TestLoggerFactory : AbstractLoggerFactory
		{
			public bool NoArgsConstructorWasCalled;

			public TestLoggerFactory() : this("someconfigfile")
			{
				NoArgsConstructorWasCalled = true;
			}

			public TestLoggerFactory(string configFile)
			{
				NoArgsConstructorWasCalled = false;
			}

			public override ILogger Create(string name)
			{
				return NullLogger.Instance;
			}

			public override ILogger Create(string name, LoggerLevel level)
			{
				return NullLogger.Instance;
			}
		}

		[Test]
		public void ShouldCallNoArgsContstructorIfConfigFileNotSpecified()
		{
			var container = new WindsorContainer().AddFacility<LoggingFacility>(f => f.LogUsing<TestLoggerFactory>());

			container.Register(Component.For<SimpleLoggingComponent>().Named("component"));
			container.Resolve<SimpleLoggingComponent>("component");

			var logFactory = container.Resolve<TestLoggerFactory>("iloggerfactory");
			Assert.IsTrue(logFactory.NoArgsConstructorWasCalled, "No args constructor was not called");
		}
	}
}