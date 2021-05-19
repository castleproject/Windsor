// Copyright 2004-2021 Castle Project - http://www.castleproject.org/
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
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class NestedGenericsTestCase : AbstractContainerTestCase
	{

		[Test]
		public void Implementation_with_single_generic_parameter_that_is_used_in_double_generic()
		{
			Container.Register(
				Component.For(typeof(IDoubleGeneric<,>))
					.ImplementedBy(typeof(Foo<>)));

			var instance = Container.Resolve<IDoubleGeneric<object, IGenericTypeWithConstraint<object>>>();
			IDoubleGeneric<object, IGenericTypeWithConstraint<object>> manualInstance = new Foo<object>();

			Assert.IsInstanceOf<IDoubleGeneric<object, IGenericTypeWithConstraint<object>>>(instance);
		}
	}

	public class Foo<T> : IDoubleGeneric<T, IGenericTypeWithConstraint<T>> where T : class {}

	public interface IGenericTypeWithConstraint<T> where T : class{}
}
