// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace CastleTests
{
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Components;
	using Castle.Windsor.Tests.Interceptors;

	using CastleTests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class GenericMultipleServicesComponentResolutionTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.Register(
				Component.For<CountingInterceptor>().LifeStyle.Transient,
				Component.For(new[] {typeof (IGeneric<>), typeof (IGenericExtended<>)})
					.ImplementedBy(typeof (GenericExtendedImpl<>))
					.Interceptors<CountingInterceptor>(),
				Component.For<UseGenericExtended1>(),
				Component.For<UseGenericExtended2>());
		}

		[Test]
		public void Can_resolve_generic_component_exposing_two_unrelated_implemented_services()
		{
			Container.Register(
				Component.For(typeof (IGeneric<>), typeof (IDummyComponent<>))
					.ImplementedBy(typeof (GenericDummyComponentImpl<>)).IsDefault());

			var generic = Container.Resolve<IGeneric<string>>();
			var dummy = Container.Resolve<IDummyComponent<string>>();

			Assert.AreSame(generic, dummy);
		}

		[Test]
		public void Can_resolve_generic_component_exposing_two_unrelated_implemented_services_each_closed_over_different_generic_argument()
		{
			Container.Register(
				Component.For(typeof (IGeneric<>), typeof (IDummyComponent<>))
					.ImplementedBy(typeof (GenericDummyComponentImpl<,>), new DuplicateGenerics()).IsDefault());

			var generic = Container.Resolve<IGeneric<string>>();
			var dummy = Container.Resolve<IDummyComponent<string>>();

			Assert.AreSame(generic, dummy);
		}

		[Test]
		public void Dependency_resolution_generic_proxy_should_implement_all_services()
		{
			var comp = Container.Resolve<UseGenericExtended1>();
			Assert.AreSame(comp.Generic, comp.GenericExtended);
		}

		[Test]
		public void Generic_handler_caching_should_not_affect_resolution()
		{
			var comp = Container.Resolve<UseGenericExtended2>();
			Assert.AreSame(comp.Generic, comp.GenericExtended);
		}
	}
}