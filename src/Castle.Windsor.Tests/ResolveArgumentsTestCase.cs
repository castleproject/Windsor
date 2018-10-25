// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace CastleTests
{
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers.SpecializedResolvers;
	using Castle.Windsor;
	using NUnit.Framework;

	[TestFixture]
	public class ResolveArgumentsTestCase : AbstractContainerTestCase
	{
		[SetUp]
		public void SetUp()
		{
			Container.Kernel.Resolver.AddSubResolver(new ListResolver(Container.Kernel));
			Container.Register(Component.For<Service>());
			Container.Register(Component.For<IDependencyWithManyImplementations>().ImplementedBy<DependencyImplementationA>());
			Container.Register(Component.For<IDependencyWithManyImplementations>().ImplementedBy<DependencyImplementationB>());
		}

		[Test]
		public void Can_Resolve_using_Arguments_as_Properties()
		{
			Assert.DoesNotThrow(() => Container.Resolve<Service>(Arguments.FromProperties(new { Dependency = new Dependency() })));
		}

		[Test]
		public void Can_ResolveAll_using_Arguments_as_Properties()
		{
			Assert.DoesNotThrow(() => Container.ResolveAll<IDependencyWithManyImplementations>(Arguments.FromProperties(new { Dependency = new Dependency() })));
		}

		[Test]
		public void Can_Resolve_using_Type_and_Arguments_as_Properties()
		{
			Assert.DoesNotThrow(() => Container.Resolve(typeof(Service), Arguments.FromProperties(new { Dependency = new Dependency() })));
		}

		[Test]
		public void Can_ResolveAll_using_Type_and_Arguments_as_Properties()
		{
			Assert.DoesNotThrow(() => Container.ResolveAll(typeof(IDependencyWithManyImplementations), Arguments.FromProperties(new { Dependency = new Dependency() })));
		}

		[Test]
		public void Can_Resolve_using_Arguments_as_Dictionary()
		{
			var dictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.Resolve<Service>(Arguments.FromNamed(dictionary)));
		}

		[Test]
		public void Can_ResolveAll_using_Arguments_as_Dictionary()
		{
			var dictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.ResolveAll<IDependencyWithManyImplementations>(Arguments.FromNamed(dictionary)));
		}

		[Test]
		public void Can_Resolve_using_Type_and_Arguments_as_Dictionary()
		{
			var dictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.Resolve(typeof(Service), Arguments.FromNamed(dictionary)));
		}

		[Test]
		public void Can_ResolveAll_using_Type_and_Arguments_as_Dictionary()
		{
			var dictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.ResolveAll(typeof(IDependencyWithManyImplementations), new Arguments().AddNamed(dictionary)));
		}

		[Test]
		public void Can_Resolve_using_Arguments_as_ReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.Resolve<Service>(new Arguments().AddNamed(readOnlyDictionary)));
		}

		[Test]
		public void Can_ResolveAll_using_Arguments_as_ReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.ResolveAll<IDependencyWithManyImplementations>(new Arguments().AddNamed(readOnlyDictionary)));
		}

		[Test]
		public void Can_Resolve_using_ReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.Resolve<Service>(readOnlyDictionary));
		}

		[Test]
		public void Can_ResolveAll_using_ReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.ResolveAll<IDependencyWithManyImplementations>(readOnlyDictionary));
		}

		[Test]
		public void Can_Resolve_using_Type_and_Arguments_as_ReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.Resolve(typeof(Service), new Arguments().AddNamed(readOnlyDictionary)));
		}

		[Test]
		public void Can_ResolveAll_using_Type_and_Arguments_as_ReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.ResolveAll(typeof(IDependencyWithManyImplementations), new Arguments().AddNamed(readOnlyDictionary)));
		}

		[Test]
		public void Can_Resolve_Type_and_ReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.Resolve(typeof(Service), readOnlyDictionary));
		}

		[Test]
		public void Can_ResolveAll_Type_and_ReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, object> readOnlyDictionary = new Dictionary<string, object> { { "dependency", new Dependency() } };
			Assert.DoesNotThrow(() => Container.ResolveAll(typeof(IDependencyWithManyImplementations), readOnlyDictionary));
		}

		[Test]
		public void Can_Resolve_using_Arguments_as_TypedComponents()
		{
			Assert.DoesNotThrow(() => Container.Resolve<Service>(new Arguments().AddTyped(new Dependency())));
		}

		[Test]
		public void Can_ResolveAll_using_Arguments_as_TypedComponents()
		{
			Assert.DoesNotThrow(() => Container.ResolveAll<IDependencyWithManyImplementations>(new Arguments().AddTyped(new Dependency())));
		}

		[Test]
		public void Can_Resolve_using_Type_and_Arguments_as_TypedComponents()
		{
			Assert.DoesNotThrow(() => Container.Resolve(typeof(Service), new Arguments().AddTyped(new Dependency())));
		}

		[Test]
		public void Can_ResolveAll_using_Type_and_Arguments_as_TypedComponents()
		{
			Assert.DoesNotThrow(() => Container.Resolve(typeof(IDependencyWithManyImplementations), new Arguments().AddTyped(new Dependency())));
		}

		private class Dependency { }

		private class Service
		{
			private readonly Dependency dependency;

			public Service(Dependency dependency)
			{
				this.dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
			}
		}

		private interface IDependencyWithManyImplementations { }

		private class DependencyImplementationA : IDependencyWithManyImplementations
		{
			private readonly Dependency dependency;

			public DependencyImplementationA(Dependency dependency)
			{
				this.dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
			}
		}

		private class DependencyImplementationB : IDependencyWithManyImplementations
		{
			private readonly Dependency dependency;

			public DependencyImplementationB(Dependency dependency)
			{
				this.dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
			}
		}
	}
}