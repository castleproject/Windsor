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

namespace Castle.MicroKernel.Tests
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
	public class TransientMultiConstructorTestCase
	{
		[Test]
		public void TransientMultiConstructorTest()
		{
			DefaultKernel container = new DefaultKernel();
			((IKernel)container).Register(Component.For(typeof(FooBar)).Named("FooBar"));

			var arguments1 = new Dictionary<object, object>();
			arguments1.Add("integer", 1);

			var arguments2 = new Dictionary<object, object>();
			arguments2.Add("datetime", DateTime.Now.AddDays(1));

			object a = container.Resolve(typeof(FooBar), arguments1);
			object b = container.Resolve(typeof(FooBar), arguments2);

			Assert.AreNotSame(a, b, "A should not be B");
		}

		[Test]
		public void TransientMultipleConstructorNonValueTypeTest()
		{
			DefaultKernel container = new DefaultKernel();
			((IKernel)container).Register(Component.For(typeof(FooBarNonValue)).Named("FooBar"));
			Tester1 bla1 = new Tester1("FOOBAR");
			Tester2 bla2 = new Tester2(666);

			var arguments1 = new Dictionary<object, object>();
			arguments1.Add("test1", bla1);

			var arguments2 = new Dictionary<object, object>();
			arguments2.Add("test2", bla2);

			object a = container.Resolve(typeof(FooBarNonValue), arguments1);
			object b = container.Resolve(typeof(FooBarNonValue), arguments2);

			Assert.AreNotSame(a, b, "A should not be B");

			// multi resolve test

			a = container.Resolve(typeof(FooBarNonValue), arguments1);
			b = container.Resolve(typeof(FooBarNonValue), arguments2);

			Assert.AreNotSame(a, b, "A should not be B");
		}
	}

	[Transient]
	public class FooBar
	{
		public FooBar(int integer)
		{
		}

		public FooBar(DateTime datetime)
		{
		}
	}

	public class Tester1
	{
		public string bar;

		public Tester1(string bar)
		{
			this.bar = bar;
		}
	}

	public class Tester2
	{
		public int foo;

		public Tester2(int foo)
		{
			this.foo = foo;
		}
	}

	[Transient]
	public class FooBarNonValue
	{
		public FooBarNonValue(Tester1 test1)
		{
		}

		public FooBarNonValue(Tester2 test2)
		{
		}
	}
}