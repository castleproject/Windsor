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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Internal;

	/// <summary>
	///   This implementation of <see cref = "IContributeComponentModelConstruction" />
	///   collects all available constructors and populates them in the model
	///   as candidates. The Kernel will pick up one of the candidates
	///   according to a heuristic.
	/// </summary>
	[Serializable]
	public class ConstructorDependenciesModelInspector : IContributeComponentModelConstruction
	{
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
		    InspectConstructors(model);
		}

        protected virtual void InspectConstructors(ComponentModel model)
        {
            var targetType = model.Implementation;
            var constructors = GetConstructors(model, targetType);

            if (constructors.Count == 0)
			{
				return;
            }

            var filters = StandardConstructorFilters.GetConstructorFilters(model, false);

            // We register each valid public constructor
            // which is not selected by the filter predicate
            // and let the ComponentFactory select an 
            // eligible amongst the candidates later
            if (filters == null)
            {
                constructors.ForEach(c => model.AddConstructor(CreateConstructorCandidate(model, c)));
            }
            else
            {
                var filteredConstructors = constructors.ToArray();
                filteredConstructors = filters.Aggregate(filteredConstructors, (c, filter) => filter(model, c));
                filteredConstructors.ForEach(c => model.AddConstructor(CreateConstructorCandidate(model, c)));
            }
        }

        private List<ConstructorInfo> GetConstructors(ComponentModel model, Type targetType)
        {
            var constructors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                                         .Where(IsValidConstructor)
                                         .ToList();
            return constructors;
        }

	    protected virtual ConstructorCandidate CreateConstructorCandidate(ComponentModel model, ConstructorInfo constructor)
		{
			var parameters = constructor.GetParameters();
			var dependencies = parameters.ConvertAll(BuildParameterDependency);
			return new ConstructorCandidate(constructor, dependencies);
		}

		private static ConstructorDependencyModel BuildParameterDependency(ParameterInfo parameter)
		{
			return new ConstructorDependencyModel(parameter);
		}

		private static bool HasDoNotSelectAttribute(ConstructorInfo constructor)
		{
			return constructor.HasAttribute<DoNotSelectAttribute>();
		}

        private static bool IsValidConstructor(ConstructorInfo constructor)
        {
            return !HasDoNotSelectAttribute(constructor);
        }
	}
}