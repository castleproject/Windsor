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
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class TypedDependenciesTestCase
	{
		private IKernel kernel;

		[Test]
		public void Can_mix_typed_arguments_with_named()
		{
			kernel.Register(Component.For<ClassWithArguments>());
			var arguments = new Dictionary<object, object>
			{
				{ "arg1", "foo" },
				{ typeof(int), 2 }
			};

			var item = kernel.Resolve<ClassWithArguments>(arguments);

			Assert.AreEqual("foo", item.Arg1);
			Assert.AreEqual(2, item.Arg2);
		}

		[Test]
		public void Can_named_arguments_take_precedense_over_typed()
		{
			kernel.Register(Component.For<ClassWithArguments>());
			var arguments = new Dictionary<object, object>
			{
				{ "arg1", "named" },
				{ typeof(string), "typed" },
				{ typeof(int), 2 }
			};

			var item = kernel.Resolve<ClassWithArguments>(arguments);

			Assert.AreEqual("named", item.Arg1);
			Assert.AreEqual(2, item.Arg2);
		}

		[Test]
		public void Can_resolve_component_with_typed_arguments()
		{
			kernel.Register(Component.For<ClassWithArguments>());
			var arguments = new Dictionary<object, object>
			{
				{ typeof(string), "foo" },
				{ typeof(int), 2 }
			};

			var item = kernel.Resolve<ClassWithArguments>(arguments);

			Assert.AreEqual("foo", item.Arg1);
			Assert.AreEqual(2, item.Arg2);
		}

		[SetUp]
		public void SetUpTests()
		{
			kernel = new DefaultKernel();
		}

		[Test]
		public void Typed_arguments_work_for_DynamicParameters()
		{
			kernel.Register(Component.For<ClassWithArguments>().DynamicParameters((k, d) => d.Insert("typed").Insert(2)));

			var item = kernel.Resolve<ClassWithArguments>();

			Assert.AreEqual("typed", item.Arg1);
			Assert.AreEqual(2, item.Arg2);
		}

		[Test]
		public void Typed_arguments_work_for_DynamicParameters_mixed()
		{
			kernel.Register(Component.For<ClassWithArguments>().DynamicParameters((k, d) => d.Insert("typed")));
			var arguments = new Dictionary<object, object>
			{
				{ typeof(int), 2 }
			};
			var item = kernel.Resolve<ClassWithArguments>(arguments);

			Assert.AreEqual("typed", item.Arg1);
			Assert.AreEqual(2, item.Arg2);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Typed_arguments_work_for_DynamicParameters_mixed2()
		{
			kernel.Resolve<ClassWithArguments>(new Arguments(new Dictionary<Type, object>
			{
				{ typeof(int), "not an int" },
				{ typeof(string), "a string" }
			}));
		}

		[Test]
		public void Typed_arguments_work_for_InLine_Parameters()
		{
			kernel.Register(Component.For<ClassWithArguments>()
			                	.DependsOn(Property.ForKey<string>().Eq("typed"),
			                	           Property.ForKey<int>().Eq(2)));

			var item = kernel.Resolve<ClassWithArguments>();

			Assert.AreEqual("typed", item.Arg1);
			Assert.AreEqual(2, item.Arg2);
		}

		[Test]
		public void Typed_arguments_work_for_ServiceOverrides()
		{
			kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl1>().Named("default"));
			kernel.Register(Component.For<ICommon>().ImplementedBy<CommonImpl2>().Named("non-default"));
			kernel.Register(
				Component.For<CommonServiceUser>().ServiceOverrides(ServiceOverride.ForKey<ICommon>().Eq("non-default")));

			var item = kernel.Resolve<CommonServiceUser>();

			Assert.IsInstanceOf<CommonImpl2>(item.CommonService);
		}

		[Test]
		public void Typed_arguments_work_for_closed_generic_ServiceOverrides()
		{
			kernel.Register(Component.For<IGeneric<string>>().ImplementedBy<GenericImpl1<string>>().Named("default"));
			kernel.Register(Component.For<IGeneric<string>>().ImplementedBy<GenericImpl2<string>>().Named("non-default"));
			kernel.Register(
				Component.For<UsesIGeneric<string>>().ServiceOverrides(ServiceOverride.ForKey<IGeneric<string>>().Eq("non-default")));

			var item = kernel.Resolve<UsesIGeneric<string>>();

			Assert.IsInstanceOf<GenericImpl2<string>>(item.Dependency);
		}

		[Test]
		public void Typed_arguments_work_for_open_generic_ServiceOverrides_closed_service()
		{
			kernel.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)).Named("default"));
			kernel.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)).Named("non-default"));
			kernel.Register(Component.For(typeof(UsesIGeneric<>))
			                	.ServiceOverrides(ServiceOverride.ForKey(typeof(IGeneric<string>)).Eq("non-default")));

			var item = kernel.Resolve<UsesIGeneric<string>>();

			Assert.IsInstanceOf<GenericImpl2<string>>(item.Dependency);
		}

		[Test]
		public void Typed_arguments_work_for_open_generic_ServiceOverrides_closed_service_preferred_over_open_service()
		{
			kernel.Register(
				Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)).Named("default"),
				Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)).Named("non-default-open").DependsOn(
					Property.ForKey("value").Eq(1)),
				Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)).Named("non-default-int").DependsOn(
					Property.ForKey("value").Eq(2)),
				Component.For(typeof(UsesIGeneric<>)).ServiceOverrides(
					ServiceOverride.ForKey(typeof(IGeneric<>)).Eq("non-default-open"),
					ServiceOverride.ForKey(typeof(IGeneric<int>)).Eq("non-default-int"))
				);

			var withString = kernel.Resolve<UsesIGeneric<string>>();
			var withInt = kernel.Resolve<UsesIGeneric<int>>();

			Assert.IsInstanceOf<GenericImpl2<string>>(withString.Dependency);
			Assert.AreEqual(1, (withString.Dependency as GenericImpl2<string>).Value);
			Assert.IsInstanceOf<GenericImpl2<int>>(withInt.Dependency);
			Assert.AreEqual(2, (withInt.Dependency as GenericImpl2<int>).Value);
		}

		[Test]
		public void Typed_arguments_work_for_open_generic_ServiceOverrides_open_service()
		{
			kernel.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl1<>)).Named("default"));
			kernel.Register(Component.For(typeof(IGeneric<>)).ImplementedBy(typeof(GenericImpl2<>)).Named("non-default"));
			kernel.Register(Component.For(typeof(UsesIGeneric<>))
			                	.ServiceOverrides(ServiceOverride.ForKey(typeof(IGeneric<>)).Eq("non-default")));

			var item = kernel.Resolve<UsesIGeneric<string>>();

			Assert.IsInstanceOf<GenericImpl2<string>>(item.Dependency);
		}
	}
}