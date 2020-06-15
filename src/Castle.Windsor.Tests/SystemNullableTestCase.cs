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

namespace CastleTests
{
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using NUnit.Framework;

	[TestFixture]
	public class SystemNullableTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Null_may_be_specified_for_non_optional_System_Nullable_constructor_parameter()
		{
			Container.Register(
				Component.For<DependencyFromContainer>(),
				Component.For<ComponentWithNonOptionalNullableParameter>());

			Container.Resolve<ComponentWithNonOptionalNullableParameter>(
				Arguments.FromProperties(new { nonOptionalNullableParameter = (int?)null }));
		}

		[Test]
		public void Non_optional_System_Nullable_constructor_parameter_is_still_required()
		{
			Container.Register(
				Component.For<DependencyFromContainer>(),
				Component.For<ComponentWithNonOptionalNullableParameter>());

			Assert.That(
				() => Container.Resolve<ComponentWithNonOptionalNullableParameter>(),
				Throws.InstanceOf<HandlerException>()
					.With.Property("Message").EqualTo($@"Can't create component '{typeof(ComponentWithNonOptionalNullableParameter)}' as it has dependencies to be satisfied.

'{typeof(ComponentWithNonOptionalNullableParameter)}' is waiting for the following dependencies:
- Parameter 'nonOptionalNullableParameter' which was not provided. Did you forget to set the dependency?
".ConvertToEnvironmentLineEndings()));
		}

		public sealed class DependencyFromContainer
		{
		}

		public sealed class ComponentWithNonOptionalNullableParameter
		{
			public ComponentWithNonOptionalNullableParameter(int? nonOptionalNullableParameter, DependencyFromContainer dependencyFromContainer)
			{
			}
		}
	}
}
