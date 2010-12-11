// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Components;

	using NUnit.Framework;

	public class GenericVarianceTestCase : AbstractContainerTestFixture
	{
		[Test]
		public void ResolveAll_can_resolve_contravariant_components()
		{
			Container.Register(Component.For<IAmContravariant<EmptyBase>>().ImplementedBy<ContravariantBase>(),
			                   Component.For<IAmContravariant<EmptySub1>>().ImplementedBy<ContravariantDerived>());

			var convariantOfDerived = Container.ResolveAll<IAmContravariant<EmptySub1>>();
			Assert.AreEqual(2, convariantOfDerived.Length);
		}
	}
}