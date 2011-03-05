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


#if(!SILVERLIGHT)

namespace CastleTests.Facilities.Remoting
{
	using System;
	using System.Runtime.Remoting;

	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Tests.Components;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	[Serializable]
	public class RemoteSingletonTestCase : AbstractRemoteTestCase
	{
		protected override String GetServerConfigFile()
		{
			return "server_simple_scenario.xml";
		}

		public void CommonAppConsumingRemoteComponentsCallback()
		{
			var service = (ICalcService)
			              Activator.GetObject(typeof(ICalcService), "tcp://localhost:2133/calcservice.rem");

			Assert.IsTrue(RemotingServices.IsTransparentProxy(service));
			Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(service));

			Assert.AreEqual(10, service.Sum(7, 3));
		}

		public void ClientContainerConsumingRemoteComponentCallback()
		{
			var clientContainer = CreateRemoteContainer(clientDomain, "client_simple_scenario.xml");

			var service = clientContainer.Resolve<ICalcService>();

			Assert.IsTrue(RemotingServices.IsTransparentProxy(service));
			Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(service));

			Assert.AreEqual(10, service.Sum(7, 3));
		}

		public void WiringRemoteComponentCallback()
		{
			var clientContainer = CreateRemoteContainer(clientDomain, "client_simple_scenario.xml");

			clientContainer.Register(Component.For(typeof(ConsumerComp)).Named("comp"));

			var service = clientContainer.Resolve<ConsumerComp>();

			Assert.IsNotNull(service.Calcservice);
			Assert.IsTrue(RemotingServices.IsTransparentProxy(service.Calcservice));
			Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(service.Calcservice));
		}

		[Test]
		public void ClientContainerConsumingRemoteComponent()
		{
			clientDomain.DoCallBack(ClientContainerConsumingRemoteComponentCallback);
		}

		[Test]
		public void CommonAppConsumingRemoteComponents()
		{
			clientDomain.DoCallBack(CommonAppConsumingRemoteComponentsCallback);
		}

		[Test]
		public void WiringRemoteComponent()
		{
			clientDomain.DoCallBack(WiringRemoteComponentCallback);
		}
	}
}

#endif