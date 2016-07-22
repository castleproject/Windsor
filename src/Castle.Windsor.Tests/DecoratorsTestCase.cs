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
	using System;

	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using CastleTests;

	using NUnit.Framework;

	[TestFixture]
	public class DecoratorsTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Should_ignore_reference_to_itself()
		{
			Kernel.Register(
				Component.For<IRepository>().ImplementedBy<Repository1>(),
				Component.For<IRepository>().ImplementedBy<DecoratedRepository>()
				);
			var repos = (Repository1)Kernel.Resolve<IRepository>();
			Assert.IsInstanceOf(typeof(DecoratedRepository), repos.InnerRepository);
		}

		[Test]
		public void Will_give_good_error_message_if_cannot_resolve_service_that_is_likely_decorated()
		{
			Kernel.Register(
				Component.For<IRepository>().ImplementedBy<Repository1>(),
				Component.For<IRepository>().ImplementedBy<DecoratedRepository2>()
				);
			var exception =
				Assert.Throws<HandlerException>( () => Kernel.Resolve<IRepository>());

			var expectedMessage =
				string.Format(
					"Can't create component 'Castle.MicroKernel.Tests.ClassComponents.Repository1' as it has dependencies to be satisfied.{0}{0}'Castle.MicroKernel.Tests.ClassComponents.Repository1' is waiting for the following dependencies:{0}- Service 'Castle.MicroKernel.Tests.ClassComponents.IRepository' which points back to the component itself.{0}A dependency cannot be satisfied by the component itself, did you forget to make this a service override and point explicitly to a different component exposing this service?{0}{0}The following components also expose the service, but none of them can be resolved:{0}'Castle.MicroKernel.Tests.ClassComponents.DecoratedRepository2' is waiting for the following dependencies:{0}- Parameter 'name' which was not provided. Did you forget to set the dependency?{0}",
					Environment.NewLine);
			Assert.AreEqual(expectedMessage, exception.Message);
		}

		[Test]
		[ExpectedException(typeof(HandlerException))]
		public void Will_give_good_error_message_if_cannot_resolve_service_that_is_likely_decorated_when_there_are_multiple_service()
		{
			Kernel.Register(
				Component.For<IRepository>().ImplementedBy<Repository1>(),
				Component.For<IRepository>().ImplementedBy<DecoratedRepository2>().Named("foo"),
				Component.For<IRepository>().ImplementedBy<Repository1>().Named("bar")
				);
			Kernel.Resolve<IRepository>();
		}
	}
}