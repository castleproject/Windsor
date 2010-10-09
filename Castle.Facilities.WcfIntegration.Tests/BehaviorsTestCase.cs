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

namespace Castle.Facilities.WcfIntegration.Tests
{
	using Castle.Facilities.WcfIntegration.Tests.Behaviors;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class BehaviorsTestCase
	{
		private IWindsorContainer container;

		[Test(Description = "This test would trigger a stack overflow if failing.")]
		public void Can_resolve_behavior_with_cyclic_dependency_on_its_extension()
		{
			container.Register(Component.For<ExtensibleBehavior>().LifeStyle.Transient,
			                   Component.For<ExtensibleBehaviorExtension>().LifeStyle.Transient);

			container.Resolve<ExtensibleBehavior>();
		}

		[SetUp]
		public void SetUpTests()
		{
			container = new WindsorContainer();
			container.AddFacility<WcfFacility>();
		}

		[TearDown]
		public void TearDownTests()
		{
			container.Dispose();
		}
	}
}