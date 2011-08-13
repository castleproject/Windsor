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
        private readonly ConcurrentDictionary<EndpointAddress, EndpointDiscoveryMetadata> services;
		
		public InMemoryServiceCatalog()
        {
			services = new ConcurrentDictionary<EndpointAddress, EndpointDiscoveryMetadata>();
        }

		public virtual EndpointDiscoveryMetadata[] ListServices()
		{
			return services.Values.ToArray<EndpointDiscoveryMetadata>();
		}

        public virtual void FindService(FindRequestContext findRequestContext)
        {
            foreach (var metadata in services.Values)
            {
                if (findRequestContext.Criteria.IsMatch(metadata))
                {
                    findRequestContext.AddMatchingEndpoint(metadata);
                }
            }
        }

        public virtual EndpointDiscoveryMetadata[] FindServices(FindCriteria criteria)
        {
			return services.Values.Where(criteria.IsMatch).ToArray();
        }

        public virtual void RegisterService(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            if (AcceptService(endpointDiscoveryMetadata))
            {
				services.AddOrUpdate(endpointDiscoveryMetadata.Address, endpointDiscoveryMetadata,
					(address, existing) => endpointDiscoveryMetadata);
            }
        }

        public virtual bool RemoveService(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            EndpointDiscoveryMetadata serviceBehavior;
            return services.TryRemove(endpointDiscoveryMetadata.Address, out serviceBehavior);
        }

        public virtual EndpointDiscoveryMetadata ResolveService(ResolveCriteria resolveCriteria)
        {
			foreach (var metadata in services.Values)
            {
                if (resolveCriteria.Address == metadata.Address)
                {
                    return metadata;
                }
            }
            return null;
        }

		protected virtual bool AcceptService(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
		{
			return true;
		}
    }
#endif
}

