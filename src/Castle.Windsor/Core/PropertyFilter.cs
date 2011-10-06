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

	public class PropertyFilter
	{
		private readonly Predicate<PropertyInfo> filter;

		public PropertyFilter(Predicate<PropertyInfo> filter, bool isRequired)
		{
			this.filter = filter;
			IsRequired = isRequired;
		}

		public bool IsRequired { get; private set; }

		public bool Matches(PropertyInfo property)
		{
			return filter(property);
		}

		public static ICollection<PropertyFilter> GetPropertyFilters(ComponentModel componentModel, bool createIfMissing)
		{
			var filters = (ICollection<PropertyFilter>)componentModel.ExtendedProperties[Constants.PropertyFilters];
			if (filters == null && createIfMissing)
			{
				filters = new List<PropertyFilter>(4);
				componentModel.ExtendedProperties[Constants.PropertyFilters] = filters;
			}
			return filters;
		}
	}
}