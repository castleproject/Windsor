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
	using System.Text;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel;

	public class DuplicatedDependenciesDiagnostic : IDuplicatedDependenciesDiagnostic
	{
		private readonly IKernel kernel;

		public DuplicatedDependenciesDiagnostic(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public string GetDetails(DependencyDuplicate duplicates)
		{
			var details = new StringBuilder();
			Describe(details, duplicates.Dependency1);
			details.Append(" duplicates ");
			Describe(details, duplicates.Dependency2);
			switch (duplicates.Reason)
			{
				case DependencyDuplicationReason.Name:
					details.Append(", they both have the same name.");
					break;
				case DependencyDuplicationReason.Type:
					details.Append(", they both have the same type.");
					break;
				case DependencyDuplicationReason.NameAndType:
					details.Append(", they both have the same namd and type.");
					break;
				case DependencyDuplicationReason.Reference:
					details.Append(", they both reference the same component " + duplicates.Dependency1.ReferencedComponentName);
					break;
			}
			return details.ToString();
		}

		public Pair<IHandler, DependencyDuplicate[]>[] Inspect()
		{
			var allHandlers = kernel.GetAssignableHandlers(typeof(object));
			var result = new List<Pair<IHandler, DependencyDuplicate[]>>();
			foreach (var handler in allHandlers)
			{
				var duplicateDependencies = FindDuplicateDependenciesFor(handler);
				if (duplicateDependencies.Length > 0)
				{
					result.Add(new Pair<IHandler, DependencyDuplicate[]>(handler, duplicateDependencies));
				}
			}
			return result.ToArray();
		}

		private void CollectDuplicatesBetween(DependencyModel[] array, ICollection<DependencyDuplicate> duplicates)
		{
			for (var i = 0; i < array.Length; i++)
			{
				for (var j = i + 1; j < array.Length; j++)
				{
					var reason = IsDuplicate(array[i], array[j]);
					if (reason != DependencyDuplicationReason.Unspecified)
					{
						duplicates.Add(new DependencyDuplicate(array[i], array[j], reason));
					}
				}
			}
		}

		private void CollectDuplicatesBetweenConstructorParameters(ConstructorCandidateCollection constructors, ICollection<DependencyDuplicate> duplicates)
		{
			foreach (var constructor in constructors)
			{
				CollectDuplicatesBetween(constructor.Dependencies, duplicates);
			}
		}

		private void CollectDuplicatesBetweenProperties(DependencyModel[] properties, ICollection<DependencyDuplicate> duplicates)
		{
			CollectDuplicatesBetween(properties, duplicates);
		}

		private void CollectDuplicatesBetweenPropertiesAndConstructors(ConstructorCandidateCollection constructors, DependencyModel[] properties, ICollection<DependencyDuplicate> duplicates)
		{
			foreach (var constructor in constructors)
			{
				foreach (var dependency in constructor.Dependencies)
				{
					foreach (var property in properties)
					{
						var reason = IsDuplicate(property, dependency);
						if (reason != DependencyDuplicationReason.Unspecified)
						{
							duplicates.Add(new DependencyDuplicate(property, dependency, reason));
						}
					}
				}
			}
		}

		private DependencyDuplicate[] FindDuplicateDependenciesFor(IHandler handler)
		{
			// TODO: handler non-default activators
			// NOTE: how exactly? We don't have enough context to know, other than via the well known activators that we ship with
			//		 but we can only inspect the type here...
			var duplicates = new HashSet<DependencyDuplicate>();
			var properties = handler.ComponentModel.Properties
				.Select(p => p.Dependency)
				.OrderBy(d => d.ToString())
				.ToArray();
			var constructors = handler.ComponentModel.Constructors;
			CollectDuplicatesBetweenProperties(properties, duplicates);
			CollectDuplicatesBetweenConstructorParameters(constructors, duplicates);
			CollectDuplicatesBetweenPropertiesAndConstructors(constructors, properties, duplicates);
			return duplicates.ToArray();
		}

		private DependencyDuplicationReason IsDuplicate(DependencyModel foo, DependencyModel bar)
		{
			if (foo.ReferencedComponentName != null || bar.ReferencedComponentName != null)
			{
				if (string.Equals(foo.ReferencedComponentName, bar.ReferencedComponentName, StringComparison.OrdinalIgnoreCase))
				{
					return DependencyDuplicationReason.Reference;
				}
			}

			if (string.Equals(foo.DependencyKey, bar.DependencyKey, StringComparison.OrdinalIgnoreCase))
			{
				if (foo.TargetItemType == bar.TargetItemType)
				{
					return DependencyDuplicationReason.NameAndType;
				}
				return DependencyDuplicationReason.Name;
			}
			if (foo.TargetItemType == bar.TargetItemType)
			{
				return DependencyDuplicationReason.Type;
			}
			return 0;
		}

		private static void Describe(StringBuilder details, DependencyModel dependency)
		{
			if (dependency is PropertyDependencyModel)
			{
				details.Append("Property ");
			}
			else if (dependency is ConstructorDependencyModel)
			{
				details.Append("Constructor parameter ");
			}
			else
			{
				details.Append("Depdendency ");
			}
			details.Append(dependency.TargetItemType.ToCSharpString() + " " + dependency.DependencyKey);
		}
	}
}