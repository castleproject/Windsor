﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
	using Castle.Windsor;
	using Castle.Windsor.Diagnostics;

	using CastleTests.ClassComponents;
	using CastleTests.Components;

	using NUnit.Framework;
	
	public class AllComponentsDiagnosticTestCase : AbstractContainerTestCase
	{
		private IAllComponentsDiagnostic diagnostic;

		protected override void AfterContainerCreated()
		{
			var host = (IDiagnosticsHost)Kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
			diagnostic = host.GetDiagnostic<IAllComponentsDiagnostic>();
		}

		[Test]
		public void Doesnt_include_closed_versions_of_generic_handler()
		{
			Container.Register(Component.For(typeof(GenericImpl1<>)));
			Container.Resolve<GenericImpl1<A>>();
			Container.Resolve<GenericImpl1<B>>();

			var handlers = diagnostic.Inspect();

			Assert.AreEqual(1, handlers.Length);
		}

		[Test]
		public void Shows_also_components_from_parent_container()
		{
			var parent = new WindsorContainer();
			parent.Register(Component.For<A>(),
			                Component.For<B>());
			Container.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)),
			                   Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)));

			parent.AddChildContainer(Container);

			var handlers = diagnostic.Inspect();

			Assert.AreEqual(4, handlers.Length);
		}

		[Test]
		public void Works_with_empty_container()
		{
			var handlers = diagnostic.Inspect();

			Assert.IsEmpty(handlers);
		}

		[Test]
		public void Works_with_generic_handlers()
		{
			Container.Register(Component.For(typeof(GenericImpl1<>)));

			var handlers = diagnostic.Inspect();

			Assert.AreEqual(1, handlers.Length);
		}

		[Test]
		public void Works_with_multi_service_components()
		{
			Container.Register(Component.For<IEmptyService, EmptyServiceA>()
			                   	.ImplementedBy<EmptyServiceA>());

			var handlers = diagnostic.Inspect();

			Assert.AreEqual(1, handlers.Length);
			Assert.AreEqual(2, handlers[0].ComponentModel.Services.Count());
		}

		[Test]
		public void Works_with_multiple_handlers_for_given_type()
		{
			Container.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)),
			                   Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)));

			var handlers = diagnostic.Inspect();

			Assert.AreEqual(2, handlers.Length);
		}
	}
}