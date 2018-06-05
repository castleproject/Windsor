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

namespace Castle.Facilities.WcfIntegration.Tests
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.ServiceModel;
	using System.ServiceModel.Discovery;
	using System.Xml;
	using NUnit.Framework;

	[TestFixture, IntegrationTest]
	public class FirstAvailablePolicyTestCase
	{
		#region Setup/Teardown

		[SetUp]
		public void TestInitialize()
		{
			contract = new XmlQualifiedName("Service", "urn:org:services");
			firstAvailable = new FirstAvailablePolicy(target => target.ContractTypeNames.Contains(contract));
		}

		#endregion

		private FirstAvailablePolicy firstAvailable;
		private XmlQualifiedName contract;

		[Test]
		public void EmptyPolicyReturnsNothing()
		{
			Assert.IsNull(firstAvailable.ChooseTarget());
		}

		[Test]
		public void CanAddToPolicy()
		{
			var endpoints = CreateEndpoints(1);
			Assert.AreSame(endpoints[0], firstAvailable.ChooseTarget());
		}

		[Test]
		public void CanRemoveFromPolicy()
		{
			var endpoints = CreateEndpoints(1);
			firstAvailable.RemoveTarget(endpoints[0]);
			Assert.IsNull(firstAvailable.ChooseTarget());
		}

		[Test]
		public void WillChooseFirst()
		{
			var endpoints = CreateEndpoints(2);
			Assert.AreSame(endpoints[0], firstAvailable.ChooseTarget());
			Assert.AreSame(endpoints[0], firstAvailable.ChooseTarget());
		}

		[Test]
		public void WillShiftNextTargetDownIfNecessary()
		{
			CreateEndpoints(4);
			firstAvailable.ChooseTarget();
			var target = firstAvailable.ChooseTarget();
			firstAvailable.RemoveTarget(target);
			target = firstAvailable.ChooseTarget();
			Assert.AreEqual("http://localhost/endpoint2", target.Address.Uri.AbsoluteUri);
		}

		[Test]
		public void CanCollectTargets()
		{
			var endpoints = CreateEndpoints(4);
			var collected = new List<EndpointDiscoveryMetadata>();
			firstAvailable.CollectTargets(collected);
			CollectionAssert.AreEqual(endpoints, collected);
		}

		private EndpointDiscoveryMetadata[] CreateEndpoints(int count)
		{
			return Enumerable.Range(1, count).Select(i =>
			{
				var endpoint = new EndpointDiscoveryMetadata
				{
					ContractTypeNames = { contract },
					Address = new EndpointAddress("http://localhost/endpoint" + i)
				};
				firstAvailable.RegisterTarget(endpoint);
				return endpoint;
			}).ToArray();
		}
	}
}
