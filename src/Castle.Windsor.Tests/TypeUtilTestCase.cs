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

namespace Castle
{
	using Castle.ClassComponents;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class TypeUtilTestCase
	{
		[Test]
		public void Closed_generic_double_type()
		{
			var name = typeof(IDoubleGeneric<A, A2>).ToCSharpString();
			Assert.AreEqual("IDoubleGeneric<A, A2>", name);
		}

		[Test]
		public void Closed_generic_on_generic_double_type()
		{
			var name = typeof(IDoubleGeneric<GenericImpl1<A>, A2>).ToCSharpString();
			Assert.AreEqual("IDoubleGeneric<GenericImpl1<A>, A2>", name);
		}

		[Test]
		public void Closed_generic_on_generic_simple_type()
		{
			var name = typeof(GenericImpl1<GenericImpl2<A>>).ToCSharpString();
			Assert.AreEqual("GenericImpl1<GenericImpl2<A>>", name);
		}

		[Test]
		public void Closed_generic_simple_type()
		{
			var name = typeof(GenericImpl1<A>).ToCSharpString();
			Assert.AreEqual("GenericImpl1<A>", name);
		}

		[Test]
		public void Generic_nested_generic_typeArray_multi_dimentional_pulls_closed_generics_to_innermost_type()
		{
			var name = typeof(GenericHasNested<A2>.NestedGeneric<AProp>[,,]).ToCSharpString();
			Assert.AreEqual("GenericHasNested<·TOuter·>.NestedGeneric<A2, AProp>[,,]", name);
		}

		[Test]
		public void Generic_nested_generic_typeArray_pulls_closed_generics_to_innermost_type()
		{
			var name = typeof(GenericHasNested<A2>.NestedGeneric<AProp>[]).ToCSharpString();
			Assert.AreEqual("GenericHasNested<·TOuter·>.NestedGeneric<A2, AProp>[]", name);
		}

		[Test]
		public void Generic_nested_generic_type_pulls_closed_generics_to_innermost_type()
		{
			var name = typeof(GenericHasNested<A2>.NestedGeneric<AProp>).ToCSharpString();
			Assert.AreEqual("GenericHasNested<·TOuter·>.NestedGeneric<A2, AProp>", name);
		}

		[Test]
		public void Generic_nested_type_array_ignores_outer_generic_argument()
		{
			var name = typeof(GenericHasNested<A2>.Nested[]).ToCSharpString();
			Assert.AreEqual("GenericHasNested<·TOuter·>.Nested<A2>[]", name);
		}

		[Test]
		public void Generic_nested_type_ignores_outer_generic_argument()
		{
			var name = typeof(GenericHasNested<A2>.Nested).ToCSharpString();
			Assert.AreEqual("GenericHasNested<·TOuter·>.Nested<A2>", name);
		}

		[Test]
		public void Non_generic_nested_type()
		{
			var name = typeof(HasNestedType.Nested).ToCSharpString();
			Assert.AreEqual("HasNestedType.Nested", name);
		}

		[Test]
		public void Non_generic_nested_type_array()
		{
			var name = typeof(HasNestedType.Nested[]).ToCSharpString();
			Assert.AreEqual("HasNestedType.Nested[]", name);
		}

		[Test]
		public void Non_generic_simple_type()
		{
			var name = typeof(APropCtor).ToCSharpString();
			Assert.AreEqual("APropCtor", name);
		}

		[Test]
		public void Non_generic_simple_type_array()
		{
			var name = typeof(APropCtor[]).ToCSharpString();
			Assert.AreEqual("APropCtor[]", name);
		}

		[Test]
		public void Open_generic_double_type()
		{
			var name = typeof(IDoubleGeneric<,>).ToCSharpString();
			Assert.AreEqual("IDoubleGeneric<·TOne·, ·TTwo·>", name);
		}

		[Test]
		public void Open_generic_simple_type()
		{
			var name = typeof(GenericImpl1<>).ToCSharpString();
			Assert.AreEqual("GenericImpl1<·T·>", name);
		}
	}
}