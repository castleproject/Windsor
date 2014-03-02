// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace CastleTests
{
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
    public class ResolveLongRunningTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_Resolve_Only_Long_Running_Services()
		{
            // Arrange
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>().IsLongRunning(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());

            // Act
			var services = Container.ResolveAllLongRunning<IEmptyService>();

            // Assert
			Assert.AreEqual(1, services.Length);
		}
	}
}