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
	using System;
	using System.Diagnostics;

	using Castle.Core.Configuration;

	[Serializable]
	public class ArrayConverter : AbstractTypeConverter
	{
		public override bool CanHandleType(Type type)
		{
			return type.IsArray;
		}

		public override object PerformConversion(String value, Type targetType)
		{
			throw new NotImplementedException();
		}

		public override object PerformConversion(IConfiguration configuration, Type targetType)
		{
			Debug.Assert(targetType.IsArray);
			var count = configuration.Children.Count;
			var itemType = targetType.GetElementType();

			var array = Array.CreateInstance(itemType, count);

			var index = 0;
			foreach (var itemConfig in configuration.Children)
			{
				var value = Context.Composition.PerformConversion(itemConfig, itemType);
				array.SetValue(value, index++);
			}

			return array;
		}
	}
}