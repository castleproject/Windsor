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

	using Castle.Core.Configuration;
	using Castle.Core.Internal;

	[Serializable]
	public class GenericDictionaryConverter : AbstractTypeConverter
	{
		public override bool CanHandleType(Type type)
		{
			if (!type.IsGenericType)
			{
				return false;
			}

			var genericDef = type.GetGenericTypeDefinition();

			return (genericDef == typeof(IDictionary<,>) || genericDef == typeof(Dictionary<,>));
		}

		public override object PerformConversion(String value, Type targetType)
		{
			throw new NotImplementedException();
		}

		public override object PerformConversion(IConfiguration configuration, Type targetType)
		{
			Debug.Assert(CanHandleType(targetType), "Got a type we can't handle!");

			var argTypes = targetType.GetGenericArguments();

			if (argTypes.Length != 2)
			{
				throw new ConverterException("Expected type with two generic arguments.");
			}

			var keyTypeName = configuration.Attributes["keyType"];
			var defaultKeyType = argTypes[0];

			var valueTypeName = configuration.Attributes["valueType"];
			var defaultValueType = argTypes[1];

			if (keyTypeName != null)
			{
				defaultKeyType = Context.Composition.PerformConversion<Type>(keyTypeName);
			}

			if (valueTypeName != null)
			{
				defaultValueType = Context.Composition.PerformConversion<Type>(valueTypeName);
			}

			var helperType = typeof(DictionaryHelper<,>).MakeGenericType(defaultKeyType, defaultValueType);
			var collectionConverterHelper = helperType.CreateInstance<IGenericCollectionConverterHelper>(this);

			return collectionConverterHelper.ConvertConfigurationToCollection(configuration);
		}

		private class DictionaryHelper<TKey, TValue> : IGenericCollectionConverterHelper
		{
			private readonly GenericDictionaryConverter parent;

			public DictionaryHelper(GenericDictionaryConverter parent)
			{
				this.parent = parent;
			}

			public object ConvertConfigurationToCollection(IConfiguration configuration)
			{
				var dict = new Dictionary<TKey, TValue>();
				foreach (var itemConfig in configuration.Children)
				{
					// Preparing the key

					var keyValue = itemConfig.Attributes["key"];

					if (keyValue == null)
					{
						throw new ConverterException("You must provide a key for the dictionary entry");
					}

					var convertKeyTo = typeof(TKey);

					if (itemConfig.Attributes["keyType"] != null)
					{
						convertKeyTo = parent.Context.Composition.PerformConversion<Type>(itemConfig.Attributes["keyType"]);
					}

					if (convertKeyTo.Is<TKey>() == false)
					{
						throw new ArgumentException(
							string.Format("Could not create dictionary<{0},{1}> because {2} is not assignable to key type {0}", typeof(TKey),
							              typeof(TValue), convertKeyTo));
					}

					var key = (TKey)parent.Context.Composition.PerformConversion(keyValue, convertKeyTo);

					// Preparing the value

					var convertValueTo = typeof(TValue);

					if (itemConfig.Attributes["valueType"] != null)
					{
						convertValueTo = parent.Context.Composition.PerformConversion<Type>(itemConfig.Attributes["valueType"]);
					}

					if (convertValueTo.Is<TValue>() == false)
					{
						throw new ArgumentException(
							string.Format("Could not create dictionary<{0},{1}> because {2} is not assignable to value type {1}",
							              typeof(TKey), typeof(TValue), convertValueTo));
					}
					if (itemConfig.Children.Count == 1)
					{
						dict.Add(key, (TValue)parent.Context.Composition.PerformConversion(itemConfig.Children[0], convertValueTo));
					}
					else
					{
						dict.Add(key, (TValue)parent.Context.Composition.PerformConversion(itemConfig.Value, convertValueTo));
					}
				}
				return dict;
			}
		}
	}
}