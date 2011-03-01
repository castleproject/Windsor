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

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel.Util;

	/// <summary>
	///   Check for a node 'parameters' within the component 
	///   configuration. For each child it, a ParameterModel is created
	///   and added to ComponentModel's Parameters collection
	/// </summary>
	[Serializable]
	public class ConfigurationParametersInspector : IContributeComponentModelConstruction
	{
		/// <summary>
		///   Inspect the configuration associated with the component
		///   and populates the parameter model collection accordingly
		/// </summary>
		/// <param name = "kernel"></param>
		/// <param name = "model"></param>
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (model.Configuration == null)
			{
				return;
			}

			var parameters = model.Configuration.Children["parameters"];
			if (parameters == null)
			{
				return;
			}

			foreach (var parameter in parameters.Children)
			{
				var name = parameter.Name;
				var value = parameter.Value;

				if (value == null && parameter.Children.Count != 0)
				{
					var parameterValue = parameter.Children[0];
					model.Parameters.Add(name, parameterValue);
				}
				else
				{
					model.Parameters.Add(name, value);
				}
			}

			// Experimental code
			InspectCollections(model);
		}

		private void AddAnyServiceOverrides(ComponentModel model, IConfiguration config)
		{
			foreach (var item in config.Children)
			{
				if (item.Children.Count > 0)
				{
					AddAnyServiceOverrides(model, item);
				}

				if (item.Value == null || !ReferenceExpressionUtil.IsReference(item.Value))
				{
					continue;
				}

				var newKey = ReferenceExpressionUtil.ExtractComponentKey(item.Value);
				model.Dependencies.Add(new DependencyModel(newKey, null, false));
			}
		}

		private void InspectCollections(ComponentModel model)
		{
			foreach (ParameterModel parameter in model.Parameters)
			{
				if (parameter.ConfigValue != null)
				{
					if (IsArray(parameter) || IsList(parameter))
					{
						AddAnyServiceOverrides(model, parameter.ConfigValue);
					}
				}

				if (parameter.Value == null || !ReferenceExpressionUtil.IsReference(parameter.Value))
				{
					continue;
				}

				var newKey = ReferenceExpressionUtil.ExtractComponentKey(parameter.Value);

				// Update dependencies to ServiceOverride

				model.Dependencies.Add(new DependencyModel(newKey, null, false));
			}
		}

		private bool IsArray(ParameterModel parameter)
		{
			return parameter.ConfigValue.Name == "array";
		}

		private bool IsList(ParameterModel parameter)
		{
			return parameter.ConfigValue.Name == "list";
		}
	}
}