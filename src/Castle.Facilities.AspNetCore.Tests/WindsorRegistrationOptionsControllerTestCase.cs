// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.AspNetCore.Tests
{
	using System;

	using Castle.Facilities.AspNetCore.Tests.Framework;
	using Castle.MicroKernel.Registration;

	using Microsoft.AspNetCore.Mvc;

	using NUnit.Framework;

	public abstract class WindsorRegistrationOptionsControllerTestCase
	{
		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public void TearDown()
		{
			testContext.Dispose();
		}

		protected Framework.TestContext testContext;

		[TestCase(typeof(OverrideController))]
		public void Should_resolve_overidden_Controllers_using_WindsorRegistrationOptions(Type optionsResolvableType)
		{
			Assert.DoesNotThrow(() => { testContext.WindsorContainer.Resolve(optionsResolvableType); });
		}

		public class OverrideController : Controller
		{
		}
	}

	[TestFixture]
	public class WindsorRegistrationOptionsForAssembliesControllerTestCase : WindsorRegistrationOptionsControllerTestCase
	{
		[SetUp]
		public override void SetUp()
		{
			testContext = TestContextFactory.Get(opts => opts
				.UseEntryAssembly(typeof(Uri).Assembly)
				.RegisterControllers(typeof(OverrideController).Assembly));
		}
	}

	[TestFixture]
	public class WindsorRegistrationOptionsForComponentsControllerTestCase : WindsorRegistrationOptionsControllerTestCase
	{
		[SetUp]
		public override void SetUp()
		{
			testContext = TestContextFactory.Get(opts => opts
				.UseEntryAssembly(typeof(Uri).Assembly)
				.RegisterControllers(Component.For<OverrideController>().LifestyleScoped().Named("controllers")));
		}
	}
}