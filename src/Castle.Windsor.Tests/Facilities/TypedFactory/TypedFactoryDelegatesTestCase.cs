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

namespace Castle.Windsor.Tests.Facilities.TypedFactory
{
	using System;

	using Castle.Core;
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Delegates;

	using NUnit.Framework;

	[TestFixture]
	public class TypedFactoryDelegatesTestCase
	{
		[SetUp]
		public void SetUpTests()
		{
			container = new WindsorContainer();
			container.AddFacility<TypedFactoryFacility>();
		}

		private WindsorContainer container;

		[Test]
		public void Affects_constructor_resolution()
		{
			container.Register(Component.For<Baz>().Named("baz"));
			container.Register(Component.For<HasTwoConstructors>().Named("fizz"));
			var factory = container.Resolve<Func<string, HasTwoConstructors>>();

			var obj = factory("naaaameee");
			Assert.AreEqual("naaaameee", obj.Name);
		}

		[Test]
		public void Can_resolve_service_via_delegate()
		{
			container.Register(Component.For<Foo>().Named("MyFoo").LifeStyle.Is(LifestyleType.Transient));
			((IWindsorContainer)container).Register(Component.For<UsesFooDelegate>());
			var dependsOnFoo = container.Resolve<UsesFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(2, foo.Number);
		}

		[Test]
		public void Delegate_obeys_lifestyle()
		{
			container.Register(Component.For<Foo>().Named("MyFoo").LifeStyle.Is(LifestyleType.Singleton));
			container.Register(Component.For<UsesFooDelegate>());
			var dependsOnFoo = container.Resolve<UsesFooDelegate>();
			var foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Delegate_parameters_are_used_in_order_first_ctor_then_properties()
		{
			container.Register(Component.For(typeof(Baz)).Named("baz"));
			container.Register(Component.For(typeof(Bar)).Named("bar"));
			container.Register(Component.For(typeof(UsesBarDelegate)).Named("barBar"));

			var dependsOnFoo = container.Resolve<UsesBarDelegate>();
			var bar = dependsOnFoo.GetBar("a name", "a description");
			Assert.AreEqual("a name", bar.Name);
			Assert.AreEqual("a description", bar.Description);
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Delegate_pulls_unspecified_dependencies_from_container()
		{
			container.Register(Component.For(typeof(Baz)).Named("baz"));
			container.Register(Component.For(typeof(Bar)).Named("bar"));
			container.Register(Component.For(typeof(UsesBarDelegate)).Named("uBar"));

			var dependsOnFoo = container.Resolve<UsesBarDelegate>();
			dependsOnFoo.GetBar("aaa", "bbb");
		}

		[Test]
		[Ignore("Not supported for Func delegates")]
		public void Does_not_duplicate_arguments_matching_delegate_parameters()
		{
			container.Register(Component.For(typeof(HasOnlyOneArgMatchingDelegatesParameter)).Named("fizz"));
			var factory = container.Resolve<Func<string, string, HasOnlyOneArgMatchingDelegatesParameter>>("fizzFactory");
			var obj = factory("arg1", "name");
			Assert.AreEqual("name", obj.Name);
			Assert.AreEqual("arg1", obj.Arg1);
		}

		[Test]
		public void Releasing_component_depending_on_the_delegate_factory_releases_what_was_pulled_from_it()
		{
			DisposableFoo.DisposedCount = 0;

			container.Register(Component.For<DisposableFoo>().LifeStyle.Transient,
			                   Component.For<UsesDisposableFooDelegate>().LifeStyle.Transient);
			var dependsOnFoo = container.Resolve<UsesDisposableFooDelegate>();
			var foo = dependsOnFoo.GetFoo();

			Assert.AreEqual(0, DisposableFoo.DisposedCount);
			container.Release(dependsOnFoo);
			Assert.AreEqual(1, DisposableFoo.DisposedCount);
		}

		[Test]
		public void Using_Func_delegate_with_duplicated_Parameter_types_throws_exception()
		{
			container.Register(Component.For(typeof(Baz)).Named("baz"));
			container.Register(Component.For(typeof(Bar)).Named("bar"));
			((IWindsorContainer)container).Register(Component.For<UsesBarDelegate>());

			var user = container.Resolve<UsesBarDelegate>();

			var exception =
				Assert.Throws<ArgumentException>(() =>
				                                 user.GetBar("aaa", "bbb"));

			Assert.AreEqual(
				"Factory delegate System.Func`3[System.String,System.String,Castle.Windsor.Tests.Facilities.TypedFactory.Delegates.Bar] has duplicated arguments of type System.String. " +
				"Using generic purpose delegates with duplicated argument types is unsupported, because then it is not possible to match arguments properly. " +
				"Use some custom delegate with meaningful argument names or interface based factory instead.",
				exception.Message);
		}
	}
}