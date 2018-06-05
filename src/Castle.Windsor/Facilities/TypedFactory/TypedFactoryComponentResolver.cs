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

	using Castle.MicroKernel;

	/// <summary>
	///   Represents a single component to be resolved via Typed Factory
	/// </summary>
	public class TypedFactoryComponentResolver
	{
		protected readonly IDictionary additionalArguments;
		protected readonly string componentName;
		protected readonly Type componentType;
		protected readonly bool fallbackToResolveByTypeIfNameNotFound;
		private readonly Type actualSelectorType;

		public TypedFactoryComponentResolver(string componentName, Type componentType, IDictionary additionalArguments,
		                                     bool fallbackToResolveByTypeIfNameNotFound, Type actualSelectorType)
		{
			if (string.IsNullOrEmpty(componentName) && componentType == null)
			{
				throw new ArgumentNullException("componentType",
				                                "At least one - componentName or componentType must not be null or empty");
			}

			this.componentType = componentType;
			this.componentName = componentName;
			this.additionalArguments = additionalArguments;
			this.fallbackToResolveByTypeIfNameNotFound = fallbackToResolveByTypeIfNameNotFound;
			this.actualSelectorType = actualSelectorType;
		}

		/// <summary>
		///   Resolves the component(s) from given kernel.
		/// </summary>
		/// <param name = "kernel"></param>
		/// <param name = "scope"></param>
		/// <returns>Resolved component(s).</returns>
		public virtual object Resolve(IKernelInternal kernel, IReleasePolicy scope)
		{
			if (LoadByName(kernel))
			{
				try
				{
					return kernel.Resolve(componentName, componentType, additionalArguments, scope);
				}
				catch (ComponentNotFoundException e)
				{
					if (actualSelectorType == typeof(DefaultDelegateComponentSelector) && fallbackToResolveByTypeIfNameNotFound == false)
					{
						e.Data["breakingChangeId"] = "typedFactoryFallbackToResolveByTypeIfNameNotFound";
						e.Data["breakingChange"] = "This exception may have been caused by a breaking change between Windsor 2.5 and 3.0 See breakingchanges.txt for more details.";
					}
					throw;
				}
			}
			return kernel.Resolve(componentType, additionalArguments, scope);
		}

		private bool LoadByName(IKernelInternal kernel)
		{
			if (componentName == null)
			{
				return false;
			}
			return fallbackToResolveByTypeIfNameNotFound == false || kernel.LoadHandlerByName(componentName, componentType, additionalArguments) != null;
		}
	}
}