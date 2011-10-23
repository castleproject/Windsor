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

namespace CastleTests.Facilities.TypedFactory.Selectors
{
	using System;
	using System.Collections;
	using System.Reflection;

	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;
	using Castle.Windsor.Tests.Facilities.TypedFactory.Components;

	public class SelectorByClosedArgumentType : DefaultTypedFactoryComponentSelector
	{
		public SelectorByClosedArgumentType()
		{
			FallbackToResolveByTypeIfNameNotFound = true;
		}

		protected override IDictionary GetArguments(MethodInfo method, object[] arguments)
		{
			//a condition checking if it's actually a method we want to be in should go here
			return new Arguments(arguments);
		}

		protected override Type GetComponentType(MethodInfo method, object[] arguments)
		{
			//a condition checking if it's actually a method we want to be in should go here
			return typeof(GenericComponent<>).MakeGenericType(arguments[0].GetType());
		}
	}
}