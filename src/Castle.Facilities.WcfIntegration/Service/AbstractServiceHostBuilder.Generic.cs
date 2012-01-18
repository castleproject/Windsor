﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.Linq;
	using System.ServiceModel;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;

	public abstract class AbstractServiceHostBuilder<TServiceModel> : AbstractServiceHostBuilder, IServiceHostBuilder<TServiceModel>
		where TServiceModel : IWcfServiceModel
	{
		protected AbstractServiceHostBuilder(IKernel kernel)
			: base(kernel)
		{
		}

		protected abstract ServiceHost CreateServiceHost(ComponentModel model, TServiceModel serviceModel, params Uri[] baseAddresses);

		protected abstract ServiceHost CreateServiceHost(ComponentModel model, Uri[] baseAddresses);

		protected abstract ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses);

		public ServiceHost Build(ComponentModel model, params Uri[] baseAddresses)
		{
			var serviceHost = CreateServiceHost(model, baseAddresses);
			ConfigureServiceHost(serviceHost, null, model);
			return serviceHost;
		}

		public ServiceHost Build(Type serviceType, params Uri[] baseAddresses)
		{
			var serviceHost = CreateServiceHost(serviceType, baseAddresses);
			ConfigureServiceHost(serviceHost, null, null);
			return serviceHost;
		}

		public ServiceHost Build(ComponentModel model, TServiceModel serviceModel, params Uri[] baseAddresses)
		{
			ValidateServiceModelInternal(model, serviceModel);
			var serviceHost = CreateServiceHost(model, serviceModel, baseAddresses);
			ConfigureServiceHost(serviceHost, serviceModel, model);
			return serviceHost;
		}

		protected virtual void ValidateComponentModel(ComponentModel model)
		{
			if (model == null)
			{
				throw new FacilityException("No service endpoint contract can be implied from the component.");
			}
			if (model.Services.Count() != 1)
			{
				throw new FacilityException("The component {0} exposes {1} services. Currently only single-service components are supported by the facility.");
			}
		}

		protected virtual void ValidateServiceModel(ComponentModel model, TServiceModel serviceModel)
		{
		}

		private void ValidateServiceModelInternal(ComponentModel model, TServiceModel serviceModel)
		{
			ValidateComponentModel(model);
			ValidateServiceModel(model, serviceModel);

			var service = model.Services.Single();
			foreach (var endpoint in serviceModel.Endpoints)
			{
				var contract = endpoint.Contract;
				if (contract == null)
				{
					endpoint.Contract = service;
				}
			}
		}
	}
}