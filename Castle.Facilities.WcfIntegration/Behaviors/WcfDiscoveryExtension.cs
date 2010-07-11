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
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Discovery;
	using Castle.Facilities.WcfIntegration.Service;

	public class WcfDiscoveryExtension : AbstractServiceHostAware
	{
		private DiscoveryEndpoint _discoveryEndpoint;

		public WcfDiscoveryExtension AtEndpoint(DiscoveryEndpoint endpoint)
		{
			_discoveryEndpoint = endpoint;
			return this;
		}

		protected override void Opening(ServiceHost serviceHost)
		{
			var serviceDiscovery = serviceHost.Description.Behaviors.Find<ServiceDiscoveryBehavior>();
			if (serviceDiscovery == null)
			{
				serviceHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
			}
			AddDiscoveryEndpoint(serviceHost);
		}

		private void AddDiscoveryEndpoint(ServiceHost serviceHost)
		{
			if (serviceHost.Description.Endpoints.OfType<DiscoveryEndpoint>().Any() == false)
			{
				var discoveryEndpoint = _discoveryEndpoint ?? new UdpDiscoveryEndpoint();
				serviceHost.Description.Endpoints.Add(discoveryEndpoint);
			}
		}
	}
}
