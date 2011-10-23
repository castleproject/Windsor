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
	using System.Linq;

	using Castle.Core.Configuration;

	public class NullableConverter:AbstractTypeConverter
	{
		private ITypeConverter innerConverter;

		public NullableConverter(ITypeConverter innerConverter)
		{
			this.innerConverter = innerConverter;
		}

		public override bool CanHandleType(Type type)
		{
			return type.IsGenericType &&
			       type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) &&
			       innerConverter.CanHandleType(GetInnerType(type));
		}

		private Type GetInnerType(Type type)
		{
			return type.GetGenericArguments().Single();
		}

		public override object PerformConversion(string value, Type targetType)
		{
			if(string.IsNullOrEmpty(value))
			{
				return null;
			}
			return innerConverter.PerformConversion(value, GetInnerType(targetType));
		}

		public override object PerformConversion(IConfiguration configuration, Type targetType)
		{
			return PerformConversion(configuration.Value, targetType);
		}
	}
#endif
}