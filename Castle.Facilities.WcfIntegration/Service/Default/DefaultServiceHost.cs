// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

	public class DefaultServiceHost : ServiceHost, IWcfServiceHost
	{
		private readonly ComponentModel model;

		public event EventHandler<EndpointCreatedArgs> EndpointCreated;
        
		public DefaultServiceHost(ComponentModel model, params Uri[] baseAddresses)
			: base(model.Implementation, baseAddresses)
		{
			this.model = model;
		}

		public DefaultServiceHost(Type serviceType, params Uri[] baseAddresses)
			: base(serviceType, baseAddresses)
		{
		}

		protected override void OnOpening()
		{
			base.OnOpening();

			AddDefaultEndpointIfNoneFound();
		}

		private void AddDefaultEndpointIfNoneFound()
		{
			if (Description != null && 
				Description.Endpoints.Where(endpoint => endpoint.IsSystemEndpoint == false)
				.Any() == false)
			{
				foreach (var endpoint in AddDefaultEndpoints())
				{
					if (EndpointCreated != null)
					{
						EndpointCreated(this, new EndpointCreatedArgs(endpoint));
					}
				}
			}
		}
	}
}
