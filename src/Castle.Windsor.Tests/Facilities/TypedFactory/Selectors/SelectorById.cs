﻿// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests.Facilities.TypedFactory.Selectors
{
	using System.Collections;
	using System.Reflection;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;

	public class SelectorById : DefaultTypedFactoryComponentSelector
	{
		protected override Arguments GetArguments(MethodInfo method, object[] arguments)
		{
			if (method.Name.Equals("ComponentNamed"))
			{
				//empty since we don't have any actual parameters
				return new Arguments();
			}

			return base.GetArguments(method, arguments);
		}

		protected override string GetComponentName(MethodInfo method, object[] arguments)
		{
			if (method.Name.Equals("ComponentNamed"))
			{
				return (string)arguments[0];
			}

			return base.GetComponentName(method, arguments);
		}
	}
}