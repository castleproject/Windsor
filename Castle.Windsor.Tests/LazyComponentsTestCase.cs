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

namespace CastleTests
{
	using System;
	using System.Reflection;
	using System.Threading;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Components;

	using CastleTests.Components;

	using NUnit.Framework;

#if !DOTNET35
	public class LazyComponentsTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_provide_lazy_as_dependency()
		{
			Container.Register(Component.For(typeof(UsesLazy<>)).LifeStyle.Transient,
			                   Component.For<A>());

			var value = Container.Resolve<UsesLazy<A>>();

			Assert.IsNotNull(value.Lazy);
			Assert.IsNotNull(value.Lazy.Value);
		}

		[Test]
		[Ignore(
			"This is not supported. Actually this is a sign of a bigger design direction - ResolveAll does not trigger lazy loaders. Should we perhaps re-implement this?"
			)]
		public void Can_pull_lazy_via_ResolveAll()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());

			var all = Container.ResolveAll<Lazy<IEmptyService>>();

			Assert.AreEqual(2, all.Length);
		}

		[Test]
		public void Can_resolve_component_via_lazy()
		{
			Container.Register(Component.For<A>());

			var lazy = Container.Resolve<Lazy<A>>();
			var a = lazy.Value;

			Assert.IsNotNull(a);
		}

		[Test]
		public void Can_resolve_lazy_before_actual_component_is_registered()
		{
			var lazy = Container.Resolve<Lazy<A>>();

			Container.Register(Component.For<A>());

			Assert.IsNotNull(lazy.Value);
		}

		[Test]
		public void Can_resolve_lazy_before_dependencies_of_actual_component_are_registered()
		{
			Container.Register(Component.For<B>());

			var lazy = Container.Resolve<Lazy<B>>();

			Container.Register(Component.For<A>());

			var b = lazy.Value;
			Assert.IsNotNull(b);
			Assert.IsNotNull(b.A);
		}

		[Test]
		public void Can_resolve_lazy_component()
		{
			Container.Register(Component.For<A>());

			Container.Resolve<Lazy<A>>();
		}

		[Test]
		public void Implicit_lazy_can_handle_generic_component()
		{
			Container.Register(Component.For(typeof(EmptyGenericClassService<>)));

			var lazy1 = Container.Resolve<Lazy<EmptyGenericClassService<A>>>();
			var lazy2 = Container.Resolve<Lazy<EmptyGenericClassService<B>>>();

			Assert.IsNotNull(lazy1.Value);
			Assert.IsNotNull(lazy2.Value);
		}

		[Test]
		public void Implicit_lazy_is_always_tracked_by_release_policy()
		{
			Container.Register(Component.For<A>());

			var lazy = Container.Resolve<Lazy<A>>();

			Assert.True(Kernel.ReleasePolicy.HasTrack(lazy));
		}

#if !SILVERLIGHT
		[Test]
		public void Implicit_lazy_is_initialized_once()
		{
			Container.Register(Component.For<A>());

			var lazy = Container.Resolve<Lazy<A>>();
			var mode = GetMode(lazy);

			Assert.AreEqual(LazyThreadSafetyMode.ExecutionAndPublication, mode);
		}
#endif

		[Test]
		public void Implicit_lazy_is_transient()
		{
			Container.Register(Component.For<A>());

			var lazy1 = Container.Resolve<Lazy<A>>();
			var lazy2 = Container.Resolve<Lazy<A>>();

			Assert.AreNotSame(lazy1, lazy2);

			var handler = Kernel.GetHandler(typeof(Lazy<A>));
			Assert.AreEqual(LifestyleType.Transient, handler.ComponentModel.LifestyleType);
		}

		[Test]
		public void Can_resolve_same_component_via_two_lazy()
		{
			Container.Register(Component.For<A>(),
			                   Component.For<B>());

			var lazy1 = Container.Resolve<Lazy<A>>();
			var lazy2 = Container.Resolve<Lazy<B>>();

			Assert.IsNotNull(lazy1.Value);
			Assert.IsNotNull(lazy2.Value);
		}

		[Test]
		public void Can_resolve_lazy_component_requiring_arguments_inline()
		{
			Container.Register(Component.For<B>());

			var a = new A();
			var arguments = new Arguments(new object[] { a });
			var missingArguments = Container.Resolve<Lazy<B>>();
			var hasArguments = Container.Resolve<Lazy<B>>(new { arguments });

			B ignore;
			Assert.Throws<DependencyResolverException>(() => ignore = missingArguments.Value);

			Assert.IsNotNull(hasArguments.Value);
			Assert.AreSame(a, hasArguments.Value.A);
		}

		[Test]
		public void Can_resolve_various_components_via_lazy()
		{
			Container.Register(Component.For<A>());

			var lazy1 = Container.Resolve<Lazy<A>>();
			var lazy2 = Container.Resolve<Lazy<A>>();

			Assert.AreNotSame(lazy1, lazy2);
			Assert.AreSame(lazy1.Value, lazy2.Value);
		}

		[Test]
		public void Implicit_lazy_resolves_default_component_for_given_service_take_1()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>());

			var lazy = Container.Resolve<Lazy<IEmptyService>>();

			Assert.IsInstanceOf<EmptyServiceA>(lazy.Value);
		}

		[Test]
		public void Implicit_lazy_resolves_default_component_for_given_service_take_2()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>());

			var lazy = Container.Resolve<Lazy<IEmptyService>>();

			Assert.IsInstanceOf<EmptyServiceB>(lazy.Value);
		}

		[Test]
		public void Lazy_throws_on_resolve_when_no_component_present_for_requested_service()
		{
			var lazy = Container.Resolve<Lazy<A>>();

			Assert.Throws<ComponentNotFoundException>(() => { var ignoreMe = lazy.Value; });
		}

		[Test]
		public void Releasing_lazy_releases_requested_component()
		{
			DisposableFoo.ResetDisposedCount();

			Container.Register(Component.For<DisposableFoo>().LifeStyle.Transient);

			var lazy = Container.Resolve<Lazy<DisposableFoo>>();

			Assert.AreEqual(0, DisposableFoo.DisposedCount);
			var value = lazy.Value;

			Container.Release(lazy);
			Assert.AreEqual(1, DisposableFoo.DisposedCount);
		}

		[Test]
		public void Resolving_lazy_doesnt_resolve_requested_component_eagerly()
		{
			HasInstanceCount.ResetInstancesCreated();

			Container.Register(Component.For<HasInstanceCount>());

			var lazy = Container.Resolve<Lazy<HasInstanceCount>>();

			Assert.AreEqual(0, HasInstanceCount.InstancesCreated);
			Assert.IsFalse(lazy.IsValueCreated);

			var value = lazy.Value;

			Assert.AreEqual(1, HasInstanceCount.InstancesCreated);
		}

		protected override void AfterContainerCreated()
		{
			Container.Register(Component.For<ILazyComponentLoader>()
			                   	.ImplementedBy<LazyOfTComponentLoader>());
		}

		private LazyThreadSafetyMode GetMode(Lazy<A> lazy)
		{
			return (LazyThreadSafetyMode)lazy.GetType().GetProperty("Mode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lazy, null);
		}
	}
#endif
}