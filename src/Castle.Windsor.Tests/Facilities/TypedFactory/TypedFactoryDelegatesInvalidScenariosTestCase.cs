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

namespace CastleTests.Facilities.TypedFactory
{
	using System;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryDelegatesInvalidScenariosTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.AddFacility<TypedFactoryFacility>();
		}

		[Test]
		public void Dependency_on_Func_of_bool_is_not_satisfied()
		{
			Container.Register(Component.For<HasFuncProperty<bool>>());

			var item = Container.Resolve<HasFuncProperty<bool>>();

			Assert.IsNull(item.Function);
		}

		[Test]
		public void Dependency_on_Func_of_string_is_not_satisfied()
		{
			Container.Register(Component.For<HasFuncProperty<string>>());

			var item = Container.Resolve<HasFuncProperty<string>>();

			Assert.IsNull(item.Function);
		}

		[Test]
		public void Dependency_on_Func_of_string_is_not_satisfied_after_resolving_valid_func()
		{
			Container.Register(Component.For<HasFuncProperty<string>>());
			Container.Resolve<Func<A>>();
			var item = Container.Resolve<HasFuncProperty<string>>();

			Assert.IsNull(item.Function);
		}
	}
}