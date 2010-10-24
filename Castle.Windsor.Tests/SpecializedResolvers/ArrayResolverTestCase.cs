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
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ArrayResolverTestCase : AbstractContainerTestFixture
	{
		[Test]
		public void Composite_service_can_be_resolved_without_triggering_circular_dependency_detection_fuse()
		{
			Container.Register(AllTypes.FromThisAssembly()
			                   	.BasedOn<IEmptyService>()
			                   	.WithService.Base()
			                   	.ConfigureFor<EmptyServiceComposite>(r => r.Forward<EmptyServiceComposite>()));

			var composite = Container.Resolve<EmptyServiceComposite>();
			Assert.AreEqual(4, composite.Inner.Length);
		}

		[Test(Description = "IOC-238")]
		public void Composite_service_can_be_resolved_without_triggering_circular_dependency_detection_fuse_composite_registered_first()
		{
			Container.Register(
				Component.For<IEmptyService, EmptyServiceComposite>().ImplementedBy<EmptyServiceComposite>(),
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecorator>(),
				Component.For<IEmptyService>().ImplementedBy<EmptyServiceDecoratorViaProperty>()
				);

			var composite = Container.Resolve<EmptyServiceComposite>();
			Assert.AreEqual(4, composite.Inner.Length);
		}

		[Test]
		public void DependencyOnArrayOfServices_OnConstructor()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                   Component.For<ArrayDepAsConstructor>());

			var comp = Container.Resolve<ArrayDepAsConstructor>();

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
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                   Component.For<ArrayDepAsProperty>());

			var comp = Container.Resolve<ArrayDepAsProperty>();

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
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));
			Container.Register(Component.For<ArrayDepAsConstructor>(),
			                   Component.For<ArrayDepAsProperty>());

			var proxy = Container.Resolve<ArrayDepAsConstructor>();
			Assert.IsNotNull(proxy.Services);

			var proxy2 = Container.Resolve<ArrayDepAsProperty>();
			Assert.IsNotNull(proxy2.Services);
		}

		[Test]
		public void DependencyOn_ref_ArrayOfServices_OnConstructor()
		{
			Container.Register(Component.For<IEmptyService>().ImplementedBy<EmptyServiceA>(),
			                   Component.For<IEmptyService>().ImplementedBy<EmptyServiceB>(),
			                   Component.For<ArrayRefDepAsConstructor>());

			var comp = Container.Resolve<ArrayRefDepAsConstructor>();

			Assert.IsNotNull(comp);
			Assert.IsNotNull(comp.Services);
			Assert.AreEqual(2, comp.Services.Length);
			foreach (var service in comp.Services)
			{
				Assert.IsNotNull(service);
			}
		}

		[SetUp]
		public void SetUp()
		{
#if SILVERLIGHT
			Init();
#endif
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
		}
	}
}