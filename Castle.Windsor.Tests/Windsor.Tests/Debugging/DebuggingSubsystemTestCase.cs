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

namespace Castle.Windsor.Tests.Windsor.Tests.Debugging
{
#if !SILVERLIGHT
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Experimental.Diagnostics;
	using Castle.Windsor.Experimental.Diagnostics.Extensions;
	using Castle.Windsor.Tests.Components;

	using CastleTests;

	using NUnit.Framework;

	[TestFixture]
	public class DebuggingSubsystemTestCase : AbstractContainerTestCase
	{
		[SetUp]
		public void InitSubSystem()
		{
			Init();
			subSystem = new DefaultDebuggingSubSystem();
			Kernel.AddSubSystem(SubSystemConstants.DebuggingKey, subSystem);
		}

		private DefaultDebuggingSubSystem subSystem;

		[Test(Description = "When failing this test causes stack overflow")]
		public void PotentialLifestyleMismatches_can_handle_dependency_cycles()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecoratorViaProperty>());

			var mismatches = subSystem.Single(x => x is PotentialLifestyleMismatches).Attach().SingleOrDefault();
			Assert.IsNull(mismatches);
		}
	}
#endif
}