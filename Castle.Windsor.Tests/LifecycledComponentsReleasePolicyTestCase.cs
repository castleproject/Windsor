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
	using System;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Releasers;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class LifecycledComponentsReleasePolicyTestCase
	{
		private IWindsorContainer container;
		private IReleasePolicy releasePolicy;

		[Test]
		public void AllComponentsReleasePolicy_is_the_default_release_policy_in_Windsor()
		{
			Assert.IsInstanceOf<LifecycledComponentsReleasePolicy>(releasePolicy);
		}

		[Test]
		public void Doesnt_track_simple_components_transient()
		{
			container.Register(Transient<A>());

			var a = container.Resolve<A>();

			Assert.IsFalse(releasePolicy.HasTrack(a));
		}

		[Test]
		public void Doesnt_track_singleton()
		{
			container.Register(Singleton<A>());

			var a = container.Resolve<A>();

			Assert.IsFalse(releasePolicy.HasTrack(a));
		}

		[Test]
		public void Release_stops_tracking_component_transient()
		{
			container.Register(Transient<DisposableFoo>());
			var foo = container.Resolve<DisposableFoo>();

			container.Release(foo);

			Assert.IsFalse(releasePolicy.HasTrack(foo));
		}

		[Test]
		public void Release_doesnt_stop_tracking_component_singleton_until_container_is_disposed()
		{
			DisposableFoo.DisposedCount = 0;
			container.Register(Singleton<DisposableFoo>());
			var foo = container.Resolve<DisposableFoo>();
			var fooWeak = new WeakReference(foo);

			container.Release(foo);
			foo = null;
			GC.Collect();

			Assert.IsTrue(fooWeak.IsAlive);
			Assert.AreEqual(0, DisposableFoo.DisposedCount);

			container.Dispose();
			GC.Collect();

			Assert.IsFalse(fooWeak.IsAlive);
			Assert.AreEqual(1, DisposableFoo.DisposedCount);
		}


		[Test]
		public void Doesnt_track_simple_components_with_simple_DynamicDependencies()
		{
			container.Register(Transient<A>().DynamicParameters(delegate { }));

			var a = container.Resolve<A>();

			Assert.IsFalse(releasePolicy.HasTrack(a));
		}

		[Test]
		public void Doesnt_track_simple_components_with_simple_dependencies()
		{
			container.Register(Transient<B>(),
			                   Transient<A>());

			var b = container.Resolve<B>();

			Assert.IsFalse(releasePolicy.HasTrack(b));
		}

		[Test]
		public void Doesnt_track_simple_components_with_simple_dependencies_having_simple_DynamicDependencies()
		{
			container.Register(Transient<B>(),
			                   Transient<A>().DynamicParameters(delegate { }));

			var b = container.Resolve<B>();

			Assert.IsFalse(releasePolicy.HasTrack(b));
		}

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
			releasePolicy = container.Kernel.ReleasePolicy;
		}

		[Test]
		public void Tracks_disposable_components()
		{
			container.Register(Transient<DisposableFoo>());

			var foo = container.Resolve<DisposableFoo>();

			Assert.IsTrue(releasePolicy.HasTrack(foo));
		}

		[Test]
		public void Tracks_simple_components_pooled()
		{
			container.Register(Pooled<A>());

			var a = container.Resolve<A>();

			Assert.IsTrue(releasePolicy.HasTrack(a));
		}

		[Test]
		public void Tracks_simple_components_with_DynamicDependencies_requiring_decommission()
		{
			container.Register(Transient<A>().DynamicParameters((kernel, parameters) => delegate { }));

			var a = container.Resolve<A>();

			Assert.IsTrue(releasePolicy.HasTrack(a));
		}

		[Test]
		public void Tracks_simple_components_with_disposable_dependencies()
		{
			container.Register(Transient<DisposableFoo>(),
			                   Transient<UsesDisposableFoo>());

			var hasFoo = container.Resolve<UsesDisposableFoo>();

			Assert.IsTrue(releasePolicy.HasTrack(hasFoo));
		}

		[Test]
		public void Tracks_simple_components_with_simple_dependencies_havingDynamicDependencies_requiring_decommission()
		{
			container.Register(Transient<B>(),
			                   Transient<A>().DynamicParameters((kernel, parameters) => delegate { }));

			var b = container.Resolve<B>();

			Assert.IsTrue(releasePolicy.HasTrack(b));
		}

		private ComponentRegistration<T> Transient<T>()
            where T : class 
		{
			return Component.For<T>().LifeStyle.Transient;
		}

		private ComponentRegistration<T> Pooled<T>()
            where T : class 
		{
			return Component.For<T>().LifeStyle.Pooled;
		}
        private ComponentRegistration<T> Singleton<T>()
            where T : class 
		{
			return Component.For<T>().LifeStyle.Singleton;
		}
	}
}