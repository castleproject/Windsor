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
	using System.Text;

	using Castle.MicroKernel;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	[Serializable]
	public class RemoteGenericComponentTestCase : AbstractRemoteTestCase
	{
		public void ClientContainerConsumingRemoteCustomComponentUsingGenericInterfaceCallback()
		{
			var clientContainer = CreateRemoteContainer(clientDomain,
			                                            "client_kernelgenericcomponent.xml");

			var service = clientContainer.Resolve<IGenericToStringService<StringBuilder>>();

			Assert.IsTrue(RemotingServices.IsTransparentProxy(service));
			Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(service));

			Assert.AreEqual("33", service.ToString(new StringBuilder("one"), new StringBuilder("two")));
		}

		public void ClientContainerConsumingRemoteGenericComponentCallback()
		{
			var clientContainer = CreateRemoteContainer(clientDomain,
			                                            "client_kernelgenericcomponent.xml");

			var service = clientContainer.Resolve<IGenericToStringService<String>>();

			Assert.IsTrue(RemotingServices.IsTransparentProxy(service));
			Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(service));

			Assert.AreEqual("onetwo", service.ToString("one", "two"));
		}

		public void ClientContainerConsumingRemoteGenericComponentWhichDoesNotExistOnServerCallback()
		{
			var clientContainer = CreateRemoteContainer(clientDomain,
			                                            "client_kernelgenericcomponent.xml");

			var service = clientContainer.Resolve<GenericToStringServiceImpl<String>>();
		}

		protected override String GetServerConfigFile()
		{
			return "server_kernelgenericcomponent.xml";
		}

		[Test]
		public void ClientContainerConsumingRemoteCustomComponentUsingGenericInterface()
		{
			clientDomain.DoCallBack(ClientContainerConsumingRemoteCustomComponentUsingGenericInterfaceCallback);
		}

		[Test]
		public void ClientContainerConsumingRemoteGenericComponent()
		{
			clientDomain.DoCallBack(ClientContainerConsumingRemoteGenericComponentCallback);
		}

		[Test]
		[ExpectedException(typeof(ComponentNotFoundException))]
		public void ClientContainerConsumingRemoteGenericComponentWhichDoesNotExistOnServer()
		{
			clientDomain.DoCallBack(ClientContainerConsumingRemoteGenericComponentWhichDoesNotExistOnServerCallback);
		}
	}
}

#endif