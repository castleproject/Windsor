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

namespace Castle.Windsor.Tests
{
	using System;

	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Lifecycle;

	using NUnit.Framework;

	[TestFixture]
	public class TypeNameConverterTestCase
	{
		[SetUp]
		public void SetUpTests()
		{
			converter = new TypeNameConverter();
		}

		private TypeNameConverter converter;

		[Test]
		public void Can_load_closed_generic_type_by_Name_single_generic_parameter()
		{
			var type = typeof(IGeneric<ICustomer>);
			var name = type.Name + "[[" + typeof(ICustomer).Name + "]]";
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(result, type);
		}

		[Test]
		public void Can_load_open_generic_type_by_name()
		{
			var type = typeof(IGeneric<>);
			var name = type.Name;
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(result, type);
		}

		[Test]
		public void Can_load_type_from_loaded_assembly_by_just_name()
		{
			var type = typeof(ICustomer);
			var name = type.Name;
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(result, type);
		}

		[Test]
		public void Can_load_type_from_loaded_assembly_by_name_with_namespace()
		{
			var type = typeof(IService); // notice we have multiple types 'IService in various namespaces'
			var name = type.FullName;
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(result, type);
		}

		[Test]
		public void Throws_when_inner_generic_type_not_unique()
		{
			var type = typeof(IGeneric<IService2>);
			var name = type.Name + "[[" + typeof(IService2).Name + "]]";
			TestDelegate code = () =>

			converter.PerformConversion(name, typeof(Type));

			var exception =
			Assert.Throws(typeof(ConverterException), code);
			Assert.That(exception.Message.StartsWith("Could not uniquely identify type for 'IService2'."));
		}

		[Test]
		public void Throws_when_type_not_unique()
		{
			var type = typeof(IService2);
			var name = type.Name;
			TestDelegate code = () =>

			converter.PerformConversion(name, typeof(Type));

			var exception =
			Assert.Throws(typeof(ConverterException), code);
			Assert.That(exception.Message.StartsWith("Could not uniquely identify type for 'IService2'."));
		}
	}
}