// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Diagnostics
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel;

	public class DuplicatedDependenciesDiagnostic : IDuplicatedDependenciesDiagnostic
	{
		private readonly IKernel kernel;

		public DuplicatedDependenciesDiagnostic(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public IDictionary<IHandler, Pair<DependencyModel, DependencyModel>[]> Inspect()
		{
			var allHandlers = kernel.GetAssignableHandlers(typeof(object));
			var result = new Dictionary<IHandler, Pair<DependencyModel, DependencyModel>[]>();
			foreach (var handler in allHandlers)
			{
				var duplicateDependencies = FindDuplicateDependenciesFor(handler);
				if (duplicateDependencies.Length > 0)
				{
					result.Add(handler, duplicateDependencies);
				}
			}
			return result;
		}

		private void CollectDuplicatesBetweenProperties(IEnumerable<PropertySet> properties, List<Pair<DependencyModel, DependencyModel>> duplicates)
		{
			var array = properties.ToArray();
			for (var i = 0; i < array.Length; i++)
			{
				for (var j = i + 1; j < array.Length; j++)
				{
					if (IsDuplicate(array[i].Dependency, array[j].Dependency))
					{
						duplicates.Add(new Pair<DependencyModel, DependencyModel>(array[i].Dependency, array[j].Dependency));
					}
				}
			}
		}

		private void CollectDuplicatesBetweenPropertiesAndConstructors(ConstructorCandidateCollection constructors, PropertySetCollection properties, List<Pair<DependencyModel, DependencyModel>> duplicates)
		{
			foreach (var constructor in constructors)
			{
				foreach (var dependency in constructor.Dependencies)
				{
					foreach (var property in properties)
					{
						if (IsDuplicate(property.Dependency, dependency))
						{
							duplicates.Add(new Pair<DependencyModel, DependencyModel>(dependency, property.Dependency));
						}
					}
				}
			}
		}

		private Pair<DependencyModel, DependencyModel>[] FindDuplicateDependenciesFor(IHandler handler)
		{
			// TODO: handler non-default activators
			// TODO: handle duplicates between properties/ctor arguments when they have both same type, but no service override...

			var duplicates = new List<Pair<DependencyModel, DependencyModel>>();
			var properties = handler.ComponentModel.Properties;
			var constructors = handler.ComponentModel.Constructors;
			CollectDuplicatesBetweenProperties(properties, duplicates);
			CollectDuplicatesBetweenPropertiesAndConstructors(constructors, properties, duplicates);
			return duplicates.ToArray();
		}

		private bool IsDuplicate(DependencyModel foo, DependencyModel bar)
		{
			if (foo.ReferencedComponentName != null || bar.ReferencedComponentName != null)
			{
				return string.Equals(foo.ReferencedComponentName, bar.ReferencedComponentName, StringComparison.OrdinalIgnoreCase);
			}

			if (string.Equals(foo.DependencyKey, bar.DependencyKey, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return foo.TargetItemType == bar.TargetItemType;
		}
	}
}