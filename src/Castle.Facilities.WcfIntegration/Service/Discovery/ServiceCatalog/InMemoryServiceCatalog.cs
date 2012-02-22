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

namespace Castle.Facilities.WcfIntegration
{
#if DOTNET40
	using System.Collections.Concurrent;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Discovery;

    public class InMemoryServiceCatalog : IServiceCatalogImplementation
    {
        private readonly ConcurrentDictionary<EndpointAddress, EndpointDiscoveryMetadata> endpoints;
		
		public InMemoryServiceCatalog()
        {
			endpoints = new ConcurrentDictionary<EndpointAddress, EndpointDiscoveryMetadata>();
        }

		public virtual EndpointDiscoveryMetadata[] ListEndpoints()
		{
			return endpoints.Values.ToArray<EndpointDiscoveryMetadata>();
		}

        public virtual void FindEndpoint(FindRequestContext findRequestContext)
        {
            foreach (var metadata in endpoints.Values)
            {
                if (findRequestContext.Criteria.IsMatch(metadata))
                {
                    findRequestContext.AddMatchingEndpoint(metadata);
                }
            }
        }

        public virtual EndpointDiscoveryMetadata[] FindEndpoints(FindCriteria criteria)
        {
			return endpoints.Values.Where(criteria.IsMatch).ToArray();
        }

        public virtual void RegisterEndpoint(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            if (AcceptEndpoint(endpointDiscoveryMetadata))
            {
				endpoints.AddOrUpdate(endpointDiscoveryMetadata.Address, endpointDiscoveryMetadata,
					(address, existing) => endpointDiscoveryMetadata);
            }
        }

        public virtual bool RemoveEndpoint(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            EndpointDiscoveryMetadata endpoint;
            return endpoints.TryRemove(endpointDiscoveryMetadata.Address, out endpoint);
        }

        public virtual EndpointDiscoveryMetadata ResolveEndpoint(ResolveCriteria resolveCriteria)
        {
			foreach (var metadata in endpoints.Values)
            {
                if (resolveCriteria.Address == metadata.Address)
                {
                    return metadata;
                }
            }
            return null;
        }

		protected virtual bool AcceptEndpoint(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
		{
			return true;
		}
    }
#endif
}

