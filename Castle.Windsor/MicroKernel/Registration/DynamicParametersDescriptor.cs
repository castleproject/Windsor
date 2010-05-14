// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	{
		private readonly DynamicParametersWithContextResolveDelegate resolve;
		private static readonly string key = "component_resolving_handler";

		public DynamicParametersDescriptor(DynamicParametersDelegate resolve)
			: this((k, c, d) => { resolve(k, d); return null; })
		{
		}

		public DynamicParametersDescriptor(DynamicParametersWithContextResolveDelegate resolve)
		{
			this.resolve = resolve;
		}

		public DynamicParametersDescriptor(DynamicParametersResolveDelegate resolve)
			: this((k, c, d) => resolve(k, d))
		{
		}

		protected internal override void ApplyToModel(IKernel kernel, ComponentModel model)
		{
			ComponentResolvingDelegate handler = (k, c) => resolve(k, c, c.AdditionalParameters);
			if (model.ExtendedProperties.Contains(key) == false)
			{
				model.ExtendedProperties[key] = handler;
				return;
			}

			var @delegate = (ComponentResolvingDelegate)model.ExtendedProperties[key];
			@delegate += handler;

			model.ExtendedProperties[key] = @delegate;
		}
	}
}