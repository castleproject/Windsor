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

	public class TypedFactoryComponent
	{
		private readonly Type componentType;
		private readonly string componentName;
		private readonly Dictionary<string, object> additionalArguments;

		public TypedFactoryComponent(string componentName, Type componentType, Dictionary<string, object> additionalArguments)
		{
			this.componentType = componentType;
			this.componentName = componentName;
			this.additionalArguments = additionalArguments ?? new Dictionary<string, object>();
		}

		public Type ComponentType
		{
			get { return componentType; }
		}

		public string ComponentName
		{
			get { return componentName; }
		}

		public Dictionary<string, object> AdditionalArguments
		{
			get { return additionalArguments; }
		}
	}
}