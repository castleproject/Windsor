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


namespace Castle.Windsor.Extensions.DependencyInjection.Tests.SubSystems
{
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Extensions.DependencyInjection.SubSystems;
	using Castle.Windsor.Extensions.DependencyInjection.Tests.Components;

	using Xunit;

	public class DependencyInjectionNamingSubsystemTests
	{
		[Fact]
		public void Can_Decoration_Resolve_When_Named()
		{
			var container = new WindsorContainer();

			var target = new DependencyInjectionNamingSubsystem();
			container.Kernel.AddSubSystem(SubSystemConstants.NamingKey, target);

			container.Register(Component.For<IUserService>().ImplementedBy<DecoratedUserService>().Named(nameof(DecoratedUserService)),
				Component.For<IUserService>().ImplementedBy<UserService>().Named(nameof(UserService))
			);

			var actualDecoratedUserService = container.Resolve<IUserService>(nameof(DecoratedUserService));
			Assert.NotNull(actualDecoratedUserService);
			var actualUserService = container.Resolve<IUserService>(nameof(UserService));
			Assert.NotNull(actualUserService);
		}

		[Fact]
		public void Can_Decorator_Resolve_Decorated_Class()
		{
			var container = new WindsorContainer();

			var target = new DependencyInjectionNamingSubsystem();
			container.Kernel.AddSubSystem(SubSystemConstants.NamingKey, target);

			container.Register(Component.For<IUserService>().ImplementedBy<DecoratedUserService>(),
				Component.For<IUserService>().ImplementedBy<UserService>()
			);

			var actual = container.Resolve<IUserService>();
			Assert.NotNull(actual);
			Assert.IsType<DecoratedUserService>(actual);
			Assert.NotNull(((DecoratedUserService)actual).UserService);
		}

		[Fact]
		public void Can_Decorator_Resolve_Decorated_Class_When_Registered_Consecutively()
		{
			var container = new WindsorContainer();

			var target = new DependencyInjectionNamingSubsystem();
			container.Kernel.AddSubSystem(SubSystemConstants.NamingKey, target);

			container.Register(Component.For<IUserService>().ImplementedBy<DecoratedUserService>());
			container.Register(Component.For<IUserService>().ImplementedBy<UserService>());

			var actual = container.Resolve<IUserService>();
			Assert.NotNull(actual);
			Assert.IsType<DecoratedUserService>(actual);
			Assert.NotNull(((DecoratedUserService)actual).UserService);
		}
	}
}
