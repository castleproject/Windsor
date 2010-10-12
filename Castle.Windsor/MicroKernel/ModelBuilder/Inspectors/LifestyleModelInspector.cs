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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	/// Inspects the component configuration and the type looking for a
	/// definition of lifestyle type. The configuration preceeds whatever
	/// is defined in the component.
	/// </summary>
	/// <remarks>
	/// This inspector is not guarantee to always set up an lifestyle type. 
	/// If nothing could be found it wont touch the model. In this case is up to
	/// the kernel to establish a default lifestyle for components.
	/// </remarks>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class LifestyleModelInspector : IContributeComponentModelConstruction
	{
		private IConversionManager converter;

		/// <summary>
		/// Searches for the lifestyle in the configuration and, if unsuccessful
		/// look for the lifestyle attribute in the implementation type.
		/// </summary>
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			EnsureConverterSet(kernel);
			if (!ReadLifestyleFromConfiguration(model))
			{
				ReadLifestyleFromType(model);
			}
		}

		private void EnsureConverterSet(IKernel kernel)
		{
			if (converter == null)
			{
				converter = kernel.GetConversionManager();
			}
		}

		/// <summary>
		/// Reads the attribute "lifestyle" associated with the 
		/// component configuration and tries to convert to <see cref="LifestyleType"/>  
		/// enum type. 
		/// </summary>
		protected virtual bool ReadLifestyleFromConfiguration(ComponentModel model)
		{
			if (model.Configuration != null)
			{
				var lifestyle = model.Configuration.Attributes["lifestyle"];
				if (lifestyle != null)
				{
					var type = converter.PerformConversion<LifestyleType>(lifestyle);
					model.LifestyleType = type;

					if (model.LifestyleType == LifestyleType.Pooled)
					{
						ExtractPoolConfig(model);
					}
					else if (model.LifestyleType == LifestyleType.Custom)
					{
						ExtractCustomConfig(model);
					}

					return true;
				}
			}

			return false;
		}

		private void ExtractPoolConfig(ComponentModel model)
		{
			var initialRaw = model.Configuration.Attributes["initialPoolSize"];
			var maxRaw = model.Configuration.Attributes["maxPoolSize"];

			if (initialRaw != null)
			{
				var initial = converter.PerformConversion<int>(initialRaw);
				model.ExtendedProperties[ExtendedPropertiesConstants.Pool_InitialPoolSize] = initial;
			}
			if (maxRaw != null)
			{
				var max = converter.PerformConversion<int>(maxRaw);
				model.ExtendedProperties[ExtendedPropertiesConstants.Pool_MaxPoolSize] = max;
			}
		}

		private void ExtractCustomConfig(ComponentModel model)
		{
			var customLifestyleTypeRaw = model.Configuration.Attributes["customLifestyleType"];
			if (customLifestyleTypeRaw != null)
			{
				var lifestyle = converter.PerformConversion<Type>(customLifestyleTypeRaw);
				ValidateLifestyleManager(lifestyle);
				model.CustomLifestyle = lifestyle;
			}
			else
			{
				const string message =
					@"The attribute 'customLifestyleType' must be specified in conjunction with the 'lifestyle' attribute set to ""custom"".";

				throw new Exception(message);
			}
		}

		protected virtual void ValidateLifestyleManager(Type customLifestyleManager)
		{
			if (customLifestyleManager.Is<ILifestyleManager>() == false)
			{
				var message =
					String.Format(
						"The Type '{0}' specified in the componentActivatorType attribute must implement {1}",
						customLifestyleManager.FullName, typeof(ILifestyleManager).FullName);
				throw new InvalidOperationException(message);
			}
		}

		/// <summary>
		/// Check if the type expose one of the lifestyle attributes
		/// defined in Castle.Model namespace.
		/// </summary>
		protected virtual void ReadLifestyleFromType(ComponentModel model)
		{
			var attributes = model.Implementation.GetAttributes<LifestyleAttribute>();
			if (attributes.Length == 0)
			{
				return;
			}
			var attribute = attributes[0];
			model.LifestyleType = attribute.Lifestyle;

			if (model.LifestyleType == LifestyleType.Custom)
			{
				var custom = (CustomLifestyleAttribute)attribute;
				ValidateLifestyleManager(custom.LifestyleHandlerType);
				model.CustomLifestyle = custom.LifestyleHandlerType;
			}
			else if (model.LifestyleType == LifestyleType.Pooled)
			{
				var pooled = (PooledAttribute)attribute;
				model.ExtendedProperties[ExtendedPropertiesConstants.Pool_InitialPoolSize] = pooled.InitialPoolSize;
				model.ExtendedProperties[ExtendedPropertiesConstants.Pool_MaxPoolSize] = pooled.MaxPoolSize;
			}
		}
	}
}