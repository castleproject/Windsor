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

	using Castle.MicroKernel;

	public class DefaultDelegateComponentSelector : DefaultTypedFactoryComponentSelector
	{
		protected override IDictionary GetArguments(MethodInfo method, object[] arguments)
		{
			var parameters = method.GetParameters();
			var arg = new Arguments();
			for (var i = 0; i < parameters.Length; i++)
			{
				if (arg.Contains(parameters[i].ParameterType))
				{
					if (IsFunc(method.DeclaringType))
					{
						throw new ArgumentException(
							string.Format("Factory delegate {0} has duplicated arguments of type {1}. " +
							              "Using generic purpose delegates with duplicated argument types is unsupported, because then it is not possible to match arguments properly. " +
							              "Use some custom delegate with meaningful argument names or interface based factory instead.",
							              method.DeclaringType, parameters[i].ParameterType));
					}

					// else we just ignore it. It will likely be matched by name so we don't want to throw prematurely. We could log this though.
				}
				else
				{
					arg.Add(parameters[i].ParameterType, arguments[i]);
				}
				arg.Add(parameters[i].Name, arguments[i]);
			}
			return arg;
		}

		protected override string GetComponentName(MethodInfo method, object[] arguments)
		{
			return null;
		}

		private bool IsFunc(Type type)
		{
			return type.FullName != null && type.FullName.StartsWith("System.Func");
		}
	}
}