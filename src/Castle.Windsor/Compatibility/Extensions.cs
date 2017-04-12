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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class EnumerableExtensions
{
	public static Y[] ConvertAll<X, Y>(this X[] items, Func<X,Y> converter)
	{
		var results = new List<Y>();

		foreach (var item in items)
		{
			var y = converter(item);

			results.Add(y);
		}

		return results.ToArray();
	}

	public static void ForEach<X>(this IEnumerable<X> items, Action<X> action)
	{
		foreach (var item in items)
			action(item);
	}
}

internal static class CustomAttributeExtensions
{
	public static IEnumerable<T> GetCustomAttributes<T>(this Assembly element) where T : Attribute
	{
		foreach (T a in Attribute.GetCustomAttributes(element, typeof(T)))
		{
			yield return a;
		}
	}

	public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element, bool inherit) where T : Attribute
	{
		foreach (T a in Attribute.GetCustomAttributes(element, typeof(T), inherit))
		{
			yield return a;
		}
	}

	public static bool HasAttribute(this MethodBase element, Type attributeType)
	{
		return Attribute.IsDefined(element, attributeType);
	}

	public static bool HasAttribute<X>(this MethodBase element)
	{
		return Attribute.IsDefined(element, typeof(X));
	}

	public static bool HasAttribute(this PropertyInfo element, Type attributeType)
	{
		return Attribute.IsDefined(element, attributeType);
	}

	public static bool HasAttribute<X>(this PropertyInfo element)
	{
		return Attribute.IsDefined(element, typeof(X));
	}
}