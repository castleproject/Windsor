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
	using System;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Debugging;
	using Castle.Windsor.Debugging.Extensions;

	using NUnit.Framework;

	[TestFixture]
	public class MultiResolveTests
	{
		[Test]
		public void CanResolveMoreThanSingleComponentForService()
		{
			IWindsorContainer container = ((IWindsorContainer)new WindsorContainer()).Register(Component.For(typeof(IClock)).ImplementedBy(typeof(IsraelClock))).Register(Component.For(typeof(IClock)).ImplementedBy(typeof(WorldClock)));

			IClock[] clocks = container.ResolveAll<IClock>();

			Assert.AreEqual(2, clocks.Length);
		}

		[Test]
		public void MultiResolveWillResolveInRegistrationOrder()
		{
			IWindsorContainer container = ((IWindsorContainer)new WindsorContainer()).Register(Component.For(typeof(IClock)).ImplementedBy(typeof(IsraelClock))).Register(Component.For(typeof(IClock)).ImplementedBy(typeof(WorldClock)));

			IClock[] clocks = container.ResolveAll<IClock>();

			Assert.AreEqual(typeof(IsraelClock), clocks[0].GetType());
			Assert.AreEqual(typeof(WorldClock), clocks[1].GetType());

			//reversing order
		    container = ((IWindsorContainer)new WindsorContainer()).Register(Component.For(typeof(IClock)).ImplementedBy(typeof(WorldClock))).Register(Component.For(typeof(IClock)).ImplementedBy(typeof(IsraelClock)));

		    clocks = container.ResolveAll<IClock>();

			Assert.AreEqual(typeof(WorldClock), clocks[0].GetType());
			Assert.AreEqual(typeof(IsraelClock), clocks[1].GetType());
		}

		[Test]
		public void CanUseMutliResolveWithGenericSpecialization()
		{
			var windsorContainer = new WindsorContainer();
			windsorContainer.Kernel.AddSubSystem("Castle.DebuggingSubSystem", new ContainerDebuggerExtensionHost());
			IWindsorContainer container = windsorContainer
				.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(DemoRepository<>)).Named("demo"),
				          Component.For(typeof(IRepository<>)).ImplementedBy(typeof(TransientRepository<>)).Named("trans"));

			IRepository<IClock> resolve = container.Resolve<IRepository<IClock>>();
			Assert.IsNotNull(resolve);

			IRepository<IsraelClock>[] repositories = container.ResolveAll<IRepository<IsraelClock>>();
			Assert.AreEqual(2, repositories.Length);
		}
	}

	public interface IClock{}
	public class IsraelClock : IClock{}
	public class WorldClock : IClock{}
	public class DependantClock : IClock
	{
		public DependantClock(IDisposable disposable)
		{
		}
	}
}