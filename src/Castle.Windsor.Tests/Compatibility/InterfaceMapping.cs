using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

#if FEATURE_NETCORE_REFLECTION_API
namespace System.Reflection
{
	internal static class InterfaceMappingReflectionExtensions
	{

		public static InterfaceMapping GetInterfaceMap(this Type implType, Type interfaceType)
		{
			InterfaceMapping map;
			map.TargetType = implType;
			map.InterfaceType = interfaceType;

			map.TargetMethods = implType.GetMethods().ToArray();
			map.InterfaceMethods = new MethodInfo[map.TargetMethods.Length];
			for(int i=0; i< map.TargetMethods.Length; i++)
			{
				map.InterfaceMethods[i] = GetCorrespondingMethodFromType(interfaceType, map.TargetMethods[i]);
			}
			return map;
		}

		// Given a MethodInfo which could be from any type, return a MethodInfo declared by the given type 
		// that matches in name and parameter types.  Null is returned if there is no match.
		//From https://github.com/dotnet/wcf/blob/master/src/System.Private.ServiceModel/src/System/ServiceModel/Description/TypeLoader.cs
		private static MethodInfo GetCorrespondingMethodFromType(Type type, MethodInfo methodInfo)
		{
			if (methodInfo.DeclaringType == type)
			{
				return methodInfo;
			}

			MethodInfo matchingMethod = type.GetTypeInfo().DeclaredMethods.SingleOrDefault(m => MethodsMatch(m, methodInfo));
			return matchingMethod;
		}

		// Returns true if the given methods match in name and parameter types
		private static bool MethodsMatch(MethodInfo method1, MethodInfo method2)
		{
			if (method1.Equals(method2))
			{
				return true;
			}

			if (method1.ReturnType != method2.ReturnType ||
				!String.Equals(method1.Name, method2.Name, StringComparison.Ordinal) ||
				!ParameterInfosMatch(method1.ReturnParameter, method2.ReturnParameter))
			{
				return false;
			}

			ParameterInfo[] parameters1 = method1.GetParameters();
			ParameterInfo[] parameters2 = method2.GetParameters();
			if (parameters1.Length != parameters2.Length)
			{
				return false;
			}

			for (int i = 0; i < parameters1.Length; ++i)
			{
				if (!ParameterInfosMatch(parameters1[i], parameters2[i]))
				{
					return false;
				}
			}

			return true;
		}

		// Returns true if 2 ParameterInfo's match in signature with respect
		// to the MemberInfo's in which they are declared. Position is required
		// to match but name is not.
		private static bool ParameterInfosMatch(ParameterInfo parameterInfo1, ParameterInfo parameterInfo2)
		{
			// Null is possible for a ParameterInfo from MethodInfo.ReturnParameter.
			// If both are null, we have no information to compare and say they are equal.
			if (parameterInfo1 == null && parameterInfo2 == null)
			{
				return true;
			}

			if (parameterInfo1 == null || parameterInfo2 == null)
			{
				return false;
			}

			if (parameterInfo1.Equals(parameterInfo2))
			{
				return true;
			}

			return ((parameterInfo1.ParameterType == parameterInfo2.ParameterType) &&
					(parameterInfo1.IsIn == parameterInfo2.IsIn) &&
					(parameterInfo1.IsOut == parameterInfo2.IsOut) &&
					(parameterInfo1.IsRetval == parameterInfo2.IsRetval) &&
					(parameterInfo1.Position == parameterInfo2.Position));
		}

	}
}
#endif
