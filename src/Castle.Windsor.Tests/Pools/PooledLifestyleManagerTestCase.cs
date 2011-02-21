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

namespace Castle.MicroKernel.Tests.Pools
{
	using System.Collections.Generic;

	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture]
	public class PooledLifestyleManagerTestCase : AbstractContainerTestCase
	{
		[Test]
		public void MaxSize()
		{
			Kernel.Register(Component.For<PoolableComponent1>());

			var instances = new List<PoolableComponent1>
			{
				Kernel.Resolve<PoolableComponent1>(),
				Kernel.Resolve<PoolableComponent1>(),
				Kernel.Resolve<PoolableComponent1>(),
				Kernel.Resolve<PoolableComponent1>(),
				Kernel.Resolve<PoolableComponent1>()
			};

			var other1 = Kernel.Resolve<PoolableComponent1>();

			CollectionAssert.DoesNotContain(instances, other1);

			foreach (var inst in instances)
			{
				Kernel.ReleaseComponent(inst);
			}

			Kernel.ReleaseComponent(other1);

			var other2 = Kernel.Resolve<PoolableComponent1>();

			Assert.AreNotEqual(other1, other2);
			CollectionAssert.Contains(instances, other2);

			Kernel.ReleaseComponent(other2);
		}

		[Test]
		public void Poolable_component_is_always_tracked()
		{
			Kernel.Register(Component.For<A>().LifeStyle.Pooled);

			var component = Kernel.Resolve<A>();

			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(component));
		}

		[Test]
		public void Recyclable_component_as_dependency_can_be_reused()
		{
			Kernel.Register(Component.For<RecyclableComponent>().LifeStyle.PooledWithSize(1, null),
			                Component.For<UseRecyclableComponent>().LifeStyle.Transient);
			var component = Kernel.Resolve<UseRecyclableComponent>();
			Container.Release(component);
			var componentAgain = Kernel.Resolve<UseRecyclableComponent>();

			Assert.AreSame(componentAgain.Dependency, component.Dependency);

			Container.Release(componentAgain);
			Assert.AreEqual(2, componentAgain.Dependency.RecycledCount);
		}

		[Test]
		public void Recyclable_component_can_be_reused()
		{
			Kernel.Register(Component.For<RecyclableComponent>().LifeStyle.PooledWithSize(1, null));
			var component = Kernel.Resolve<RecyclableComponent>();
			Container.Release(component);
			var componentAgain = Kernel.Resolve<RecyclableComponent>();

			Assert.AreSame(componentAgain, component);

			Container.Release(componentAgain);
			Assert.AreEqual(2, componentAgain.RecycledCount);
		}

		[Test]
		public void Recyclable_component_gets_recycled_just_once_on_subsequent_release()
		{
			Kernel.Register(Component.For<RecyclableComponent>().LifeStyle.PooledWithSize(1, null));
			var component = Kernel.Resolve<RecyclableComponent>();

			Assert.AreEqual(0, component.RecycledCount);

			Container.Release(component);
			Container.Release(component);
			Container.Release(component);
			Assert.AreEqual(1, component.RecycledCount);
		}

		[Test]
		public void Recyclable_component_gets_recycled_on_release()
		{
			Kernel.Register(Component.For<RecyclableComponent>().LifeStyle.PooledWithSize(1, null));
			var component = Kernel.Resolve<RecyclableComponent>();

			Assert.AreEqual(0, component.RecycledCount);

			Container.Release(component);
			Assert.AreEqual(1, component.RecycledCount);
		}

		[Test]
		public void Recyclable_component_with_on_release_action_not_released_more_than_necessary()
		{
			var count = 0;
			Kernel.Register(Component.For<RecyclableComponent>().LifeStyle.PooledWithSize(1, null)
			                	.DynamicParameters((k, d) =>
			                	{
			                		count++;
			                		return delegate { count--; };
			                	}));

			RecyclableComponent component = null;
			for (var i = 0; i < 10; i++)
			{
				component = Kernel.Resolve<RecyclableComponent>();
				Container.Release(component);
			}
			Assert.AreEqual(10, component.RecycledCount);
			Assert.AreEqual(0, count);
		}

		[Test]
		public void SimpleUsage()
		{
			Kernel.Register(Component.For<PoolableComponent1>());

			var inst1 = Kernel.Resolve<PoolableComponent1>();
			var inst2 = Kernel.Resolve<PoolableComponent1>();

			Kernel.ReleaseComponent(inst2);
			Kernel.ReleaseComponent(inst1);

			var other1 = Kernel.Resolve<PoolableComponent1>();
			var other2 = Kernel.Resolve<PoolableComponent1>();

			Assert.AreSame(inst1, other1);
			Assert.AreSame(inst2, other2);

			Kernel.ReleaseComponent(inst2);
			Kernel.ReleaseComponent(inst1);
		}
	}
}