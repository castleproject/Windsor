// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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
	using System.Reflection;

	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;
	using Castle.Windsor;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Proxy;

	using NUnit.Framework;

	public class CollectionResolverWithPropagatingContainerTestCase
		: CollectionResolverTestCase
	{
		/// <summary>Build a container, where all <see cref = "CreationContext" /> are propagating.</summary>
		/// <returns>A Castle Windsor container</returns>
		protected override WindsorContainer BuildContainer()
		{
			return
				new WindsorContainer(
					new DefaultKernel(
						new InlineDependenciesPropagatingDependencyResolver(),
						new DefaultProxyFactory()),
					new DefaultComponentInstaller());
		}

		[Test]
		public void collection_sub_resolver_should_honor_composition_context_if_kernel_propagates_inline_dependencies()
		{
			Container.Register(Component.For<ComponentA>().LifestyleTransient());
			Container.Register(Component.For<IComponentB>().ImplementedBy<ComponentB1>().LifestyleTransient());
			Container.Register(Component.For<IComponentB>().ImplementedBy<ComponentB2>().LifestyleTransient());

			var additionalArguments = Arguments.FromProperties(new { greeting = "Hello propagating system." });
			var componentA = Kernel.Resolve<ComponentA>(additionalArguments);
			Assert.That(componentA, Is.Not.Null);
			Assert.That(componentA.Greeting, Is.Not.Null);
			Assert.That(componentA.ComponentsOfB, Is.Not.Null);
			Assert.That(componentA.ComponentsOfB.Length, Is.EqualTo(2));
			foreach (IComponentB componentB in componentA.ComponentsOfB)
			{
				Assert.That(componentA.Greeting, Is.EqualTo(componentB.Greeting));
			}
		}

		public class InlineDependenciesPropagatingDependencyResolver
			: DefaultDependencyResolver
		{
			protected override CreationContext RebuildContextForParameter(
				CreationContext current,
				Type parameterType)
			{
				return parameterType.GetTypeInfo().ContainsGenericParameters
					? current
					: new CreationContext(parameterType, current, true);
			}
		}

		public class ComponentA
		{
			public ComponentA(
				IKernel kernel,
				IComponentB[] componentsOfB,
				string greeting)
			{
				Kernel = kernel;
				ComponentsOfB = componentsOfB;
				Greeting = greeting;
			}

			public IKernel Kernel { get; }
			public IComponentB[] ComponentsOfB { get; }
			public string Greeting { get; }
		}

		public interface IComponentB
		{
			string Greeting { get; }
		}

		public abstract class ComponentB : IComponentB
		{
			protected ComponentB(string greeting)
			{
				Greeting = greeting;
			}

			public string Greeting { get; }
		}

		public class ComponentB1 : ComponentB
		{
			public ComponentB1(string greeting) : base(greeting)
			{
			}
		}

		public class ComponentB2 : ComponentB
		{
			public ComponentB2(string greeting) : base(greeting)
			{
			}
		}
	}
}
