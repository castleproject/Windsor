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
	using System;
	using System.Collections.Generic;
	using System.ServiceModel.Discovery;
	using System.Threading;

	using Castle.Core.Internal;

	public delegate bool PolicyMembership(EndpointDiscoveryMetadata endpoint);

	public abstract class ListBasedLoadBalancePolicy : ILoadBalancePolicy
	{
		private readonly PolicyMembership membership;
		private readonly List<EndpointDiscoveryMetadata> targets;
		private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();

		protected ListBasedLoadBalancePolicy(PolicyMembership membership)
		{
			if (membership == null)
			{
				throw new ArgumentNullException("membership");
			}
			this.membership = membership;
			targets = new List<EndpointDiscoveryMetadata>();
		}

		protected abstract void ChooseTarget(ChooseContext choose);

		public EndpointDiscoveryMetadata ChooseTarget(FindCriteria criteria = null)
		{
			var choose = new ChooseContext(this, criteria);
			ChooseTarget(choose);
			return choose.SelectedEndpoint;
		}

		public bool RegisterTarget(EndpointDiscoveryMetadata target)
		{
			if (membership(target) == false)
				return false;
			@lock.EnterWriteLock();
			try
			{
				var index = FindTargetIndex(target);
				if (index >= 0)
					targets[index] = target;
				else
					targets.Add(target);
			}
			finally
			{
				@lock.ExitWriteLock();
			}
			return true; 
		}

		public bool RemoveTarget(EndpointDiscoveryMetadata target)
		{
			if (membership(target) == false)
				return false;

			@lock.EnterWriteLock();
			try
			{
				var index = FindTargetIndex(target);
				if (index >= 0)
				{
					targets.RemoveAt(index);
					return true;
				}
			}
			finally
			{
				@lock.ExitWriteLock();
			}
			return false;
		}

		public void CollectTargets(ICollection<EndpointDiscoveryMetadata> collected)
		{
			@lock.EnterReadLock();
			try
			{
				targets.ForEach(collected.Add);
			}
			finally
			{
				@lock.ExitReadLock();
			}
		}

		private int FindTargetIndex(EndpointDiscoveryMetadata target)
		{
			var address = target.Address;
			return targets.FindIndex(match => match.Address == address);
		}

		#region Nested Class: ListContext

		protected class ChooseContext
		{
            private readonly ListBasedLoadBalancePolicy policy;
			private readonly FindCriteria criteria;
			private EndpointDiscoveryMetadata selectedEndpoint;

			public ChooseContext(ListBasedLoadBalancePolicy policy, FindCriteria criteria)
			{
                this.policy = policy;
				this.criteria = criteria;
			}

			public FindCriteria Criteria
			{
				get { return criteria; }
			}

			public EndpointDiscoveryMetadata SelectedEndpoint
			{
				get { return selectedEndpoint; }
			}

			public bool Matches(EndpointDiscoveryMetadata endpoint)
			{
				return criteria == null || criteria.IsMatch(endpoint);
			}

			public void ReadList(Func<IList<EndpointDiscoveryMetadata>, EndpointDiscoveryMetadata> readList)
			{
				if (readList != null)
				{
					policy.@lock.EnterReadLock();
					try
					{
						selectedEndpoint = readList(policy.targets.AsReadOnly());
					}
					finally
					{
						policy.@lock.ExitReadLock();
					}
				}
			}

			public void ModifyList(Func<IList<EndpointDiscoveryMetadata>, EndpointDiscoveryMetadata> modifyList)
			{
				if (modifyList != null)
				{
					policy.@lock.EnterWriteLock();
					try
					{
						selectedEndpoint = modifyList(policy.targets);
					}
					finally
					{
						policy.@lock.EnterWriteLock();
					}
				}
			}
		}

		#endregion
	}
}