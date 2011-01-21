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

namespace Castle.MicroKernel.Registration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel.Util;

    public class ServiceOverrideDescriptor<S> : AbstractPropertyDescriptor<S>
        where S : class 
	{
		public ServiceOverrideDescriptor(params ServiceOverride[] overrides)
			: base(overrides)
		{
		}

		public ServiceOverrideDescriptor(IDictionary dictionary)
			: base(dictionary)
		{
		}


		public ServiceOverrideDescriptor(object overridesAsAnonymousType)
			: base(new ReflectionBasedDictionaryAdapter(overridesAsAnonymousType))
		{
		}

		protected override void ApplyProperty(IKernel kernel, ComponentModel model, object key, object value,
		                                      Property property)
		{
			if (value is string)
			{
				ApplySimpleReference(kernel, model, key, (String)value);
			}
			else if (value is IEnumerable<String>)
			{
				var serviceOverride = (ServiceOverride)property;
				ApplyReferenceList(kernel, model, key, (IEnumerable<String>)value, serviceOverride);
			}
		}

		private void ApplySimpleReference(IKernel kernel, ComponentModel model,
		                                  object key, String componentKey)
		{
			var reference = FormattedReferenceExpression(componentKey);
			Registration.AddParameter(kernel, model, GetKeyString(key), reference);
		}

		private string GetKeyString(object key)
		{
			if ((key is Type))
				return (key as Type).AssemblyQualifiedName;

			return key.ToString();
		}

		private void ApplyReferenceList(IKernel kernel, ComponentModel model,
		                                object key, IEnumerable<String> items,
		                                ServiceOverride serviceOverride)
		{
			var list = new MutableConfiguration("list");

			if (serviceOverride != null && serviceOverride.Type != null)
				list.Attributes.Add("type", serviceOverride.Type.AssemblyQualifiedName);

			foreach (var item in items)
			{
				var reference = FormattedReferenceExpression(item);
				var node = new MutableConfiguration("item", reference);
				list.Children.Add(node);
			}

			Registration.AddParameter(kernel, model, GetKeyString(key), list);
		}

		private static String FormattedReferenceExpression(String value)
		{
			if (!ReferenceExpressionUtil.IsReference(value))
				value = String.Format("${{{0}}}", value);
			return value;
		}
	}
}