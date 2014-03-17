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

namespace Castle.MicroKernel.SubSystems.Conversion
{
#if (SILVERLIGHT)
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;

	using Castle.Core.Internal;
	
	/// <summary>
	/// Our own minimalistic implementation of TypeDescriptor class, which does not exist in Silverlight.
	/// It exists solely to support functionality of <see cref="ComponentModelConverter"/> and does not provide
	/// full functionality of the actually thing from the full .NET framework.
	/// </summary>
	public class TypeDescriptor
	{
		private static readonly Lock @lock = Lock.Create();
		private static readonly IDictionary<Type,TypeConverter> convertersByType = new Dictionary<Type, TypeConverter>();


		public static TypeConverter GetConverter(Type type)
		{
			using(var holder = @lock.ForReadingUpgradeable())
			{
				TypeConverter converter;
				if (convertersByType.TryGetValue(type, out converter))
				{
					return converter;
				}

				holder.Upgrade();
				if (convertersByType.TryGetValue(type, out converter))
				{
					return converter;
				}

				var converterAttribute = Attribute.GetCustomAttribute(type, typeof(TypeConverterAttribute), false) as TypeConverterAttribute;
				if (converterAttribute == null)
				{
					return null;
				}
				var converterType = Type.GetType(converterAttribute.ConverterTypeName, false);
				if (converterType == null)
				{
					// this should really never happen, but just in case...
					return null;
				}
				try
				{
					converter = (TypeConverter)Activator.CreateInstance(converterType);
				}
				catch (Exception)
				{
					// bummer... looks like we're dealing with some fancy converter.
					// we should really get some logging in place here...
					return null;
				}
				convertersByType.Add(type, converter);
				return converter;
			}
		}
	}
#endif
}