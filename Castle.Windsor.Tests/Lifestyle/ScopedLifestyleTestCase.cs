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

namespace Castle.Lifestyle
{
	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	public class ScopedLifestyleTestCase : AbstractContainerTestCase
	{
		protected override WindsorContainer BuildContainer()
		{
			var container = new WindsorContainer();
			container.Kernel.AddSubSystem("scope", new ScopeSubsystem(new ThreadScopeAccessor()));
			return container;
		}

		[Test]
		public void Resolve_scoped_component_within_a_scope_successful()
		{
			Container.Register(Component.For<A>().LifeStyle.Scoped());
			using (Container.BeginScope())
			{
				Container.Resolve<A>();
			}
		}

		[Test]
		public void Resolve_scoped_component_without_a_scope_throws_helpful_exception()
		{
			Container.Register(Component.For<A>().LifeStyle.Scoped());

			var exception = Assert.Throws<ComponentResolutionException>(() =>
			                                                            Container.Resolve<A>());

			Assert.AreEqual(
				"Component 'Castle.Windsor.Tests.A' has scoped lifestyle, and it could not be resolved because no scope is accessible.  Did you forget to call container.BeginScope()?",
				exception.Message);
		}
	}
}