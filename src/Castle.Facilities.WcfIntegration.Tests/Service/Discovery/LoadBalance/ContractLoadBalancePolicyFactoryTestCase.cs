// Copyright 2004-2013 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.WcfIntegration.Tests
{
#if !DOTNET35
	using System;
	using System.Collections.Generic;
	using System.ServiceModel.Discovery;
	using System.Xml;

	using NUnit.Framework;

	[TestFixture]
	public class ContractLoadBalancePolicyFactoryTestCase
	{
		private class TestPolicy : ILoadBalancePolicy
		{
			public EndpointDiscoveryMetadata ChooseTarget(FindCriteria criteria = null)
			{
				throw new NotImplementedException();
			}

			public void CollectTargets(ICollection<EndpointDiscoveryMetadata> collected)
			{
				throw new NotImplementedException();
			}

			public bool RegisterTarget(EndpointDiscoveryMetadata target)
			{
				throw new NotImplementedException();
			}

			public bool RemoveTarget(EndpointDiscoveryMetadata target)
			{
				throw new NotImplementedException();
			}
		}

		private class TestPolicyPrivateCtor : TestPolicy
		{
			private TestPolicyPrivateCtor(PolicyMembership membership)
			{
			}
		}

		[Test]
		public void CanCreatePolicyFactory()
		{
			var roundRobinFactory = new ContractLoadBalancePolicyFactory<RoundRobinPolicy>();
			var endpoint = new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("foo") }
			};
			var policies = roundRobinFactory.CreatePolicies(endpoint);
			Assert.AreEqual(1, policies.Length);
			Assert.IsInstanceOf<RoundRobinPolicy>(policies[0]);
			Assert.IsNull(policies[0].ChooseTarget());
		}

		[Test]
		public void CanCreatePolicyWithPrivateConstructor()
		{
			var factory = new ContractLoadBalancePolicyFactory<TestPolicyPrivateCtor>();
			var policy = factory.CreatePolicies(new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("foo") }
			});
		}

		[Test]
		public void WillAcceptEndpointsSupportingContract()
		{
			var roundRobinFactory = new ContractLoadBalancePolicyFactory<RoundRobinPolicy>();
			var endpoint = new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("foo") }
			};
			var policies = roundRobinFactory.CreatePolicies(endpoint);
			Assert.IsTrue(policies[0].RegisterTarget(new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("foo") }
			}));
		}

		[Test]
		public void WillCreatePolicyPerContract()
		{
			var roundRobinFactory = new ContractLoadBalancePolicyFactory<RoundRobinPolicy>();
			var endpoint = new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("foo"), new XmlQualifiedName("bar") }
			};
			var policies = roundRobinFactory.CreatePolicies(endpoint);
			Assert.AreEqual(2, policies.Length);
		}

		[Test]
		public void WillRejectEndpointsNotSupportingContract()
		{
			var roundRobinFactory = new ContractLoadBalancePolicyFactory<RoundRobinPolicy>();
			var endpoint = new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("foo") }
			};
			var policies = roundRobinFactory.CreatePolicies(endpoint);
			Assert.IsFalse(policies[0].RegisterTarget(new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("bar") }
			}));
		}

		[Test]
		[ExpectedException(typeof(TypeInitializationException))]
		public void WillRejectPoliciesWithoutContractConstructor()
		{
			new ContractLoadBalancePolicyFactory<TestPolicy>();
		}
	}
#endif
}