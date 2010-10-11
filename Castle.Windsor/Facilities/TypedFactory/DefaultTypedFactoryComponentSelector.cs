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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Collections;
	using System.Reflection;

	using Castle.Core.Internal;
	using Castle.MicroKernel;

	public class DefaultTypedFactoryComponentSelector : ITypedFactoryComponentSelector
	{
		public TypedFactoryComponent SelectComponent(MethodInfo method, Type type, object[] arguments)
		{
			var componentName = GetComponentName(method, arguments);
			var componentType = GetComponentType(method, arguments);
			var additionalArguments = GetArguments(method, arguments);

			return BuildFactoryComponent(method, componentName, componentType, additionalArguments);
		}

		protected virtual TypedFactoryComponent BuildFactoryComponent(MethodInfo method, string componentName,
		                                                              Type componentType, IDictionary additionalArguments)
		{
			var itemType = componentType.GetCompatibleArrayItemType();
			if (itemType != null)
			{
				return new TypedFactoryComponentCollection(itemType, additionalArguments);
			}
			return new TypedFactoryComponent(componentName, componentType, additionalArguments);
		}

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

		protected virtual string GetComponentName(MethodInfo method, object[] arguments)
		{
			string componentName = null;
			if (method.Name.StartsWith("Get"))
			{
				componentName = method.Name.Substring("Get".Length);
			}
			return componentName;
		}

		protected virtual Type GetComponentType(MethodInfo method, object[] arguments)
		{
			return method.ReturnType;
		}
	}
}