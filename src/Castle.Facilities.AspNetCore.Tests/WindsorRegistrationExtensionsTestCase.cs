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
	using System.Linq;

	using Castle.Core;
	using Castle.Facilities.AspNetCore.Tests.Fakes;
	using Castle.Facilities.AspNetCore.Tests.Framework;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using Microsoft.Extensions.DependencyInjection;

	using NUnit.Framework;

	[TestFixture]
	public class WindsorRegistrationExtensionsTestCase
	{
		[SetUp]
		public void SetUp()
		{
			testContext = TestContextFactory.Get();
		}

		[TearDown]
		public void TearDown()
		{
			testContext?.Dispose();
		}

		private Framework.TestContext testContext;

		[TestCase(typeof(ControllerWindsorOnly))]
		[TestCase(typeof(TagHelperWindsorOnly))]
		[TestCase(typeof(ViewComponentWindsorOnly))]
		public void Should_resolve_WindsorOnly_Controllers_TagHelpers_and_ViewComponents_from_WindsorContainer(Type serviceType)
		{
			Assert.DoesNotThrow(() =>
			{
				testContext.WindsorContainer.Resolve(serviceType);
			});
		}

		[TestCase(typeof(ControllerServiceProviderOnly))]
		[TestCase(typeof(TagHelperServiceProviderOnly))]
		[TestCase(typeof(ViewComponentServiceProviderOnly))]
		public void Should_resolve_ServiceProviderOnly_Controllers_TagHelpers_and_ViewComponents_from_ServiceProvider(Type serviceType)
		{
			Assert.DoesNotThrow(() =>
			{
				testContext.ServiceProvider.GetRequiredService(serviceType);
			});
		}


		[TestCase(typeof(ControllerCrossWired))]
		[TestCase(typeof(TagHelperCrossWired))]
		[TestCase(typeof(ViewComponentCrossWired))]
		[TestCase(typeof(ControllerServiceProviderOnly))]
		[TestCase(typeof(TagHelperServiceProviderOnly))]
		[TestCase(typeof(ViewComponentServiceProviderOnly))]
		public void Should_resolve_ServiceProviderOnly_and_CrossWired_Controllers_TagHelpers_and_ViewComponents_from_WindsorContainer_and_ServiceProvider(Type serviceType)
		{
			Assert.DoesNotThrow(() =>
			{
				testContext.WindsorContainer.Resolve(serviceType);
				testContext.ServiceProvider.GetRequiredService(serviceType);
			});
		}

		[TestCase(typeof(CrossWiredScoped))]
		[TestCase(typeof(CrossWiredSingleton))]
		public void Should_resolve_CrossWired_Singleton_and_Scoped_as_same_instance_from_WindsorContainer_and_ServiceProvider(Type serviceType)
		{
			var instanceA = testContext.WindsorContainer.Resolve(serviceType);
			var instanceB = testContext.ServiceProvider.GetRequiredService(serviceType);

			Assert.That(instanceA, Is.EqualTo(instanceB));
		}

		[TestCase(typeof(CrossWiredTransient))]
		public void Should_resolve_CrossWired_Transient_as_different_instances_from_WindsorContainer_and_ServiceProvider(Type serviceType)
		{
			var instanceA = testContext.WindsorContainer.Resolve(serviceType);
			var instanceB = testContext.ServiceProvider.GetRequiredService(serviceType);

			Assert.That(instanceA, Is.Not.EqualTo(instanceB));
		}

		[TestCase(typeof(CrossWiredSingletonDisposable))]
		[TestCase(typeof(WindsorOnlySingletonDisposable))]
		public void Should_not_Dispose_CrossWired_or_WindsorOnly_Singleton_disposables_when_Disposing_Windsor_Scope(Type serviceType)
		{
			var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
			testContext.DisposeWindsorScope();

			Assert.That(singleton.Disposed, Is.False);
			Assert.That(singleton.DisposedCount, Is.EqualTo(0));
		}

		[TestCase(typeof(WindsorOnlySingletonDisposable))]
		public void Should_Dispose_WindsorOnly_Singleton_disposables_only_when_Disposing_WindsorContainer(Type serviceType)
		{
			var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
			testContext.DisposeWindsorContainer();

			Assert.That(singleton.Disposed, Is.True);
			Assert.That(singleton.DisposedCount, Is.EqualTo(1));
		}

		[TestCase(typeof(CrossWiredSingletonDisposable))]
		public void Should_not_Dispose_CrossWired_Singleton_disposables_when_Disposing_WindsorContainer_because_it_is_tracked_by_the_ServiceProvider(Type serviceType)
		{
			var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
			testContext.DisposeWindsorContainer();

			Assert.That(singleton.Disposed, Is.False);
			Assert.That(singleton.DisposedCount, Is.EqualTo(0));
		}

		[TestCase(typeof(CrossWiredSingletonDisposable))]
		[TestCase(typeof(ServiceProviderOnlySingletonDisposable))]
		public void Should_Dispose_CrossWired_and_ServiceProviderOnly_Singleton_disposables_when_Disposing_ServiceProvider(Type serviceType)
		{
			var singleton = (IDisposableObservable)testContext.ServiceProvider.GetRequiredService(serviceType);
			testContext.DisposeServiceProvider();

			Assert.That(singleton.Disposed, Is.True);
			Assert.That(singleton.DisposedCount, Is.EqualTo(1));
		}

		[TestCase(typeof(WindsorOnlyScopedDisposable))]
		public void Should_Dispose_WindsorOnly_Scoped_disposables_when_Disposing_Windsor_Scope(Type serviceType)
		{
			var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
			testContext.DisposeWindsorScope();

			Assert.That(singleton.Disposed, Is.True);
			Assert.That(singleton.DisposedCount, Is.EqualTo(1));
		}

		[TestCase(typeof(CrossWiredScopedDisposable))]
		public void Should_not_Dispose_CrossWired_Scoped_disposables_when_Disposing_Windsor_Scope_because_it_is_tracked_by_the_ServiceProvider(Type serviceType)
		{
			var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
			testContext.DisposeWindsorScope();

			Assert.That(singleton.Disposed, Is.False);
			Assert.That(singleton.DisposedCount, Is.EqualTo(0));
		}

		[TestCase(typeof(CrossWiredScopedDisposable))]
		[TestCase(typeof(CrossWiredTransientDisposable))]
		public void Should_Dispose_CrossWired_Scoped_and_Transient_disposables_when_Disposing_ServiceProvider_Scope(Type serviceType)
		{
			IDisposableObservable scoped;
			using (var serviceProviderScope = testContext.ServiceProvider.CreateScope())
			{
				scoped = (IDisposableObservable)serviceProviderScope.ServiceProvider.GetRequiredService(serviceType);
			}

			Assert.That(scoped.Disposed, Is.True);
			Assert.That(scoped.DisposedCount, Is.EqualTo(1));
		}

		[TestCase(typeof(CrossWiredTransientDisposable))]
		public void Should_not_Dispose_CrossWired_Transient_disposables_when_Disposing_Windsor_Scope_because_is_tracked_by_the_ServiceProvider(Type serviceType)
		{
			var singleton = (IDisposableObservable)testContext.WindsorContainer.Resolve(serviceType);
			testContext.DisposeWindsorScope();

			Assert.That(singleton.Disposed, Is.False);
			Assert.That(singleton.DisposedCount, Is.EqualTo(0));
		}

		[TestCase(typeof(CrossWiredSingletonDisposable))]
		[TestCase(typeof(ServiceProviderOnlySingletonDisposable))]
		public void Should_not_Dispose_CrossWired_or_ServiceOnly_Singleton_disposables_when_Disposing_ServiceProviderScope(Type serviceType)
		{
			IDisposableObservable singleton;

			using (var serviceProviderScope = testContext.ServiceProvider.CreateScope())
			{
				singleton = (IDisposableObservable)serviceProviderScope.ServiceProvider.GetRequiredService(serviceType);
			}

			Assert.That(singleton.Disposed, Is.False);
			Assert.That(singleton.DisposedCount, Is.EqualTo(0));
		}

		[TestCase(typeof(CompositeTagHelper))]
		[TestCase(typeof(CompositeController))]
		[TestCase(typeof(CompositeViewComponent))]
		public void Should_resolve_Composite_Singleton_from_WindsorContainer(Type compositeType)
		{
			testContext.WindsorContainer.Register(Component.For(compositeType).LifestyleSingleton());

			Assert.DoesNotThrow(() =>
			{
				testContext.WindsorContainer.Resolve(compositeType);
			});
		}

		[TestCase(typeof(CompositeTagHelper))]
		[TestCase(typeof(CompositeController))]
		[TestCase(typeof(CompositeViewComponent))]
		public void Should_resolve_Composite_Scoped_from_WindsorContainer(Type compositeType)
		{
			testContext.WindsorContainer.Register(Component.For(compositeType).LifestyleScoped());

			Assert.DoesNotThrow(() =>
			{
				testContext.WindsorContainer.Resolve(compositeType);
			});
		}

		[TestCase(typeof(CompositeTagHelper))]
		[TestCase(typeof(CompositeController))]
		[TestCase(typeof(CompositeViewComponent))]
		public void Should_resolve_Composite_Transient_from_WindsorContainer(Type compositeType)
		{
			testContext.WindsorContainer.Register(Component.For(compositeType).LifestyleTransient());

			Assert.DoesNotThrow(() =>
			{
				testContext.WindsorContainer.Resolve(compositeType);
			});
		}

		[TestCase(typeof(CompositeTagHelper))]
		[TestCase(typeof(CompositeController))]
		[TestCase(typeof(CompositeViewComponent))]
		public void Should_resolve_Composite_Singleton_CrossWired_from_ServiceProvider(Type compositeType)
		{
			testContext.WindsorContainer.Register(Component.For(compositeType).CrossWired().LifestyleSingleton());

			Assert.DoesNotThrow(() =>
			{
				using (var sp = testContext.ServiceCollection.BuildServiceProvider())
				{
					sp.GetRequiredService(compositeType);
				}
			});
		}

		[TestCase(typeof(CompositeTagHelper))]
		[TestCase(typeof(CompositeController))]
		[TestCase(typeof(CompositeViewComponent))]
		public void Should_resolve_Composite_Scoped_CrossWired_from_ServiceProvider(Type compositeType)
		{
			testContext.WindsorContainer.Register(Component.For(compositeType).CrossWired().LifestyleScoped());

			Assert.DoesNotThrow(() =>
			{
				using (var sp = testContext.ServiceCollection.BuildServiceProvider())
				{
					sp.GetRequiredService(compositeType);
				}
			});
		}

		[TestCase(typeof(CompositeTagHelper))]
		[TestCase(typeof(CompositeController))]
		[TestCase(typeof(CompositeViewComponent))]
		public void Should_resolve_Composite_Transient_CrossWired_from_ServiceProvider(Type compositeType)
		{
			testContext.WindsorContainer.Register(Component.For(compositeType).CrossWired().LifestyleTransient());

			Assert.DoesNotThrow(() =>
			{
				using (var sp = testContext.ServiceCollection.BuildServiceProvider())
				{
					sp.GetRequiredService(compositeType);
				}
			});
		}

		[Test]
		public void Should_resolve_Multiple_Transient_CrossWired_from_ServiceProvider()
		{
			testContext.WindsorContainer.Register(Types.FromAssemblyContaining<AuthorisationHandlerOne>()
				.BasedOn<Microsoft.AspNetCore.Authorization.IAuthorizationHandler>().WithServiceBase()
				.LifestyleTransient().Configure(c => c.CrossWired()));

			using (var sp = testContext.ServiceCollection.BuildServiceProvider())
			{
				var services = sp.GetServices<Microsoft.AspNetCore.Authorization.IAuthorizationHandler>();

				Assert.That(services, Has.Exactly(3).Items);
				Assert.That(services.Select(s => s.GetType()), Is.Unique);
			}
		}

		[TestCase(LifestyleType.Bound)]
		[TestCase(LifestyleType.Custom)]
		[TestCase(LifestyleType.Pooled)]
		[TestCase(LifestyleType.Thread)]
		//[TestCase(LifestyleType.Undefined)] // Already throws System.ArgumentOutOfRangeException: Undefined is not a valid lifestyle type 
		public void Should_throw_if_CrossWired_with_these_LifestyleTypes(LifestyleType unsupportedLifestyleType)
		{
			Assert.Throws<NotSupportedException>(() =>
			{
				var componentRegistration = Component.For<AnyComponent>().CrossWired();
				componentRegistration.LifeStyle.Is(unsupportedLifestyleType);
				if (unsupportedLifestyleType == LifestyleType.Custom)
				{
					componentRegistration.LifestyleCustom<AnyComponentWithLifestyleManager>();
				}
				testContext.WindsorContainer.Register(componentRegistration);
			});
		}

		[Test]
		public void Should_throw_if_Facility_is_added_without_calling_CrossWiresInto_on_IWindsorContainer_AddFacility()
		{
			using (var container = new WindsorContainer())
			{
				Assert.Throws<InvalidOperationException>(() =>
				{
					container.AddFacility<AspNetCoreFacility>();
				});
			}
		}

		[Test] // https://github.com/castleproject/Windsor/issues/411
		public void Should_resolve_IMiddleware_from_Windsor()
		{
			testContext.WindsorContainer.GetFacility<AspNetCoreFacility>().RegistersMiddlewareInto(testContext.ApplicationBuilder);

			testContext.WindsorContainer.Register(Component.For<AnyMiddleware>().LifestyleScoped().AsMiddleware());

			Assert.DoesNotThrow(() => { testContext.WindsorContainer.Resolve<AnyMiddleware>(); });
		}

		[Test] // https://github.com/castleproject/Windsor/issues/411
		public void Should_resolve_IMiddleware_from_Windsor_with_custom_dependencies()
		{
			testContext.WindsorContainer.GetFacility<AspNetCoreFacility>().RegistersMiddlewareInto(testContext.ApplicationBuilder);

			testContext.WindsorContainer.Register(Component.For<AnyMiddleware>().DependsOn(Dependency.OnValue<AnyComponent>(new AnyComponent())).LifestyleScoped().AsMiddleware());

			Assert.DoesNotThrow(() => { testContext.WindsorContainer.Resolve<AnyMiddleware>(); });
		}
	}
}