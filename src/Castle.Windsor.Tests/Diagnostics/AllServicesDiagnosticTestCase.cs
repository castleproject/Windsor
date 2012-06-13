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
	using Castle.Windsor.Diagnostics;

	using CastleTests.ClassComponents;
	using CastleTests.Components;

	using NUnit.Framework;

#if !SILVERLIGHT
	// althought diagnostics are available in Silverlight they are not installed by default.

	public class AllServicesDiagnosticTestCase : AbstractContainerTestCase
	{
		private IAllServicesDiagnostic diagnostic;

		protected override void AfterContainerCreated()
		{
			var host = (IDiagnosticsHost)Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
			diagnostic = host.GetDiagnostic<IAllServicesDiagnostic>();
		}

		[Test]
		[Ignore("No nice and robust way of supporting that.")]
		public void Default_component_for_given_service_comes_first()
		{
			Container.Register(Component.For<IEmptyService, EmptyServiceA>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().IsDefault(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
			                   Component.For<A>());

			var services = diagnostic.Inspect();
			Assert.AreEqual(typeof(EmptyServiceB), services[typeof(IEmptyService)].First().ComponentModel.Implementation);
		}

		[Test]
		public void Groups_components_by_exposed_service()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                   Component.For<A>());

			var services = diagnostic.Inspect();
			Assert.AreEqual(2, services.Count);
			Assert.AreEqual(2, services[typeof(IEmptyService)].Count());
			Assert.AreEqual(1, services[typeof(A)].Count());
		}

		[Test]
		public void Open_generic_handlers_appear_once()
		{
			Container.Register(Component.For(typeof(GenericImpl1<>)));
			Container.Resolve<GenericImpl1<A>>();
			Container.Resolve<GenericImpl1<B>>();

			var services = diagnostic.Inspect();
			Assert.AreEqual(1, services.Count);
			Assert.IsTrue(services.Contains(typeof(GenericImpl1<>)));
		}

		[Test]
		public void Works_for_multi_service_components()
		{
			Container.Register(Component.For<IEmptyService, EmptyServiceA>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                   Component.For<A>());

			var services = diagnostic.Inspect();
			Assert.AreEqual(3, services.Count);
		}
	}
#endif
}