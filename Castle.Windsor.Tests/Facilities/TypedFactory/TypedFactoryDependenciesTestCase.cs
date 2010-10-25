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

namespace Castle.Windsor.Tests.Facilities.TypedFactory
{
	using System;
	using System.Linq;
	using System.Text;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryDependenciesTestCase : AbstractContainerTestFixture
	{
		[Test]
		public void Delegate_factory_depends_on_default_interceptor()
		{
			Container.AddFacility<TypedFactoryFacility>()
				.Register(Component.For<Func<A>>().AsFactory());

			AssertHasDependency<Func<A>>(TypedFactoryFacility.InterceptorKey);
		}

		[Test]
		public void Interface_factory_depends_on_default_interceptor()
		{
			Container.AddFacility<TypedFactoryFacility>()
				.Register(Component.For<DummyComponentFactory>().AsFactory());

			AssertHasDependency<DummyComponentFactory>(TypedFactoryFacility.InterceptorKey);
		}

		[Test]
		public void Interface_factory_depends_on_default_selector_by_default()
		{
			Container.AddFacility<TypedFactoryFacility>()
				.Register(Component.For<DummyComponentFactory>().AsFactory());

			AssertHasDependency<DummyComponentFactory>("Castle.TypedFactory.DefaultInterfaceFactoryComponentSelector");
		}

		private void AssertHasDependency<TComponnet>(string key)
		{
			var handler = GetHandler<TComponnet>();
			Assert.IsTrue(handler.ComponentModel.Dependencies.Any(d => d.DependencyKey == key), "Dependencies found: {0}",
			              BuildDependencyByKeyList(handler));
		}

		private string BuildDependencyByKeyList(IHandler handler)
		{
			var message = new StringBuilder();
			foreach (var dependency in handler.ComponentModel.Dependencies)
			{
				message.AppendLine(dependency.DependencyKey);
			}
			return message.ToString();
		}

		private IHandler GetHandler<T>()
		{
			var handler = Container.Kernel.GetHandler(typeof(T));
			Assert.IsNotNull(handler);
			return handler;
		}
	}
}