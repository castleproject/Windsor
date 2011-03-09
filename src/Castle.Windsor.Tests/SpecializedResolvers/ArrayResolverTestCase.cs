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

namespace Castle.MicroKernel.Tests.SpecializedResolvers
{
	using System.Linq;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers.SpecializedResolvers;
	using Castle.Windsor.Tests;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ArrayResolverTestCase : AbstractContainerTestCase
	{

		[Test(Description = "IOC-239")]
		public void ArrayResolution_UnresolvableDependencyCausesResolutionFailure()
		{
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));
			Container.Register(
				Component.For<IDependency>().ImplementedBy<ResolvableDependency>(),
				Component.For<IDependency>().ImplementedBy<UnresolvalbeDependencyWithPrimitiveConstructor>(),
				Component.For<IDependOnArray>().ImplementedBy<DependsOnArray>()
				);
			Container.Resolve<IDependOnArray>();
		}

		[Test(Description = "IOC-239")]
		public void ArrayResolution_UnresolvableDependencyCausesResolutionFailure_ServiceConstructor()
		{
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));
			Container.Register(
				Component.For<IDependency>().ImplementedBy<ResolvableDependency>(),
				Component.For<IDependency>().ImplementedBy<UnresolvalbeDependencyWithAdditionalServiceConstructor>(),
				Component.For<IDependOnArray>().ImplementedBy<DependsOnArray>()
				);
			Container.Resolve<IDependOnArray>();
		}

		[Test(Description = "IOC-239")]
		public void ArrayResolution_UnresolvableDependencyIsNotIncluded()
		{
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
			Container.Register(
				Component.For<IDependency>().ImplementedBy<ResolvableDependency>(),
				Component.For<IDependency>().ImplementedBy<UnresolvalbeDependency>(),
				Component.For<IDependOnArray>().ImplementedBy<DependsOnArray>()
				);
			Container.Resolve<IDependOnArray>();
		}

		[Test]
		public void Composite_service_can_be_resolved_without_triggering_circular_dependency_detection_fuse()
		{
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
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
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
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
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
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
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
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
			Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel));
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

		[Test(Description = "IOC-240")]
		public void InjectAll()
		{
			Container.Kernel.Resolver.AddSubResolver(new ArrayResolver(Container.Kernel, true));
			Container.Install(new CollectionServiceOverridesInstaller());
			var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectAll");
			var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
			Assert.That(dependencies, Has.Count.EqualTo(3));
			Assert.That(dependencies, Has.Member(typeof(EmptyServiceA)));
			Assert.That(dependencies, Has.Member(typeof(EmptyServiceB)));
			Assert.That(dependencies, Has.Member(typeof(EmptyServiceDecoratorViaProperty)));
		}

		[Test(Description = "IOC-240")]
		public void InjectFooAndBarOnly_WithArrayResolver()
		{
			Container.Kernel.Resolver.AddSubResolver(new ArrayResolver(Container.Kernel, true));
			Container.Install(new CollectionServiceOverridesInstaller());
			var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectFooAndBarOnly");
			var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
			Assert.That(dependencies, Has.Count.EqualTo(2));
			Assert.That(dependencies, Has.Member(typeof(EmptyServiceA)));
			Assert.That(dependencies, Has.Member(typeof(EmptyServiceB)));
		}

		[Test(Description = "IOC-240")]
		public void InjectFooAndBarOnly_WithoutArrayResolver()
		{
			Container.Install(new CollectionServiceOverridesInstaller());
			var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectFooAndBarOnly");
			var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
			Assert.That(dependencies, Has.Count.EqualTo(2));
			Assert.That(dependencies, Has.Member(typeof(EmptyServiceA)));
			Assert.That(dependencies, Has.Member(typeof(EmptyServiceB)));
		}

		[Test(Description = "IOC-240")]
		public void InjectFooOnly_WithArrayResolver()
		{
			Container.Kernel.Resolver.AddSubResolver(new ArrayResolver(Container.Kernel, true));
			Container.Install(new CollectionServiceOverridesInstaller());
			var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectFooOnly");
			var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
			Assert.That(dependencies, Has.Count.EqualTo(1));
			Assert.That(dependencies, Has.Member(typeof(EmptyServiceA)));
		}

		[Test(Description = "IOC-240")]
		public void InjectFooOnly_WithoutArrayResolver()
		{
			Container.Install(new CollectionServiceOverridesInstaller());
			var fooItemTest = Container.Resolve<ArrayDepAsConstructor>("InjectFooOnly");
			var dependencies = fooItemTest.Services.Select(d => d.GetType()).ToList();
			Assert.That(dependencies, Has.Count.EqualTo(1));
			Assert.That(dependencies, Has.Member(typeof(EmptyServiceA)));
		}
	}
}