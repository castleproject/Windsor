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

namespace Castle.Facilities.AspNetCore.Contributors
{
	using System;

	using Castle.MicroKernel.Lifestyle;
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ModelBuilder;

	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;

	public class MiddlewareComponentModelContributor : IContributeComponentModelConstruction
	{
		private IServiceProvider provider;
		private readonly IServiceCollection services;
		private readonly IApplicationBuilder applicationBuilder;

		public MiddlewareComponentModelContributor(IServiceCollection services, IApplicationBuilder applicationBuilder)
		{
			this.services = services ?? throw new ArgumentNullException(nameof(services));
			this.applicationBuilder = applicationBuilder ?? throw new InvalidOperationException("Please call `Container.GetFacility<AspNetCoreFacility>(f => f.RegistersMiddlewareInto(applicationBuilder));` first. This should happen before any middleware registration. Please see https://github.com/castleproject/Windsor/blob/master/docs/aspnetcore-facility.md");
		}

		public void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (model.Configuration.Attributes.Get(AspNetCoreFacility.IsRegisteredAsMiddlewareIntoApplicationBuilderKey) == Boolean.TrueString)
			{
				foreach (var service in model.Services)
				{
					applicationBuilder.Use(async (context, next) =>
					{
						var windsorScope = kernel.BeginScope();
						var serviceProviderScope = (provider = provider ?? services.BuildServiceProvider()).CreateScope();
						try
						{
							var middleware = (IMiddleware) kernel.Resolve(service); 
							await middleware.InvokeAsync(context, async (ctx) => await next());
						}
						finally
						{
							serviceProviderScope.Dispose();
							windsorScope.Dispose();
						}
					});
				}
			}
		}
	}
}