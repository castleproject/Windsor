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

namespace Castle.Core
{
	using System;

	/// <summary>
	/// This attribute is useful only when you want to register all components
	/// on an assembly as a batch process. 
	/// By doing so, the batch register will look 
	/// for this attribute to distinguish components from other classes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CastleComponentAttribute : LifestyleAttribute
	{
		public CastleComponentAttribute(String key) : this(key, null)
		{
		}

		public CastleComponentAttribute(Type service)
			: this(null, service)
		{
		}
		public CastleComponentAttribute(String key, Type service) : this(key, service, LifestyleType.Undefined)
		{
		}

		public CastleComponentAttribute(String key, Type service, LifestyleType lifestyle) : base(lifestyle)
		{
			Key = key;
			Service = service;
		}

		public String Key { get; private set; }

		public Type Service { get; private set; }

		public static CastleComponentAttribute GetDefaultsFor(Type type)
		{
			var attribute = (CastleComponentAttribute)GetCustomAttribute(type, typeof(CastleComponentAttribute));
			if (attribute != null)
			{
				if(attribute.Service == null)
				{
					attribute.Service = type;
				}
				return attribute;
			}
			return new CastleComponentAttribute(type);
		}
	}
}