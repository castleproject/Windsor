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

namespace CastleTests.Lifecycle
{
	using System;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Pools;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class DecomissioningResponsibilitiesTestCase : AbstractContainerTestCase
	{
		public class Indirection
		{
			private readonly NonDisposableRoot fakeRoot;

			public Indirection(NonDisposableRoot fakeRoot)
			{
				this.fakeRoot = fakeRoot;
			}

			public NonDisposableRoot FakeRoot
			{
				get { return fakeRoot; }
			}
		}

		public class NonDisposableRoot
		{
			private readonly A a;
			private readonly B b;

			public NonDisposableRoot(A a, B b)
			{
				this.a = a;
				this.b = b;
			}

			public A A
			{
				get { return a; }
			}

			public B B
			{
				get { return b; }
			}
		}

		public class A : DisposableBase
		{
		}

		public class B : DisposableBase
		{
		}

		public class C : DisposableBase
		{
		}

		public class GenA<T> : DisposableBase
		{
			public B BField { get; set; }

			public GenB<T> GenBField { get; set; }
		}

		public class GenB<T> : DisposableBase
		{
		}

		public class DisposableSpamService : DisposableBase
		{
			private readonly PoolableComponent1 pool;
			private readonly DisposableTemplateEngine templateEngine;

			public DisposableSpamService(DisposableTemplateEngine templateEngine)
			{
				this.templateEngine = templateEngine;
			}

			public DisposableSpamService(DisposableTemplateEngine templateEngine,
			                             PoolableComponent1 pool)
			{
				this.templateEngine = templateEngine;
				this.pool = pool;
			}

			public DefaultMailSenderService MailSender { get; set; }

			public PoolableComponent1 Pool
			{
				get { return pool; }
			}

			public DisposableTemplateEngine TemplateEngine
			{
				get { return templateEngine; }
			}
		}

		public class DisposableTemplateEngine : DisposableBase
		{
		}

		[Test]
		public void ComponentsAreOnlyDisposedOnce()
		{
			Kernel.Register(
				Component.For<DisposableSpamService>().Named("spamservice").LifeStyle.Transient,
				Component.For<DisposableTemplateEngine>().Named("templateengine").LifeStyle.Transient);

			var instance1 = Kernel.Resolve<DisposableSpamService>("spamservice");
			Assert.IsFalse(instance1.IsDisposed);
			Assert.IsFalse(instance1.TemplateEngine.IsDisposed);

			Kernel.ReleaseComponent(instance1);
			Kernel.ReleaseComponent(instance1);
			Kernel.ReleaseComponent(instance1);
		}

		[Test]
		public void DisposingSubLevelBurdenWontDisposeComponentAsTheyAreDisposedAlready()
		{
			Kernel.Register(
				Component.For<DisposableSpamService>().Named("spamservice").LifeStyle.Transient);
			Kernel.Register(
				Component.For<DisposableTemplateEngine>().Named("templateengine").LifeStyle.Transient);

			var instance1 = Kernel.Resolve<DisposableSpamService>("spamservice");
			Assert.IsFalse(instance1.IsDisposed);
			Assert.IsFalse(instance1.TemplateEngine.IsDisposed);

			Kernel.ReleaseComponent(instance1);
			Kernel.ReleaseComponent(instance1.TemplateEngine);
		}

		[Test]
		public void GenericTransientComponentsAreReleasedInChain()
		{
			Kernel.Register(Component.For(typeof(GenA<>)).LifeStyle.Transient);
			Kernel.Register(Component.For(typeof(GenB<>)).LifeStyle.Transient);

			var instance1 = Kernel.Resolve<GenA<string>>();
			Assert.IsFalse(instance1.IsDisposed);
			Assert.IsFalse(instance1.GenBField.IsDisposed);

			Kernel.ReleaseComponent(instance1);

			Assert.IsTrue(instance1.IsDisposed);
			Assert.IsTrue(instance1.GenBField.IsDisposed);
		}

		[Test]
		public void SingletonReferencedComponentIsNotDisposed()
		{
			Kernel.Register(
				Component.For(typeof(DisposableSpamService)).Named("spamservice").LifeStyle.Transient);
			Kernel.Register(
				Component.For(typeof(DefaultMailSenderService)).Named("mailsender").LifeStyle.Singleton);
			Kernel.Register(
				Component.For(typeof(DisposableTemplateEngine)).Named("templateengine").LifeStyle.Transient);

			var instance1 = Kernel.Resolve<DisposableSpamService>("spamservice");
			Assert.IsFalse(instance1.IsDisposed);
			Assert.IsFalse(instance1.TemplateEngine.IsDisposed);

			Kernel.ReleaseComponent(instance1);

			Assert.IsTrue(instance1.IsDisposed);
			Assert.IsTrue(instance1.TemplateEngine.IsDisposed);
			Assert.IsFalse(instance1.MailSender.IsDisposed);
		}

		[Test]
		public void TransientReferencedComponentsAreReleasedInChain()
		{
			Kernel.Register(
				Component.For<DisposableSpamService>().LifeStyle.Transient,
				Component.For<DisposableTemplateEngine>().LifeStyle.Transient
				);

			var service = Kernel.Resolve<DisposableSpamService>();
			Assert.IsFalse(service.IsDisposed);
			Assert.IsFalse(service.TemplateEngine.IsDisposed);

			Kernel.ReleaseComponent(service);

			Assert.IsTrue(service.IsDisposed);
			Assert.IsTrue(service.TemplateEngine.IsDisposed);
		}

		[Test]
		public void TransientReferencesAreNotHeldByContainer()
		{
			Kernel.Register(Component.For<EmptyClass>().LifeStyle.Transient);

			ReferenceTracker
				.Track(() => Kernel.Resolve<EmptyClass>())
				.AssertNoLongerReferenced();
		}

		[Test]
		public void WhenRootComponentIsNotDisposableButDependenciesAre_DependenciesShouldBeDisposed()
		{
			Kernel.Register(Component.For(typeof(NonDisposableRoot)).Named("root").LifeStyle.Transient);
			Kernel.Register(Component.For(typeof(A)).Named("a").LifeStyle.Transient);
			Kernel.Register(Component.For(typeof(B)).Named("b").LifeStyle.Transient);

			var instance1 = Kernel.Resolve<NonDisposableRoot>();
			Assert.IsFalse(instance1.A.IsDisposed);
			Assert.IsFalse(instance1.B.IsDisposed);

			Kernel.ReleaseComponent(instance1);

			Assert.IsTrue(instance1.A.IsDisposed);
			Assert.IsTrue(instance1.B.IsDisposed);
		}

		[Test]
		public void WhenRootComponentIsNotDisposableButThirdLevelDependenciesAre_DependenciesShouldBeDisposed()
		{
			Kernel.Register(Component.For(typeof(Indirection)).Named("root").LifeStyle.Transient);
			Kernel.Register(Component.For(typeof(NonDisposableRoot)).Named("secroot").LifeStyle.Transient);
			Kernel.Register(Component.For(typeof(A)).Named("a").LifeStyle.Transient);
			Kernel.Register(Component.For(typeof(B)).Named("b").LifeStyle.Transient);

			var instance1 = Kernel.Resolve<Indirection>();
			Assert.IsFalse(instance1.FakeRoot.A.IsDisposed);
			Assert.IsFalse(instance1.FakeRoot.B.IsDisposed);

			Kernel.ReleaseComponent(instance1);

			Assert.IsTrue(instance1.FakeRoot.A.IsDisposed);
			Assert.IsTrue(instance1.FakeRoot.B.IsDisposed);
		}

		[Test]
		[Bug("IOC-320")]
		public void Expected_exception_during_creation_doesnt_prevent_from_being_released_properly()
		{
			Container.Register(Component.For<GenA<int>>().LifestyleTransient(),
			                   Component.For<B>().UsingFactoryMethod<B>(delegate
			                   {
			                   	throw new NotImplementedException("boo hoo!");
			                   }).LifestyleTransient()
			                   	.OnDestroy(Assert.IsNotNull));

			var a = Container.Resolve<GenA<int>>();

			Container.Release(a);
		}

	}
}