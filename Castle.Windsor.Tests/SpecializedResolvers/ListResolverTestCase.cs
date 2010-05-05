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
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core.Interceptor;
	using Castle.Windsor.Proxy;

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
			kernel.Register(Component.For<IService>().ImplementedBy<A>(),
							Component.For<IService>().ImplementedBy<B>(),
							Component.For<CollectionDepAsProperty>());

			var comp = kernel.Resolve<CollectionDepAsProperty>();

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
			kernel.Register(Component.For<IService>().ImplementedBy<A>(),
							Component.For<IService>().ImplementedBy<B>(),
							Component.For<CollectionDepAsConstructor>());

			var comp = kernel.Resolve<CollectionDepAsConstructor>();

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
				Component.For<IService>().ImplementedBy<A>().Interceptors("a"),
				Component.For<IService>().ImplementedBy<B>().Interceptors("b"),
				Component.For<CollectionDepAsConstructor>(),
				Component.For<CollectionDepAsProperty>());

			var proxy = kernel.Resolve<CollectionDepAsConstructor>().Services[0] as IProxyTargetAccessor;
			Assert.IsNotNull(proxy);
			Assert.AreSame(proxy.GetInterceptors()[0], kernel.Resolve<StandardInterceptor>("a"));

			proxy = kernel.Resolve<CollectionDepAsConstructor>().Services[1] as IProxyTargetAccessor;
			Assert.IsNotNull(proxy);
			Assert.AreSame(proxy.GetInterceptors()[0], kernel.Resolve<StandardInterceptor>("b"));

			proxy = kernel.Resolve<CollectionDepAsProperty>().Services[0] as IProxyTargetAccessor;
			Assert.IsNotNull(proxy);
			Assert.AreSame(proxy.GetInterceptors()[0], kernel.Resolve<StandardInterceptor>("a"));

			proxy = kernel.Resolve<CollectionDepAsProperty>().Services[1] as IProxyTargetAccessor;
			Assert.IsNotNull(proxy);
			Assert.AreSame(proxy.GetInterceptors()[0], kernel.Resolve<StandardInterceptor>("b"));
		}

		[Test]
		public void DependencyOnListWhenEmpty()
		{
			kernel.Resolver.AddSubResolver(new ListResolver(kernel, true));
			kernel.Register(Component.For<CollectionDepAsConstructor>(),
			                Component.For<CollectionDepAsProperty>());

			var proxy = kernel.Resolve<CollectionDepAsConstructor>();
			Assert.IsNotNull(proxy.Services);

			var proxy2 = kernel.Resolve<CollectionDepAsProperty>();
			Assert.IsNotNull(proxy2.Services);
		}

		public class CollectionDepAsProperty
		{
			public IList<IService> Services { get; set; }
		}

		public class CollectionDepAsConstructor
		{
			private readonly IList<IService> services;

			public CollectionDepAsConstructor(IList<IService> services)
			{
				this.services = services;
			}

			public IList<IService> Services
			{
				get { return services; }
			}
		}

		public interface IService
		{
		}

		public class A : IService
		{
		}

		public class B : IService
		{
		}
	}
}
