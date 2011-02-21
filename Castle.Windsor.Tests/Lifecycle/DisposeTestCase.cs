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

namespace Castle.Windsor.Tests.Lifecycle
{
	using System;

	using Castle.Components;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.ClassComponents;
	using Castle.Windsor.Tests.Components;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Components;

	using NUnit.Framework;

	[TestFixture]
	public class DisposeTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Disposable_component_for_nondisposable_service_built_via_factory_should_be_disposed_when_released()
		{
			SimpleServiceDisposable.DisposedCount = 0;
			Container.Register(Component.For<ISimpleService>()
			                   	.UsingFactoryMethod(() => new SimpleServiceDisposable())
			                   	.LifeStyle.Transient);

			var service = Container.Resolve<ISimpleService>();

			Assert.AreEqual(0, SimpleServiceDisposable.DisposedCount);

			Container.Release(service);

			Assert.AreEqual(1, SimpleServiceDisposable.DisposedCount);
		}

		[Test]
		public void Disposable_component_for_nondisposable_service_is_tracked()
		{
			Container.Register(Component.For<ISimpleService>()
			                   	.ImplementedBy<SimpleServiceDisposable>()
			                   	.LifeStyle.Transient);

			var service = Container.Resolve<ISimpleService>();

			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(service));
		}

		[Test]
		public void Disposable_component_for_nondisposable_service_should_be_disposed_when_released()
		{
			SimpleServiceDisposable.DisposedCount = 0;
			Container.Register(Component.For<ISimpleService>()
			                   	.ImplementedBy<SimpleServiceDisposable>()
			                   	.LifeStyle.Transient);

			var service = Container.Resolve<ISimpleService>();
			Container.Release(service);

			Assert.AreEqual(1, SimpleServiceDisposable.DisposedCount);
		}

		[Test]
		public void Disposable_service_is_tracked()
		{
			Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient);

			var foo = Container.Resolve<DisposableFoo>();

			Assert.IsTrue(Kernel.ReleasePolicy.HasTrack(foo));
		}

		[Test]
		public void Disposable_services_should_be_disposed_when_released()
		{
			DisposableFoo.DisposedCount = 0;
			Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient);

			var foo = Container.Resolve<DisposableFoo>();
			Container.Release(foo);

			Assert.AreEqual(1, DisposableFoo.DisposedCount);
		}

		[Test]
		public void Disposable_singleton_dependency_of_transient_open_generic_is_disposed()
		{
			DisposableFoo.DisposedCount = 0;
			Container.Register(
				Component.For(typeof(GenericComponent<>)).LifeStyle.Transient,
				Component.For<DisposableFoo>().LifeStyle.Singleton
				);

			var depender = Container.Resolve<GenericComponent<DisposableFoo>>();
			var weak = new WeakReference(depender.Value);
			depender = null;
			Container.Dispose();
			GC.Collect();

			Assert.AreEqual(1, DisposableFoo.DisposedCount);
			Assert.IsFalse(weak.IsAlive);
		}

		[Test]
		public void Disposable_singleton_generic_closed_disposed()
		{
			Container.Register(Component.For<DisposableGeneric<A>>());
			var component = Container.Resolve<DisposableGeneric<A>>();

			Container.Dispose();

			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Disposable_singleton_generic_closed_inherited_disposed()
		{
			Container.Register(Component.For<DisposableGenericA>());
			var component = Container.Resolve<DisposableGenericA>();

			Container.Dispose();

			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Disposable_singleton_generic_open_disposed()
		{
			Container.Register(Component.For(typeof(DisposableGeneric<>)));
			var component = Container.Resolve<DisposableGeneric<A>>();

			Container.Dispose();

			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Disposable_transient_generic_closed_disposed()
		{
			Container.Register(Component.For<DisposableGeneric<A>>().LifeStyle.Transient);
			var component = Container.Resolve<DisposableGeneric<A>>();

			Container.Dispose();

			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Disposable_transient_generic_closed_inherited_disposed()
		{
			Container.Register(Component.For<DisposableGenericA>().LifeStyle.Transient);
			var component = Container.Resolve<DisposableGenericA>();

			Container.Dispose();

			Assert.IsTrue(component.Disposed);
		}

		[Test]
		public void Disposable_transient_generic_open_disposed()
		{
			Container.Register(Component.For(typeof(DisposableGeneric<>)).LifeStyle.Transient);
			var component = Container.Resolve<DisposableGeneric<A>>();

			Container.Dispose();

			Assert.IsTrue(component.Disposed);
		}
	}
}