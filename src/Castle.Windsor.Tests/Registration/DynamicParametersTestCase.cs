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

namespace Castle.MicroKernel.Tests.Registration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class DynamicParametersTestCase : AbstractContainerTestCase
	{
		[Test]
		public void Arguments_are_case_insensitive_when_using_anonymous_object()
		{
			var wasCalled = false;
			Kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
			{
				Assert.IsTrue(d.Contains("ArG1"));
				wasCalled = true;
			}));

			Kernel.Resolve<ClassWithArguments>(new Arguments().Insert("arg2", 2).Insert("arg1", "foo"));

			Assert.IsTrue(wasCalled);
		}

		[Test]
		public void Can_dynamically_override_services()
		{
			Kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>()
					.Named("defaultCustomer"),
				Component.For<ICustomer>().ImplementedBy<CustomerImpl2>()
					.Named("otherCustomer")
					.DependsOn(
						Parameter.ForKey("name").Eq("foo"), // static parameters, resolved at registration time
						Parameter.ForKey("address").Eq("bar st 13"),
						Parameter.ForKey("age").Eq("5")),
				Component.For<CommonImplWithDependency>()
					.LifeStyle.Transient
					.DynamicParameters((k, d) => // dynamic parameters
					{
						var randomNumber = 2;
						if (randomNumber == 2)
						{
							d["customer"] = k.Resolve<ICustomer>("otherCustomer");
						}
					}));

			var component = Kernel.Resolve<CommonImplWithDependency>();
			Assert.IsInstanceOf<CustomerImpl2>(component.Customer);
		}

		[Test]
		public void Can_mix_registration_and_call_site_parameters()
		{
			Kernel.Register(
				Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) => d["arg1"] = "foo"));

			var component = Kernel.Resolve<ClassWithArguments>(new Arguments().Insert("arg2", 2));
			Assert.AreEqual(2, component.Arg2);
			Assert.AreEqual("foo", component.Arg1);
		}

		[Test]
		public void Can_release_components_with_dynamic_parameters()
		{
			var releaseCalled = 0;
			Kernel.Register(
				Component.For<ClassWithArguments>().LifeStyle.Transient
					.DynamicParameters((k, d) =>
					{
						d["arg1"] = "foo";
						return kk => ++releaseCalled;
					})
					.DynamicParameters((k, d) => { return kk => ++releaseCalled; }));

			var component = Kernel.Resolve<ClassWithArguments>(new Arguments().Insert("arg2", 2));
			Assert.AreEqual(2, component.Arg2);
			Assert.AreEqual("foo", component.Arg1);

			Kernel.ReleaseComponent(component);
			Assert.AreEqual(2, releaseCalled);
		}

		[Test]
		public void Can_release_generics_with_dynamic_parameters()
		{
			var releaseCalled = 0;
			Kernel.Register(
				Component.For(typeof(IGenericClassWithParameter<>))
					.ImplementedBy(typeof(GenericClassWithParameter<>)).LifeStyle.Transient
					.DynamicParameters((k, d) =>
					{
						d["name"] = "foo";
						return kk => ++releaseCalled;
					})
					.DynamicParameters((k, d) => { return kk => ++releaseCalled; }));

			var component = Kernel.Resolve<IGenericClassWithParameter<int>>(new Arguments().Insert("name", "bar"));
			Assert.AreEqual("foo", component.Name);

			Kernel.ReleaseComponent(component);
			Assert.AreEqual(2, releaseCalled);
		}

		[Test]
		public void DynamicParameters_will_not_enforce_passed_IDictionary_to_be_writeable()
		{
			var wasCalled = false;
			Kernel.Register(Component.For<DefaultCustomer>().LifeStyle.Transient.DynamicParameters((k, d) =>
			{
				Assert.Throws(typeof(NotSupportedException), () =>
				                                             d.Add("foo", "It will throw"));
				wasCalled = true;
			}));

			Kernel.Resolve<DefaultCustomer>(new ReadOnlyDictionary());

			Assert.IsTrue(wasCalled);
		}

		[Test]
		public void Should_handle_multiple_calls()
		{
			var arg1 = "bar";
			var arg2 = 5;
			Kernel.Register(Component.For<ClassWithArguments>()
			                	.LifeStyle.Transient
			                	.DynamicParameters((k, d) => { d["arg1"] = arg1; })
			                	.DynamicParameters((k, d) => { d["arg2"] = arg2; }));
			var component = Kernel.Resolve<ClassWithArguments>(new Arguments().Insert("arg2", 2).Insert("arg1", "foo"));
			Assert.AreEqual(arg1, component.Arg1);
			Assert.AreEqual(arg2, component.Arg2);
		}

		[Test]
		public void Should_have_access_to_parameters_passed_from_call_site()
		{
			string arg1 = null;
			var arg2 = 0;
			Kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
			{
				arg1 = (string)d["arg1"];
				arg2 = (int)d["arg2"];
			}));
			var component = Kernel.Resolve<ClassWithArguments>(new Arguments().Insert("arg2", 2).Insert("arg1", "foo"));
			Assert.AreEqual("foo", arg1);
			Assert.AreEqual(2, arg2);
		}

		[Test]
		public void Should_not_require_explicit_registration()
		{
			Kernel.Register(Component.For<CommonSub2Impl>().LifeStyle.Transient.DynamicParameters((k, d) => { }));
			Assert.DoesNotThrow(() => Kernel.Resolve<CommonSub2Impl>());
		}

		[Test]
		public void Should_override_parameters_passed_from_call_site()
		{
			var arg1 = "bar";
			var arg2 = 5;
			Kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
			{
				d["arg1"] = arg1;
				d["arg2"] = arg2;
			}));
			var component = Kernel.Resolve<ClassWithArguments>(new Arguments().Insert("arg2", 2).Insert("arg1", "foo"));
			Assert.AreEqual(arg1, component.Arg1);
			Assert.AreEqual(arg2, component.Arg2);
		}

		[Test]
		public void Should_resolve_component_when_no_parameters_passed_from_call_site()
		{
			var arg1 = "bar";
			var arg2 = 5;
			Kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
			{
				d["arg1"] = arg1;
				d["arg2"] = arg2;
			}));

			Assert.DoesNotThrow(() =>
			                    Kernel.Resolve<ClassWithArguments>());
		}
	}

	public class ReadOnlyDictionary : Dictionary<object, object>, IDictionary
	{
		public bool IsReadOnly
		{
			get { return true; }
		}

		public new void Add(object key, object value)
		{
			throw new NotSupportedException();
		}
	}
}