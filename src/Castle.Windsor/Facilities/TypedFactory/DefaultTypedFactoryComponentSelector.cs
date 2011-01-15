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
		public ITypedFactoryComponentResolver SelectComponent(MethodInfo method, Type type, object[] arguments)
		{
			var componentName = GetComponentName(method, arguments);
			var componentType = GetComponentType(method, arguments);
			var additionalArguments = GetArguments(method, arguments);

			return BuildFactoryComponent(method, componentName, componentType, additionalArguments);
		}

		/// <summary>
		///   Builds <see cref = "TypedFactoryComponentResolver" /> for given call.
		///   By default if <paramref name = "componentType" /> is a collection
		///   returns <see cref = "TypedFactoryCollectionResolver" /> for the collection's item type,
		///   otherwise standard <see cref = "TypedFactoryComponentResolver" />.
		/// </summary>
		/// <param name = "method"></param>
		/// <param name = "componentName"></param>
		/// <param name = "componentType"></param>
		/// <param name = "additionalArguments"></param>
		/// <returns></returns>
		protected virtual ITypedFactoryComponentResolver BuildFactoryComponent(MethodInfo method, string componentName, Type componentType,
		                                                                       IDictionary additionalArguments)
		{
			var itemType = componentType.GetCompatibleArrayItemType();
			if (itemType == null)
			{
				return new TypedFactoryComponentResolver(componentName, componentType, additionalArguments);
			}
			return new TypedFactoryCollectionResolver(itemType, additionalArguments);
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
			if (method.Name.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
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