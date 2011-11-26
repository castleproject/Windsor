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
	using Castle.MicroKernel;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Components;

	using CastleTests.ContainerExtensions;

	using NUnit.Framework;

	[TestFixture]
	public class CustomSubDependencyResolversTestCase : AbstractContainerTestCase
	{
		[Test]
		[ExpectedException(typeof(CircularDependencyException))]
		public void Can_detect_dependency_cycle_introduced_by_poorly_implemented_subresolver()
		{
			Kernel.Resolver.AddSubResolver(new BadDependencyResolver(Kernel));
			Container
				.Register(
					Component.For<IItemService>().ImplementedBy<ItemService>(),
					Component.For<IBookStore>().ImplementedBy<BookStore>()
				);
			Container.Resolve<IItemService>();
		}

		[Test]
		[ExpectedException(typeof(CircularDependencyException))]
		public void Can_detect_waiting_dependency_pointed_to_by_sub_resolver()
		{
			Kernel.Resolver.AddSubResolver(new GoodDependencyResolver());
			Container
				.Register(
					Component.For<IItemService>().ImplementedBy<ItemService>(),
					Component.For<IBookStore>().ImplementedBy<BookStore>()
				);
			Container.Resolve<IItemService>();
		}
	}
}