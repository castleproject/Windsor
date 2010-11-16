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

namespace Castle.Windsor.Tests
{
	using Castle.Generics;
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class MultiServiceGenericComponentsTestCase : AbstractContainerTestFixture
	{
		[Test]
		public void Closed_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_key()
		{
			Container.Register(
				Component.For<Generics.IRepository<A>, IARepository>()
					.ImplementedBy<ARepository<B>>()
					.Named("repo")
				);
			Assert.AreSame(
				Container.Resolve<Generics.IRepository<A>>("repo"),
				Container.Resolve<IARepository>("repo")
				);
		}

		[Test]
		public void Closed_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_type()
		{
			Container.Register(
				Component.For<Generics.IRepository<A>, IARepository>()
					.ImplementedBy<ARepository<B>>()
				);
			Assert.AreSame(
				Container.Resolve<Generics.IRepository<A>>(),
				Container.Resolve<IARepository>()
				);
		}

		[Test]
		public void Closed_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_key()
		{
			Container.Register(
				Component.For<IARepository, Generics.IRepository<A>>()
					.ImplementedBy<ARepository<B>>()
					.Named("repo")
				);
			Assert.AreSame(
				Container.Resolve<Generics.IRepository<A>>("repo"),
				Container.Resolve<IARepository>("repo")
				);
		}

		[Test]
		public void Closed_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_type()
		{
			Container.Register(
				Component.For<IARepository, Generics.IRepository<A>>()
					.ImplementedBy<ARepository<B>>()
				);
			Assert.AreSame(
				Container.Resolve<Generics.IRepository<A>>(),
				Container.Resolve<IARepository>()
				);
		}

		[Test]
		public void Non_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_key()
		{
			Container.Register(
				Component.For<Generics.IRepository<A>, IARepository>()
					.ImplementedBy<ARepository>()
					.Named("repo")
				);
			Assert.AreSame(
				Container.Resolve<Generics.IRepository<A>>("repo"),
				Container.Resolve<IARepository>("repo")
				);
		}

		[Test]
		public void Non_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_type()
		{
			Container.Register(
				Component.For<Generics.IRepository<A>, IARepository>()
					.ImplementedBy<ARepository>()
				);
			Assert.AreSame(
				Container.Resolve<Generics.IRepository<A>>(),
				Container.Resolve<IARepository>()
				);
		}

		[Test]
		public void Non_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_key()
		{
			Container.Register(
				Component.For<IARepository, Generics.IRepository<A>>()
					.ImplementedBy<ARepository>()
					.Named("repo")
				);
			Assert.AreSame(
				Container.Resolve<Generics.IRepository<A>>("repo"),
				Container.Resolve<IARepository>("repo")
				);
		}

		[Test]
		public void Non_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_type()
		{
			Container.Register(
				Component.For<IARepository, Generics.IRepository<A>>()
					.ImplementedBy<ARepository>()
				);
			Assert.AreSame(
				Container.Resolve<Generics.IRepository<A>>(),
				Container.Resolve<IARepository>()
				);
		}

		[Test]
		public void Open_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_key()
		{
			Container.Register(
				Component.For(typeof(Generics.IRepository<>)).Forward<IRepository>()
					.ImplementedBy(typeof(Repository<>))
					.Named("repo")
				);

			Container.Resolve<Generics.IRepository<A>>("repo");

			// TODO: how to handle scenario when this one is requested? Should we throw nice exception?
			// Container.Resolve<IARepository>("repo");
		}

		[Test]
		public void Open_generic_component_with_generic_and_non_generic_service__generic_first_resolve_by_type()
		{
			Container.Register(
				Component.For(typeof(Generics.IRepository<>)).Forward<IRepository>()
					.ImplementedBy(typeof(Repository<>))
				);

			Container.Resolve<Generics.IRepository<A>>();

			// TODO: how to handle scenario when this one is requested? Should we throw nice exception?
			// Container.Resolve<IARepository>("repo");
		}

		[Test(Description = "IOC-248")]
		[Ignore("failing now")]
		public void Open_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_key()
		{
			Container.Register(
				Component.For<IRepository>().Forward(typeof(Generics.IRepository<>))
					.ImplementedBy(typeof(Repository<>))
					.Named("repo")
				);

			Container.Resolve<Generics.IRepository<A>>("repo");

			// TODO: how to handle scenario when this one is requested? Should we throw nice exception?
			// Container.Resolve<IARepository>("repo");
		}

		[Test(Description = "IOC-248")]
		[Ignore("failing now")]
		public void Open_generic_component_with_generic_and_non_generic_service__non_generic_first_resolve_by_type()
		{
			Container.Register(
				Component.For<IRepository>().Forward(typeof(Generics.IRepository<>))
					.ImplementedBy(typeof(Repository<>))
				);

			Container.Resolve<Generics.IRepository<A>>();

			// TODO: how to handle scenario when this one is requested? Should we throw nice exception?
			// Container.Resolve<IARepository>("repo");
		}
	}
}