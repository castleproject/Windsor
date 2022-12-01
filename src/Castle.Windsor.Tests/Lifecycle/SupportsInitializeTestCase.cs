// Copyright 2004-2022 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests.Lifecycle
{
	using Castle.MicroKernel.Registration;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class SupportsInitializeTestCase : AbstractContainerTestCase
	{
		[Test]
		public void SupportsInitialize_components_are_not_tracked()
		{
			Container.Register(Component.For<ISimpleService>()
			                   	.ImplementedBy<SimpleServiceSupportInitialize>()
			                   	.LifeStyle.Transient);

			ReferenceTracker
				.Track(() => Container.Resolve<ISimpleService>())
				.AssertNoLongerReferenced();
		}

		[Test]
		public void SupportsInitialize_components_for_non_SupportsInitialize_service_get_initialized_when_resolved()
		{
			Container.Register(Component.For<ISimpleService>()
			                   	.ImplementedBy<SimpleServiceSupportInitialize>()
			                   	.LifeStyle.Transient);

			var server = (SimpleServiceSupportInitialize)Container.Resolve<ISimpleService>();

			Assert.IsTrue(server.InitBegun);
			Assert.IsTrue(server.InitEnded);
		}

		[Test]
		public void
			SupportsInitialize_components_for_non_SupportsInitialize_service_get_initialized_when_resolved_via_factoryMethod()
		{
			Container.Register(Component.For<ISimpleService>()
			                   	.UsingFactoryMethod(() => new SimpleServiceSupportInitialize())
			                   	.LifeStyle.Transient);

			var server = (SimpleServiceSupportInitialize)Container.Resolve<ISimpleService>();

			Assert.IsTrue(server.InitBegun);
			Assert.IsTrue(server.InitEnded);
		}

		[Test]
		public void SupportsInitialize_components_get_initialized_when_resolved()
		{
			Container.Register(Component.For<SupportInitializeComponent>());

			var server = Container.Resolve<SupportInitializeComponent>();

			Assert.IsTrue(server.InitBegun);
			Assert.IsTrue(server.InitEnded);
		}
	}
}
