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

namespace Castle.MicroKernel.Registration
{
	using System.Collections;

	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;

	public delegate void DynamicParametersDelegate(IKernel kernel, IDictionary parameters);

	public delegate ComponentReleasingDelegate DynamicParametersWithContextResolveDelegate(
		IKernel kernel, CreationContext creationContext, IDictionary parameters);

	public delegate ComponentReleasingDelegate DynamicParametersResolveDelegate(IKernel kernel, IDictionary parameters);

	public class DynamicParametersDescriptor<S> : ComponentDescriptor<S>
		where S : class
	{
		private static readonly string key = "component_resolving_handler";
		private readonly DynamicParametersWithContextResolveDelegate resolve;

		public DynamicParametersDescriptor(DynamicParametersWithContextResolveDelegate resolve)
		{
			this.resolve = resolve;
		}

		protected internal override void ApplyToModel(IKernel kernel, ComponentModel model)
		{
			var dynamicParameters = GetDynamicParametersExtension(model);
			dynamicParameters.AddHandler((k, c) => resolve(k, c, c.AdditionalArguments));
		}

		private ComponentLifecycleExtension GetDynamicParametersExtension(ComponentModel model)
		{
			if (model.ExtendedProperties.Contains(key))
			{
				return (ComponentLifecycleExtension)model.ExtendedProperties[key];
			}

			var dynamicParameters = new ComponentLifecycleExtension();
			model.ExtendedProperties[key] = dynamicParameters;
			model.ResolveExtensions(true).Add(dynamicParameters);
			return dynamicParameters;
		}
	}
}