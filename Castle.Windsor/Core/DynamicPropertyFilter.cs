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
	using System.Reflection;

	using Castle.Core.Internal;

	public class DynamicPropertyFilter
	{
		private readonly Func<ComponentModel, PropertyInfo, bool> filter;

		public DynamicPropertyFilter(Func<ComponentModel, PropertyInfo, bool> filter, bool isRequired)
		{
			this.filter = filter;
			IsRequired = isRequired;
		}

		public bool IsRequired { get; private set; }

		public bool Matches(ComponentModel component, PropertyInfo property)
		{
			return filter(component, property);
		}

		public static DynamicPropertyFilter Create(PropertyFilter filter)
		{
			switch (filter)
			{
				case PropertyFilter.Default:
					return Default();
				case PropertyFilter.IgnoreBase:
					return IgnoreBase();
				case PropertyFilter.RequireAll:
					return RequireAll();
				case PropertyFilter.RequireBase:
					return RequireBase();
				default:
					throw new ArgumentOutOfRangeException(
						string.Format(
							"The value {0} does not translate to a valid property filter. This is most likely a bug in the calling code.",
							filter));
			}
		}

		public static DynamicPropertyFilter Default()
		{
			return new DynamicPropertyFilter(delegate { return true; }, false);
		}

		public static ICollection<DynamicPropertyFilter> GetPropertyFilters(ComponentModel componentModel,
		                                                                    bool createIfMissing)
		{
			var filters = (ICollection<DynamicPropertyFilter>)componentModel.ExtendedProperties[Constants.PropertyFilters];
			if (filters == null && createIfMissing)
			{
				filters = new List<DynamicPropertyFilter>(4);
				componentModel.ExtendedProperties[Constants.PropertyFilters] = filters;
			}
			return filters;
		}

		private static DynamicPropertyFilter IgnoreBase()
		{
			return new DynamicPropertyFilter((m, p) => m.Implementation == p.DeclaringType, false);
		}

		private static DynamicPropertyFilter RequireAll()
		{
			return new DynamicPropertyFilter(delegate { return true; }, true);
		}

		private static DynamicPropertyFilter RequireBase()
		{
			return new DynamicPropertyFilter((m, p) => m.Implementation != p.DeclaringType, true);
		}
	}
}