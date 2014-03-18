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

namespace CastleTests
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading;

	using Castle.Core.Configuration;
	using Castle.MicroKernel.SubSystems.Conversion;

	using NUnit.Framework;

	[TestFixture]
	public class DefaultConversionManagerTestCase
	{
		private readonly DefaultConversionManager converter = new DefaultConversionManager();

#if !SILVERLIGHT
		// currently not supported by SL
		[Test]
		[SetCulture("pl-PL")]
		[Bug("IOC-314")]
		public void Converting_numbers_uses_oridinal_culture()
		{
			Assert.AreEqual(",", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);

			var result = converter.PerformConversion<Decimal>("123.456");

			Assert.AreEqual(123.456m, result);
		}
#endif

		[Test]
		public void PerformConversionInt()
		{
			Assert.AreEqual(100, converter.PerformConversion("100", typeof(int)));
			Assert.AreEqual(1234, converter.PerformConversion("1234", typeof(int)));
		}

		[Test]
		public void PerformConversionChar()
		{
			Assert.AreEqual('a', converter.PerformConversion("a", typeof(Char)));
		}

		[Test]
		public void PerformConversionBool()
		{
			Assert.AreEqual(true, converter.PerformConversion("true", typeof(bool)));
			Assert.AreEqual(false, converter.PerformConversion("false", typeof(bool)));
		}

#if SILVERLIGHT
		[Ignore("This does not work in tests under Silverlight")]
#endif

		[Test]
		public void PerformConversionType()
		{
			Assert.AreEqual(typeof(DefaultConversionManagerTestCase),
			                converter.PerformConversion(
			                	"CastleTests.DefaultConversionManagerTestCase, Castle.Windsor.Tests",
			                	typeof(Type)));
		}

		[Test]
		public void PerformConversionList()
		{
			var config = new MutableConfiguration("list");
			config.Attributes["type"] = "System.String";

			config.Children.Add(new MutableConfiguration("item", "first"));
			config.Children.Add(new MutableConfiguration("item", "second"));
			config.Children.Add(new MutableConfiguration("item", "third"));

			Assert.IsTrue(converter.CanHandleType(typeof(IList)));
#if (!SILVERLIGHT)
			Assert.IsTrue(converter.CanHandleType(typeof(ArrayList)));
#endif

			var list = (IList)converter.PerformConversion(config, typeof(IList));
			Assert.IsNotNull(list);
			Assert.AreEqual("first", list[0]);
			Assert.AreEqual("second", list[1]);
			Assert.AreEqual("third", list[2]);
		}

		[Test]
		public void Dictionary()
		{
			var config = new MutableConfiguration("dictionary");
			config.Attributes["keyType"] = "System.String";
			config.Attributes["valueType"] = "System.String";

			var firstItem = new MutableConfiguration("item", "first");
			firstItem.Attributes["key"] = "key1";
			config.Children.Add(firstItem);
			var secondItem = new MutableConfiguration("item", "second");
			secondItem.Attributes["key"] = "key2";
			config.Children.Add(secondItem);
			var thirdItem = new MutableConfiguration("item", "third");
			thirdItem.Attributes["key"] = "key3";
			config.Children.Add(thirdItem);

			var intItem = new MutableConfiguration("item", "40");
			intItem.Attributes["key"] = "4";
			intItem.Attributes["keyType"] = "System.Int32, mscorlib";
			intItem.Attributes["valueType"] = "System.Int32, mscorlib";

			config.Children.Add(intItem);

			var dateItem = new MutableConfiguration("item", "2005/12/1");
			dateItem.Attributes["key"] = "2000/1/1";
			dateItem.Attributes["keyType"] = "System.DateTime, mscorlib";
			dateItem.Attributes["valueType"] = "System.DateTime, mscorlib";

			config.Children.Add(dateItem);

			Assert.IsTrue(converter.CanHandleType(typeof(IDictionary)));
#if (!SILVERLIGHT)
			Assert.IsTrue(converter.CanHandleType(typeof(Hashtable)));
#endif

			var dict = (IDictionary)
			           converter.PerformConversion(config, typeof(IDictionary));

			Assert.IsNotNull(dict);

			Assert.AreEqual("first", dict["key1"]);
			Assert.AreEqual("second", dict["key2"]);
			Assert.AreEqual("third", dict["key3"]);
			Assert.AreEqual(40, dict[4]);
			Assert.AreEqual(new DateTime(2005, 12, 1), dict[new DateTime(2000, 1, 1)]);
		}

		[Test]
		public void DictionaryWithDifferentValueTypes()
		{
			var config = new MutableConfiguration("dictionary");

			config.CreateChild("entry")
				.Attribute("key", "intentry")
				.Attribute("valueType", "System.Int32, mscorlib")
				.Value = "123";

			config.CreateChild("entry")
				.Attribute("key", "values")
				.Attribute("valueType", "System.Int32[], mscorlib")
				.CreateChild("array")
				.Attribute("type", "System.Int32, mscorlib")
				.CreateChild("item", "400");

			var dict =
				(IDictionary)converter.PerformConversion(config, typeof(IDictionary));

			Assert.IsNotNull(dict);

			Assert.AreEqual(123, dict["intentry"]);
			var values = (int[])dict["values"];
			Assert.IsNotNull(values);
			Assert.AreEqual(1, values.Length);
			Assert.AreEqual(400, values[0]);
		}

		[Test]
		public void GenericPerformConversionList()
		{
			var config = new MutableConfiguration("list");
			config.Attributes["type"] = "System.Int64";

			config.Children.Add(new MutableConfiguration("item", "345"));
			config.Children.Add(new MutableConfiguration("item", "3147"));
			config.Children.Add(new MutableConfiguration("item", "997"));

			Assert.IsTrue(converter.CanHandleType(typeof(IList<double>)));
			Assert.IsTrue(converter.CanHandleType(typeof(List<string>)));

			var list = (IList<long>)converter.PerformConversion(config, typeof(IList<long>));
			Assert.IsNotNull(list);
			Assert.AreEqual(345L, list[0]);
			Assert.AreEqual(3147L, list[1]);
			Assert.AreEqual(997L, list[2]);
		}

		[Test]
		public void ListOfLongGuessingType()
		{
			var config = new MutableConfiguration("list");

			config.Children.Add(new MutableConfiguration("item", "345"));
			config.Children.Add(new MutableConfiguration("item", "3147"));
			config.Children.Add(new MutableConfiguration("item", "997"));

			Assert.IsTrue(converter.CanHandleType(typeof(IList<double>)));
			Assert.IsTrue(converter.CanHandleType(typeof(List<string>)));

			var list = (IList<long>)converter.PerformConversion(config, typeof(IList<long>));
			Assert.IsNotNull(list);
			Assert.AreEqual(345L, list[0]);
			Assert.AreEqual(3147L, list[1]);
			Assert.AreEqual(997L, list[2]);
		}

		[Test]
		public void GenericDictionary()
		{
			var config = new MutableConfiguration("dictionary");
			config.Attributes["keyType"] = "System.String";
			config.Attributes["valueType"] = "System.Int32";

			var firstItem = new MutableConfiguration("item", "1");
			firstItem.Attributes["key"] = "key1";
			config.Children.Add(firstItem);
			var secondItem = new MutableConfiguration("item", "2");
			secondItem.Attributes["key"] = "key2";
			config.Children.Add(secondItem);
			var thirdItem = new MutableConfiguration("item", "3");
			thirdItem.Attributes["key"] = "key3";
			config.Children.Add(thirdItem);

			Assert.IsTrue(converter.CanHandleType(typeof(IDictionary<string, string>)));
			Assert.IsTrue(converter.CanHandleType(typeof(Dictionary<string, int>)));

			var dict =
				(IDictionary<string, int>)converter.PerformConversion(config, typeof(IDictionary<string, int>));

			Assert.IsNotNull(dict);

			Assert.AreEqual(1, dict["key1"]);
			Assert.AreEqual(2, dict["key2"]);
			Assert.AreEqual(3, dict["key3"]);
		}

		[Test]
		public void Array()
		{
			var config = new MutableConfiguration("array");

			config.Children.Add(new MutableConfiguration("item", "first"));
			config.Children.Add(new MutableConfiguration("item", "second"));
			config.Children.Add(new MutableConfiguration("item", "third"));

			Assert.IsTrue(converter.CanHandleType(typeof(String[])));

			var array = (String[])
			            converter.PerformConversion(config, typeof(String[]));

			Assert.IsNotNull(array);

			Assert.AreEqual("first", array[0]);
			Assert.AreEqual("second", array[1]);
			Assert.AreEqual("third", array[2]);
		}

		[Test]
		public void PerformConversionTimeSpan()
		{
			Assert.AreEqual(TimeSpan.Zero, converter.PerformConversion("0", typeof(TimeSpan)));
			Assert.AreEqual(TimeSpan.FromDays(14), converter.PerformConversion("14", typeof(TimeSpan)));
			Assert.AreEqual(new TimeSpan(0, 1, 2, 3), converter.PerformConversion("1:2:3", typeof(TimeSpan)));
			Assert.AreEqual(new TimeSpan(0, 0, 0, 0, 250), converter.PerformConversion("0:0:0.250", typeof(TimeSpan)));
			Assert.AreEqual(new TimeSpan(10, 20, 30, 40, 500),
			                converter.PerformConversion("10.20:30:40.50", typeof(TimeSpan)));
		}
	}
}