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
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture]
	public class RegistrationOrderingTestsCase : AbstractContainerTestCase
	{
		[Test]
		public void CtorSourceOrderDoesNotMatter()
		{
			Container.Register(Component.For<D_DB>());

			Assert.IsNotNull(Container.Resolve<D_DB>());
		}

		[Test]
		public void LoadingInSequence()
		{
			Container.Register(Component.For<A>(),
			                   Component.For<B>(),
			                   Component.For<C>());

			Assert.IsNotNull(Container.Resolve<C>());
			Assert.IsNotNull(Container.Resolve<B>());
			Assert.IsNotNull(Container.Resolve<A>());
		}

		[Test]
		public void LoadingOutOfSequence()
		{
			Container.Register(Component.For<C>(),
			                   Component.For<B>(),
			                   Component.For<A>());

			Assert.IsNotNull(Container.Resolve<C>());
			Assert.IsNotNull(Container.Resolve<B>());
			Assert.IsNotNull(Container.Resolve<A>());
		}

		[Test]
		public void LoadingOutOfSequenceWithExtraLoad()
		{
			Container.Register(Component.For<C>(),
			                   Component.For<B>(),
			                   Component.For<A>(),
			                   Component.For<object>());

			Assert.IsNotNull(Container.Resolve<C>());
			Assert.IsNotNull(Container.Resolve<B>());
			Assert.IsNotNull(Container.Resolve<A>());
		}

		[Test]
		public void LoadingPartiallyInSequence()
		{
			Container.Register(Component.For<B>(),
			                   Component.For<C>(),
			                   Component.For<A>());

			Assert.IsNotNull(Container.Resolve<C>());
			Assert.IsNotNull(Container.Resolve<B>());
			Assert.IsNotNull(Container.Resolve<A>());
		}
	}
}