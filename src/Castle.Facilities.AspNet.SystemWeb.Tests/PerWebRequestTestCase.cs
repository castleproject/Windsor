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

namespace Castle.Facilities.AspNet.SystemWeb.Tests
{
	using System;
	using System.IO;
	using System.Web;

	using Castle.Core;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class PerWebRequestTestCase
	{
		private FakePerWebRequestLifestyleModule fakeModule;

		[OneTimeSetUp]
		public void SetUpFixture()
		{
			fakeModule = new FakePerWebRequestLifestyleModule();
		}

		[Test]
		public void Should_be_able_to_register_using_attribute_for_per_web_request_lifestyle()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<PerWebRequestComponentWithAttributedLifestyle>().Named("P"));
			var handler = container.Kernel.GetHandler("P");
			Assert.That(handler.ComponentModel.LifestyleType, Is.EqualTo(LifestyleType.Scoped));
		}

		[Test]
		public void Should_be_able_to_register_using_component_registration_extension_for_per_web_request_lifestyle()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<PerWebRequestComponent>().LifestylePerWebRequest().Named("P"));
			var handler = container.Kernel.GetHandler("P");
			Assert.That(handler.ComponentModel.LifestyleType, Is.EqualTo(LifestyleType.Scoped));
		}

		[Test]
		public void Should_be_able_to_register_resolve_and_release_per_web_request_lifestyle_component_using_fake_module()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<PerWebRequestComponent>().LifestylePerWebRequest().Named("P"));

			var service = container.Resolve<PerWebRequestComponent>();

			bool wasReleased = false;
			object releasedService = null;

			container.Kernel.ComponentDestroyed += (model, instance) =>
			{
				wasReleased = true;
				releasedService = instance;
			};

			fakeModule.FireSimulatedEndRequest();

			Assert.That(wasReleased, Is.True);
			Assert.That(releasedService, Is.EqualTo(service));
		}

		public class PerWebRequestComponent
		{
		}

		[PerWebRequest]
		public class PerWebRequestComponentWithAttributedLifestyle
		{
		}

		public class FakePerWebRequestLifestyleModule : PerWebRequestLifestyleModule
		{
			private readonly HttpApplication httpApplication = new HttpApplication();

			public FakePerWebRequestLifestyleModule()
			{
				Init(httpApplication);
				HttpContext.Current = new HttpContext(
					new HttpRequest("", "http://tempuri.org", ""),
					new HttpResponse(new StringWriter())
				);
			}

			public void FireSimulatedEndRequest()
			{
				EndRequest(httpApplication.Context, new EventArgs());
			}
		}
	}
}
