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
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Diagnostics;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	public class PotentiallyMisconfiguredComponentsDiagnosticTestCase : AbstractContainerTestCase
	{
		private IPotentiallyMisconfiguredComponentsDiagnostic diagnostic;

		protected override void AfterContainerCreated()
		{
			var host = Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey) as IDiagnosticsHost;
#if SILVERLIGHT
			host.AddDiagnostic<IPotentiallyMisconfiguredComponentsDiagnostic>(new PotentiallyMisconfiguredComponentsDiagnostic(Kernel));
#endif
			diagnostic = host.GetDiagnostic<IPotentiallyMisconfiguredComponentsDiagnostic>();
		}

		[Test]
		public void Empty_when_all_components_healthy()
		{
			Container.Register(Component.For<A>(), Component.For<B>(), Component.For<C>());

			var handlers = diagnostic.Inspect();

			Assert.IsEmpty(handlers);
		}

		[Test]
		public void Has_all_components_with_missing_or_waiting_dependencies()
		{
			Container.Register(Component.For<B>(), Component.For<C>());

			var handlers = diagnostic.Inspect();

			Assert.AreEqual(2, handlers.Length);
		}
	}
}