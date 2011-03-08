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

namespace Castle.MicroKernel.Tests
{
	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture]
	public class SubResolverTestCase
	{
		[Test]
		public void WillAskResolverWhenTryingToResolveDependencyAfterAnotherHandlerWasRegistered()
		{
			var resolver = new FooBarResolver();

			IKernel kernel = new DefaultKernel();
			kernel.Resolver.AddSubResolver(resolver);

			kernel.Register(Component.For<Foo>());
			var handler = kernel.GetHandler(typeof(Foo));

			Assert.AreEqual(HandlerState.WaitingDependency, handler.CurrentState);

			resolver.Result = 15;

			kernel.Register(Component.For<A>());

			Assert.AreEqual(HandlerState.Valid, handler.CurrentState);
		}
	}

	public class Foo
	{
		private int bar;

		public Foo(int bar)
		{
			this.bar = bar;
		}
	}

	public class FooBarResolver : ISubDependencyResolver
	{
		public int? Result;

		public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			return Result != null;
		}

		public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
		{
			return Result.Value;
		}
	}
}