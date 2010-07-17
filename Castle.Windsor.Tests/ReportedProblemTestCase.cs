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

namespace Castle.Windsor.Tests
{
    using System;

    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor.Tests.Components;
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
			container.Register(Component.For(typeof(Employee)).Named("C"));
			container.Register(Component.For(typeof(Reviewer)).Named("B"));
			container.Register(Component.For(typeof(ReviewableEmployee)).Named("A"));

			Assert.IsNotNull(container.Resolve("A", new Arguments()));
			Assert.IsNotNull(container.Resolve("B", new Arguments()));
			Assert.IsNotNull(container.Resolve("C", new Arguments()));

		}
	}
}
