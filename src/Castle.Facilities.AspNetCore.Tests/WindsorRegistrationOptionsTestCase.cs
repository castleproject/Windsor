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

	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Razor.TagHelpers;

	using NUnit.Framework;

	[TestFixture, Order(3)]
	public class WindsorRegistrationOptionsTestCase
	{
		[SetUp]
		public void SetUp()
		{
			testContext = TestContextFactory.Get(opts => opts
				.UseEntryAssembly(typeof(WindsorRegistrationOptionsTestCase).Assembly)
				.RegisterTagHelpers(typeof(OverrideTagHelper).Assembly)
				.RegisterControllers(typeof(OverrideController).Assembly)
				.RegisterTagHelpers(typeof(OverrideViewComponent).Assembly));
		}

		[TearDown]
		public void TearDown()
		{
			testContext.Dispose();
		}

		private Framework.TestContext testContext;

		[TestCase(typeof(OverrideTagHelper))]
		[TestCase(typeof(OverrideController))]
		[TestCase(typeof(OverrideViewComponent))]
		public void Should_resolve_overidden_Controllers_TagHelpers_and_ViewComponents_using_WindsorRegistrationOptions(Type optionsResolvableType)
		{
			Assert.DoesNotThrow(() => { testContext.WindsorContainer.Resolve(optionsResolvableType); });
		}

		public class OverrideTagHelper : TagHelper
		{
		}

		public class OverrideController : Controller
		{
		}

		public class OverrideViewComponent : ViewComponent
		{
		}
	}
}