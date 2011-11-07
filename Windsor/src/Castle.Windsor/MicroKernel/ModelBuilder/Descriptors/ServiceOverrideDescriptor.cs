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
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Util;

	public class ServiceOverrideDescriptor : AbstractPropertyDescriptor
	{
		private readonly object value;

		public ServiceOverrideDescriptor(params ServiceOverride[] overrides)
		{
			value = overrides;
		}

		public ServiceOverrideDescriptor(IDictionary dictionary)
		{
			value = dictionary;
		}

		public override void BuildComponentModel(IKernel kernel, ComponentModel model)
		{
			var dictionary = value as IDictionary;
			if (dictionary != null)
			{
				foreach (DictionaryEntry property in dictionary)
				{
					Apply(model, property.Key, property.Value, null);
				}
			}
			var overrides = value as ServiceOverride[];
			if (overrides != null)
			{
				Array.ForEach(overrides, o => Apply(model, o.DependencyKey, o.Value, o));
			}
		}

		private void Apply(ComponentModel model, Object dependencyKey, Object dependencyValue, ServiceOverride @override)
		{
			if (dependencyValue is String)
			{
				ApplySimpleReference(model, dependencyKey, (String)dependencyValue);
			}
			else if (dependencyValue is IEnumerable<String>)
			{
				ApplyReferenceList(model, dependencyKey, (IEnumerable<String>)dependencyValue, @override);
			}
			else if (dependencyValue is Type)
			{
				ApplySimpleReference(model, dependencyKey, ComponentName.DefaultNameFor((Type)dependencyValue));
			}
			else if (dependencyValue is IEnumerable<Type>)
			{
				ApplyReferenceList(model, dependencyKey, ((IEnumerable<Type>)dependencyValue).Select(ComponentName.DefaultNameFor), @override);
			}
		}

		private void ApplyReferenceList(ComponentModel model, object name, IEnumerable<String> items, ServiceOverride serviceOverride)
		{
			var list = new MutableConfiguration("list");

			if (serviceOverride != null && serviceOverride.Type != null)
			{
				list.Attributes.Add("type", serviceOverride.Type.AssemblyQualifiedName);
			}

			foreach (var item in items)
			{
				var reference = ReferenceExpressionUtil.BuildReference(item);
				list.Children.Add(new MutableConfiguration("item", reference));
			}

			AddParameter(model, GetNameString(name), list);
		}

		private void ApplySimpleReference(ComponentModel model, object dependencyName, String componentKey)
		{
			var reference = ReferenceExpressionUtil.BuildReference(componentKey);
			AddParameter(model, GetNameString(dependencyName), reference);
		}

		private string GetNameString(object key)
		{
			if (key is Type)
			{
				return ((Type)key).AssemblyQualifiedName;
			}

			return key.ToString();
		}
	}
}