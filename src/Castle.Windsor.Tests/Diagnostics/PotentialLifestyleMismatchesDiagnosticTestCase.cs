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
	using System.Linq;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Diagnostics;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.ClassComponents;
	using Castle.Windsor.Tests.Interceptors;

	using CastleTests.Components;

	using NUnit.Framework;

	public class PotentialLifestyleMismatchesDiagnosticTestCase : AbstractContainerTestCase
	{
		private IPotentialLifestyleMismatchesDiagnostic diagnostic;

		protected override void AfterContainerCreated()
		{
			var host = Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IDiagnosticsHost;
#if SILVERLIGHT
			host.AddDiagnostic<IPotentialLifestyleMismatchesDiagnostic>(new PotentialLifestyleMismatchesDiagnostic(Kernel));
#endif
			diagnostic = host.GetDiagnostic<IPotentialLifestyleMismatchesDiagnostic>();
		}

		[Test]
		public void Can_detect_singleton_depending_on_transient()
		{
			Container.Register(Component.For<B>().LifeStyle.Singleton,
			                   Component.For<A>().LifeStyle.Transient);

			var mismatches = diagnostic.Inspect();
			Assert.AreEqual(1, mismatches.Length);
		}

		[Test]
		public void Can_detect_singleton_depending_on_transient_directly_and_indirectly()
		{
			Container.Register(Component.For<CBA>().LifeStyle.Singleton,
			                   Component.For<B>().LifeStyle.Singleton,
			                   Component.For<A>().LifeStyle.Transient);

			var items = diagnostic.Inspect();
			Assert.AreEqual(3, items.Length);
			var cbaMismatches = items.Where(i => i.First().Services.Single() == typeof(CBA)).ToArray();
			Assert.AreEqual(2, cbaMismatches.Length);
		}

		[Test]
		public void Can_detect_singleton_depending_on_transient_indirectly()
		{
			Container.Register(Component.For<C>().LifeStyle.Singleton,
			                   Component.For<B>().LifeStyle.Singleton,
			                   Component.For<A>().LifeStyle.Transient);

			var mismatches = diagnostic.Inspect();
			Assert.AreEqual(2, mismatches.Length);
		}

		[Test]
		public void Can_detect_singleton_depending_on_transient_indirectly_via_custom_lifestyle()
		{
			Container.Register(Component.For<C>().LifeStyle.Singleton,
			                   Component.For<B>().LifeStyle.Custom<CustomLifestyleManager>(),
			                   Component.For<A>().LifeStyle.Transient);

			var mismatches = diagnostic.Inspect();
			Assert.AreEqual(1, mismatches.Length);
		}

		[Test]
		public void Can_detect_singleton_depending_on_two_transients_directly_and_indirectly()
		{
			Container.Register(Component.For<CBA>().LifeStyle.Singleton,
			                   Component.For<B>().LifeStyle.Transient,
			                   Component.For<A>().LifeStyle.Transient);

			var items = diagnostic.Inspect();
			Assert.AreEqual(2, items.Length);
			var cbaMismatches = items.Where(i => i.First().Services.Single() == typeof(CBA)).ToArray();
			Assert.AreEqual(2, cbaMismatches.Length);
		}

		[Test(Description = "When failing this test causes stack overflow")]
		public void Can_handle_dependency_cycles()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecoratorViaProperty>());

			var mismatches = diagnostic.Inspect();
			Assert.IsEmpty(mismatches);
		}

		[Test]
		public void Decorators_dont_trigger_stack_overflow()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<UsesIEmptyService>());
			var items = diagnostic.Inspect();
			Assert.IsEmpty(items);
		}

		[Test(Description = "If the test fails, StackOverflowException is thrown")]
		public void Does_not_crash_on_dependency_cycles()
		{
			Container.Register(Component.For<InterceptorThatCauseStackOverflow>().Named("interceptor"),
			                   Component.For<ICameraService>().ImplementedBy<CameraService>().Interceptors<InterceptorThatCauseStackOverflow>(),
			                   Component.For<ICameraService>().ImplementedBy<CameraService>().Named("ok to resolve - has no interceptors"));
			var items = diagnostic.Inspect();
			Assert.IsEmpty(items);
		}
	}
}
