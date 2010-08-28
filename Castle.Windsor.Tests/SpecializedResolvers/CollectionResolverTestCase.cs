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

namespace Castle.MicroKernel.Tests.SpecializedResolvers
{
	using System;
	using System.Linq;

	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers.SpecializedResolvers;
	using Castle.Windsor.Tests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class CollectionResolverTestCase
	{
		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel));
		}

		private IKernel kernel;

		[Test]
		public void DependencyOnArrayOfServices_OnConstructor()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<ArrayDepAsConstructor>());

			var component = kernel.Resolve<ArrayDepAsConstructor>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(2, component.Services.Length);
			foreach (var service in component.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOn_ref_ArrayOfServices_OnConstructor()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
							Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
							Component.For<ArrayRefDepAsConstructor>());

			var component = kernel.Resolve<ArrayRefDepAsConstructor>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(2, component.Services.Length);
			foreach (var service in component.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnArrayOfServices_OnConstructor_empty_allowed_empty_provided()
		{
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, allowEmptyCollections: true));
			kernel.Register(Component.For<ArrayDepAsConstructor>());

			var component = kernel.Resolve<ArrayDepAsConstructor>();

			Assert.IsNotNull(component.Services);
			Assert.IsEmpty(component.Services);
		}

		[Test]
		public void DependencyOnArrayOfServices_OnConstructor_empty_not_allowed_throws()
		{
			kernel.Register(Component.For<ArrayDepAsConstructor>());

			Assert.Throws<HandlerException>(() => kernel.Resolve<ArrayDepAsConstructor>());
		}

		[Test]
		public void DependencyOnArrayOfServices_OnProperty()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<ArrayDepAsProperty>());

			var component = kernel.Resolve<ArrayDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(2, component.Services.Length);
			foreach (var service in component.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnArrayOfServices_OnProperty_empty()
		{
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, allowEmptyCollections: true));
			kernel.Register(Component.For<ArrayDepAsProperty>());

			var component = kernel.Resolve<ArrayDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.IsEmpty(component.Services);
		}

		[Test]
		public void DependencyOnArrayOfServices_OnProperty_empty_not_allowed_null()
		{
			kernel.Register(Component.For<ArrayDepAsProperty>());

			var component = kernel.Resolve<ArrayDepAsProperty>();
			Assert.IsNull(component.Services);
		}

		[Test]
		public void DependencyOnCollectionOfServices_OnConstructor()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<CollectionDepAsConstructor>());

			var component = kernel.Resolve<CollectionDepAsConstructor>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(2, component.Services.Count);
			foreach (var service in component.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnCollectionOfServices_OnConstructor_empty()
		{
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, allowEmptyCollections: true));
			kernel.Register(Component.For<CollectionDepAsConstructor>());

			var component = kernel.Resolve<CollectionDepAsConstructor>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(0, component.Services.Count);
		}

		[Test]
		public void DependencyOnCollectionOfServices_OnConstructor_empty_not_allowed_throws()
		{
			kernel.Register(Component.For<CollectionDepAsConstructor>());

			Assert.Throws<HandlerException>(() => kernel.Resolve<CollectionDepAsConstructor>());
		}

		[Test]
		public void DependencyOnCollectionOfServices_OnProperty()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<CollectionDepAsProperty>());

			var component = kernel.Resolve<CollectionDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(2, component.Services.Count);
			foreach (var service in component.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnCollectionOfServices_OnProperty_empty()
		{
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, allowEmptyCollections: true));
			kernel.Register(Component.For<CollectionDepAsProperty>());

			var component = kernel.Resolve<CollectionDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(0, component.Services.Count);
		}

		[Test]
		public void DependencyOnCollectionOfServices_OnProperty_empty_not_allowed_throws()
		{
			kernel.Register(Component.For<CollectionDepAsConstructor>());

			Assert.Throws<HandlerException>(() => kernel.Resolve<CollectionDepAsConstructor>());
		}

		[Test]
		public void DependencyOnEnumerableOfServices_OnConstructor()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<EnumerableDepAsProperty>());

			var component = kernel.Resolve<EnumerableDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(2, component.Services.Count());
			foreach (var service in component.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnEnumerableOfServices_OnConstructor_empty()
		{
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, allowEmptyCollections: true));
			kernel.Register(Component.For<EnumerableDepAsProperty>());

			var component = kernel.Resolve<EnumerableDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.IsFalse(component.Services.Any());
		}

		[Test]
		public void DependencyOnEnumerableOfServices_OnConstructor_empty_not_allowed_throws()
		{
			kernel.Register(Component.For<EnumerableDepAsConstructor>());

			Assert.Throws<HandlerException>(() => kernel.Resolve<EnumerableDepAsConstructor>());
		}

		[Test]
		public void DependencyOnEnumerableOfServices_OnProperty()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<EnumerableDepAsProperty>());

			var component = kernel.Resolve<EnumerableDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(2, component.Services.Count());
			foreach (var service in component.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnEnumerableOfServices_OnProperty_empty()
		{
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, allowEmptyCollections: true));
			kernel.Register(Component.For<EnumerableDepAsProperty>());

			var component = kernel.Resolve<EnumerableDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.IsFalse(component.Services.Any());
		}

		[Test]
		public void DependencyOnEnumerableOfServices_OnProperty_empty_not_allowed_null()
		{
			kernel.Register(Component.For<EnumerableDepAsProperty>());

			var component = kernel.Resolve<EnumerableDepAsProperty>();
			Assert.IsNull(component.Services);
		}

		[Test]
		public void DependencyOnListOfServices_OnConstructor()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<ListDepAsConstructor>());

			var component = kernel.Resolve<ListDepAsConstructor>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(2, component.Services.Count);
			foreach (var service in component.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnListOfServices_OnConstructor_empty()
		{
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, allowEmptyCollections: true));
			kernel.Register(Component.For<ListDepAsConstructor>());

			var component = kernel.Resolve<ListDepAsConstructor>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(0, component.Services.Count);
		}

		[Test]
		public void DependencyOnListOfServices_OnConstructor_empty_not_allowed_throws()
		{
			kernel.Register(Component.For<ListDepAsConstructor>());

			Assert.Throws<HandlerException>(() => kernel.Resolve<ListDepAsConstructor>());
		}

		[Test]
		public void DependencyOnListOfServices_OnProperty()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<ListDepAsProperty>());

			var component = kernel.Resolve<ListDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(2, component.Services.Count);
			foreach (var service in component.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnListOfServices_OnProperty_empty()
		{
			kernel.Resolver.AddSubResolver(new CollectionResolver(kernel, allowEmptyCollections: true));
			kernel.Register(Component.For<ListDepAsProperty>());

			var component = kernel.Resolve<ListDepAsProperty>();

			Assert.IsNotNull(component.Services);
			Assert.AreEqual(0, component.Services.Count);
		}

		[Test]
		public void DependencyOnListOfServices_OnProperty_empty_not_allowed_null()
		{
			kernel.Register(Component.For<ListDepAsProperty>());

			var component = kernel.Resolve<ListDepAsProperty>();
			Assert.IsNull(component.Services);
		}

		[Test]
		public void List_is_readonly()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<ListDepAsConstructor>());

			var component = kernel.Resolve<ListDepAsConstructor>();

			Assert.IsTrue(component.Services.IsReadOnly);
			Assert.Throws<NotSupportedException>(() => component.Services.Add(new EmptyServiceA()));
		}
	}
}