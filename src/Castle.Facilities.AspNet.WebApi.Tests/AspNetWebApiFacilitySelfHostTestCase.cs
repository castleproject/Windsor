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
	using System.IO;
	using System.Web;
	using System.Web.Http;
	using System.Web.Http.Controllers;

	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class AspNetWebApiFacilitySelfHostTestCase
	{
		AspNetWebApiFacility facility = null;
		ApiController releasedController = null;
		private WindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
			container.AddFacility<AspNetWebApiFacility>(x =>
			{
				facility = x;
				x.UsingSelfHosting();
			});

			container.Kernel.ComponentDestroyed += (model, instance) => releasedController = (ApiController)instance;
		}

		[Test]
		public void Should_be_able_to_resolve_and_release_transient_controller()
		{
			container.Register(Component.For<AspNetWebApiFacilityWebHostTestCase.WebApiController>().LifestyleTransient());
			var result = ResolveAndReleaseControllerUsingSelfHostDependencyResolver();
			Assert.That(releasedController, Is.EqualTo(result));
		}

		[Test]
		public void Should_be_able_to_resolve_and_release_scoped_controller()
		{
			container.Register(Component.For<AspNetWebApiFacilityWebHostTestCase.WebApiController>().LifestyleScoped());
			facility.WithLifestyleScopedPerWebRequest();
			var result = ResolveAndReleaseControllerUsingSelfHostDependencyResolver();
			Assert.That(releasedController, Is.EqualTo(result));
		}

		[Test]
		public void Should_call_out_using_event_before_controller_is_created_using_dependency_scopes()
		{
			container.Register(Component.For<AspNetWebApiFacilityWebHostTestCase.WebApiController>().LifestyleTransient());
			var beforeControllerResolved = false;
			facility.BeforeControllerResolved((a, b) => beforeControllerResolved = true);
			ResolveAndReleaseControllerUsingSelfHostDependencyResolverScopes();
			Assert.That(beforeControllerResolved, Is.True);
		}

		[Test]
		public void Should_call_out_using_event_after_controller_is_released_using_dependency_scopes()
		{
			container.Register(Component.For<AspNetWebApiFacilityWebHostTestCase.WebApiController>().LifestyleTransient());
			var afterControllerReleased = false;
			facility.AfterControllerReleased((a, b) => afterControllerReleased = true);
			ResolveAndReleaseControllerUsingSelfHostDependencyResolverScopes();
			Assert.That(afterControllerReleased, Is.True);
		}

		private IHttpController ResolveAndReleaseControllerUsingSelfHostDependencyResolver()
		{
			IHttpController result = null;
			result = (IHttpController)facility.DependencyResolver.GetService(typeof(AspNetWebApiFacilityWebHostTestCase.WebApiController));
			facility.DependencyResolver.Dispose();
			return result;
		}

		private void ResolveAndReleaseControllerUsingSelfHostDependencyResolverScopes()
		{
			using (var scope = facility.DependencyResolver.BeginScope())
			{
				// This is where it all went wrong for IoC on aspnet core, service location and scopes really should not have happened.
				scope.GetService(typeof(AspNetWebApiFacilityWebHostTestCase.WebApiController));
			}
		}
	}
}
