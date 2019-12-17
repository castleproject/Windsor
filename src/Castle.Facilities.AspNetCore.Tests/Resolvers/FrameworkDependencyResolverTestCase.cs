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

namespace Castle.Facilities.AspNetCore.Tests.Resolvers
{
	using System;

	using Castle.Facilities.AspNetCore.Tests.Fakes;
	using Castle.Facilities.AspNetCore.Resolvers;
	using Castle.Facilities.AspNetCore.Tests.Framework;

	using Microsoft.Extensions.DependencyInjection;

	using NUnit.Framework;

	[TestFixture]
	public class FrameworkDependencyResolverTestCase
	{
		private Framework.TestContext testContext;
		private FrameworkDependencyResolver frameworkDependencyResolver;

		[SetUp]
		public void SetUp()
		{
			testContext = TestContextFactory.Get();
			frameworkDependencyResolver = new FrameworkDependencyResolver(testContext.ServiceCollection);
			frameworkDependencyResolver.AcceptServiceProvider(testContext.ServiceProvider);
		}

		[TearDown]
		public void TearDown()
		{
			testContext.Dispose();
		}

		[Test]
		public void Should_not_match_null()
		{
			Assert.That(frameworkDependencyResolver.HasMatchingType(null), Is.False);
		}

		[TestCase(typeof(ServiceProviderOnlyTransient))]
		[TestCase(typeof(ServiceProviderOnlyTransientGeneric<OpenOptions>))]
		[TestCase(typeof(ServiceProviderOnlyTransientGeneric<ClosedOptions>))]
		[TestCase(typeof(ServiceProviderOnlyTransientDisposable))]
		[TestCase(typeof(ServiceProviderOnlyScoped))]
		[TestCase(typeof(ServiceProviderOnlyScopedGeneric<OpenOptions>))]
		[TestCase(typeof(ServiceProviderOnlyScopedGeneric<ClosedOptions>))]
		[TestCase(typeof(ServiceProviderOnlyScopedDisposable))]
		[TestCase(typeof(ServiceProviderOnlySingleton))]
		[TestCase(typeof(ServiceProviderOnlySingletonGeneric<OpenOptions>))]
		[TestCase(typeof(ServiceProviderOnlySingletonGeneric<ClosedOptions>))]
		[TestCase(typeof(ServiceProviderOnlySingletonDisposable))]
		[TestCase(typeof(ControllerServiceProviderOnly))]
		[TestCase(typeof(TagHelperServiceProviderOnly))]
		[TestCase(typeof(ViewComponentServiceProviderOnly))]
		public void Should_match_ServiceProvider_services(Type serviceType)
		{
			Assert.That(frameworkDependencyResolver.HasMatchingType(serviceType), Is.True);
		}

		[TestCase(typeof(CrossWiredTransient))]
		[TestCase(typeof(CrossWiredTransientGeneric<OpenOptions>))]
		[TestCase(typeof(CrossWiredTransientGeneric<ClosedOptions>))]
		[TestCase(typeof(CrossWiredTransientDisposable))]
		[TestCase(typeof(CrossWiredScoped))]
		[TestCase(typeof(CrossWiredScopedGeneric<OpenOptions>))]
		[TestCase(typeof(CrossWiredScopedGeneric<ClosedOptions>))]
		[TestCase(typeof(CrossWiredScopedDisposable))]
		[TestCase(typeof(CrossWiredSingleton))]
		[TestCase(typeof(CrossWiredSingletonGeneric<OpenOptions>))]
		[TestCase(typeof(CrossWiredSingletonGeneric<ClosedOptions>))]
		[TestCase(typeof(CrossWiredSingletonDisposable))]
		[TestCase(typeof(ControllerCrossWired))]
		[TestCase(typeof(TagHelperCrossWired))]
		[TestCase(typeof(ViewComponentCrossWired))]
		public void Should_match_CrossWired_services(Type serviceType)
		{
			Assert.That(frameworkDependencyResolver.HasMatchingType(serviceType), Is.True);
		}

		[TestCase(typeof(WindsorOnlyTransient))]
		[TestCase(typeof(WindsorOnlyTransientGeneric<OpenOptions>))]
		[TestCase(typeof(WindsorOnlyTransientGeneric<ClosedOptions>))]
		[TestCase(typeof(WindsorOnlyTransientDisposable))]
		[TestCase(typeof(WindsorOnlyScoped))]
		[TestCase(typeof(WindsorOnlyScopedGeneric<OpenOptions>))]
		[TestCase(typeof(WindsorOnlyScopedGeneric<ClosedOptions>))]
		[TestCase(typeof(WindsorOnlyScopedDisposable))]
		[TestCase(typeof(WindsorOnlySingleton))]
		[TestCase(typeof(WindsorOnlySingletonGeneric<OpenOptions>))]
		[TestCase(typeof(WindsorOnlySingletonGeneric<ClosedOptions>))]
		[TestCase(typeof(WindsorOnlySingletonDisposable))]
		[TestCase(typeof(ControllerWindsorOnly))]
		[TestCase(typeof(TagHelperWindsorOnly))]
		[TestCase(typeof(ViewComponentWindsorOnly))]
		public void Should_not_match_WindsorOnly_services(Type serviceType)
		{
			Assert.That(!frameworkDependencyResolver.HasMatchingType(serviceType), Is.True);
		}

		[TestCase(typeof(ServiceProviderOnlyTransient))]
		[TestCase(typeof(ServiceProviderOnlyTransientGeneric<OpenOptions>))]
		[TestCase(typeof(ServiceProviderOnlyTransientGeneric<ClosedOptions>))]
		[TestCase(typeof(ServiceProviderOnlyTransientDisposable))]
		[TestCase(typeof(ServiceProviderOnlyScoped))]
		[TestCase(typeof(ServiceProviderOnlyScopedGeneric<OpenOptions>))]
		[TestCase(typeof(ServiceProviderOnlyScopedGeneric<ClosedOptions>))]
		[TestCase(typeof(ServiceProviderOnlyScopedDisposable))]
		[TestCase(typeof(ServiceProviderOnlySingleton))]
		[TestCase(typeof(ServiceProviderOnlySingletonGeneric<OpenOptions>))]
		[TestCase(typeof(ServiceProviderOnlySingletonGeneric<ClosedOptions>))]
		[TestCase(typeof(ServiceProviderOnlySingletonDisposable))]
		[TestCase(typeof(ControllerServiceProviderOnly))]
		[TestCase(typeof(TagHelperServiceProviderOnly))]
		[TestCase(typeof(ViewComponentServiceProviderOnly))]
		public void Should_resolve_all_ServiceProviderOnly_services_from_ServiceProvider(Type serviceType)
		{
			Assert.DoesNotThrow(() =>
			{
				testContext.ServiceProvider.GetRequiredService(serviceType);
			});
		}

		[TestCase(typeof(CrossWiredTransient))]
		[TestCase(typeof(CrossWiredTransientGeneric<OpenOptions>))]
		[TestCase(typeof(CrossWiredTransientGeneric<ClosedOptions>))]
		[TestCase(typeof(CrossWiredTransientDisposable))]
		[TestCase(typeof(CrossWiredScoped))]
		[TestCase(typeof(CrossWiredScopedGeneric<OpenOptions>))]
		[TestCase(typeof(CrossWiredScopedGeneric<ClosedOptions>))]
		[TestCase(typeof(CrossWiredScopedDisposable))]
		[TestCase(typeof(CrossWiredSingleton))]
		[TestCase(typeof(CrossWiredSingletonGeneric<OpenOptions>))]
		[TestCase(typeof(CrossWiredSingletonGeneric<ClosedOptions>))]
		[TestCase(typeof(CrossWiredSingletonDisposable))]
		[TestCase(typeof(ControllerCrossWired))]
		[TestCase(typeof(TagHelperCrossWired))]
		[TestCase(typeof(ViewComponentCrossWired))]
		public void Should_resolve_all_CrossWiredOnly_services_from_ServiceProvider(Type serviceType)
		{
			Assert.DoesNotThrow(() =>
			{
				testContext.ServiceProvider.GetRequiredService(serviceType);
			});
		}

		[TestCase(typeof(ControllerCrossWired))]
		[TestCase(typeof(TagHelperCrossWired))]
		[TestCase(typeof(ViewComponentCrossWired))]
		[TestCase(typeof(ControllerWindsorOnly))]
		[TestCase(typeof(TagHelperWindsorOnly))]
		[TestCase(typeof(ViewComponentWindsorOnly))]
		[TestCase(typeof(ControllerServiceProviderOnly))]
		[TestCase(typeof(TagHelperServiceProviderOnly))]
		[TestCase(typeof(ViewComponentServiceProviderOnly))]
		public void Should_resolve_ServiceProviderOnly_and_WindsorOnly_and_CrossWired_registered_Controllers_TagHelpers_and_ViewComponents_from_WindsorContainer(Type serviceType)
		{
			Assert.DoesNotThrow(() =>
			{
				testContext.WindsorContainer.Resolve(serviceType);
			});
		}
	}
}