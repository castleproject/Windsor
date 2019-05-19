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

namespace Castle.Facilities.AspNetCore
{
	using System;

	using Castle.Facilities.AspNetCore.Contributors;
	using Castle.Facilities.NetCore;
	using Castle.MicroKernel.Facilities;
	using Castle.Windsor;

	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.DependencyInjection;

	public class AspNetCoreFacility : NetCoreFacility
	{
		internal const string IsRegisteredAsMiddlewareIntoApplicationBuilderKey = "windsor-registration-is-also-registered-as-middleware";

		private MiddlewareComponentModelContributor middlewareComponentModelContributor;

		protected override void Init()
		{
			base.Init();
		}



		/// <summary>
		/// Registers Windsor `aware` <see cref="IMiddleware"/> into the <see cref="IApplicationBuilder"/> via the <see cref="WindsorRegistrationExtensions.AsMiddleware"/> component registration extension
		/// </summary>
		/// <param name="applicationBuilder"><see cref="IApplicationBuilder"/></param>
		public void RegistersMiddlewareInto(IApplicationBuilder applicationBuilder)
		{
			middlewareComponentModelContributor = new MiddlewareComponentModelContributor(crossWiringComponentModelContributor.Services, applicationBuilder);
			Kernel.ComponentModelBuilder.AddContributor(middlewareComponentModelContributor); // Happens after Init() in Startup.Configure(IApplicationBuilder, ...)
		}
	}
}