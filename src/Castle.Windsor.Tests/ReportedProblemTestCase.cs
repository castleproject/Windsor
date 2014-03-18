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

namespace Castle.Windsor.Tests
{
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ReportedProblemTestCase
	{
		private IWindsorContainer container;

		[SetUp]
		public void Init()
		{
			container = new WindsorContainer();
		}

		[Test]
		public void StackOverflowProblem()
		{
			container.Register(Component.For<Employee>());
			container.Register(Component.For<Reviewer>());
			container.Register(Component.For<ReviewableEmployee>());

			Assert.IsNotNull(container.Resolve<ReviewableEmployee>());
			Assert.IsNotNull(container.Resolve<Reviewer>());
			Assert.IsNotNull(container.Resolve<Employee>());
		}
	}
}