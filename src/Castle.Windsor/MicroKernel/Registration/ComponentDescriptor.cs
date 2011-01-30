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
	using System;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel.ModelBuilder;

	public abstract class ComponentDescriptor<TService> : IComponentModelDescriptor
		where TService : class
	{
		protected bool IsOverWrite
		{
			get { return Registration.IsOverWrite; }
		}

		internal ComponentRegistration<TService> Registration { private get; set; }

		public virtual void BuildComponentModel(IKernel kernel, ComponentModel model)
		{
			ApplyToConfiguration(kernel, model.Configuration);
		}

		public virtual void ConfigureComponentModel(IKernel kernel, ComponentModel model)
		{
			ApplyToModel(kernel, model);
		}

		protected internal virtual void ApplyToConfiguration(IKernel kernel, IConfiguration configuration)
		{
		}

		protected internal virtual void ApplyToModel(IKernel kernel, ComponentModel model)
		{
		}

		protected void AddParameter(ComponentModel model, String key, IConfiguration value)
		{
			var parameters = EnsureParametersConfiguration(model);
			var parameter = new MutableConfiguration(key);
			parameter.Children.Add(value);
			parameters.Children.Add(parameter);
		}

		internal void AddParameter(ComponentModel model, String key, String value)
		{
			var parameters = EnsureParametersConfiguration(model);
			var parameter = new MutableConfiguration(key, value);
			parameters.Children.Add(parameter);
		}

		private IConfiguration EnsureParametersConfiguration(ComponentModel model)
		{
			var parameters = model.Configuration.Children["parameters"];
			if (parameters == null)
			{
				parameters = new MutableConfiguration("parameters");
				model.Configuration.Children.Add(parameters);
			}
			return parameters;
		}
	}
}