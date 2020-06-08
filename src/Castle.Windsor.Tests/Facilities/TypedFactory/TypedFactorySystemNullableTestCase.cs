// Copyright 2020 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Facilities.TypedFactory
{
	using System;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactorySystemNullableTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.AddFacility<TypedFactoryFacility>();
		}

		[Test]
		public void Null_may_be_specified_through_typed_factory_for_non_optional_System_Nullable_constructor_parameter()
		{
			Container.Register(
				Component.For<DependencyFromContainer>(),
				Component.For<ComponentWithNonOptionalNullableParameter>(),
				Component.For<ComponentWithNonOptionalNullableParameter.Factory>().AsFactory());

			var factory = Container.Resolve<ComponentWithNonOptionalNullableParameter.Factory>();
			factory.Invoke(nonOptionalNullableParameter: null);
		}

		[Test]
		public void Non_optional_System_Nullable_constructor_parameter_is_still_required()
		{
			Container.Register(
				Component.For<DependencyFromContainer>(),
				Component.For<ComponentWithNonOptionalNullableParameter>(),
				Component.For<Func<ComponentWithNonOptionalNullableParameter>>().AsFactory());

			var factory = Container.Resolve<Func<ComponentWithNonOptionalNullableParameter>>();

			Assert.That(() => factory.Invoke(), Throws.InstanceOf<DependencyResolverException>()
				.With.Property("Message").EqualTo($"Could not resolve non-optional dependency for '{typeof(ComponentWithNonOptionalNullableParameter)}' ({typeof(ComponentWithNonOptionalNullableParameter)}). Parameter 'nonOptionalNullableParameter' type '{typeof(int?).FullName}'"));
		}

		public sealed class DependencyFromContainer
		{
		}

		public sealed class ComponentWithNonOptionalNullableParameter
		{
			public delegate ComponentWithNonOptionalNullableParameter Factory(int? nonOptionalNullableParameter);

			public ComponentWithNonOptionalNullableParameter(int? nonOptionalNullableParameter, DependencyFromContainer dependencyFromContainer)
			{
			}
		}
	}
}
