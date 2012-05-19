using System;
using System.Collections.Generic;

namespace Castle.Facilities.WcfIntegration.Tests
{
#if DOTNET40
	using System.ServiceModel.Discovery;
	using System.Xml;
	using NUnit.Framework;

	[TestFixture]
	public class ContractLoadBalancePolicyFactoryTestCase
	{
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

		[Test, ExpectedException(typeof(TypeInitializationException))]
		public void WillRejectPoliciesWithoutContractConstructor()
		{
			new ContractLoadBalancePolicyFactory<TestPolicy>();
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

		#region Nested Class: TestPolicy

		class TestPolicy : ILoadBalancePolicy
		{
			public EndpointDiscoveryMetadata ChooseTarget(FindCriteria criteria = null)
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

			public void CollectTargets(ICollection<EndpointDiscoveryMetadata> collected)
			{
				throw new NotImplementedException();
			}
		}

		class TestPolicyPrivateCtor : TestPolicy
		{
			private TestPolicyPrivateCtor(PolicyMembership membership)
			{
			}
		}

		#endregion
	}
#endif
}
