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

namespace Castle.MicroKernel.Tests.Pools
{
	using System.Collections.Generic;

	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture]
	public class PooledLifestyleManagerTestCase : AbstractContainerTestFixture
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