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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System;
	using System.Reflection;
	using Castle.Core;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.MicroKernel.Util;

	/// <summary>
	/// This implementation of <see cref="IContributeComponentModelConstruction"/>
	/// collects all available constructors and populates them in the model
	/// as candidates. The Kernel will pick up one of the candidates
	/// according to a heuristic.
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class ConstructorDependenciesModelInspector : IContributeComponentModelConstruction
	{
#if (!SILVERLIGHT)
		[NonSerialized]
#endif
		private IConversionManager converter;

		public ConstructorDependenciesModelInspector()
		{
		}

		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (converter == null)
			{
				converter = (IConversionManager) kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);
			}

			var targetType = model.Implementation;

			var constructors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

			foreach(var constructor in constructors)
			{
				// We register each public constructor
				// and let the ComponentFactory select an 
				// eligible amongst the candidates later
				model.Constructors.Add(CreateConstructorCandidate(model, constructor));
			}
		}

		protected virtual ConstructorCandidate CreateConstructorCandidate(ComponentModel model, ConstructorInfo constructor)
		{
			var parameters = constructor.GetParameters();

			var dependencies = new DependencyModel[parameters.Length];

			for(int i = 0; i < parameters.Length; i++)
			{
				var parameter = parameters[i];

				var paramType = parameter.ParameterType;

				// This approach is somewhat problematic. We should use
				// another strategy to differentiate types and classify dependencies
				if (converter.IsSupportedAndPrimitiveType(paramType))
				{
					dependencies[i] = new DependencyModel(DependencyType.Parameter, parameter.Name, paramType, false);
				}
				else if (String.IsNullOrEmpty(parameter.Name) == false)
				{
					var modelParameter = model.Parameters[parameter.Name];

					if (modelParameter != null && ReferenceExpressionUtil.IsReference(modelParameter.Value))
					{
						var key = ReferenceExpressionUtil.ExtractComponentKey(modelParameter.Value);

						dependencies[i] = new DependencyModel(DependencyType.ServiceOverride, key, paramType, false);
					}
					else
					{
						dependencies[i] = new DependencyModel(DependencyType.Service, parameter.Name, paramType, false);
					}
				}
				else
				{
					dependencies[i] = new DependencyModel(DependencyType.Service, null, paramType, false);
				}
			}

			return new ConstructorCandidate(constructor, dependencies);
		}
	}
}
