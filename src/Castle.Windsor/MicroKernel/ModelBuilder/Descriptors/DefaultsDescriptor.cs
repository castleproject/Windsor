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
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Core.Internal;

	public class DefaultsDescriptor : IComponentModelDescriptor
	{
		private readonly Type implementation;
		private readonly ComponentName name;

		public DefaultsDescriptor(ComponentName name, Type implementation)
		{
			this.name = name;
			this.implementation = implementation;
		}

		public void BuildComponentModel(IKernel kernel, ComponentModel model)
		{
			if (model.Implementation == null)
			{
				model.Implementation = implementation ?? FirstService(model);
			}

			EnsureComponentName(model);
			EnsureComponentConfiguration(kernel, model);
		}

		public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
		{
		}

		private void EnsureComponentConfiguration(IKernel kernel, ComponentModel model)
		{
			var configuration = kernel.ConfigurationStore.GetComponentConfiguration(model.Name);
			if (configuration == null)
			{
				configuration = new MutableConfiguration("component");
				kernel.ConfigurationStore.AddComponentConfiguration(model.Name, configuration);
			}
			if (model.Configuration == null)
			{
				model.Configuration = configuration;
			}
			return;
		}

		private void EnsureComponentName(ComponentModel model)
		{
			if (model.ComponentName != null)
			{
				return;
			}
			if (name != null)
			{
				model.ComponentName = name;
				return;
			}
			if (model.Implementation == typeof(LateBoundComponent))
			{
				model.ComponentName = new ComponentName("Late bound " + FirstService(model).FullName, false);
				return;
			}
			model.ComponentName = new ComponentName(model.Implementation.FullName, false);
		}

		private Type FirstService(ComponentModel model)
		{
			return model.Services.First();
		}
	}
}