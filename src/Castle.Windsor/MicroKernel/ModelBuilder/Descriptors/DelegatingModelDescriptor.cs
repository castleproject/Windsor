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

	using Castle.Core;

	public class DelegatingModelDescriptor : IComponentModelDescriptor
	{
		private readonly Action<IKernel, ComponentModel> builder;
		private readonly Action<IKernel, ComponentModel> configurer;

		public DelegatingModelDescriptor(Action<IKernel, ComponentModel> builder = null, Action<IKernel, ComponentModel> configurer = null)
		{
			this.builder = builder;
			this.configurer = configurer;
		}

		public void BuildComponentModel(IKernel kernel, ComponentModel model)
		{
			if (builder == null)
			{
				return;
			}
			builder(kernel, model);
		}

		public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
		{
			if (configurer == null)
			{
				return;
			}
			configurer(kernel, model);
		}
	}
}