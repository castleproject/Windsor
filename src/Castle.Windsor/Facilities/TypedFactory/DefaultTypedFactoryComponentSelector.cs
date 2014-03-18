﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Collections;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel;

	[Singleton]
	public class DefaultTypedFactoryComponentSelector : ITypedFactoryComponentSelector
	{
		/// <param name = "getMethodsResolveByName">If set to <c>true</c>, all methods with names like 'GetSomething' will try to resolve by name component 'something'. Defaults to <c>true</c>.</param>
		/// <param name = "fallbackToResolveByTypeIfNameNotFound">If set to <c>true</c>, will fallback to resolving by type, if can not find component with specified name. This property is here for backward compatibility. It is recommended not to use it. Defaults to <c>false</c>.</param>
		public DefaultTypedFactoryComponentSelector(bool getMethodsResolveByName = true,
		                                            bool fallbackToResolveByTypeIfNameNotFound = false)
		{
			FallbackToResolveByTypeIfNameNotFound = fallbackToResolveByTypeIfNameNotFound;
			GetMethodsResolveByName = getMethodsResolveByName;
		}

		protected DefaultTypedFactoryComponentSelector() : this(true, false)
		{
		}

		/// <summary>
		///   If set to <c>true</c>, will fallback to resolving by type, if can not find component with specified name. This property is here for backward compatibility. It is recommended not to use it.
		/// </summary>
		protected bool FallbackToResolveByTypeIfNameNotFound { get; set; }

		/// <summary>
		///   If set to <c>true</c>, all methods with names like 'GetSomething' will try to resolve by name component 'something'.
		/// </summary>
		protected bool GetMethodsResolveByName { get; set; }

		public Func<IKernelInternal, IReleasePolicy, object> SelectComponent(MethodInfo method, Type type, object[] arguments)
		{
			var componentName = GetComponentName(method, arguments);
			var componentType = GetComponentType(method, arguments);
			var additionalArguments = GetArguments(method, arguments);

			return BuildFactoryComponent(method, componentName, componentType, additionalArguments);
		}

		/// <summary>
		///   Builds <see cref = "TypedFactoryComponentResolver" /> for given call.
		///   By default if <paramref name = "componentType" /> is a collection
		///   returns factory calling <see cref = "IKernel.ResolveAll(System.Type)" /> on collection's item type,
		///   otherwise standard <see cref = "TypedFactoryComponentResolver" />.
		/// </summary>
		/// <param name = "method"></param>
		/// <param name = "componentName"></param>
		/// <param name = "componentType"></param>
		/// <param name = "additionalArguments"></param>
		/// <returns></returns>
		protected virtual Func<IKernelInternal, IReleasePolicy, object> BuildFactoryComponent(MethodInfo method,
		                                                                                      string componentName,
		                                                                                      Type componentType,
		                                                                                      IDictionary additionalArguments)
		{
			var itemType = componentType.GetCompatibleArrayItemType();
			if (itemType == null)
			{
				return new TypedFactoryComponentResolver(componentName,
				                                         componentType,
				                                         additionalArguments,
				                                         FallbackToResolveByTypeIfNameNotFound, GetType()).Resolve;
			}
			return (k, s) => k.ResolveAll(itemType, additionalArguments, s);
		}

		/// <summary>
		///   Selects arguments to be passed to resolution pipeline.
		///   By default passes all given <paramref name = "arguments" /> 
		///   keyed by names of their corresponding <paramref name = "method" /> parameters.
		/// </summary>
		/// <param name = "method"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		protected virtual IDictionary GetArguments(MethodInfo method, object[] arguments)
		{
			if (arguments == null)
			{
				return null;
			}
			var argumentMap = new Arguments();
			var parameters = method.GetParameters();
			for (var i = 0; i < parameters.Length; i++)
			{
				argumentMap.Add(parameters[i].Name, arguments[i]);
			}
			return argumentMap;
		}

		/// <summary>
		///   Selects name of the component to resolve.
		///   If <paramref name = "method" /> Name is GetFoo returns "Foo", otherwise <c>null</c>.
		/// </summary>
		/// <param name = "method"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		protected virtual string GetComponentName(MethodInfo method, object[] arguments)
		{
			string componentName = null;
			if (GetMethodsResolveByName && method.Name.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
			{
				componentName = method.Name.Substring("Get".Length);
			}
			return componentName;
		}

		/// <summary>
		///   Selects type of the component to resolve. Uses <paramref name = "method" /> return type.
		/// </summary>
		/// <param name = "method"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		protected virtual Type GetComponentType(MethodInfo method, object[] arguments)
		{
			return method.ReturnType;
		}
	}
}