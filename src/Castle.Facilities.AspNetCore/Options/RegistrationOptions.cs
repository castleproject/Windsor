// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.AspNetCore.Options
{
	using System;

	using Castle.Windsor;

	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;

	public class RegistrationOptions
	{

		private readonly IServiceCollection services;
		private readonly IWindsorContainer container;

		public RegistrationOptions(IServiceCollection services, IWindsorContainer container)
		{
			this.services = services;
			this.container = container;
		}

		public void AddHttpContextAccessor()
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		}

		public void AddContainerResolvedScoped<T>() where T : class
		{
			services.AddScoped(provider => container.Resolve<T>());
		}

		public void AddContainerResolvedSingleton<T>() where T : class
		{
			services.AddSingleton(provider => container.Resolve<T>());
		}

		public void AddContainerResolvedTransient<T>() where T : class
		{
			services.AddTransient (provider => container.Resolve<T>());
		}
	}
}