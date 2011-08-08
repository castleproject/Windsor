// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.LoggingFacility
{
	using Castle.Core.Logging;
	using Castle.Facilities.Logging;
	using Castle.Facilities.Logging.Tests.Classes;
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class ConsoleLoggerTestCase : AbstractContainerTestCase
	{
		[Test]
		[Bug("FACILITIES-153")]
		public void Can_specify_level_at_registration_time()
		{
			Container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console).WithLevel(LoggerLevel.Fatal));

			Container.Register(Component.For<SimpleLoggingComponent>());

			var item = Container.Resolve<SimpleLoggingComponent>();
			Assert.IsTrue(item.Logger.IsFatalEnabled);
			Assert.IsFalse(item.Logger.IsErrorEnabled);
		}
	}
}