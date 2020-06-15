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
	using System.Reflection;
	using System.Text;

	public static class TypeUtil
	{
		/// <summary>
		/// Indicates whether the type is explicitly marked as nullable. Currently only detects nullable value types via
		/// <see cref="Nullable{T}"/>, but could be expanded to detect C# nullable reference types.
		/// </summary>
		internal static bool IsNullable(this Type type)
		{
			// For now, only detects nullable value types. Detecting nullable reference types is complicated.
			// Spec: https://github.com/dotnet/roslyn/blob/version-3.2.0/docs/features/nullable-metadata.md
			// Reflection API proposal: https://github.com/dotnet/runtime/issues/29723

			return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		/// <summary>
		///   Checks if given <paramref name="type" /> is a primitive type or collection of primitive types. Value types, <see cref="string" /> are considered primitive and can not be registered as components in Windsor
		/// </summary>
		/// <param name="type"> </param>
		/// <returns> </returns>
		public static bool IsPrimitiveTypeOrCollection(this Type type)
		{
			if (type.IsPrimitiveType())
			{
				return true;
			}

			var itemType = type.GetCompatibleArrayItemType();
			return itemType != null && itemType.IsPrimitiveTypeOrCollection();
		}

		/// <summary>
		///   Checkis if given <paramref name="type" /> is a primitive type. Value types and <see cref="string" /> are considered primitive and can not be registered as components in Windsor
		/// </summary>
		/// <param name="type"> </param>
		/// <returns> </returns>
		public static bool IsPrimitiveType(this Type type)
		{
			return type == null || type.GetTypeInfo().IsValueType || type == typeof(string);
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
#if FEATURE_GAC
				var hasAssembliesFromGac = openGeneric.GetTypeInfo().Assembly.GlobalAssemblyCache;
#endif
				message.AppendLine("This was unexpected! Looks like you hit a really weird bug in .NET (yes, it's really not Windsor's fault).");
				message.AppendLine("We were just about to make a generic version of " + openGeneric.AssemblyQualifiedName + " with the following generic arguments:");
				foreach (var argument in arguments)
				{
					message.AppendLine("\t" + argument.AssemblyQualifiedName);
#if FEATURE_GAC
					if (hasAssembliesFromGac == false)
					{
						hasAssembliesFromGac = argument.GetTypeInfo().Assembly.GlobalAssemblyCache;
					}
#endif
				}
				if (Debugger.IsAttached)
				{
					message.AppendLine("It look like your debugger is attached. Try running the code without the debugger. It's likely it will work correctly.");
				}
				message.AppendLine("If you're running the code inside your IDE try rebuilding your code (Clean, then Build) and make sure you don't have conflicting versions of referenced assemblies.");
#if FEATURE_GAC
				if (hasAssembliesFromGac)
				{
					message.AppendLine("Notice that some assemblies involved were coming from GAC.");
				}
#endif
				message.AppendLine("If you tried all of the above and the issue still persists try asking on StackOverflow or castle users group.");
				throw new ArgumentException(message.ToString(), e);
			}
		}

		private static void AppendGenericParameters(StringBuilder name, Type[] genericArguments, int skip, int take)
		{
			if (take == 0) return;

			name.Append("<");

			for (var i = skip; i < skip + take; i++)
			{
				if (i > skip)
				{
					name.Append(", ");
				}
				ToCSharpString(genericArguments[i], name);
			}

			name.Append(">");
		}

		private static void ToCSharpString(Type type, StringBuilder name, Type startType = null)
		{
			var inheritedGenericArgs = 0;

			if (startType == null)
			{
				startType = type;
			}

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
				name.Append(type.Name);
				return;
			}
			if (type.IsNested)
			{
				var declaringType = type.DeclaringType;

				if (declaringType.GetTypeInfo().IsGenericType)
				{
					inheritedGenericArgs = declaringType.GetGenericArguments().Length;
				}

				ToCSharpString(declaringType, name, startType);
				//                                  ^^^^^^^^^
				// This forwards the innermost type to the formatting of the enclosing type
				// so that if it is generic, the type arguments can still be retrieved.
				// (type.DeclaringType returns a generic type definition and thus loses the args.)
				name.Append(".");
			}
			if (type.GetTypeInfo().IsGenericType == false)
			{
				name.Append(type.Name);
				return;
			}
			name.Append(type.Name.Split('`')[0]);

			// Given a (CLS-compliant) nested type Outer<A>.Inner<B>, Inner really has two generic parameters;
			// it inherits all those of the enclosing type. So for formatting purposes, we need to figure out
			// how many *additional* type parameters Inner declares itself, then only append those:
			var ownGenericArgs = type.GetGenericArguments().Length - inheritedGenericArgs;
			AppendGenericParameters(name, startType.GetGenericArguments(), skip: inheritedGenericArgs, take: ownGenericArgs);
		}
	}
}