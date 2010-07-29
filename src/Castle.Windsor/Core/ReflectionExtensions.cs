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

namespace Castle.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Castle.DynamicProxy.Generators.Emitters;

	public static class ReflectionExtensions
	{
		public static TAttribute[] GetAttributes<TAttribute>(this MemberInfo item) where TAttribute : Attribute
		{
			return (TAttribute[])Attribute.GetCustomAttributes(item, typeof(TAttribute), true);
		}

		public static bool HasDefaultValue(this ParameterInfo item)
		{
			return (item.Attributes & ParameterAttributes.HasDefault) != 0;
		}
		
		public static bool Is<TType>(this Type type)
		{
			return typeof(TType).IsAssignableFrom(type);
		}

		/// <summary>
		/// If the extended type is a Foo[] or IEnumerable{Foo} which is assignable from Foo[] this method will return typeof(Foo)
		/// otherwise <c>null</c>.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type GetCompatibileArrayItemType(this Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			if (!type.IsGenericType)
			{
				return null;
			}
			var enumerable = GetEnumerableType(type);
			if (enumerable != null)
			{
				var itemType = enumerable.GetGenericArguments().Single();
				var array = itemType.MakeArrayType();
				if (type.IsAssignableFrom(array))
				{
					return itemType;
				}
			}

			return null;
		}

		private static Type GetEnumerableType(Type type)
		{
			return type.GetAllInterfaces()
				.Where(@interface => @interface.IsGenericType)
				.SingleOrDefault(@interface => @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		}
	}
}