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
	using System.Collections.Generic;

	using Castle.MicroKernel.Context;

	using NUnit.Framework;

	[TestFixture]
	public class ComponentArgumentsEnumeratorTestCase
	{
		private ComponentArgumentsEnumerator enumerator;

		private void BuildEnumeratorFor(params IArgumentsStore[] stores)
		{
			enumerator = new ComponentArgumentsEnumerator(new List<IArgumentsStore>(stores));
			return;
		}

		[Test]
		public void Can_enumerate_single_empty()
		{
			BuildEnumeratorFor(new NamedArgumentsStore());
			Assert.IsNull(enumerator.Value);
			Assert.IsFalse(enumerator.MoveNext());
		}

		[Test]
		public void Can_enumerate_two_empty()
		{
			BuildEnumeratorFor(new NamedArgumentsStore(), new TypedArgumentsStore());

			Assert.IsNull(enumerator.Value);
			Assert.IsFalse(enumerator.MoveNext());
		}

		[Test]
		public void Can_enumerate_two_first_empty()
		{
			BuildEnumeratorFor(new NamedArgumentsStore { { "foo", "bar" } }, new TypedArgumentsStore());

			Assert.IsNull(enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("bar", enumerator.Value);
			Assert.IsFalse(enumerator.MoveNext());
		}

		[Test]
		public void Can_enumerate_two_non_empty()
		{
			BuildEnumeratorFor(new TypedArgumentsStore { { typeof(object), "baz" } },
			                   new NamedArgumentsStore { { "foo", "bar" } });

			Assert.IsNull(enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("baz", enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("bar", enumerator.Value);
			Assert.IsFalse(enumerator.MoveNext());
		}

		[Test]
		public void Can_enumerate_two_non_empty_first_bigger()
		{
			BuildEnumeratorFor(new NamedArgumentsStore { { "foo", "bar" }, { "fizz", "buzz" } },
			                   new TypedArgumentsStore { { typeof(object), "baz" } });

			Assert.IsNull(enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("bar", enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("buzz", enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("baz", enumerator.Value);
			Assert.IsFalse(enumerator.MoveNext());
		}

		[Test]
		public void Can_enumerate_two_non_empty_second_bigger()
		{
			BuildEnumeratorFor(new TypedArgumentsStore { { typeof(object), "baz" } },
			                   new NamedArgumentsStore { { "foo", "bar" }, { "fizz", "buzz" } });

			Assert.IsNull(enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("baz", enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("bar", enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("buzz", enumerator.Value);
			Assert.IsFalse(enumerator.MoveNext());
		}

		[Test]
		public void Can_enumerate_two_second_empty()
		{
			BuildEnumeratorFor(new TypedArgumentsStore(), new NamedArgumentsStore { { "foo", "bar" } });

			Assert.IsNull(enumerator.Value);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual("bar", enumerator.Value);
			Assert.IsFalse(enumerator.MoveNext());
		}
	}
}