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
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Reflection;

	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Util;

	[Serializable]
	public class GenericListConverter : AbstractTypeConverter
	{
		public override bool CanHandleType(Type type)
		{
			if (!type.GetTypeInfo().IsGenericType)
			{
				return false;
			}

			var genericDef = type.GetGenericTypeDefinition();

			return (genericDef == typeof(IList<>)
			        || genericDef == typeof(ICollection<>)
			        || genericDef == typeof(List<>)
			        || genericDef == typeof(IEnumerable<>));
		}

		public override object PerformConversion(String value, Type targetType)
		{
			if (ReferenceExpressionUtil.IsReference(value))
			{
				string newValue = ReferenceExpressionUtil.ExtractComponentName(value);
				var handler = Context.Kernel.LoadHandlerByName(newValue, targetType, null);
				if (handler == null)
				{
					throw new ConverterException(string.Format("Component '{0}' was not found in the container.", newValue));
				}

				return handler.Resolve(Context.CurrentCreationContext ?? CreationContext.CreateEmpty());
			}
			throw new NotImplementedException();
		}

		public override object PerformConversion(IConfiguration configuration, Type targetType)
		{
			Debug.Assert(CanHandleType(targetType));

			var argTypes = targetType.GetGenericArguments();

			if (argTypes.Length != 1)
			{
				throw new ConverterException("Expected type with one generic argument.");
			}

			var itemType = configuration.Attributes["type"];
			var convertTo = argTypes[0];

			if (itemType != null)
			{
				convertTo = Context.Composition.PerformConversion<Type>(itemType);
			}

			var helperType = typeof(ListHelper<>).MakeGenericType(convertTo);
			var converterHelper = helperType.CreateInstance<IGenericCollectionConverterHelper>(this);
			return converterHelper.ConvertConfigurationToCollection(configuration);
		}

		private class ListHelper<T> : IGenericCollectionConverterHelper
		{
			private readonly GenericListConverter parent;

			public ListHelper(GenericListConverter parent)
			{
				this.parent = parent;
			}

			public object ConvertConfigurationToCollection(IConfiguration configuration)
			{
				var list = new List<T>();
				foreach (var itemConfig in configuration.Children)
				{
					var item = parent.Context.Composition.PerformConversion<T>(itemConfig);
					list.Add(item);
				}

				return list;
			}
		}
	}
}