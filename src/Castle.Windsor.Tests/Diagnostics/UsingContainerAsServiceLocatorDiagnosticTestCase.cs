// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Diagnostics
{
	using System;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;
	using Castle.Windsor;
	using Castle.Windsor.Diagnostics;

	using CastleTests.Components;
	using CastleTests.Interceptors;

	using NUnit.Framework;

	public class UsingContainerAsServiceLocatorDiagnosticTestCase : AbstractContainerTestCase
	{
		private IUsingContainerAsServiceLocatorDiagnostic diagnostic;

		protected override void AfterContainerCreated()
		{
			var host = Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IDiagnosticsHost;
#if SILVERLIGHT
			host.AddDiagnostic<IUsingContainerAsServiceLocatorDiagnostic>(new UsingContainerAsServiceLocatorDiagnostic(Kernel));
#endif
			diagnostic = host.GetDiagnostic<IUsingContainerAsServiceLocatorDiagnostic>();
		}

		[TestCase(typeof(IKernel))]
		[TestCase(typeof(IKernelInternal))]
		[TestCase(typeof(IKernelEvents))]
		[TestCase(typeof(IWindsorContainer))]
		[TestCase(typeof(DefaultKernel))]
		[TestCase(typeof(WindsorContainer))]
		public void Detects_ctor_dependency_on(Type type)
		{
			var generic = typeof(GenericWithCtor<>).MakeGenericType(type);
			Container.Register(Component.For(generic),
			                   Component.For<A>());

			var serviceLocators = diagnostic.Inspect();
			Assert.AreEqual(1, serviceLocators.Length);
		}

		[TestCase(typeof(IKernel))]
		[TestCase(typeof(IKernelInternal))]
		[TestCase(typeof(IKernelEvents))]
		[TestCase(typeof(IWindsorContainer))]
		[TestCase(typeof(DefaultKernel))]
		[TestCase(typeof(WindsorContainer))]
		public void Detects_property_dependency_on(Type type)
		{
			var generic = typeof(GenericWithProperty<>).MakeGenericType(type);
			Container.Register(Component.For(generic),
			                   Component.For<A>());

			var serviceLocators = diagnostic.Inspect();
			Assert.AreEqual(1, serviceLocators.Length);
		}

		[Test]
		public void Ignores_interceptors()
		{
			Container.Register(
				Component.For<DependsOnTViaCtorInterceptor<IKernel>>().Named("a"),
				Component.For<DependsOnTViaPropertyInterceptor<IKernel>>().Named("b"),
				Component.For<B>().Interceptors("a"),
				Component.For<A>().Interceptors("b"));

			var serviceLocators = diagnostic.Inspect();
			Assert.IsEmpty(serviceLocators);
		}
		
#if !DOTNET35
		[Test]
		public void Ignores_lazy()
		{
			Container.Register(Component.For<ILazyComponentLoader>()
			                   	.ImplementedBy<LazyOfTComponentLoader>());
			Container.Register(Component.For<B>(),
			                   Component.For<A>());

			Container.Resolve<Lazy<B>>(); // to trigger lazy registration of lazy

			var serviceLocators = diagnostic.Inspect();
			Assert.IsEmpty(serviceLocators);
		}
#endif

		[Test]
		public void Successfully_handles_cases_with_no_SL_usages()
		{
			Container.Register(Component.For<B>(),
			                   Component.For<A>());

			var serviceLocators = diagnostic.Inspect();
			Assert.IsEmpty(serviceLocators);
		}
	}
}