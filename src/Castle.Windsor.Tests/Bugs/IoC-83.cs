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

namespace Castle.MicroKernel.Tests.Bugs
{
	using System;

	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Registration;

	using CastleTests.Components;

	using NUnit.Framework;

	/// <summary>
	/// For IoC-120 also
	/// </summary>
	[TestFixture]
	public class IoC_83
	{
		[Test]
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