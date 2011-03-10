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

namespace CastleTests.Lifestyle
{
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	public class ScopedLifestyleImplicitGraphScopingTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Kernel.AddSubSystem("scope", new ScopeSubsystem(new ThreadScopeAccessor()));
		}

		[Test]
		public void Implicitly_graph_scoped_component_instances_are_reused()
		{
			Container.Register(
				Component.For<A>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();
			Assert.AreSame(cba.A, cba.B.A);
		}
	}
}