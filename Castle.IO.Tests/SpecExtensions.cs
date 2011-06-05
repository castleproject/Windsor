#region License
/* Authors:
 *      Sebastien Lambla (seb@serialseb.com)
 * Copyright:
 *      (C) 2007-2009 Caffeine IT & naughtyProd Ltd (http://www.caffeine-it.com)
 * License:
 *      This file is distributed under the terms of the MIT License found at the end of this file.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Castle.IO.Tests
{
    public static class SpecExtensions
    {
        public static bool IsEqualTo(this Collection<string> actualValue, Collection<string> expectedValue)
        {
            if ((actualValue == null) != (expectedValue == null))
                return false;
            if (actualValue == null && expectedValue == null)
                return true;
            if (actualValue.Count != expectedValue.Count)
                return false;
            for (int i = 0; i < actualValue.Count; i++)
                if (actualValue[i] != expectedValue[i])
                    return false;
            return true;
        }

        public static IEnumerable<T> ShouldAllBe<T>(this IEnumerable<T> values, T expected)
        {
            foreach (var value in values)
            {
                Assert.That(value, Is.EqualTo(expected));
            }
            return values;
        }
        public static T ShouldBe<T>(this T valueToAnalyse, T expectedValue)
        {
            Assert.That(valueToAnalyse, Is.EqualTo(expectedValue));
            return valueToAnalyse;
        }

        public static Type ShouldBe<T>(this Type valueToAnalyse)
        {
            Assert.That(valueToAnalyse, Is.EqualTo(typeof(T)));
            return valueToAnalyse;
        }

        public static Uri ShouldBe(this Uri actual, string expected)
        {
            var uri = new Uri(expected);
            Assert.That(actual, Is.EqualTo(uri));
            return uri;
        }

        public static KeyValuePair<TKey,TValue> ShouldBe<TKey,TValue>(this KeyValuePair<TKey,TValue> actual, TKey expectedName, TValue expectedValue)
        {
            actual.Key.ShouldBe(expectedName);
            actual.Value.ShouldBe(expectedValue);
            return actual;
        }
        public static void ShouldBe(this Stream actualStream, Stream expectedStream)
        {
            int actRead = 0, expRead = 0;
            while ((actRead = actualStream.ReadByte()) != -1 & (expRead = expectedStream.ReadByte()) != -1)
            {
                actRead.ShouldBe(expRead);
            }
            if (actRead != expRead)
                Assert.Fail("Streams have a different length.");
        }

        public static TExpected ShouldBeAssignableTo<TExpected>(this object obj)
        {
            if (!typeof(TExpected).IsAssignableFrom(obj.GetType()))
                Assert.Fail("TargetType {0} is not implementing {1}.", obj.GetType().Name, typeof(TExpected).Name);
            return (TExpected)obj;
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> values)
        {
            values.ShouldHaveCountOf(0);
        }

        /// <summary>
        /// Tests that two objects have the same public properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        public static bool ShouldBeEquivalentTo<T>(this T actual, T expected)
        {
            foreach (var property in actual.GetType().GetProperties())
            {
                property.GetValue(actual, null).ShouldBe(property.GetValue(expected, null));
            }
            return true;
        }

        public static bool ShouldBeFalse(this bool value)
        {
            Assert.That(value, Is.False);
            return false;
        }

        public static void ShouldBeGreaterThan<T>(this T actual, T expected) where T : IComparable<T>
        {
            Assert.That(actual, Is.GreaterThan(expected));
        }

        public static void ShouldBeLessThan<T>(this T actual, T expected) where T : IComparable<T>
        {
            Assert.That(actual, Is.LessThan(expected));
        }


        public static void ShouldBeNull<T>(this T obj)
        {
            Assert.IsNull(obj);
        }

        public static TExpected ShouldBeOfType<TExpected>(this object obj)
        {
            Assert.That(obj, Is.InstanceOf<TExpected>());
            return obj == null ? default(TExpected) : (TExpected)obj;
        }
        public static T Check<T>(this T obj, Action<T> assertion)
        {
            assertion(obj);
            return obj;
        }
        public static void ShouldBeTheSameInstanceAs(this object actual, object expected)
        {
            Assert.That(actual, Is.SameAs(expected));
        }

        public static void ShouldBeTrue(this bool value)
        {
            Assert.That(value, Is.True);
        }

        public static void ShouldCompleteSuccessfully(this Action codeToExecute)
        {
            codeToExecute();
        }

        public static IDictionary<T, U> ShouldContain<T, U>(this IDictionary<T, U> dic, T key, U value)
        {
            dic[key].ShouldBe(value);
            return dic;
        }

        public static IEnumerable<T> ShouldContain<T>(this IEnumerable<T> list, T expected)
        {
            ShouldContain(list, expected, (t, t2) => t.Equals(t2));
            return list;
        }

        public static IEnumerable<T> ShouldContain<T>(this IEnumerable<T> list, T expected, Func<T, T, bool> match)
        {
            foreach (var t in list)
                if (match(t, expected))
                    return list;

            Assert.Fail("Looking for element {0} but didn't find any.", expected);
            return null;
        }

        public static string ShouldContain(this string baseString, string textToFind)
        {
            if (baseString.IndexOf(textToFind) == -1)
                Assert.Fail("text '{0}' not found in '{1}'", textToFind, baseString);
            return baseString;
        }

        public static IEnumerable<T> ShouldHaveCountOf<T>(this IEnumerable<T> values, int count)
        {
            values.ShouldNotBeNull().Count().ShouldBe(count);
            return values;
        }

        public static bool ShouldHaveSameElementsAs(this NameValueCollection expectedResult, 
                                                    NameValueCollection actualResult)
        {
            Assert.AreEqual(expectedResult.Count, actualResult.Count);
            foreach (string key in expectedResult.Keys)
            {
                Assert.AreEqual(expectedResult[key], actualResult[key]);
                if (expectedResult[key] != actualResult[key])
                    return false;
            }
            return true;
        }

        public static IEnumerable<T> ShouldHaveSameElementsAs<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
        {
            var enumerator1 = actual.GetEnumerator();
            var enumerator2 = expected.GetEnumerator();
            bool moveNext1 = false, moveNext2 = false;
            while (((moveNext1 = enumerator1.MoveNext()) & (moveNext2 = enumerator2.MoveNext()))
                   && moveNext1 == moveNext2)
                Assert.AreEqual(enumerator1.Current, enumerator2.Current);
            if (moveNext1 != moveNext2)
                Assert.Fail("The two enumerables didn't have the same number of elements.");
            enumerator1.Dispose();
            enumerator2.Dispose();
            return actual;
        }

        public static void ShouldHaveSameElementsAs<TKey, TValue>(this IDictionary<TKey, TValue> actual, 
                                                                  IDictionary<TKey, TValue> expected)
        {
            actual.Count.ShouldBe(expected.Count);
            foreach (var key in actual.Keys)
                expected[key].ShouldBe(actual[key]);
        }

        public static void ShouldHaveSameElementsAs<T, V>(this IEnumerable<T> r1, 
                                                          IEnumerable<V> r2, 
                                                          Func<T, V, bool> comparer)
        {
            using (var enumerator1 = r1.GetEnumerator())
            using (var enumerator2 = r2.GetEnumerator())
            {
                while (true)
                {
                    bool enum1HasMoved = enumerator1.MoveNext();
                    bool enum2HasMoved = enumerator2.MoveNext();
                    if (!enum1HasMoved && !enum2HasMoved)
                        return;
                    if (enum1HasMoved != enum2HasMoved)
                        return;
                    Assert.IsTrue(comparer(enumerator1.Current, enumerator2.Current), 
                                  "Two elements didnt match:\na:\n{0}\nb:\n{1}", 
                                  enumerator1.Current.ToString(), 
                                  enumerator2.Current.ToString());
                }
            }
        }

        public static T ShouldNotBe<T>(this T valueToAnalyse, T expectedValue)
        {
            Assert.That(valueToAnalyse, Is.Not.EqualTo(expectedValue));
            return valueToAnalyse;
        }

        public static T ShouldNotBeNull<T>(this T obj) where T : class
        {
            Assert.IsNotNull(obj);
            return obj;
        }

        public static T ShouldNotBeNull<T>(this T? obj) where T : struct
        {
            Assert.True(obj.HasValue);
            return obj.Value;
        }

        public static void ShouldNotBeTheSameInstanceAs(this object actual, object expected)
        {
            Assert.That(actual, Is.Not.SameAs(expected));
        }

        public static void ShouldNotContain(this string baseString, string textToFind)
        {
            if (baseString.IndexOf(textToFind) != -1)
                Assert.Fail("text '{0}' found in '{1}'", textToFind, baseString);
        }

        public static void ShouldReturn<T>(this Func<T> codeToExecute, T expectedValue)
        {
            codeToExecute().ShouldBe(expectedValue);
        }

        public static T ShouldThrow<T>(this Action codeToExecute) where T : Exception
        {
            try
            {
                codeToExecute();
            }
            catch (Exception e)
            {
                if (!typeof(T).IsInstanceOfType(e))
                    Assert.Fail("Expected exception of type \"{0}\" but got \"{1}\" instead.", 
                                typeof(T).Name, 
                                e.GetType().Name);
                else
                    return (T)e;
            }

            Assert.Fail("Expected an exception of type \"{0}\" but none were thrown.", typeof(T).Name);
            return null; // this never happens as Fail will throw...
        }
        public static IEnumerable<T> ShouldHaveAtLeastOne<T>(this IEnumerable<T> enumerable, Func<T,bool> predicate) where T:class
        {
            enumerable.Any(predicate).ShouldBeTrue();
            return enumerable;
        }
        public static IEnumerable<T> ShouldHaveAll<T>(this IEnumerable<T> enumerable, Func<T,bool> predicate) where T:class
        {
            enumerable.All(predicate).ShouldBeTrue();
            return enumerable;
        }
    }
}

#region Full license
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion