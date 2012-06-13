// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
#if DOTNET40
	using System;
	using System.ServiceModel.Discovery;
	using Castle.Facilities.WcfIntegration.Internal;

	public class DiscoveryProxyLocator : DiscoveryEndpointProvider
	{
		private static readonly TimeSpan DefaultDuration = new TimeSpan(0, 0, 10);

		public string Domain { get; set; }

		public TimeSpan? Duration { get; set; }

		public DiscoveryVersion DiscoveryVersion { get; set; }

		public UdpDiscoveryEndpoint UdpDiscoveryEndpoint { get; set; }

		public override DiscoveryEndpoint GetDiscoveryEndpoint()
		{
			var forContract = DiscoveryVersion != null
				? new DiscoveryEndpoint(DiscoveryVersion, ServiceDiscoveryMode.Managed)
				: new DiscoveryEndpoint();

			using (var discover = new DiscoveryClient(GetUdpDiscoveryEndpoint()))
			{
				var criteria = GetSearchCriteria(forContract.Contract.ContractType);
				RestrictDomain(criteria);

				var discovered = discover.Find(criteria);
				if (discovered.Endpoints.Count > 0)
				{
					var endpointMetadata = discovered.Endpoints[0];
					var binding = AbstractChannelBuilder.GetBindingFromMetadata(endpointMetadata);
					return new DiscoveryEndpoint(binding, endpointMetadata.Address);
				}
			}

			return null;
		}

		protected virtual FindCriteria GetSearchCriteria(Type contractType)
		{
			return new FindCriteria(contractType) 
			{
				Duration = Duration.GetValueOrDefault(DefaultDuration),
				MaxResults = 1
			};
		}

		protected virtual UdpDiscoveryEndpoint GetUdpDiscoveryEndpoint()
		{
			return UdpDiscoveryEndpoint ?? new UdpDiscoveryEndpoint();
		}

		private void RestrictDomain(FindCriteria criteria)
		{
			if (string.IsNullOrEmpty(Domain) == false)
			{
				var domain = new WcfDiscoveryDomain(Domain);
				criteria.Scopes.AddAll(domain.Scopes);
				criteria.Extensions.AddAll(domain.Extensions);
			}
		}
	}
#endif
}
