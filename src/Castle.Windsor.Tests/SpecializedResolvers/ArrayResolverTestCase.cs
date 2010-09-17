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
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers.SpecializedResolvers;
	using Castle.Windsor.Tests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ArrayResolverTestCase
	{
		private IKernel kernel;

		[Test]
		public void Composite_service_can_be_resolved_without_triggering_circular_dependency_detection_fuse()
		{
			kernel.Register(AllTypes.FromThisAssembly()
			                	.BasedOn<IEmptyService>()
			                	.WithService.Base()
			                	.ConfigureFor<EmptyServiceComposite>(r => r.Forward<EmptyServiceComposite>()));

			var composite = kernel.Resolve<EmptyServiceComposite>();
			Assert.AreEqual(2, composite.Inner.Length);
		}

		[Test]
		public void DependencyOnArrayOfServices_OnConstructor()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<ArrayDepAsConstructor>());

			var comp = kernel.Resolve<ArrayDepAsConstructor>();

			Assert.IsNotNull(comp);
			Assert.IsNotNull(comp.Services);
			Assert.AreEqual(2, comp.Services.Length);
			foreach (var service in comp.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnArrayOfServices_OnProperty()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<ArrayDepAsProperty>());

			var comp = kernel.Resolve<ArrayDepAsProperty>();

			Assert.IsNotNull(comp);
			Assert.IsNotNull(comp.Services);
			Assert.AreEqual(2, comp.Services.Length);
			foreach (var service in comp.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[Test]
		public void DependencyOnArrayWhenEmpty()
		{
			kernel.Resolver.AddSubResolver(new ArrayResolver(kernel, true));
			kernel.Register(Component.For<ArrayDepAsConstructor>(),
			                Component.For<ArrayDepAsProperty>());

			var proxy = kernel.Resolve<ArrayDepAsConstructor>();
			Assert.IsNotNull(proxy.Services);

			var proxy2 = kernel.Resolve<ArrayDepAsProperty>();
			Assert.IsNotNull(proxy2.Services);
		}

		[Test]
		public void DependencyOn_ref_ArrayOfServices_OnConstructor()
		{
			kernel.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                Component.For<ArrayRefDepAsConstructor>());

			var comp = kernel.Resolve<ArrayRefDepAsConstructor>();

			Assert.IsNotNull(comp);
			Assert.IsNotNull(comp.Services);
			Assert.AreEqual(2, comp.Services.Length);
			foreach (var service in comp.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();
			kernel.Resolver.AddSubResolver(new ArrayResolver(kernel));
		}
	}
}