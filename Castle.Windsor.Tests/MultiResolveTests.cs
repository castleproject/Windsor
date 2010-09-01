// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

	[TestFixture]
	public class MultiResolveTests
	{
		[Test]
		public void CanResolveMoreThanSingleComponentForService()
		{
			IWindsorContainer container = new WindsorContainer()
				.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
				          Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());
			IEmptyService[] clocks = container.ResolveAll<IEmptyService>();

			Assert.AreEqual(2, clocks.Length);
		}

		[Test]
		public void MultiResolveWillResolveInRegistrationOrder()
		{
			IWindsorContainer container = new WindsorContainer()
				.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
						  Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());

			IEmptyService[] clocks = container.ResolveAll<IEmptyService>();

			Assert.AreEqual(typeof(EmptyServiceA), clocks[0].GetType());
			Assert.AreEqual(typeof(EmptyServiceB), clocks[1].GetType());

			//reversing order

			container = new WindsorContainer()
				.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
						  Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());

			clocks = container.ResolveAll<IEmptyService>();

			Assert.AreEqual(typeof(EmptyServiceB), clocks[0].GetType());
			Assert.AreEqual(typeof(EmptyServiceA), clocks[1].GetType());
		}

		[Test]
		public void CanUseMutliResolveWithGenericSpecialization()
		{
			IWindsorContainer container = new WindsorContainer()
				.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(DemoRepository<>)),
				          Component.For(typeof(IRepository<>)).ImplementedBy(typeof(TransientRepository<>)));

			var resolve = container.Resolve<IRepository<IEmptyService>>();
			Assert.IsNotNull(resolve);
			
			IRepository<EmptyServiceA>[] repositories = container.ResolveAll<IRepository<EmptyServiceA>>();
			Assert.AreEqual(2, repositories.Length);
		}
	}
}