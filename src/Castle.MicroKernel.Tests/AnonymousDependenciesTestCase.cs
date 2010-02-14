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
	public class AnonymousDependenciesTestCase
	{

		[SetUp]
		public void SetUpTests()
		{
			kernel = new DefaultKernel();
		}

		private IKernel kernel;

		[Test]
		public void Can_mix_typed_arguments_with_named()
		{
			kernel.AddComponent<ClassWithArguments>();
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
			kernel.AddComponent<ClassWithArguments>();
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
			kernel.AddComponent<ClassWithArguments>();
			var arguments = new Dictionary<object, object>
			{
				{ typeof(string), "foo" },
				{ typeof(int), 2 }
			};

			var item = kernel.Resolve<ClassWithArguments>(arguments);

			Assert.AreEqual("foo", item.Arg1);
			Assert.AreEqual(2, item.Arg2);
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
	}
}