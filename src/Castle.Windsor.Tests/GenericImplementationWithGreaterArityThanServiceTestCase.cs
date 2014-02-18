// Copyright 2004-2013 Castle Project - http://www.castleproject.org/
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
	using System;

	using Castle.Core.Internal;
	using Castle.Generics;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using CastleTests.Components;
	using CastleTests.TestImplementationsOfExtensionPoints;

	using NUnit.Framework;

	[TestFixture]
	public class GenericImplementationWithGreaterArityThanServiceTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_create_component_with_generic_impl_for_non_generic_services()
		{
			Container.Register(Component.For<IService>().ImplementedBy(typeof(ServiceImplGeneric<>), new UseStringGenericStrategy()));

			var item = Container.Resolve<IService>();

			Assert.IsInstanceOf<ServiceImplGeneric<string>>(item);
		}

		[Test]
		public void Can_create_component_with_simple_double_generic_impl_for_multi_class_registration()
		{
			Container.Register(
				Classes.FromThisAssembly().BasedOn(typeof(Generics.IRepository<>))
					.If(t => t == typeof(DoubleGenericRepository<,>))
					.WithServiceBase()
					.Configure(
						c => c.ExtendedProperties(
							Property.ForKey(Constants.GenericImplementationMatchingStrategy)
								.Eq(new DuplicateGenerics()))));

			var repository = Container.Resolve<Generics.IRepository<A>>();

			Assert.IsInstanceOf<DoubleGenericRepository<A, A>>(repository);
		}

		[Test]
		public void Can_create_component_with_simple_double_generic_impl_for_single_generic_service()
		{
			Container.Register(Component.For(typeof(Generics.IRepository<>)).ImplementedBy(typeof(DoubleGenericRepository<,>))
				.ExtendedProperties(Property.ForKey(Constants.GenericImplementationMatchingStrategy).Eq(new DuplicateGenerics())));

			var repository = Container.Resolve<Generics.IRepository<A>>();

			Assert.IsInstanceOf<DoubleGenericRepository<A, A>>(repository);
		}

		[Test]
		public void Can_create_component_with_simple_double_generic_impl_for_single_generic_service_via_ImplementedBy()
		{
			Container.Register(Component.For(typeof(Generics.IRepository<>)).ImplementedBy(typeof(DoubleGenericRepository<,>), new DuplicateGenerics()));

			var repository = Container.Resolve<Generics.IRepository<A>>();

			Assert.IsInstanceOf<DoubleGenericRepository<A, A>>(repository);
		}

		[Test]
		public void Null_strategy_is_ignored()
		{
			Container.Register(Component.For(typeof(Generics.IRepository<>)).ImplementedBy(typeof(DoubleGenericRepository<,>))
				.ExtendedProperties(Property.ForKey(Constants.GenericImplementationMatchingStrategy).Eq(null)));

			var exception = Assert.Throws<HandlerException>(() =>
				Container.Resolve<Generics.IRepository<A>>());

			var message =
				string.Format(
					"Requested type CastleTests.Generics.IRepository`1[CastleTests.Components.A] has 1 generic parameter(s), whereas component implementation type Castle.Generics.DoubleGenericRepository`2[T1,T2] requires 2.{0}This means that Windsor does not have enough information to properly create that component for you.{0}You can instruct Windsor which types it should use to close this generic component by supplying an implementation of IGenericImplementationMatchingStrategy.{0}Please consult the documentation for examples of how to do that.",
					Environment.NewLine);
			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Throws_helpful_message_when_generic_matching_strategy_returns_null()
		{
			Container.Register(Component.For(typeof(Generics.IRepository<>))
				.ImplementedBy(typeof(DoubleGenericRepository<,>), new StubGenericImplementationMatchingStrategy(default(Type[]))));

			var exception = Assert.Throws<HandlerException>(() =>
				Container.Resolve<Generics.IRepository<A>>());

			var message =
				string.Format(
					"Requested type CastleTests.Generics.IRepository`1[CastleTests.Components.A] has 1 generic parameter(s), whereas component implementation type Castle.Generics.DoubleGenericRepository`2[T1,T2] requires 2.{0}This means that Windsor does not have enough information to properly create that component for you.{0}This is most likely a bug in the IGenericImplementationMatchingStrategy implementation this component uses (CastleTests.StubGenericImplementationMatchingStrategy).{0}Please consult the documentation for examples of how to implement it properly.",
					Environment.NewLine);
			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Throws_helpful_message_when_generic_matching_strategy_returns_too_few_types()
		{
			Container.Register(Component.For(typeof(Castle.MicroKernel.Tests.ClassComponents.IRepository<>))
				.ImplementedBy(typeof(DoubleRepository<,>), new StubGenericImplementationMatchingStrategy(typeof(string))));

			var exception = Assert.Throws<HandlerException>(() =>
				Container.Resolve<Castle.MicroKernel.Tests.ClassComponents.IRepository<string>>());

			var message =
				string.Format(
					"Requested type Castle.MicroKernel.Tests.ClassComponents.IRepository`1[System.String] has 1 generic parameter(s), whereas component implementation type Castle.MicroKernel.Tests.ClassComponents.DoubleRepository`2[T,T2] requires 2.{0}This means that Windsor does not have enough information to properly create that component for you.{0}This is most likely a bug in the IGenericImplementationMatchingStrategy implementation this component uses (CastleTests.StubGenericImplementationMatchingStrategy).{0}Please consult the documentation for examples of how to implement it properly.",
					Environment.NewLine);
			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Throws_helpful_message_when_generic_matching_strategy_returns_types_that_wont_work_with_the_type()
		{
			Container.Register(Component.For(typeof(Castle.MicroKernel.Tests.ClassComponents.IRepository<>))
				.ImplementedBy(typeof(DoubleRepository<,>), new StubGenericImplementationMatchingStrategy(typeof(string), typeof(IEmployee))));

			var exception = Assert.Throws<GenericHandlerTypeMismatchException>(() =>
				Container.Resolve<Castle.MicroKernel.Tests.ClassComponents.IRepository<string>>());

			var message =
				@"Types System.String, CastleTests.Components.IEmployee don't satisfy generic constraints of implementation type Castle.MicroKernel.Tests.ClassComponents.DoubleRepository`2 of component 'Castle.MicroKernel.Tests.ClassComponents.DoubleRepository`2'.this is likely a bug in the IGenericImplementationMatchingStrategy used (CastleTests.StubGenericImplementationMatchingStrategy)";
			Assert.AreEqual(message, exception.Message);
		}
	}
}