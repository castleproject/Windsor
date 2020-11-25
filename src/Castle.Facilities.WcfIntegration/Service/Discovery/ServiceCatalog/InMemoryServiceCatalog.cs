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
	using System.Threading;

	public class InMemoryServiceCatalog : IServiceCatalogImplementation
    {
		private readonly List<ILoadBalancePolicy> policies;
		private readonly ILoadBalancePolicyFactory policyFactory;
        private readonly Dictionary<EndpointAddress, EndpointDiscoveryMetadata> endpoints;
        private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();

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
			@lock.EnterReadLock();
			try
			{
				return endpoints.Values.ToArray<EndpointDiscoveryMetadata>();
			}
			finally
			{
				@lock.ExitReadLock();
			}
		}

        public virtual void FindEndpoints(FindRequestContext findRequestContext)
        {
	        @lock.EnterReadLock();
	        try
	        {
		        foreach (var endpoint in MatchTargets(findRequestContext.Criteria))
		        {
			        findRequestContext.AddMatchingEndpoint(endpoint);
		        }
	        }
	        finally
	        {
		        @lock.ExitReadLock();
	        }
        }

        public virtual EndpointDiscoveryMetadata[] FindEndpoints(FindCriteria criteria)
        {
	        @lock.EnterReadLock();
	        try
	        {
		        return MatchTargets(criteria).ToArray();
	        }
	        finally
	        {
		        @lock.ExitReadLock();
	        }
        }

        public virtual bool RegisterEndpoint(EndpointDiscoveryMetadata endpoint)
        {
			var registered = false;
            if (AcceptEndpoint(endpoint))
            {
	            @lock.EnterUpgradeableReadLock();
	            try
	            {
		            policies.ForEach(policy => registered = registered | policy.RegisterTarget(endpoint));
		            @lock.EnterWriteLock();
		            try
		            {
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
			            {
				            endpoints[endpoint.Address] = endpoint;
			            }    
		            }
		            finally
		            {
			            @lock.ExitWriteLock();
		            }
	            }
	            finally
	            {
		            @lock.ExitUpgradeableReadLock();
	            }
            }
            return registered;
        }

        public virtual bool RemoveEndpoint(EndpointDiscoveryMetadata endpoint)
        {
			var removed = false;
			
			@lock.EnterUpgradeableReadLock();
			try
			{
				policies.ForEach(policy => removed = removed | policy.RemoveTarget(endpoint));
				if (removed)
				{
					@lock.EnterWriteLock();
					try
					{
						endpoints.Remove(endpoint.Address);    
					}
					finally
					{
						@lock.ExitWriteLock();
					}
				}
			}
			finally
			{
				@lock.ExitUpgradeableReadLock();
			}

			return removed;
        }

        public virtual EndpointDiscoveryMetadata ResolveEndpoint(ResolveCriteria resolveCriteria)
        {
	        @lock.EnterReadLock();
	        try
	        {
		        EndpointDiscoveryMetadata endpoint;
		        return endpoints.TryGetValue(resolveCriteria.Address, out endpoint)
			        ? endpoint
			        : null;
	        }
	        finally
	        {
		        @lock.ExitReadLock();
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