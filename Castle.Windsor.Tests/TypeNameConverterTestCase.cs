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
	using Castle.Windsor.Tests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class TypeNameConverterTestCase
	{
		[SetUp]
		public void SetUpTests()
		{
			converter = new TypeNameConverter(new TypeNameParser());
		}

		private TypeNameConverter converter;

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
		[Test]
		public void Can_handle_generic_of_generics_properly()
		{
			var type = typeof(IGeneric<IGeneric<ICustomer>>);
			var name = type.FullName;
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(type, result);
		}

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
		[Test]
		public void Can_handle_multi_generic_with_generic_of_generics_properly()
		{
			var type = typeof(IDoubleGeneric<ICustomer, IGeneric<ICustomer>>);
			var name = type.Name + "[[" +
			           typeof(ICustomer).Name + "],[" +
			           typeof(IGeneric<>).Name + "[[" + typeof(ICustomer).Name + "]]"
			           + "]]";
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(type, result);
		}

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
		[Test]
		public void Can_handle_multi_generic_with_multi_generic_of_generics_properly_1()
		{
			var type = typeof(IDoubleGeneric<IDoubleGeneric<ICustomer, IClock>, ICustomer>);
			var name = typeof(IDoubleGeneric<,>).Name
			           + "[[" +
			           typeof(IDoubleGeneric<,>).Name +
			           "[[" +
			           typeof(ICustomer).Name + "],[" +
			           typeof(IClock).Name
			           + "]]"
					   + "],[" +
			           typeof(ICustomer).Name + 
			           "]]";
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(type, result);
		}


#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
		[Test]
		public void Can_handle_multi_generic_with_multi_generic_of_generics_properly_2()
		{
			var type = typeof(IDoubleGeneric<ICustomer, IDoubleGeneric<ICustomer, IClock>>);
			var name = typeof(IDoubleGeneric<,>).Name
			           + "[[" +
						   typeof(ICustomer).Name + "],[" +
						   typeof(IDoubleGeneric<,>).Name + 
						   "[[" +
							   typeof(ICustomer).Name + "],[" +
							   typeof(IClock).Name
						   + "]]"
			           + "]]";
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(type, result);
		}

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
		[Test]
		public void Can_load_closed_generic_type_by_Name_single_generic_parameter()
		{
			var type = typeof(IGeneric<ICustomer>);
			var name = type.Name + "[[" + typeof(ICustomer).Name + "]]";
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(result, type);
		}

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
		[Test]
		public void Can_load_closed_generic_type_by_Name_two_generic_parameters()
		{
			var type = typeof(IDoubleGeneric<ICustomer, ISpecification>);
			var name = type.Name + "[[" + typeof(ICustomer).Name + "],[" + typeof(ISpecification) + "]]";
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(result, type);
		}

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
		[Test]
		public void Can_load_open_generic_type_by_name()
		{
			var type = typeof(IGeneric<>);
			var name = type.Name;
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(type, result);
		}


#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
		[Test]
		public void Can_load_type_from_loaded_assembly_by_just_name()
		{
			var type = typeof(ICustomer);
			var name = type.Name;
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(type, result);
		}

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
		[Test]
		public void Can_load_type_from_loaded_assembly_by_name_with_namespace()
		{
			var type = typeof(IService); // notice we have multiple types 'IService in various namespaces'
			var name = type.FullName;
			var result = converter.PerformConversion(name, typeof(Type));
			Assert.AreEqual(type, result);
		}

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
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

#if SILVERLIGHT
		[Ignore("Type conversion does not work in tests under Silverlight because the assembly is not in the starting manifest (of Statlight)")]
#endif
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