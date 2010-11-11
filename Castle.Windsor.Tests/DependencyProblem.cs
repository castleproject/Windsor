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
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	/// <summary>
	/// Reported at http://forum.castleproject.org/posts/list/17.page
	/// </summary>
	[TestFixture]
	public class DependencyProblem
	{
		[SetUp]
		public void Init()
		{
			container = new WindsorContainer();
		}

		private IWindsorContainer container;

		public class A
		{
			public A(B b)
			{
			}
		}

		public class B
		{
			public B(C b)
			{
			}
		}

		public class C
		{
		}

		public class D
		{
			public D(B b)
			{
			}

			public D()
			{
			}
		}

		[Test]
		public void CtorSourceOrderDoesNotMatter()
		{
			container.Register(Component.For(typeof(D)).Named("D"));
			Assert.IsNotNull(container.Resolve<D>("D"));
		}

		[Test]
		public void LoadingInSequence()
		{
			container.Register(Component.For(typeof(C)).Named("C"));
			container.Register(Component.For(typeof(B)).Named("B"));
			container.Register(Component.For(typeof(A)).Named("A"));

			Assert.IsNotNull(container.Resolve<A>("A"));
			Assert.IsNotNull(container.Resolve<B>("B"));
			Assert.IsNotNull(container.Resolve<C>("C"));
		}

		[Test]
		public void LoadingOutOfSequence()
		{
			container.Register(Component.For(typeof(A)).Named("A"));
			container.Register(Component.For(typeof(B)).Named("B"));
			container.Register(Component.For(typeof(C)).Named("C"));

			Assert.IsNotNull(container.Resolve<A>("A"));
			Assert.IsNotNull(container.Resolve<B>("B"));
			Assert.IsNotNull(container.Resolve<C>("C"));
		}

		[Test]
		public void LoadingOutOfSequenceWithExtraLoad()
		{
			container.Register(Component.For(typeof(A)).Named("A"));
			container.Register(Component.For(typeof(B)).Named("B"));
			container.Register(Component.For(typeof(C)).Named("C"));
			container.Register(Component.For(typeof(object)).Named("NotUsed"));

			Assert.IsNotNull(container.Resolve<A>("A"));
			Assert.IsNotNull(container.Resolve<B>("B"));
			Assert.IsNotNull(container.Resolve<C>("C"));
		}

		[Test]
		public void LoadingPartiallyInSequence()
		{
			container.Register(Component.For(typeof(B)).Named("B"));
			container.Register(Component.For(typeof(C)).Named("C"));
			container.Register(Component.For(typeof(A)).Named("A"));

			Assert.IsNotNull(container.Resolve<A>("A"));
			Assert.IsNotNull(container.Resolve<B>("B"));
			Assert.IsNotNull(container.Resolve<C>("C"));
		}
	}
}