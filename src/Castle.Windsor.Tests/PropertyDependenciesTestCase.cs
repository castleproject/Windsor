// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

	using CastleTests.ClassComponents;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class PropertyDependenciesTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Can_opt_out_of_setting_base_properties_via_enum()
		{
			Container.Register(
				Component.For<A>(),
				Component.For<B>(),
				Component.For<AbPropChild>().Properties(PropertyFilter.IgnoreBase));

			var item = Container.Resolve<AbPropChild>();
			Assert.IsNull(item.Prop);
			Assert.IsNotNull(item.PropB);
		}

		[Test]
		public void Can_opt_out_of_setting_properties_open_generic_via_enum()
		{
			Container.Register(Component.For(typeof(GenericImpl2<>))
				                   .DependsOn(Dependency.OnValue(typeof(int), 5))
				                   .Properties(PropertyFilter.IgnoreAll));

			var item = Container.Resolve<GenericImpl2<A>>();
			Assert.AreEqual(0, item.Value);
		}

		[Test]
		public void Can_opt_out_of_setting_properties_open_generic_via_predicate()
		{
			Container.Register(Component.For(typeof(GenericImpl2<>))
				                   .DependsOn(Dependency.OnValue(typeof(int), 5))
				                   .PropertiesIgnore(p => true));

			var item = Container.Resolve<GenericImpl2<A>>();
			Assert.AreEqual(0, item.Value);
		}

		[Test]
		public void Can_opt_out_of_setting_properties_via_enum()
		{
			Container.Register(
				Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
				Component.For<CommonServiceUser2>()
					.Properties(PropertyFilter.IgnoreAll));

			var item = Container.Resolve<CommonServiceUser2>();
			Assert.IsNull(item.CommonService);
		}

		[Test]
		public void Can_opt_out_of_setting_properties_via_predicate()
		{
			Container.Register(
				Component.For<ICommon>().ImplementedBy<CommonImpl1>(),
				Component.For<CommonServiceUser2>()
					.PropertiesIgnore(p => true));

			var item = Container.Resolve<CommonServiceUser2>();
			Assert.IsNull(item.CommonService);
		}

		[Test]
		public void Can_require_setting_properties_open_generic_via_enum()
		{
			Container.Register(Component.For(typeof(GenericImpl2<>)).Properties(PropertyFilter.RequireAll));

			Assert.Throws<HandlerException>(() => Container.Resolve<GenericImpl2<A>>());
		}

		[Test]
		public void Can_require_setting_properties_open_generic_via_predicate()
		{
			Container.Register(Component.For(typeof(GenericImpl2<>)).PropertiesRequire(p => true));

			Assert.Throws<HandlerException>(() => Container.Resolve<GenericImpl2<A>>());
		}

		[Test]
		public void Can_require_setting_properties_via_enum()
		{
			Container.Register(Component.For<CommonServiceUser2>().Properties(PropertyFilter.RequireAll));

			Assert.Throws<HandlerException>(() => Container.Resolve<CommonServiceUser2>());
		}

		[Test]
		public void Can_require_setting_properties_via_predicate()
		{
			Container.Register(Component.For<CommonServiceUser2>().PropertiesRequire(p => true));

			Assert.Throws<HandlerException>(() => Container.Resolve<CommonServiceUser2>());
		}

		[Test]
		public void First_one_wins()
		{
			Container.Register(Component.For<CommonServiceUser2>().Properties(PropertyFilter.IgnoreAll)
				                   .Properties(PropertyFilter.RequireAll));

			Container.Resolve<CommonServiceUser2>();
		}

		[Test]
		public void First_one_wins_2()
		{
			Container.Register(Component.For<CommonServiceUser2>().Properties(PropertyFilter.RequireAll)
				                   .Properties(PropertyFilter.IgnoreAll));

			Assert.Throws<HandlerException>(() => Container.Resolve<CommonServiceUser2>());
		}
	}
}