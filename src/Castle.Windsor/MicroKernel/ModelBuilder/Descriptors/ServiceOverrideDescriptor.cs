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

namespace Castle.MicroKernel.ModelBuilder.Descriptors
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Util;

	public class ServiceOverrideDescriptor : AbstractPropertyDescriptor
	{
		private readonly IDictionary dictionary;
		private readonly ServiceOverride[] overrides;

		public ServiceOverrideDescriptor(params ServiceOverride[] overrides)
		{
			this.overrides = overrides;
		}

		public ServiceOverrideDescriptor(IDictionary dictionary)
		{
			this.dictionary = dictionary;
		}

		public override void BuildComponentModel(IKernel kernel, ComponentModel model)
		{
			if (dictionary != null)
			{
				foreach (DictionaryEntry property in dictionary)
				{
					Apply(model, property.Key, property.Value, null);
				}
			}
			if (overrides != null)
			{
				Array.ForEach(overrides, o => Apply(model, o.Key, o.Value, o));
			}
		}

		private void Apply(ComponentModel model, object key, object value, ServiceOverride @override)
		{
			if (value is string)
			{
				ApplySimpleReference(model, key, (String)value);
			}
			else if (value is IEnumerable<String>)
			{
				ApplyReferenceList(model, key, (IEnumerable<String>)value, @override);
			}
		}

		private void ApplyReferenceList(ComponentModel model, object key, IEnumerable<String> items, ServiceOverride serviceOverride)
		{
			var list = new MutableConfiguration("list");

			if (serviceOverride != null && serviceOverride.Type != null)
			{
				list.Attributes.Add("type", serviceOverride.Type.AssemblyQualifiedName);
			}

			foreach (var item in items)
			{
				var reference = FormattedReferenceExpression(item);
				var node = new MutableConfiguration("item", reference);
				list.Children.Add(node);
			}

			AddParameter(model, GetKeyString(key), list);
		}

		private void ApplySimpleReference(ComponentModel model, object dependencyKey, String componentKey)
		{
			var reference = FormattedReferenceExpression(componentKey);
			AddParameter(model, GetKeyString(dependencyKey), reference);
		}

		private string GetKeyString(object key)
		{
			if (key is Type)
			{
				return ((Type)key).AssemblyQualifiedName;
			}

			return key.ToString();
		}

		private static String FormattedReferenceExpression(String value)
		{
			if (!ReferenceExpressionUtil.IsReference(value))
			{
				value = String.Format("${{{0}}}", value);
			}
			return value;
		}
	}
}