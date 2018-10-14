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

namespace Castle.Windsor.Tests
{
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.ClassComponents;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	public class ByRefDependenciesTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_resolve_type_with_by_ref_dependency()
		{
			Container.Register(Component.For<A>(),
			                   Component.For<HasByRefCtorArgument>());

			Container.Resolve<HasByRefCtorArgument>();
		}

		[Test]
		public void Can_resolve_type_with_by_ref_dependency_provided_inline()
		{
			Container.Register(Component.For<HasByRefCtorArgument>());

			Container.Resolve<HasByRefCtorArgument>(Arguments.FromProperties(new { a = new A() }));
		}

		[Test]
		public void Can_resolve_type_with_by_ref_dependency_provided_inline_via_anonymous_type()
		{
			Container.Register(Component.For<HasByRefCtorArgument>());

			Container.Resolve<HasByRefCtorArgument>(Arguments.FromProperties(new { a = new A() }));
		}
	}
}