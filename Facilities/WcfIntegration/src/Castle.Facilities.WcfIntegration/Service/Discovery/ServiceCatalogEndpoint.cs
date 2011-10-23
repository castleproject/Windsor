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
// See the License for the specifi

namespace Castle.Facilities.WcfIntegration
{
#if DOTNET40
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Discovery;

	public class ServiceCatalogEndpoint
	{
		public Uri[] Scopes { get; set; }
		
		public Binding Binding { get; set; }

		public TimeSpan? DiscoveryDuration { get; set; }

		public DiscoveryVersion DiscoveryVersion { get; set; }

		public UdpDiscoveryEndpoint UdpDiscoveryEndpoint { get; set; }

		public DiscoveryEndpoint Discover(bool required = true)
		{
			if (DiscoveryVersion == null)
			{
				return DiscoverEndpoint(new DiscoveryEndpoint(), required);
			}
			return DiscoverEndpoint(new DiscoveryEndpoint(DiscoveryVersion, ServiceDiscoveryMode.Managed), required);
		}

		public static implicit operator DiscoveryEndpoint(ServiceCatalogEndpoint endpoint)
		{
			return endpoint.Discover();
		}

		private DiscoveryEndpoint DiscoverEndpoint(DiscoveryEndpoint endpoint, bool required)
		{
			using (var discover = new DiscoveryClient(UdpDiscoveryEndpoint ?? new UdpDiscoveryEndpoint()))
			{
				var criteria = new FindCriteria(endpoint.Contract.ContractType) { MaxResults = 1 };
				if (DiscoveryDuration.HasValue)
				{
					criteria.Duration = DiscoveryDuration.Value;
				}

				var discovered = discover.Find(criteria);
				if (discovered.Endpoints.Count > 0)
				{
					var endpointMetadata = discovered.Endpoints[0];
					var binding = Binding ?? AbstractChannelBuilder.GetBindingFromMetadata(endpointMetadata);
					return new DiscoveryEndpoint(binding, endpointMetadata.Address);
				}

				if (required)
				{
					throw new EndpointNotFoundException("Unable to locate a ServiceCatalog on the network.");
				}

				return null;
			}
		}
	}
#endif
}
