using System;

namespace Castle.Facilities.WcfIntegration.Tests
{
#if DOTNET40
	using System.ServiceModel.Discovery;
	using System.Xml;
	using NUnit.Framework;

	[TestFixture, IntegrationTest]
	public class ScopeLoadBalancePolicyFactoryTestCase
	{
		[Test]
		public void CanCreatePolicyFactory()
		{
			var roundRobinFactory = new ScopeLoadBalancePolicyFactory<RoundRobinPolicy>();
			var endpoint = new EndpointDiscoveryMetadata
			{
				Scopes = { new Uri("urn:scope:foo") }
			};
			var policies = roundRobinFactory.CreatePolicies(endpoint);
			Assert.AreEqual(1, policies.Length);
			Assert.IsInstanceOf<RoundRobinPolicy>(policies[0]);
			Assert.IsNull(policies[0].ChooseTarget());
		}

		[Test]
		public void WillCreatePolicyPerScope()
		{
			var roundRobinFactory = new ScopeLoadBalancePolicyFactory<RoundRobinPolicy>();
			var endpoint = new EndpointDiscoveryMetadata
			{
				Scopes = { new Uri("urn:scope:foo"), new Uri("urn:scope:bar") }
			};
			var policies = roundRobinFactory.CreatePolicies(endpoint);
			Assert.AreEqual(2, policies.Length);
		}

		[Test]
		public void WillAcceptEndpointsSupportingScope()
		{
			var roundRobinFactory = new ScopeLoadBalancePolicyFactory<RoundRobinPolicy>();
			var endpoint = new EndpointDiscoveryMetadata
			{
				Scopes = { new Uri("urn:scope:foo") }
			};
			var policies = roundRobinFactory.CreatePolicies(endpoint);
			Assert.IsTrue(policies[0].RegisterTarget(new EndpointDiscoveryMetadata
			{
				Scopes = { new Uri("urn:scope:foo") }
			}));
		}

		[Test]
		public void WillRejectEndpointsNotSupportingScope()
		{
			var roundRobinFactory = new ScopeLoadBalancePolicyFactory<RoundRobinPolicy>();
			var endpoint = new EndpointDiscoveryMetadata
			{
				Scopes = { new Uri("urn:scope:foo") }
			};
			var policies = roundRobinFactory.CreatePolicies(endpoint);
			Assert.IsFalse(policies[0].RegisterTarget(new EndpointDiscoveryMetadata
			{
				Scopes = { new Uri("urn:scope:bar") }
			}));
		}

		[Test]
		public void WillFallBackToContractPolicyWhenNoScopes()
		{
			var roundRobinFactory = new ScopeLoadBalancePolicyFactory<RoundRobinPolicy>();
			var endpoint = new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("foo") }
			};
			var policies = roundRobinFactory.CreatePolicies(endpoint);
			Assert.IsTrue(policies[0].RegisterTarget(new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("foo") }
			}));
			Assert.IsFalse(policies[0].RegisterTarget(new EndpointDiscoveryMetadata
			{
				ContractTypeNames = { new XmlQualifiedName("bar") }
			}));
		}
	}
#endif
}
