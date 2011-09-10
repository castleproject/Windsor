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

namespace Castle.Windsor.Tests.Facilities.TypedFactory
{
	using System;
	using System.Linq;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Factories;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryDependenciesTestCase : AbstractContainerTestCase
	{
		private void AssertHasDependency<TComponnet>(string name)
		{
			var handler = GetHandler<TComponnet>();
			var dependency = handler.ComponentModel.Dependencies.SingleOrDefault(d => d.DependencyKey == name);
			Assert.IsNotNull(dependency, "Dependency on '{0}' should exist.", name);
		}

		private IHandler GetHandler<T>()
		{
			var handler = Container.Kernel.GetHandler(typeof(T));
			Assert.IsNotNull(handler);
			return handler;
		}

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
	}
}