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

namespace CastleTests.Registration
{
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Tests.Interceptors;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class TypesTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Based_on_interface_types_registered()
		{
			Container.Register(Types.FromThisAssembly()
			                   	.BasedOn(typeof(ICommon))
				);

			var handlers = Kernel.GetHandlers(typeof(ICommon));
			Assert.AreEqual(1, handlers.Length);

			handlers = Kernel.GetAssignableHandlers(typeof(ICommon));
			Assert.Greater(handlers.Length, 1);
		}

		[Test]
		public void Interface_registered_with_no_implementation_with_interceptor_can_be_used()
		{
			Container.Register(
				Component.For<ReturnDefaultInterceptor>(),
				Types.FromThisAssembly()
					.BasedOn(typeof(ISimpleService))
					.If(t => t.IsInterface)
					.Configure(t => t.Interceptors<ReturnDefaultInterceptor>())
				);

			var common = Container.Resolve<ISimpleService>();
			common.Operation();
		}
	}
}