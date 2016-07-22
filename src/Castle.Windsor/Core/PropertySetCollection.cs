// Copyright 2004-2013 Castle Project - http://www.castleproject.org/
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
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Castle.Core.Internal;

	/// <summary>
	///     Collection of <see cref = "PropertySet" />
	/// </summary>
	[Serializable]
	public class PropertySetCollection : IMutableCollection<PropertySet>
	{
		private readonly HashSet<PropertySet> properties = new HashSet<PropertySet>();

		public int Count
		{
			get { return properties.Count; }
		}

		/// <summary>Finds a PropertySet the by PropertyInfo.</summary>
		/// <param name = "info">The info.</param>
		/// <returns></returns>
		public PropertySet FindByPropertyInfo(PropertyInfo info)
		{
			return this.FirstOrDefault(prop => info == prop.Property);
		}

		public IEnumerator<PropertySet> GetEnumerator()
		{
			return properties.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return properties.GetEnumerator();
		}

		void IMutableCollection<PropertySet>.Add(PropertySet property)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			properties.Add(property);
		}

		bool IMutableCollection<PropertySet>.Remove(PropertySet item)
		{
			return properties.Remove(item);
		}
	}
}