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

namespace CastleTests.Activators
{
	using System;

	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ConstructorTestCase : AbstractContainerTestCase
	{
		[Test]
		[Bug("IOC-120")]
		public void Can_resolve_component_with_internal_ctor()
		{
			Container.Register(Component.For<EmptyClass>(),
			                   Component.For<HasInternalConstructor>());

			var exception = Assert.Throws<ComponentActivatorException>(() =>
			                                                           Container.Resolve<HasInternalConstructor>());
			var expected =
#if SILVERLIGHT
				string.Format("Type {0} does not have a public default constructor and could not be instantiated.",
				              typeof(HasInternalConstructor).FullName);
#else
				string.Format(
					"Could not find a public constructor for type {0}. Windsor can not instantiate types that don't expose public constructors. To expose the type as a service add public constructor, or use custom component activator.",
					typeof(HasInternalConstructor).FullName);
#endif

			Assert.AreEqual(expected,
			                exception.InnerException.Message);
		}

		[Test]
		[Bug("IOC-83")]
		[Bug("IOC-120")]
		public void When_attemting_to_resolve_component_with_nonpublic_ctor_should_throw_meaningfull_exception()
		{
			var kernel = new DefaultKernel();

			kernel.Register(Component.For<HasProtectedConstructor>());

			Exception exception =
				Assert.Throws<ComponentActivatorException>(() =>
					kernel.Resolve<HasProtectedConstructor>());

			exception = exception.InnerException;
			Assert.IsNotNull(exception);
			StringAssert.Contains("public", exception.Message, "Exception should say that constructor has to be public.");

		}
	}
}
