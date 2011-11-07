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

namespace Castle.Facilities.WcfIntegration.Data
{
	using System;
	using System.ServiceModel;

	using Castle.Core;
	using Castle.MicroKernel;

	public class DataServiceHostBuilder : AbstractServiceHostBuilder<DataServiceModel>
	{
		public DataServiceHostBuilder(IKernel kernel) : base(kernel)
		{
		}

		protected override ServiceHost CreateServiceHost(ComponentModel model, DataServiceModel serviceModel, params Uri[] baseAddresses)
		{
			return CreateServiceHost(model.Implementation, GetEffectiveBaseAddresses(serviceModel, baseAddresses));
		}

		protected override ServiceHost CreateServiceHost(ComponentModel model, Uri[] baseAddresses)
		{
			return CreateServiceHost(model.Implementation, baseAddresses);
		}

		protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			return new DataServiceHost(serviceType, baseAddresses);
		}
	}
}