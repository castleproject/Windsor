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

namespace Castle.Core
{
	using System;

	/// <summary>
	///   Represents a dependency (other component or a 
	///   fixed value available through external configuration).
	/// </summary>
	[Serializable]
	public class DependencyModel
	{
		private readonly bool isValueType;
		private readonly Type targetItemType;
		private readonly Type targetType;

		/// <summary>
		///   Initializes a new instance of the <see cref = "DependencyModel" /> class.
		/// </summary>
		/// <param name = "dependencyKey">The dependency key.</param>
		/// <param name = "targetType">Type of the target.</param>
		/// <param name = "isOptional">if set to <c>true</c> [is optional].</param>
		public DependencyModel(String dependencyKey, Type targetType, bool isOptional)
			: this(dependencyKey, targetType, isOptional, false, null)
		{
		}

		// TODO: add configuration so that information about override is attached to the dependency
		public DependencyModel(string dependencyKey, Type targetType, bool isOptional, bool hasDefaultValue, object defaultValue)
		{
			this.targetType = targetType;
			if (targetType != null && targetType.IsByRef)
			{
				targetItemType = targetType.GetElementType();
			}
			else
			{
				targetItemType = targetType;
			}
			if (targetItemType != null)
			{
				isValueType = targetItemType.IsValueType;
			}
			DependencyKey = dependencyKey;
			IsOptional = isOptional;
			HasDefaultValue = hasDefaultValue;
			DefaultValue = defaultValue;
		}

		public object DefaultValue { get; private set; }

		/// <summary>
		///   Gets or sets the dependency key.
		/// </summary>
		/// <value>The dependency key.</value>
		public string DependencyKey { get; set; }

		public bool HasDefaultValue { get; private set; }

		/// <summary>
		///   Gets or sets whether this dependency is optional.
		/// </summary>
		/// <value>
		///   <c>true</c> if this dependency is optional; otherwise, <c>false</c>.
		/// </value>
		public bool IsOptional { get; set; }

		public bool IsValueType
		{
			get { return isValueType; }
		}

		/// <summary>
		///   Gets the service type of the dependency.
		///   This is the same type as <see cref = "TargetType" /> or if <see cref = "TargetType" /> is by ref,
		///   then it's the element type of the reference. (in other words if dependency 
		///   is <c>out IFoo foo</c> this will be <c>IFoo</c>, while <see cref = "TargetType" /> will be <c>&amp;IFoo</c>);
		/// </summary>
		public Type TargetItemType
		{
			get { return targetItemType; }
		}

		/// <summary>
		///   Gets the type of the target.
		/// </summary>
		/// <value>The type of the target.</value>
		public Type TargetType
		{
			get { return targetType; }
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			var other = obj as DependencyModel;
			if (other == null)
			{
				return false;
			}
			return Equals(other.targetType, targetType) &&
			       Equals(other.DependencyKey, DependencyKey);
		}

		public ParameterModel FindMatchingParameter(ComponentModel model)
		{
			if (model.HasParameters == false)
			{
				return null;
			}
			return ObtainParameterModelByKey(model) ?? ObtainParameterModelByType(model);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var result = (targetType != null ? targetType.GetHashCode() : 0);
				result = (result*397) ^ (DependencyKey != null ? DependencyKey.GetHashCode() : 0);
				return result;
			}
		}

		/// <summary>
		///   Returns a <see cref = "T:System.String" /> that represents the current <see cref = "T:System.Object" />.
		/// </summary>
		/// <returns>
		///   A <see cref = "T:System.String" /> that represents the current <see cref = "T:System.Object" />.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Dependency '{0}' type '{1}'", DependencyKey, TargetType);
		}

		private ParameterModel GetParameterModelByType(Type type, ComponentModel model)
		{
			if (type == null)
			{
				return null;
			}

			var key = type.AssemblyQualifiedName;
			if (key == null)
			{
				return null;
			}

			return model.Parameters[key];
		}

		private ParameterModel ObtainParameterModelByKey(ComponentModel model)
		{
			var key = DependencyKey;
			if (key == null)
			{
				return null;
			}

			return model.Parameters[key];
		}

		private ParameterModel ObtainParameterModelByType(ComponentModel model)
		{
			var type = targetItemType;
			if (type == null)
			{
				// for example it's an interceptor
				return null;
			}
			var parameter = GetParameterModelByType(type, model);
			if (parameter == null && type.IsGenericType)
			{
				parameter = GetParameterModelByType(type.GetGenericTypeDefinition(), model);
			}
			return parameter;
		}
	}
}