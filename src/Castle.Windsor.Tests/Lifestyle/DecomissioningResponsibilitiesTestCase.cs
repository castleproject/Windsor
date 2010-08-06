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

namespace Castle.MicroKernel.Tests.Lifestyle
{
	using System;

	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Pools;
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class DecomissioningResponsibilitiesTestCase
	{
		[SetUp]
		public void Setup()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void TearDown()
		{
			kernel.Dispose();
		}

		private IKernel kernel;

		public class Indirection
		{
			private NonDisposableRoot fakeRoot;

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
			private A a;
			private B b;

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

		public class EmptyClass
		{
		}

		[Test]
		public void ComponentsAreOnlyDisposedOnce()
		{
			kernel.Register(
				Component.For(typeof(DisposableSpamService)).Named("spamservice").LifeStyle.Transient);
			kernel.Register(
				Component.For(typeof(DisposableTemplateEngine)).Named("templateengine").LifeStyle.Transient);

			var instance1 = kernel.Resolve<DisposableSpamService>("spamservice");
			Assert.IsFalse(instance1.IsDisposed);
			Assert.IsFalse(instance1.TemplateEngine.IsDisposed);

			kernel.ReleaseComponent(instance1);
			kernel.ReleaseComponent(instance1);
			kernel.ReleaseComponent(instance1);
		}

		[Test]
		public void DisposingSubLevelBurdenWontDisposeComponentAsTheyAreDisposedAlready()
		{
			kernel.Register(
				Component.For(typeof(DisposableSpamService)).Named("spamservice").LifeStyle.Transient);
			kernel.Register(
				Component.For(typeof(DisposableTemplateEngine)).Named("templateengine").LifeStyle.Transient);

			var instance1 = kernel.Resolve<DisposableSpamService>("spamservice");
			Assert.IsFalse(instance1.IsDisposed);
			Assert.IsFalse(instance1.TemplateEngine.IsDisposed);

			kernel.ReleaseComponent(instance1);
			kernel.ReleaseComponent(instance1.TemplateEngine);
		}

		[Test]
		public void GenericTransientComponentsAreReleasedInChain()
		{
			kernel.Register(Component.For(typeof(GenA<>)).Named("gena").LifeStyle.Transient);
			kernel.Register(Component.For(typeof(GenB<>)).Named("genb").LifeStyle.Transient);

			var instance1 = kernel.Resolve<GenA<string>>();
			Assert.IsFalse(instance1.IsDisposed);
			Assert.IsFalse(instance1.GenBField.IsDisposed);

			kernel.ReleaseComponent(instance1);

			Assert.IsTrue(instance1.IsDisposed);
			Assert.IsTrue(instance1.GenBField.IsDisposed);
		}

		[Test]
		public void SingletonReferencedComponentIsNotDisposed()
		{
			kernel.Register(
				Component.For(typeof(DisposableSpamService)).Named("spamservice").LifeStyle.Transient);
			kernel.Register(
				Component.For(typeof(DefaultMailSenderService)).Named("mailsender").LifeStyle.Singleton);
			kernel.Register(
				Component.For(typeof(DisposableTemplateEngine)).Named("templateengine").LifeStyle.Transient);

			var instance1 = kernel.Resolve<DisposableSpamService>("spamservice");
			Assert.IsFalse(instance1.IsDisposed);
			Assert.IsFalse(instance1.TemplateEngine.IsDisposed);

			kernel.ReleaseComponent(instance1);

			Assert.IsTrue(instance1.IsDisposed);
			Assert.IsTrue(instance1.TemplateEngine.IsDisposed);
			Assert.IsFalse(instance1.MailSender.IsDisposed);
		}

		[Test]
		public void TransientReferencedComponentsAreReleasedInChain()
		{
			kernel.Register(
				Component.For<DisposableSpamService>().LifeStyle.Transient,
				Component.For<DisposableTemplateEngine>().LifeStyle.Transient
				);

			var service = kernel.Resolve<DisposableSpamService>();
			Assert.IsFalse(service.IsDisposed);
			Assert.IsFalse(service.TemplateEngine.IsDisposed);

			kernel.ReleaseComponent(service);

			Assert.IsTrue(service.IsDisposed);
			Assert.IsTrue(service.TemplateEngine.IsDisposed);
		}

		[Test]
		public void TransientReferencesAreNotHeldByContainer()
		{
			kernel.Register(Component.For<EmptyClass>().LifeStyle.Transient);
			var emptyClassWeakReference = new WeakReference(kernel.Resolve<EmptyClass>());

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.IsFalse(emptyClassWeakReference.IsAlive);
		}

		[Test]
		public void WhenRootComponentIsNotDisposableButDependenciesAre_DependenciesShouldBeDisposed()
		{
			kernel.Register(Component.For(typeof(NonDisposableRoot)).Named("root").LifeStyle.Transient);
			kernel.Register(Component.For(typeof(A)).Named("a").LifeStyle.Transient);
			kernel.Register(Component.For(typeof(B)).Named("b").LifeStyle.Transient);

			var instance1 = kernel.Resolve<NonDisposableRoot>();
			Assert.IsFalse(instance1.A.IsDisposed);
			Assert.IsFalse(instance1.B.IsDisposed);

			kernel.ReleaseComponent(instance1);

			Assert.IsTrue(instance1.A.IsDisposed);
			Assert.IsTrue(instance1.B.IsDisposed);
		}

		[Test]
		public void WhenRootComponentIsNotDisposableButThirdLevelDependenciesAre_DependenciesShouldBeDisposed()
		{
			kernel.Register(Component.For(typeof(Indirection)).Named("root").LifeStyle.Transient);
			kernel.Register(Component.For(typeof(NonDisposableRoot)).Named("secroot").LifeStyle.Transient);
			kernel.Register(Component.For(typeof(A)).Named("a").LifeStyle.Transient);
			kernel.Register(Component.For(typeof(B)).Named("b").LifeStyle.Transient);

			var instance1 = kernel.Resolve<Indirection>();
			Assert.IsFalse(instance1.FakeRoot.A.IsDisposed);
			Assert.IsFalse(instance1.FakeRoot.B.IsDisposed);

			kernel.ReleaseComponent(instance1);

			Assert.IsTrue(instance1.FakeRoot.A.IsDisposed);
			Assert.IsTrue(instance1.FakeRoot.B.IsDisposed);
		}
	}
}