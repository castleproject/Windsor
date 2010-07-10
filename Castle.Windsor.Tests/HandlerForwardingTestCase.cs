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

namespace Castle.MicroKernel.Tests
{
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class HandlerForwardingTestCase
	{
		private IKernel kernel;

		[Test]
		public void Can_register_handler_forwarding()
		{
			kernel.Register(
				Component.For<IUserRepository, IRepository>()
					.ImplementedBy<MyRepository>()
				);

			Assert.AreSame(
				kernel.Resolve<IRepository>(),
				kernel.Resolve<IUserRepository>()
				);
		}

		[Test]
		public void Can_register_handler_forwarding_using_generics()
		{
			kernel.Register(
				Component.For<IUserRepository, IRepository<User>>()
					.ImplementedBy<MyRepository>()
				);
			Assert.AreSame(
				kernel.Resolve<IRepository<User>>(),
				kernel.Resolve<IUserRepository>()
				);
		}

		[Test]
		public void Can_register_handler_forwarding_with_dependencies()
		{
			kernel.Register(
				Component.For<IUserRepository, IRepository>()
					.ImplementedBy<MyRepository2>(),
				Component.For<ServiceUsingRepository>(),
				Component.For<User>()
				);

			kernel.Resolve<ServiceUsingRepository>();
		}

		[Test]
		public void Can_register_several_handler_forwarding()
		{
			kernel.Register(
				Component.For<IUserRepository>()
					.Forward<IRepository, IRepository<User>>()
					.ImplementedBy<MyRepository>()
				);

			Assert.AreSame(
				kernel.Resolve<IRepository<User>>(),
				kernel.Resolve<IUserRepository>()
				);
			Assert.AreSame(
				kernel.Resolve<IRepository>(),
				kernel.Resolve<IUserRepository>()
				);
		}

		[Test]
		public void ResolveAll_Will_Only_Resolve_Unique_Handlers()
		{
			kernel.Register(
				Component.For<IUserRepository, IRepository>()
					.ImplementedBy<MyRepository>()
				);

			var repos = kernel.ResolveAll<IRepository>();
			Assert.AreEqual(1, repos.Length);
		}

		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
		}

		#region Nested type: IRepository

		public interface IRepository
		{
		}

		public interface IRepository<T> : IRepository
		{
		}

		#endregion

		#region Nested type: IUserRepository

		public interface IUserRepository : IRepository<User>
		{
		}

		#endregion

		#region Nested type: MyRepository

		public class MyRepository : IUserRepository
		{
		}

		#endregion

		#region Nested type: MyRepository2

		public class MyRepository2 : IUserRepository
		{
			public MyRepository2(User user)
			{
			}
		}

		#endregion

		#region Nested type: ServiceUsingRepository

		public class ServiceUsingRepository
		{
			public ServiceUsingRepository(IRepository repos)
			{
			}
		}

		#endregion

		#region Nested type: User

		public class User
		{
		}

		#endregion
	}
}