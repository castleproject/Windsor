// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.AspNet.Mvc.Tests
{
	using System.Web.Mvc;
	using System.Web.Routing;

	using Castle.Facilities.AspNet.Mvc;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class ControllerFinderTestCase
	{
		private AspNetMvcFacility facility = null;
		private WindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();

			container.AddFacility<AspNetMvcFacility>(x =>
			{
				facility = x;
				x.AddControllerAssembly<MvcTestController>();
			});
		}

		[TestCase("MvcTest")]
		[TestCase("mvctest")]
		[TestCase("Mvctest")]
		[TestCase("MVCtest")]
		public void Should_find_controller_by_name_ignoring_case(string controllerName)
		{
			container.Register(Component.For<MvcTestController>().LifestyleScoped());

			var result = ResolveController(controllerName);

			Assert.That(typeof(MvcTestController), Is.EqualTo(result.GetType()));
		}

		private IController ResolveController(string controllerName)
		{
			using (container.BeginScope())
				return facility.ControllerFactory.CreateController(new RequestContext(), controllerName);
		}

		public class MvcTestController : Controller
		{
		}
	}
}
