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

namespace Castle.Facilities.NetCore
{
	using System;

	using Castle.Facilities.NetCore.Contributors;
	using Castle.MicroKernel.Facilities;
	using Castle.Windsor;


	using Microsoft.Extensions.DependencyInjection;

	public class NetCoreFacility : AbstractFacility
	{
		public const string IsCrossWiredIntoServiceCollectionKey = "windsor-registration-is-also-registered-in-service-collection";

		protected CrossWiringComponentModelContributor crossWiringComponentModelContributor;


		protected override void Init()
		{
			Kernel.ComponentModelBuilder.AddContributor(crossWiringComponentModelContributor ?? throw new InvalidOperationException("Please call `Container.AddFacility<NetCoreFacility>(f => f.CrossWiresInto(services));` first. This should happen before any cross wiring registration. Please see https://github.com/castleproject/Windsor/blob/master/docs/aspnetcore-facility.md"));
		}

		/// <summary>
		/// Installation of the <see cref="CrossWiringComponentModelContributor"/> for registering components in both the <see cref="IWindsorContainer"/> and the <see cref="IServiceCollection"/> via the <see cref="WindsorRegistrationExtensions.CrossWired"/> component registration extension
		/// </summary>
		/// <param name="services"><see cref="IServiceCollection"/></param>
		public void CrossWiresInto(IServiceCollection services)
		{
			crossWiringComponentModelContributor = new CrossWiringComponentModelContributor(services);
		}


	}
}