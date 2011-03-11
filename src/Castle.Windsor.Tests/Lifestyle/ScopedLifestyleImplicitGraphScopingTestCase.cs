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
		public void Scoped_component_created_for_outermost_sub_graph()
		{
			Container.Register(
				Component.For<A>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().ImplementedBy<CBADecorator>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();
			var inner = ((CBADecorator)cba).Inner;
			Assert.AreSame(cba.A, inner.A);
		}

		[Test]
		public void Scoped_component_disposable_not_tracked()
		{
			Container.Register(
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			Assert.False(Kernel.ReleasePolicy.HasTrack(cba.A));
		}

		[Test]
		public void Scoped_component_disposable_root_tracked()
		{
			Container.Register(
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			Assert.True(Kernel.ReleasePolicy.HasTrack(cba));
		}

		[Test]
		public void Scoped_component_doesnt_unnecessarily_force_root_to_be_tracked()
		{
			Container.Register(
				Component.For<A>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			Assert.False(Kernel.ReleasePolicy.HasTrack(cba));
			Assert.False(Kernel.ReleasePolicy.HasTrack(cba.B));
		}

		[Test]
		public void Scoped_component_doesnt_unnecessarily_get_tracked()
		{
			Container.Register(
				Component.For<A>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			Assert.False(Kernel.ReleasePolicy.HasTrack(cba.A));
		}

		[Test]
		public void Scoped_component_not_released_prematurely()
		{
			Container.Register(
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().ImplementedBy<BDisposable>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			var b = (BDisposable)cba.B;
			var wasADisposedAtTheTimeWhenDisposingB = false;
			b.OnDisposing = () => wasADisposedAtTheTimeWhenDisposingB = ((ADisposable)b.A).Disposed;

			Container.Release(cba);
			Assert.True(b.Disposed);
			Assert.False(wasADisposedAtTheTimeWhenDisposingB);
		}

		[Test]
		public void Scoped_component_not_released_prematurely_interdependencies()
		{
			Container.Register(
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().ImplementedBy<BDisposable>().LifeStyle.ScopedPer<CBA>(),
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			var b = (BDisposable)cba.B;
			var wasADisposedAtTheTimeWhenDisposingB = false;
			b.OnDisposing = () => wasADisposedAtTheTimeWhenDisposingB = ((ADisposable)b.A).Disposed;

			Container.Release(cba);
			Assert.True(b.Disposed);
			Assert.False(wasADisposedAtTheTimeWhenDisposingB);
		}

		[Test]
		public void Scoped_component_not_reused_across_resolves()
		{
			Container.Register(
				Component.For<A>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var one = Container.Resolve<CBA>();
			var two = Container.Resolve<CBA>();

			Assert.AreNotSame(one.A, two.A);
			Assert.AreNotSame(one.B.A, two.B.A);
			Assert.AreNotSame(one.B.A, two.A);
		}

		[Test]
		public void Scoped_component_released_when_releasing_root_disposable()
		{
			Container.Register(
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.ScopedPer<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();
			var a = (ADisposable)cba.A;

			Container.Release(cba);

			Assert.True(a.Disposed);
		}

		[Test]
		public void Scoped_component_reused()
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