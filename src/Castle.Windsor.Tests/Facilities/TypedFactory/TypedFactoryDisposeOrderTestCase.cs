// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.Facilities.TypedFactory
{
	using System;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	public sealed class TypedFactoryDisposeOrderTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.AddFacility<TypedFactoryFacility>();
		}

		[Test]
		public void Typed_factories_are_not_disposed_before_their_dependents()
		{
			Container.Register(
				Component.For<Dependency>(),
				Component.For<Dependent>());

			Container.Resolve<Dependent>();
		}

		public sealed class Dependency : IDisposable
		{
			private bool isDisposed;

			public void Use()
			{
				if (isDisposed) throw new ObjectDisposedException(nameof(Dependency));
			}

			public void Dispose()
			{
				isDisposed = true;
			}
		}

		public sealed class Dependent : IDisposable
		{
			private readonly Func<Dependency> factory;

			public Dependent(Func<Dependency> factory)
			{
				this.factory = factory;
			}

			public void Dispose()
			{
				using (var needed = factory.Invoke())
				{
					needed.Use();
				}
			}
		}
	}
}
