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

	using Castle.MicroKernel;

	/// <summary>
	/// Represents a single component to be resolved via Typed Factory
	/// </summary>
	public class TypedFactoryComponent
	{
		private readonly Type componentType;
		private readonly string componentName;
		private readonly IDictionary additionalArguments;


		public TypedFactoryComponent(string componentName, Type componentType, IDictionary additionalArguments)
		{
			if (string.IsNullOrEmpty(componentName) && componentType == null)
			{
				throw new ArgumentNullException("componentType",
				                                "At least one - componentName or componentType must not be null or empty");
			}

			this.componentType = componentType;
			this.componentName = componentName;
			this.additionalArguments = additionalArguments ?? new Arguments();
		}

		public Type ComponentType
		{
			get { return componentType; }
		}

		public string ComponentName
		{
			get { return componentName; }
		}

		public IDictionary AdditionalArguments
		{
			get { return additionalArguments; }
		}

		/// <summary>
		/// Resolves the component(s) from given kernel.
		/// </summary>
		/// <param name="kernel"></param>
		/// <returns>Resolved component(s).</returns>
		public virtual object Resolve(IKernel kernel)
		{
			if (string.IsNullOrEmpty(ComponentName) == false && kernel.HasComponent(ComponentName))
			{
				if (ComponentType == null)
				{
					return kernel.Resolve(ComponentName, AdditionalArguments);
				}

				return kernel.Resolve(ComponentName, ComponentType, AdditionalArguments);
			}

			return kernel.Resolve(ComponentType, AdditionalArguments);
		}
	}
}