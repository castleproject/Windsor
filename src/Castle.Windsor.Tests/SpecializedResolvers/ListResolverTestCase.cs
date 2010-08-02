// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Tests.SpecializedResolvers
{
	using System.Linq;

	using Castle.DynamicProxy;
	using Castle.Windsor.Proxy;
	using Castle.Windsor.Tests.Components;

	using MicroKernel.Registration;
	using NUnit.Framework;
	using Resolvers.SpecializedResolvers;

	[TestFixture]
	public class ListResolverTestCase
	{
		private IKernel kernel;

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel(new DefaultProxyFactory());
			kernel.Resolver.AddSubResolver(new ListResolver(kernel));
		}

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[Test]
		public void DependencyOnListOfServices_OnProperty()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
							Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
							Component.For<ListDepAsProperty>());

			var comp = kernel.Resolve<ListDepAsProperty>();

			Assert.IsNotNull(comp);
			Assert.IsNotNull(comp.Services);
			Assert.AreEqual(2, comp.Services.Count);
			foreach (var service in comp.Services.AsEnumerable())
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnListOfServices_OnConstructor()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
							Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
							Component.For<ListDepAsConstructor>());

			var comp = kernel.Resolve<ListDepAsConstructor>();

			Assert.IsNotNull(comp);
			Assert.IsNotNull(comp.Services);
			Assert.AreEqual(2, comp.Services.Count);
			foreach (var service in comp.Services.AsEnumerable())
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnListOfInterceptedServices()
		{
			kernel.Register(
				Component.For<StandardInterceptor>().Named("a"),
				Component.For<StandardInterceptor>().Named("b"),
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>().Interceptors("a"),
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>().Interceptors("b"),
				Component.For<ListDepAsConstructor>(),
				Component.For<ListDepAsProperty>());

			var proxy = kernel.Resolve<ListDepAsConstructor>().Services[0] as IProxyTargetAccessor;
			Assert.IsNotNull(proxy);
			Assert.AreSame(proxy.GetInterceptors()[0], kernel.Resolve<StandardInterceptor>("a"));

			proxy = kernel.Resolve<ListDepAsConstructor>().Services[1] as IProxyTargetAccessor;
			Assert.IsNotNull(proxy);
			Assert.AreSame(proxy.GetInterceptors()[0], kernel.Resolve<StandardInterceptor>("b"));

			proxy = kernel.Resolve<ListDepAsProperty>().Services[0] as IProxyTargetAccessor;
			Assert.IsNotNull(proxy);
			Assert.AreSame(proxy.GetInterceptors()[0], kernel.Resolve<StandardInterceptor>("a"));

			proxy = kernel.Resolve<ListDepAsProperty>().Services[1] as IProxyTargetAccessor;
			Assert.IsNotNull(proxy);
			Assert.AreSame(proxy.GetInterceptors()[0], kernel.Resolve<StandardInterceptor>("b"));
		}

		[Test]
		public void DependencyOnListWhenEmpty()
		{
			kernel.Resolver.AddSubResolver(new ListResolver(kernel, true));
			kernel.Register(Component.For<ListDepAsConstructor>(),
			                Component.For<ListDepAsProperty>());

			var proxy = kernel.Resolve<ListDepAsConstructor>();
			Assert.IsNotNull(proxy.Services);

			var proxy2 = kernel.Resolve<ListDepAsProperty>();
			Assert.IsNotNull(proxy2.Services);
		}
	}
}
