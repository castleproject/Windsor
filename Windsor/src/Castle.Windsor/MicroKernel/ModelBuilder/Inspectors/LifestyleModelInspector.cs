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
	using Castle.Core.Internal;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	///   Inspects the component configuration and the type looking for a
	///   definition of lifestyle type. The configuration preceeds whatever
	///   is defined in the component.
	/// </summary>
	/// <remarks>
	///   This inspector is not guarantee to always set up an lifestyle type. 
	///   If nothing could be found it wont touch the model. In this case is up to
	///   the kernel to establish a default lifestyle for components.
	/// </remarks>
	[Serializable]
	public class LifestyleModelInspector : IContributeComponentModelConstruction
	{
		private readonly IConversionManager converter;

		public LifestyleModelInspector(IConversionManager converter)
		{
			this.converter = converter;
		}

		/// <summary>
		///   Searches for the lifestyle in the configuration and, if unsuccessful
		///   look for the lifestyle attribute in the implementation type.
		/// </summary>
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (!ReadLifestyleFromConfiguration(model))
			{
				ReadLifestyleFromType(model);
			}
		}

		protected Type ExtractCustomType(ComponentModel model)
		{
			var typeNameRaw = model.Configuration.Attributes["customLifestyleType"];
			if (typeNameRaw == null)
			{
				return null;
			}
			var lifestyle = converter.PerformConversion<Type>(typeNameRaw);
			ValidateLifestyleManager(lifestyle);
			return lifestyle;
		}

		/// <summary>
		///   Reads the attribute "lifestyle" associated with the 
		///   component configuration and tries to convert to <see cref = "LifestyleType" />  
		///   enum type.
		/// </summary>
		protected virtual bool ReadLifestyleFromConfiguration(ComponentModel model)
		{
			if (model.Configuration == null)
			{
				return false;
			}

			var lifestyle = model.Configuration.Attributes["lifestyle"];
			if (lifestyle == null)
			{
				var customType = ExtractCustomType(model);
				if (customType != null)
				{
					model.LifestyleType = LifestyleType.Custom;
					model.CustomLifestyle = customType;
					return true;
				}
				var binderType = ExtractBinderType(model);
				if (binderType != null)
				{
					var binder = ExtractBinder(binderType, model.Name);
					model.LifestyleType = LifestyleType.Bound;
					model.ExtendedProperties[Constants.ScopeRootSelector] = binder;
					return true;
				}
				return false;
			}

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

		/// <summary>
		///   Check if the type expose one of the lifestyle attributes
		///   defined in Castle.Model namespace.
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
			else if (model.LifestyleType == LifestyleType.Bound)
			{
				var binder = ExtractBinder(((BoundToAttribute)attribute).ScopeRootBinderType, model.Name);
				model.ExtendedProperties[Constants.ScopeRootSelector] = binder;
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

		private Func<IHandler[], IHandler> ExtractBinder(Type scopeRootBinderType, string name)
		{
			var filterMethod =
				scopeRootBinderType.FindMembers(MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public, IsBindMethod, null)
					.FirstOrDefault();
			if (filterMethod == null)
			{
				throw new InvalidOperationException(
					string.Format(
						"Type {0} which was designated as 'scopeRootBinderType' for component {1} does not have any public instance method matching signature of 'IHandler Method(IHandler[] pickOne)' and can not be used as scope root binder.",
						scopeRootBinderType.Name, name));
			}
			var instance = scopeRootBinderType.CreateInstance<object>();
			return
				(Func<IHandler[], IHandler>)
				Delegate.CreateDelegate(typeof(Func<IHandler[], IHandler>), instance, (MethodInfo)filterMethod);
		}

		private Type ExtractBinderType(ComponentModel model)
		{
			var typeNameRaw = model.Configuration.Attributes["scopeRootBinderType"];
			if (typeNameRaw == null)
			{
				return null;
			}
			return converter.PerformConversion<Type>(typeNameRaw);
		}

		private void ExtractCustomConfig(ComponentModel model)
		{
			var lifestyle = ExtractCustomType(model);
			if (lifestyle != null)
			{
				model.CustomLifestyle = lifestyle;
			}
			else
			{
				throw new InvalidOperationException(
					@"The attribute 'customLifestyleType' must be specified in conjunction with the 'lifestyle' attribute set to ""custom"".");
			}
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

		private bool IsBindMethod(MemberInfo methodMember, object _)
		{
			var method = (MethodInfo)methodMember;
			if (method.ReturnType != typeof(IHandler))
			{
				return false;
			}
			var parameters = method.GetParameters();
			if (parameters.Length != 1)
			{
				return false;
			}
			if (parameters[0].ParameterType != typeof(IHandler[]))
			{
				return false;
			}
			return true;
		}
	}
}