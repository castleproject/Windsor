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

namespace Castle
{
	using Castle.Core;
	using Castle.Generics;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture]
	public class GenericImplementationWithGreaterArityThanServiceTestCase : AbstractContainerTestFixture
	{
		[Test]
		public void Can_create_component_with_simple_double_generic_impl_for_single_generic_service()
		{
			Container.Register(Component.For(typeof(Generics.IRepository<>)).ImplementedBy(typeof(DoubleGenericRepository<,>))
			                   	.ExtendedProperties(Property.ForKey(ComponentModel.GenericImplementationMatchingStrategy).Eq(new DuplicateGenerics())));

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
	}
}