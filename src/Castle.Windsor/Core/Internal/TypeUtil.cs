// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.Core.Internal
{
	using System;
	using System.Text;

	public static class TypeUtil
	{
		/// <summary>
		///   Checkis if given <paramref name="type" /> is a primitive type. Value types, <see cref="string" /> or collections of thereof are considered primitive and can not be registered as components in Windsor
		/// </summary>
		/// <param name="type"> </param>
		/// <returns> </returns>
		public static bool IsPrimitiveType(this Type type)
		{
			if (type == null || type.IsValueType || type == typeof (string))
			{
				return true;
			}

			var itemType = type.GetCompatibleArrayItemType();
			return itemType != null && itemType.IsPrimitiveType();
		}


		public static string ToCSharpString(this Type type)
		{
			try
			{
				var name = new StringBuilder();
				ToCSharpString(type, name);
				return name.ToString();
			}
			catch (Exception)
			{
				// in case we messed up something...
				return type.Name;
			}
		}

		private static void AppendGenericParameters(StringBuilder name, Type[] genericArguments)
		{
			name.Append("<");

			for (var i = 0; i < genericArguments.Length - 1; i++)
			{
				ToCSharpString(genericArguments[i], name);
				name.Append(", ");
			}
			if (genericArguments.Length > 0)
			{
				ToCSharpString(genericArguments[genericArguments.Length - 1], name);
			}
			name.Append(">");
		}

		private static void ToCSharpString(Type type, StringBuilder name)
		{
			if (type.IsArray)
			{
				var elementType = type.GetElementType();
				ToCSharpString(elementType, name);
				name.Append(type.Name.Substring(elementType.Name.Length));
				return;
			}
			if (type.IsGenericParameter)
			{
				//NOTE: this has to go before type.IsNested because nested generic type is also a generic parameter and otherwise we'd have stack overflow
				name.AppendFormat("·{0}·", type.Name);
				return;
			}
			if (type.IsNested)
			{
				ToCSharpString(type.DeclaringType, name);
				name.Append(".");
			}
			if (type.IsGenericType == false)
			{
				name.Append(type.Name);
				return;
			}
			name.Append(type.Name.Split('`')[0]);
			AppendGenericParameters(name, type.GetGenericArguments());
		}
	}
}