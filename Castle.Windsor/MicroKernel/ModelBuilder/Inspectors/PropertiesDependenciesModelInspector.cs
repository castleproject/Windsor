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
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	///   This implementation of <see cref = "IContributeComponentModelConstruction" />
	///   collects all potential writable public properties exposed by the component 
	///   implementation and populates the model with them.
	///   The Kernel might be able to set some of these properties when the component 
	///   is requested.
	/// </summary>
	[Serializable]
	public class PropertiesDependenciesModelInspector : IContributeComponentModelConstruction
	{
		[NonSerialized]
		private readonly IConversionManager converter;

		public PropertiesDependenciesModelInspector(IConversionManager converter)
		{
			this.converter = converter;
		}

		/// <summary>
		///   Adds the properties as optional dependencies of this component.
		/// </summary>
		/// <param name = "kernel"></param>
		/// <param name = "model"></param>
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			InspectProperties(model);
		}

		protected virtual void InspectProperties(ComponentModel model)
		{
			var targetType = model.Implementation;
#if SILVERLIGHT
			if(targetType.IsVisible == false) return;
#endif
			if (model.InspectionBehavior == PropertiesInspectionBehavior.Undefined)
			{
				model.InspectionBehavior = GetInspectionBehaviorFromTheConfiguration(model.Configuration);
			}

			if (model.InspectionBehavior == PropertiesInspectionBehavior.None)
			{
				// Nothing to be inspected
				return;
			}

			BindingFlags bindingFlags;
			if (model.InspectionBehavior == PropertiesInspectionBehavior.DeclaredOnly)
			{
				bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
			}
			else // if (model.InspectionBehavior == PropertiesInspectionBehavior.All) or Undefined
			{
				bindingFlags = BindingFlags.Public | BindingFlags.Instance;
			}

			var properties = targetType.GetProperties(bindingFlags);
			foreach (var property in properties)
			{
				if (!property.CanWrite || property.GetSetMethod() == null)
				{
					continue;
				}

				var indexerParams = property.GetIndexParameters();

				if (indexerParams != null && indexerParams.Length != 0)
				{
					continue;
				}

				if (property.IsDefined(typeof(DoNotWireAttribute), true))
				{
					continue;
				}

				DependencyModel dependency;

				var propertyType = property.PropertyType;

				// All these dependencies are simple guesses
				// So we make them optional (the 'true' parameter below)

				if (converter.IsSupportedAndPrimitiveType(propertyType))
				{
					dependency = new DependencyModel(property.Name, propertyType, isOptional: true);
				}
				else if (propertyType.IsInterface || propertyType.IsClass)
				{
					dependency = new DependencyModel(property.Name, propertyType, isOptional: true);
				}
				else
				{
					// What is it?!
					// Awkward type, probably.

					continue;
				}

				model.Properties.Add(new PropertySet(property, dependency));
			}
		}

		private PropertiesInspectionBehavior GetInspectionBehaviorFromTheConfiguration(IConfiguration config)
		{
			if (config == null || config.Attributes["inspectionBehavior"] == null)
			{
				// return default behavior
				return PropertiesInspectionBehavior.All;
			}

			var enumStringVal = config.Attributes["inspectionBehavior"];

			try
			{
				return (PropertiesInspectionBehavior)
				       Enum.Parse(typeof(PropertiesInspectionBehavior), enumStringVal, true);
			}
			catch (Exception)
			{
				var enumType = typeof(PropertiesInspectionBehavior);
				var infos = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
				var enumNames = infos.Select(x => x.Name);
				var message = String.Format("Error on properties inspection. Could not convert the inspectionBehavior attribute value into an expected enum value. " +
				                            "Value found is '{0}' while possible values are '{1}'",
				                            enumStringVal, String.Join(",", enumNames.ToArray()));

				throw new KernelException(message);
			}
		}
	}
}