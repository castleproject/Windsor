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
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Discovery;

	using Castle.Core;
	using Castle.Core.Internal;
	using System;

    public class InMemoryServiceCatalog : IServiceCatalogImplementation
    {
		private readonly List<ILoadBalancePolicy> policies;
		private readonly ILoadBalancePolicyFactory policyFactory;
        private readonly Dictionary<EndpointAddress, EndpointDiscoveryMetadata> endpoints;
		private readonly Lock @lock = Lock.Create();

		public InMemoryServiceCatalog()
			: this(new ContractLoadBalancePolicyFactory<RoundRobinPolicy>())
		{
		}

		public InMemoryServiceCatalog(ILoadBalancePolicyFactory policyFactory)
        {
			this.policyFactory = policyFactory;
            policies = new List<ILoadBalancePolicy>();
			endpoints = new Dictionary<EndpointAddress, EndpointDiscoveryMetadata>();
        }

		public virtual EndpointDiscoveryMetadata[] ListEndpoints()
		{
			using (@lock.ForReading())
			{
				return endpoints.Values.ToArray<EndpointDiscoveryMetadata>();
			}
		}

        public virtual void FindEndpoints(FindRequestContext findRequestContext)
        {
			using (@lock.ForReading())
			{
				foreach (var endpoint in MatchTargets(findRequestContext.Criteria))
				{
					findRequestContext.AddMatchingEndpoint(endpoint);
				}
			}
        }

        public virtual EndpointDiscoveryMetadata[] FindEndpoints(FindCriteria criteria)
        {
			using (@lock.ForReading())
			{
				return MatchTargets(criteria).ToArray();
			}
        }

        public virtual bool RegisterEndpoint(EndpointDiscoveryMetadata endpoint)
        {
			var registered = false;
            if (AcceptEndpoint(endpoint))
            {
				using (var locker = @lock.ForReadingUpgradeable())
				{
					policies.ForEach(policy => registered = registered | policy.RegisterTarget(endpoint));

					locker.Upgrade();

					if (registered == false)
					{
						var newPolicies = policyFactory.CreatePolicies(endpoint);
						Array.ForEach(newPolicies, newPolicy =>
						{
							registered = registered | newPolicy.RegisterTarget(endpoint);
							policies.Add(newPolicy);
						});
					}

					if (registered)
						endpoints[endpoint.Address] = endpoint;
				}
            }
			return registered;
        }

        public virtual bool RemoveEndpoint(EndpointDiscoveryMetadata endpoint)
        {
			var removed = false;
			using (var locker = @lock.ForReadingUpgradeable())
			{
				policies.ForEach(policy => removed = removed | policy.RemoveTarget(endpoint));

				if (removed)
				{
					locker.Upgrade();
					endpoints.Remove(endpoint.Address);
				}
			}
			return removed;
        }

        public virtual EndpointDiscoveryMetadata ResolveEndpoint(ResolveCriteria resolveCriteria)
        {
			using (@lock.ForReading())
			{
				EndpointDiscoveryMetadata endpoint;
				return endpoints.TryGetValue(resolveCriteria.Address, out endpoint)
					? endpoint
					: null;
			}
        }

		protected virtual bool AcceptEndpoint(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
		{
			return true;
		}

		private IEnumerable<EndpointDiscoveryMetadata> MatchTargets(FindCriteria criteria)
		{
			var collected = new HashSet<EndpointDiscoveryMetadata>(ReferenceEqualityComparer<EndpointDiscoveryMetadata>.Instance);

			foreach (var policy in policies)
			{
				while (collected.Count < criteria.MaxResults)
				{
					var target = policy.ChooseTarget(criteria);
					if (target == null || collected.Add(target) == false)
						break;
				}

				if (collected.Count >= criteria.MaxResults)
					break;
			}

			return collected;
		}
    }
}