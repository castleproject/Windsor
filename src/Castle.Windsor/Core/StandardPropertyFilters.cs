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
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Castle.Core.Internal;

	public class StandardPropertyFilters
	{
		public static PropertyDependencyFilter Create(PropertyFilter filter)
		{
			switch (filter)
			{
				case PropertyFilter.Default:
					return Default;
				case PropertyFilter.IgnoreAll:
					return IgnoreAll;
				case PropertyFilter.IgnoreBase:
					return IgnoreBase;
				case PropertyFilter.RequireAll:
					return RequireAll;
				case PropertyFilter.RequireBase:
					return RequireBase;
				default:
					throw new ArgumentOutOfRangeException(
						string.Format(
							"The value {0} does not translate to a valid property filter. This is most likely a bug in the calling code.",
							filter));
			}
		}

		public static PropertySet[] Default(ComponentModel model, PropertyInfo[] properties, PropertySetBuilder propertySetBuilder)
		{
			return properties.Select(p => propertySetBuilder(p, true)).ToArray();
		}

		public static PropertyDependencyFilter FromFunction(Func<ComponentModel, PropertyInfo, bool> filter, bool isRequired)
		{
			return (model, properties, callback) => properties.Where(p => filter(model, p))
			                                        	.Select(p => callback(p, isRequired == false))
			                                        	.ToArray();
		}

		public static ICollection<PropertyDependencyFilter> GetPropertyFilters(ComponentModel componentModel, bool createIfMissing)
		{
			var filters = (ICollection<PropertyDependencyFilter>)componentModel.ExtendedProperties[Constants.PropertyFilters];
			if (filters == null && createIfMissing)
			{
				filters = new List<PropertyDependencyFilter>(4);
				componentModel.ExtendedProperties[Constants.PropertyFilters] = filters;
			}
			return filters;
		}

		public static PropertySet[] IgnoreAll(ComponentModel model, PropertyInfo[] properties,
		                                      PropertySetBuilder propertySetBuilder)
		{
			return new PropertySet[0];
		}

		public static PropertySet[] IgnoreBase(ComponentModel model, PropertyInfo[] properties,
		                                       PropertySetBuilder propertySetBuilder)
		{
			return properties.Where(p => p.DeclaringType == model.Implementation)
				.Select(p => propertySetBuilder(p, true)).ToArray();
		}

		public static PropertySet[] RequireAll(ComponentModel model, PropertyInfo[] properties,
		                                       PropertySetBuilder propertySetBuilder)
		{
			return properties.Select(p => propertySetBuilder(p, false)).ToArray();
		}

		public static PropertySet[] RequireBase(ComponentModel model, PropertyInfo[] properties,
		                                        PropertySetBuilder propertySetBuilder)
		{
			return properties.Where(p => p.DeclaringType != model.Implementation)
				.Select(p => propertySetBuilder(p, false)).ToArray();
		}
	}
}