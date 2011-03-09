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
	using System;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.ClassComponents;

	using CastleTests.Components;

	using NUnit.Framework;

	public class ScopedLifestyleTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Kernel.AddSubSystem("scope", new ScopeSubsystem(new ThreadScopeAccessor()));
		}

		[Test]
		public void Can_apply_scoped_lifestyle_via_attribute()
		{
			Container.Register(Component.For<ScopedComponent>());

			var handler = Kernel.GetHandler(typeof(ScopedComponent));
			Assert.AreEqual(LifestyleType.Scoped, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Ending_scope_releases_component()
		{
			DisposableFoo.DisposedCount = 0;

			Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped);

			using (Container.BeginScope())
			{
				Container.Resolve<DisposableFoo>();
			}

			Assert.AreEqual(1, DisposableFoo.DisposedCount);
		}

		[Test]
		public void Resolve_scoped_component_within_a_scope_successful()
		{
			Container.Register(Component.For<A>().LifeStyle.Scoped);
			using (Container.BeginScope())
			{
				Container.Resolve<A>();
			}
		}

		[Test]
		public void Resolve_scoped_component_within_a_scope_successful_registered_via_attribute()
		{
			Container.Register(Component.For<ScopedComponent>());
			using (Container.BeginScope())
			{
				Container.Resolve<ScopedComponent>();
			}
		}

		[Test]
		public void Resolve_scoped_component_without_a_scope_throws_helpful_exception()
		{
			Container.Register(Component.For<A>().LifeStyle.Scoped);

			var exception = Assert.Throws<ComponentResolutionException>(() =>
			                                                            Container.Resolve<A>());

			Assert.AreEqual(
				"Component 'Castle.Windsor.Tests.A' has scoped lifestyle, and it could not be resolved because no scope is accessible.  Did you forget to call container.BeginScope()?",
				exception.Message);
		}

		[Test]
		[Ignore("This fails... not sure what the behavior should be... that has to be discussed based on some real life usages")]
		public void Scoped_component_instance_from_outer_scope_is_reused_within_nested_scope()
		{
			Container.Register(Component.For<A>().LifeStyle.Scoped);

			using (Container.BeginScope())
			{
				var a1 = Container.Resolve<A>();
				using (Container.BeginScope())
				{
					var a2 = Container.Resolve<A>();
					Assert.AreSame(a1, a2);
				}
			}
		}

		[Test]
		public void Scoped_component_instance_is_reused_within_the_scope()
		{
			Container.Register(Component.For<A>().LifeStyle.Scoped);

			using (Container.BeginScope())
			{
				var a1 = Container.Resolve<A>();
				var a2 = Container.Resolve<A>();
				Assert.AreSame(a1, a2);
			}
		}

		[Test]
		public void Scoped_component_is_bound_to_the_innermost_scope()
		{
			DisposableFoo.DisposedCount = 0;

			Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped);

			using (Container.BeginScope())
			{
				using (Container.BeginScope())
				{
					Container.Resolve<DisposableFoo>();
					Assert.AreEqual(0, DisposableFoo.DisposedCount);
				}
				Assert.AreEqual(1, DisposableFoo.DisposedCount);
			}
			Assert.AreEqual(1, DisposableFoo.DisposedCount);
		}

		[Test]
		public void Scoped_component_is_not_released_by_call_to_container_Release()
		{
			DisposableFoo foo;
			DisposableFoo.DisposedCount = 0;

			Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped);

			using (Container.BeginScope())
			{
				foo = Container.Resolve<DisposableFoo>();
				Container.Release(foo);
				Assert.AreEqual(0, DisposableFoo.DisposedCount);
			}
		}

		[Test]
		public void Scoped_component_is_not_tracked_by_the_release_policy()
		{
			DisposableFoo foo;
			DisposableFoo.DisposedCount = 0;

			Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped);

			using (Container.BeginScope())
			{
				foo = Container.Resolve<DisposableFoo>();
				Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(foo));
			}
		}

		[Test]
		public void Transient_depending_on_scoped_component_is_not_tracked_by_the_container()
		{
			Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped,
			                   Component.For<UsesDisposableFoo>().LifeStyle.Transient);

			using (Container.BeginScope())
			{
				var udf = Container.Resolve<UsesDisposableFoo>();
				var weakUdt = new WeakReference(udf);
				udf = null;
				GC.Collect();
				Assert.IsFalse(weakUdt.IsAlive);
			}
		}

		[Test]
		public void Transient_depending_on_scoped_component_is_not_tracked_by_the_release_policy()
		{
			Container.Register(Component.For<DisposableFoo>().LifeStyle.Scoped,
			                   Component.For<UsesDisposableFoo>().LifeStyle.Transient);

			using (Container.BeginScope())
			{
				var udf = Container.Resolve<UsesDisposableFoo>();
				Assert.IsFalse(Kernel.ReleasePolicy.HasTrack(udf));
			}
		}
	}
}