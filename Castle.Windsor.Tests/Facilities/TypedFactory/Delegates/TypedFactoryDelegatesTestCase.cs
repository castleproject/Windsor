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

namespace Castle.Windsor.Tests.Facilities.TypedFactory.Delegates
{
	using System;

	using Castle.Core;
	using Castle.Facilities.LightweighFactory.Tests;
	using Castle.Facilities.TypedFactory;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryDelegatesTestCase
	{

		[SetUp]
		public void SetUpTests()
		{
			container = new WindsorContainer();
			container.AddFacility<LightweightFactoryFacility>();
		}

		private WindsorContainer container;

		[Test]
		public void Affects_constructor_resolution()
		{
			container.AddComponent<Baz>("baz");
			container.AddComponent<HasTwoConstructors>("fizz");
			var factory = container.Resolve<Func<string, HasTwoConstructors>>("fizzFactory");

			var obj = factory("naaaameee");
			Assert.AreEqual("naaaameee", obj.Name);
		}

		[Test]
		public void Can_resolve_service_via_delegate()
		{
			container.AddComponentLifeStyle<Foo>("MyFoo", LifestyleType.Transient);
			container.AddComponent<UsesFooDelegate>();
			var dependsOnFoo = container.Resolve<UsesFooDelegate>();
			Foo foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(2, foo.Number);
		}

		[Test]
		public void Delegate_obeys_lifestyle()
		{
			container.AddComponentLifeStyle<Foo>("MyFoo", LifestyleType.Singleton);
			container.AddComponent<UsesFooDelegate>();
			var dependsOnFoo = container.Resolve<UsesFooDelegate>();
			Foo foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
		}

		[Test]
		public void Delegate_parameters_are_used_in_order_first_ctor_then_properties()
		{
			container.AddComponent<Baz>("baz");
			container.AddComponent<Bar>("bar");
			container.AddComponent<UsesBarDelegate>("barBar");

			var dependsOnFoo = container.Resolve<UsesBarDelegate>();
			var bar = dependsOnFoo.GetBar("a name", "a description");
			Assert.AreEqual("a name", bar.Name);
			Assert.AreEqual("a description", bar.Description);
		}

		[Test]
		public void Delegate_pulls_unspecified_dependencies_from_container()
		{
			container.AddComponent<Baz>("baz");
			container.AddComponent<Bar>("bar");
			container.AddComponent<UsesBarDelegate>("uBar");

			var dependsOnFoo = container.Resolve<UsesBarDelegate>();
			dependsOnFoo.GetBar("aaa", "bbb");
		}

		[Test]
		public void Does_not_duplicate_arguments_matching_delegate_parameters()
		{
			container.AddComponent<HasOnlyOneArgMatchingDelegatesParameter>("fizz");
			var factory = container.Resolve<Func<string, string, HasOnlyOneArgMatchingDelegatesParameter>>("fizzFactory");
			var obj = factory("arg1", "name");
			Assert.AreEqual("name", obj.Name);
			Assert.AreEqual("arg1", obj.Arg1);
		}
	}
}