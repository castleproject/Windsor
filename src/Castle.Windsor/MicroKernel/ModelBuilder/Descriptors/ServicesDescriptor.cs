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

	public class ServicesDescriptor : IComponentModelDescriptor
	{
		private readonly Type[] services;

		public ServicesDescriptor(Type[] services)
		{
			this.services = services;
		}

		public void BuildComponentModel(IKernel kernel, ComponentModel model)
		{
			Array.ForEach(services, model.AddService);
		}

		public void ConfigureComponentModel(IKernel kernel, ComponentModel model)
		{
		}
	}
}