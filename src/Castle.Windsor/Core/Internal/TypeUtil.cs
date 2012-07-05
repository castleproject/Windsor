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
	using System.Diagnostics;
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
			if (type == null || type.IsValueType || type == typeof(string))
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

		/// <summary>
		///   Calls <see cref="Type.MakeGenericType" /> and if a generic constraint is violated returns <c>null</c> instead of throwing <see
		///    cref="ArgumentException" />.
		/// </summary>
		/// <param name="openGeneric"> </param>
		/// <param name="arguments"> </param>
		/// <returns> </returns>
		[DebuggerHidden]
		public static Type TryMakeGenericType(this Type openGeneric, Type[] arguments)
		{
			try
			{
				return openGeneric.MakeGenericType(arguments);
			}
			catch (ArgumentException)
			{
				// Any element of typeArguments does not satisfy the constraints specified for the corresponding type parameter of the current generic type.
				// NOTE: We try and catch because there's no public API to reliably, and robustly test for that upfront
				// there's RuntimeTypeHandle.SatisfiesConstraints method but it's internal. 
				return null;
			}
			catch (TypeLoadException e)
			{
				//Yeah, this exception is undocumented, yet it does get thrown in some cases (I was unable to reproduce it reliably)
				var message = new StringBuilder();
				var hasAssembliesFromGac = openGeneric.Assembly.GlobalAssemblyCache;

				message.AppendLine("This was unexpected! Looks like you hit a really weird bug in .NET (yes, it's really not Windsor's fault).");
				message.AppendLine("We were just about to make a generic version of " + openGeneric.AssemblyQualifiedName + " with the following generic arguments:");
				foreach (var argument in arguments)
				{
					message.AppendLine("\t" + argument.AssemblyQualifiedName);
					if (hasAssembliesFromGac == false)
					{
						hasAssembliesFromGac = argument.Assembly.GlobalAssemblyCache;
					}
				}
				if (Debugger.IsAttached)
				{
					message.AppendLine("It look like your debugger is attached. Try running the code without the debugger. It's likely it will work correctly.");
				}
				message.AppendLine("If you're running the code inside your IDE try rebuilding your code (Clean, then Build) and make sure you don't have conflicting versions of referenced assemblies.");
				if (hasAssembliesFromGac)
				{
					message.AppendLine("Notice that some assemblies involved were coming from GAC.");
				}
				message.AppendLine("If you tried all of the above and the issue still persists try asking on StackOverflow or castle users group.");
				throw new ArgumentException(message.ToString(), e);
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