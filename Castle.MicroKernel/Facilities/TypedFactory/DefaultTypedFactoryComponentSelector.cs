// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Facilities.TypedFactory
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	public class DefaultTypedFactoryComponentSelector : ITypedFactoryComponentSelector
	{
		public TypedFactoryComponent SelectComponent(MethodInfo method, Type type, object[] arguments)
		{
			var componentName = GetComponentName(method);
			var componentType = GetComponentType(method);
			var additionalArguments = GetArguments(method, arguments);
			return new TypedFactoryComponent(componentName, componentType, additionalArguments);
		}

		private Type GetComponentType(MethodInfo method)
		{
			return method.ReturnType;
		}

		private string GetComponentName(MethodInfo method)
		{
			string componentName = null;
			if (method.Name.StartsWith("Get"))
			{
				componentName = method.Name.Substring("get".Length);
			}
			return componentName;
		}

		private Dictionary<string, object> GetArguments(MethodInfo method, object[] arguments)
		{
			var argumentMap = new Dictionary<string, object>();
			var parameters = method.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				argumentMap.Add(parameters[i].Name, arguments[i]);
			}
			return argumentMap;
		}
	}
}