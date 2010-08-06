// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.ClassComponents;
	using Castle.Windsor.Tests.Components;
	using Castle.Windsor.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class FailureCleanUpTestCase
	{
		[SetUp]
		public void Init()
		{
			container = new WindsorContainer();
		}

		private IWindsorContainer container;

		[Test]
		public void When_constructor_throws_ctor_dependencies_get_released()
		{
			SimpleServiceDisposable.DisposeCount = 0;
			container.Register(
				Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>().LifeStyle.Transient,
				Component.For<ThrowsInCtorWithDisposableDependency>()
				);

			Assert.Throws<ComponentActivatorException>(() => container.Resolve<ThrowsInCtorWithDisposableDependency>());
			Assert.AreEqual(1, SimpleServiceDisposable.DisposeCount);
		}

		[Test]
		public void When_constructor_dependency_throws_previous_dependencies_get_released()
		{
			SimpleServiceDisposable.DisposeCount = 0;
			container.Register(
				Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>().LifeStyle.Transient,
				Component.For<ThrowsInCtor>().LifeStyle.Transient,
				Component.For<DependsOnThrowingComponent>()
				);

			Assert.Throws<ComponentActivatorException>(() => container.Resolve<DependsOnThrowingComponent>());
			Assert.AreEqual(1, SimpleServiceDisposable.DisposeCount);
		}

		[Test]
		public void When_interceptor_throws_previous_dependencies_get_released()
		{
			SimpleServiceDisposable.DisposeCount = 0;
			container.Register(
				Component.For<ThrowInCtorInterceptor>().LifeStyle.Transient,
				Component.For<ISimpleService>().ImplementedBy<SimpleServiceDisposable>().LifeStyle.Transient
					.Interceptors<ThrowInCtorInterceptor>()
				);

			Assert.Throws<ComponentActivatorException>(() => container.Resolve<ISimpleService>());
			Assert.AreEqual(1, SimpleServiceDisposable.DisposeCount);
		}
	}
}