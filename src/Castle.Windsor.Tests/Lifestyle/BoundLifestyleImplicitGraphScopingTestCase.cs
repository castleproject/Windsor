// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using System.Linq;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers.SpecializedResolvers;

	using CastleTests.Components;

	using NUnit.Framework;

	public class BoundLifestyleImplicitGraphScopingTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Scoped_component_created_for_outermost_sub_graph()
		{
			Container.Register(
				Component.For<A>().LifeStyle.BoundTo<CBA>(),
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
				Component.For<A>().ImplementedBy<ADisposable>().LifestyleBoundTo<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			Assert.False(Kernel.ReleasePolicy.HasTrack(cba.A));
		}

		[Test]
		public void Scoped_component_disposable_root_tracked()
		{
			Container.Register(
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			Assert.True(Kernel.ReleasePolicy.HasTrack(cba));
		}

		[Test]
		public void Scoped_component_doesnt_unnecessarily_force_root_to_be_tracked()
		{
			Container.Register(
				Component.For<A>().LifeStyle.BoundTo<CBA>(),
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
				Component.For<A>().LifeStyle.BoundTo<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			Assert.False(Kernel.ReleasePolicy.HasTrack(cba.A));
		}

		[Test]
		public void Scoped_component_not_released_prematurely()
		{
			Container.Register(
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<CBA>(),
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
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<CBA>(),
				Component.For<B>().ImplementedBy<BDisposable>().LifeStyle.BoundTo<CBA>(),
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
				Component.For<A>().LifeStyle.BoundTo<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var one = Container.Resolve<CBA>();
			var two = Container.Resolve<CBA>();

			Assert.AreNotSame(one.A, two.A);
			Assert.AreNotSame(one.B.A, two.B.A);
			Assert.AreNotSame(one.B.A, two.A);
		}

		[Test]
		public void Scoped_component_properly_release_when_roots_collection_is_involved()
		{
			Kernel.Resolver.AddSubResolver(new CollectionResolver(Kernel));
			Container.Register(
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<AppScreenCBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient,
				Component.For<IAppScreen>().ImplementedBy<AppScreenCBA>().LifeStyle.Transient.Named("1"),
				Component.For<IAppScreen>().ImplementedBy<AppScreenCBA>().LifeStyle.Transient.Named("2"),
				Component.For<IAppScreen>().ImplementedBy<AppScreenCBA>().LifeStyle.Transient.Named("3"),
				Component.For<AppHost>().LifeStyle.Transient);

			var host = Container.Resolve<AppHost>();

			var a = host.Screens.Cast<AppScreenCBA>().Select(s => s.Dependency.A as ADisposable).ToArray();

			Container.Dispose();

			Assert.True(a.All(x => x.Disposed));
		}

		[Test]
		public void Scoped_component_properly_scoped_when_roots_collection_is_involved()
		{
			Kernel.Resolver.AddSubResolver(new CollectionResolver(Kernel));
			Container.Register(
				Component.For<A>().LifeStyle.BoundTo<AppScreenCBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient,
				Component.For<IAppScreen>().ImplementedBy<AppScreenCBA>().LifeStyle.Transient.Named("1"),
				Component.For<IAppScreen>().ImplementedBy<AppScreenCBA>().LifeStyle.Transient.Named("2"),
				Component.For<IAppScreen>().ImplementedBy<AppScreenCBA>().LifeStyle.Transient.Named("3"),
				Component.For<AppHost>().LifeStyle.Transient);

			var host = Container.Resolve<AppHost>();

			var a = host.Screens.Cast<AppScreenCBA>().Select(s => s.Dependency.A).Distinct().ToArray();

			Assert.AreEqual(3, a.Length);
		}

		[Test]
		public void Scoped_component_released_when_releasing_root_disposable()
		{
			Container.Register(
				Component.For<A>().ImplementedBy<ADisposable>().LifeStyle.BoundTo<CBA>(),
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
				Component.For<A>().LifestyleBoundTo<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			Assert.AreSame(cba.A, cba.B.A);
		}

		[Test]
		public void Scoped_nearest_component_created_for_innermost_sub_graph()
		{
			Container.Register(
				Component.For<A>().LifeStyle.BoundToNearest<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().ImplementedBy<CBADecorator>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();
			var inner = ((CBADecorator)cba).Inner;

			Assert.AreNotSame(cba.A, inner.A);
		}

		[Test]
		public void Scoped_nearest_component_reused_in_subgraph()
		{
			Container.Register(
				Component.For<A>().LifestyleBoundToNearest<CBA>(),
				Component.For<B>().LifeStyle.Transient,
				Component.For<CBA>().LifeStyle.Transient);

			var cba = Container.Resolve<CBA>();

			Assert.AreSame(cba.A, cba.B.A);
		}
	}
}