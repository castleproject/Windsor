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

namespace Castle.Windsor.Tests.MicroKernel
{
	using System.Collections.Generic;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Components;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class ArgumentsTestCase : AbstractContainerTestCase
	{
		[Test]
		public void By_default_any_type_as_key_is_supported()
		{
			var arguments = new Arguments();
			var key = new object();
			var value = "foo";

			arguments.Add(key, value);

			Assert.AreEqual("foo", arguments[key]);
		}

		[Test]
		[Bug("IOC-147")]
		public void Can_have_dictionary_as_inline_dependency()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<HasDictionaryDependency>());

			var dictionaryProperty = new Dictionary<string, string>();

			var obj = container.Resolve<HasDictionaryDependency>(new Arguments().InsertProperties(new { dictionaryProperty }));
			Assert.AreSame(dictionaryProperty, obj.DictionaryProperty);
		}

		[Test]
		[Bug("IOC-142")]
		public void Can_satisfy_nullable_property_dependency()
		{
			Container.Register(Component.For<HasNullableIntProperty>());

			var arguments = new Arguments("SomeVal", 5);
			var s = Container.Resolve<HasNullableIntProperty>(arguments);

			Assert.IsNotNull(s.SomeVal);
		}

		[Test]
		[Bug("IOC-142")]
		public void Can_satisfy_nullable_ctor_dependency()
		{
			Container.Register(Component.For<HasNullableDoubleConstructor>());

			var s = Container.Resolve<HasNullableDoubleConstructor>(new Arguments("foo", 5d));
			Assert.IsNotNull(s);
		}

		[Test]
		[Bug("IOC-92")]
		public void Can_mix_hashtable_parameters_and_configuration_parameters()
		{
			Container.Register(
				Component.For<HasStringAndIntDependency>()
					.DependsOn(Parameter.ForKey("x").Eq("abc"))
				);

			Container.Resolve<HasStringAndIntDependency>(new Arguments("y", 1));
		}

		[Test]
		public void Handles_Type_as_key()
		{
			var arguments = new Arguments();
			var key = typeof(object);
			var value = new object();

			arguments.Add(key, value);

			Assert.AreEqual(1, arguments.Count);
			Assert.IsTrue(arguments.Contains(key));
			Assert.AreSame(value, arguments[key]);
		}

		[Test]
		public void Handles_string_as_key()
		{
			var arguments = new Arguments();
			var key = "Foo";
			var value = new object();

			arguments.Add(key, value);

			Assert.AreEqual(1, arguments.Count);
			Assert.IsTrue(arguments.Contains(key));
			Assert.AreSame(value, arguments[key]);
		}

		[Test]
		public void Handles_string_as_key_case_insensitive()
		{
			var arguments = new Arguments();
			var key = "foo";
			var value = new object();

			arguments.Add(key, value);

			Assert.IsTrue(arguments.Contains(key.ToLower()));
			Assert.IsTrue(arguments.Contains(key.ToUpper()));
		}
	}

	public class CustomStringComparer : IArgumentsComparer
	{
		public bool RunEqualityComparison(object x, object y, out bool areEqual)
		{
			if (x is string)
			{
				areEqual = true;
				return true;
			}
			areEqual = false;
			return false;
		}

		public bool RunHasCodeCalculation(object o, out int hashCode)
		{
			if (o is string)
			{
				hashCode = "boo!".GetHashCode();
				return true;
			}
			hashCode = 0;
			return false;
		}
	}
}