// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class DynamicParametersTestCase
	{
		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void TearDown()
		{
			kernel.Dispose();
		}

		private DefaultKernel kernel;

		[Test]
		public void Can_mix_registration_and_call_site_parameters()
		{
			kernel.Register(
				Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) => d["arg1"] = "foo"));

			var component = kernel.Resolve<ClassWithArguments>(new { arg2 = 2 });
			Assert.AreEqual(2, component.Arg2);
			Assert.AreEqual("foo", component.Arg1);
		}

		[Test]
		public void Can_dynamically_override_services()
		{
			kernel.Register(
				Component.For<ICustomer>()
					.ImplementedBy<CustomerImpl>()
					.Named("defaultCustomer"),
				Component.For<ICustomer>().ImplementedBy<CustomerImpl2>()
					.Named("otherCustomer")
					.Parameters(
						Parameter.ForKey("name").Eq("foo"), // static parameters, resolved at registration time
						Parameter.ForKey("address").Eq("bar st 13"),
						Parameter.ForKey("age").Eq("5")),
				Component.For<CommonImplWithDependancy>()
					.LifeStyle.Transient
					.DynamicParameters((k, d) => // dynamic parameters
					{
						var randomNumber = 2;
						if (randomNumber == 2)
						{
							d["customer"] = k.Resolve<ICustomer>("otherCustomer");
						}
					}));

			var component = kernel.Resolve<CommonImplWithDependancy>();
			Assert.IsInstanceOf<CustomerImpl2>(component.Customer);
		}

		[Test]
		public void Should_have_access_to_parameters_passed_from_call_site()
		{
			string arg1 = null;
			int arg2 = 0;
			kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
			{
				arg1 = (string)d["arg1"];
				arg2 = (int)d["arg2"];
			}));
			var component = kernel.Resolve<ClassWithArguments>(new { arg2 = 2, arg1 = "foo" });
			Assert.AreEqual("foo", arg1);
			Assert.AreEqual(2, arg2);
		}

		[Test]
		public void Should_not_require_explicit_registration()
		{
			kernel.Register(Component.For<CommonSub2Impl>().LifeStyle.Transient.DynamicParameters((k, d) => { }));
			Assert.DoesNotThrow(() => kernel.Resolve<CommonSub2Impl>());
		}

		[Test]
		public void Should_override_parameters_passed_from_call_site()
		{
			string arg1 = "bar";
			int arg2 = 5;
			kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
			{
				d["arg1"] = arg1;
				d["arg2"] = arg2;
			}));
			var component = kernel.Resolve<ClassWithArguments>(new { arg2 = 2, arg1 = "foo" });
			Assert.AreEqual(arg1, component.Arg1);
			Assert.AreEqual(arg2, component.Arg2);
		}

		[Test]
		public void Should_handle_multiple_calls()
		{
			string arg1 = "bar";
			int arg2 = 5;
			kernel.Register(Component.For<ClassWithArguments>()
			                	.LifeStyle.Transient
			                	.DynamicParameters((k, d) =>
			                	{
			                		d["arg1"] = arg1;
			                	})
			                	.DynamicParameters((k, d) =>
			                	{
			                		d["arg2"] = arg2;
			                	}));
			var component = kernel.Resolve<ClassWithArguments>(new { arg2 = 2, arg1 = "foo" });
			Assert.AreEqual(arg1, component.Arg1);
			Assert.AreEqual(arg2, component.Arg2);
		}

		[Test]
		public void Should_resolve_component_when_no_parameters_passed_from_call_site()
		{
			string arg1 = "bar";
			int arg2 = 5;
			kernel.Register(Component.For<ClassWithArguments>().LifeStyle.Transient.DynamicParameters((k, d) =>
			{
				d["arg1"] = arg1;
				d["arg2"] = arg2;
			}));
			//Assert.DoesNotThrow(() =>
			kernel.Resolve<ClassWithArguments>();//);
		}
	}
}