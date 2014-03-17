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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System;
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>Inspects the component configuration and the type looking for a definition of lifestyle type. The configuration preceeds whatever is defined in the component.</summary>
	/// <remarks>
	///     This inspector is not guarantee to always set up an lifestyle type. If nothing could be found it wont touch the model. In this case is up to the kernel to establish a default lifestyle for
	///     components.
	/// </remarks>
	[Serializable]
	public class LifestyleModelInspector : IContributeComponentModelConstruction
	{
		private readonly IConversionManager converter;

		public LifestyleModelInspector(IConversionManager converter)
		{
			this.converter = converter;
		}

		/// <summary>Searches for the lifestyle in the configuration and, if unsuccessful look for the lifestyle attribute in the implementation type.</summary>
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (!ReadLifestyleFromConfiguration(model))
			{
				ReadLifestyleFromType(model);
			}
		}

		/// <summary>
		///     Reads the attribute "lifestyle" associated with the component configuration and tries to convert to <see cref = "LifestyleType" />
		///     enum type.
		/// </summary>
		protected virtual bool ReadLifestyleFromConfiguration(ComponentModel model)
		{
			if (model.Configuration == null)
			{
				return false;
			}

			var lifestyleRaw = model.Configuration.Attributes["lifestyle"];
			if (lifestyleRaw != null)
			{
				var lifestyleType = converter.PerformConversion<LifestyleType>(lifestyleRaw);
				model.LifestyleType = lifestyleType;
				switch (lifestyleType)
				{
					case LifestyleType.Singleton:
					case LifestyleType.Transient:
#if !(SILVERLIGHT || CLIENTPROFILE)
					case LifestyleType.PerWebRequest:
#endif
					case LifestyleType.Thread:
						return true;
					case LifestyleType.Pooled:
						ExtractPoolConfig(model);
						return true;
					case LifestyleType.Custom:
						var lifestyle = GetMandatoryTypeFromAttribute(model, "customLifestyleType", lifestyleType);
						ValidateTypeFromAttribute(lifestyle, typeof(ILifestyleManager), "customLifestyleType");
						model.CustomLifestyle = lifestyle;

						return true;
					case LifestyleType.Scoped:
						var scopeAccessorType = GetTypeFromAttribute(model, "scopeAccessorType");
						if (scopeAccessorType != null)
						{
							ValidateTypeFromAttribute(scopeAccessorType, typeof(IScopeAccessor), "scopeAccessorType");
							model.ExtendedProperties[Constants.ScopeAccessorType] = scopeAccessorType;
						}
						return true;
					case LifestyleType.Bound:
						var binderType = GetTypeFromAttribute(model, "scopeRootBinderType");
						if (binderType != null)
						{
							var binder = ExtractBinder(binderType, model.Name);
							model.ExtendedProperties[Constants.ScopeRootSelector] = binder;
						}
						return true;
					default:
						throw new InvalidOperationException(string.Format("Component {0} has {1} lifestyle. This is not a valid value.", model.Name, lifestyleType));
				}
			}
			else
			{
				// type was not present, but we might figure out the lifestyle based on presence of some attributes related to some lifestyles
				var binderType = GetTypeFromAttribute(model, "scopeRootBinderType");
				if (binderType != null)
				{
					var binder = ExtractBinder(binderType, model.Name);
					model.ExtendedProperties[Constants.ScopeRootSelector] = binder;
					model.LifestyleType = LifestyleType.Bound;
					return true;
				}
				var scopeAccessorType = GetTypeFromAttribute(model, "scopeAccessorType");
				if (scopeAccessorType != null)
				{
					ValidateTypeFromAttribute(scopeAccessorType, typeof(IScopeAccessor), "scopeAccessorType");
					model.ExtendedProperties[Constants.ScopeAccessorType] = scopeAccessorType;
					model.LifestyleType = LifestyleType.Scoped;
					return true;
				}
				var customLifestyleType = GetTypeFromAttribute(model, "customLifestyleType");
				if (customLifestyleType != null)
				{
					ValidateTypeFromAttribute(customLifestyleType, typeof(ILifestyleManager), "customLifestyleType");
					model.CustomLifestyle = customLifestyleType;
					model.LifestyleType = LifestyleType.Custom;
					return true;
				}
			}
			return false;
		}

		/// <summary>Check if the type expose one of the lifestyle attributes defined in Castle.Model namespace.</summary>
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
				ValidateTypeFromAttribute(custom.CustomLifestyleType, typeof(ILifestyleManager), "CustomLifestyleType");
				model.CustomLifestyle = custom.CustomLifestyleType;
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
			else if (model.LifestyleType == LifestyleType.Scoped)
			{
				var scoped = (ScopedAttribute)attribute;
				if (scoped.ScopeAccessorType != null)
				{
					ValidateTypeFromAttribute(scoped.ScopeAccessorType, typeof(IScopeAccessor), "ScopeAccessorType");
					model.ExtendedProperties[Constants.ScopeAccessorType] = scoped.ScopeAccessorType;
				}
			}
		}

		protected virtual void ValidateTypeFromAttribute(Type typeFromAttribute, Type expectedInterface, string attribute)
		{
			if (expectedInterface.IsAssignableFrom(typeFromAttribute))
			{
				return;
			}
			throw new InvalidOperationException(String.Format("The Type '{0}' specified in the '{2}' attribute must implement {1}", typeFromAttribute.FullName, expectedInterface.FullName, attribute));
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

		private Type GetMandatoryTypeFromAttribute(ComponentModel model, string attribute, LifestyleType lifestyleType)
		{
			var rawAttribute = model.Configuration.Attributes[attribute];
			if (rawAttribute == null)
			{
				throw new InvalidOperationException(string.Format("Component {0} has {1} lifestyle, but its configuration doesn't have mandatory '{2}' attribute.", model.Name, lifestyleType, attribute));
			}
			return converter.PerformConversion<Type>(rawAttribute);
		}

		private Type GetTypeFromAttribute(ComponentModel model, string attribute)
		{
			var rawAttribute = model.Configuration.Attributes[attribute];
			if (rawAttribute == null)
			{
				return null;
			}
			return converter.PerformConversion<Type>(rawAttribute);
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