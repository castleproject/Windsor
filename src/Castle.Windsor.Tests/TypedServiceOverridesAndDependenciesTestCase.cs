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

namespace CastleTests
{
	using Castle.Core;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using CastleTests.Components;

	using NUnit.Framework;

	public class TypedServiceOverridesAndDependenciesTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Picks_component_implemented_by_that_type()
		{
			Container.Register(Component.For<CommonServiceUser>()
			                   	.DependsOn(Dependency.OnComponent<ICommon, CommonImpl2>()),
			                   Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
			                   Component.For<ICommon>().ImplementedBy<CommonImpl2>());

			var item = Container.Resolve<CommonServiceUser>();
			Assert.IsInstanceOf<CommonImpl2>(item.CommonService);
		}

		[Test]
		public void Picks_component_implemented_by_that_type_open_generic()
		{
			Container.Register(Component.For(typeof(UsesIGeneric<>))
			                   	.DependsOn(Dependency.OnComponent(typeof(IGeneric<>), typeof(GenericImpl2<>))),
			                   Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)),
			                   Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)));

			var item = Container.Resolve<UsesIGeneric<A>>();
			Assert.IsInstanceOf<GenericImpl2<A>>(item.Dependency);
		}

		[Test]
		public void Picks_component_implemented_by_that_type_open_generic_if_matching_closed_registered()
		{
			Container.Register(Component.For(typeof(UsesIGeneric<>))
			                   	.DependsOn(Dependency.OnComponent(typeof(IGeneric<>), typeof(GenericImpl2<>))),
			                   Component.For<IGeneric<A>>().ImplementedBy<GenericImpl3<A>>(),
			                   Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)),
			                   Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)));

			var item = Container.Resolve<UsesIGeneric<A>>();
			Assert.IsInstanceOf<GenericImpl2<A>>(item.Dependency);
		}

		[Test(
			Description =
				"This is not exactly perfect. IMO we should throw here, but making it work like that would require some serious changes and I don't think it's a common scenario."
			)]
		public void Picks_component_implemented_by_that_type_with_default_name_if_multiple()
		{
			Container.Register(Component.For<CommonServiceUser>()
			                   	.DependsOn(Dependency.OnComponent<ICommon, CommonImpl2>()),
			                   Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
			                   Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("something"),
			                   Component.For<ICommon>().ImplementedBy<CommonImpl2>());

			var item = Container.Resolve<CommonServiceUser>();
			Assert.IsInstanceOf<CommonImpl2>(item.CommonService);

			var default2 = Container.Resolve<ICommon>(ComponentName.DefaultNameFor(typeof(CommonImpl2)));

			Assert.AreSame(default2, item.CommonService);
		}

		[Test]
		public void Throws_if_component_implemented_by_that_type_non_default_name()
		{
			Container.Register(Component.For<CommonServiceUser>()
			                   	.DependsOn(Dependency.OnComponent<ICommon, CommonImpl2>()),
			                   Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
			                   Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("two"));

			Assert.Throws<HandlerException>(() => Container.Resolve<CommonServiceUser>());
		}
	}
}