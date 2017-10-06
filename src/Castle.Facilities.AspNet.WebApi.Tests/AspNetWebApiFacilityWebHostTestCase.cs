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

namespace Castle.Facilities.AspNet.WebApi.Tests
{
	using System;
	using System.IO;
	using System.Net.Http;
	using System.Web;
	using System.Web.Http;
	using System.Web.Http.Controllers;

	using Castle.Facilities.AspNet.WebApi.Exceptions;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class AspNetWebApiFacilityWebHostTestCase
	{
		AspNetWebApiFacility facility = null;
		ApiController releasedController = null;
		private WindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			HttpContext.Current = new HttpContext(
				new HttpRequest("", "http://tempuri.org", ""),
				new HttpResponse(new StringWriter())
			);

			container = new WindsorContainer();
			container.AddFacility<AspNetWebApiFacility>(x => { facility = x; });
			container.Kernel.ComponentDestroyed += (model, instance) => releasedController = (ApiController)instance;
		}

		[Test]
		public void Should_throw_if_scoped_lifestyles_are_potentially_not_enabled()
		{
			Assert.Throws<LifestyleScopesPotentiallyNotEnabledException>(() =>
			{
				container.Register(Component.For<WebApiController>().LifestyleScoped());
				//facility.WithLifestyleScopedPerWebRequest(); // <- intentionally left out
				var result = ResolveAndReleaseControllerUsingWebHostActivator();
				Assert.That(releasedController, Is.EqualTo(result));
			});
		}

		[Test]
		public void Should_be_able_to_resolve_and_release_transient_controller()
		{
			container.Register(Component.For<WebApiController>().LifestyleTransient());
			var result = ResolveAndReleaseControllerUsingWebHostActivator();
			Assert.That(releasedController, Is.EqualTo(result));
		}

		[Test]
		public void Should_be_able_to_resolve_and_release_scoped_controller()
		{
			container.Register(Component.For<WebApiController>().LifestyleScoped());
			facility.WithLifestyleScopedPerWebRequest();
			var result = ResolveAndReleaseControllerUsingWebHostActivator();
			Assert.That(releasedController, Is.EqualTo(result));
		}

		[Test]
		public void Should_call_out_using_event_before_controller_is_created()
		{
			container.Register(Component.For<WebApiController>().LifestyleTransient());
			var beforeControllerResolved = false;
			facility.BeforeControllerResolved((a, b) => beforeControllerResolved = true);
			ResolveAndReleaseControllerUsingWebHostActivator();
			Assert.That(beforeControllerResolved, Is.True);
		}

		[Test]
		public void Should_call_out_using_event_after_controller_is_released()
		{
			container.Register(Component.For<WebApiController>().LifestyleTransient());
			var afterControllerReleased = false;
			facility.AfterControllerReleased((a, b) => afterControllerReleased = true);
			ResolveAndReleaseControllerUsingWebHostActivator();
			Assert.That(afterControllerReleased, Is.True);
		}

		private IHttpController ResolveAndReleaseControllerUsingWebHostActivator()
		{
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("http://tempuri.org"));
			var result = facility.ControllerActivator.Create(httpRequestMessage, null, typeof(WebApiController));
			httpRequestMessage.DisposeRequestResources();
			return result;
		}

		public class WebApiController : ApiController
		{
		}
	}
}
