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

namespace Castle.Windsor.Tests.Bugs
{
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class IoC_120
	{
		[Test]
		public void Can_resolve_component_with_internal_ctor()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<EmptyClass>(),
			                   Component.For<HasInternalConstructor>());

			var exception = Assert.Throws<ComponentActivatorException>(() =>
			                                                           container.Resolve<HasInternalConstructor>());
			var expected =
#if SILVERLIGHT
				string.Format("Type {0} does not have a public default constructor and could not be instantiated.",
				              typeof(HasInternalConstructor).FullName);
#else
 string.Format("Could not find a public constructor for type {0}. Windsor can not instantiate types that don't expose public constructors. To expose the type as a service add public constructor, or use custom component activator.", typeof(HasInternalConstructor).FullName);
#endif

			Assert.AreEqual(expected,
			                exception.InnerException.Message);
		}
	}
}


