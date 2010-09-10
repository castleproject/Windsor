// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests.Facilities.Remoting
{
	using System;
	using System.Text;
	using System.Runtime.Remoting;

	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture]
	[Serializable]
	public class RemoteGenericComponentTestCase : AbstractRemoteTestCase
	{
		[Test]
		public void ClientContainerConsumingRemoteCustomComponentUsingGenericInterface()
		{
			clientDomain.DoCallBack(ClientContainerConsumingRemoteCustomComponentUsingGenericInterfaceCallback);
		}

		public void ClientContainerConsumingRemoteCustomComponentUsingGenericInterfaceCallback()
		{
			var clientContainer = CreateRemoteContainer(clientDomain,
			                                            ConfigHelper.ResolveConfigPath(
			                                            	"Facilities/Remoting/Configs/client_kernelgenericcomponent.xml"));

			var service = clientContainer.Resolve<IGenericToStringService<StringBuilder>>();

			Assert.IsTrue(RemotingServices.IsTransparentProxy(service));
			Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(service));

			Assert.AreEqual("33", service.ToString(new StringBuilder("one"), new StringBuilder("two")));
		}

		[Test]
		public void ClientContainerConsumingRemoteGenericComponent()
		{
			clientDomain.DoCallBack(ClientContainerConsumingRemoteGenericComponentCallback);
		}

		public void ClientContainerConsumingRemoteGenericComponentCallback()
		{
			var clientContainer = CreateRemoteContainer(clientDomain,
			                                            ConfigHelper.ResolveConfigPath(
			                                            	"Facilities/Remoting/Configs/client_kernelgenericcomponent.xml"));

			var service = clientContainer.Resolve<IGenericToStringService<String>>();

			Assert.IsTrue(RemotingServices.IsTransparentProxy(service));
			Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(service));

			Assert.AreEqual("onetwo", service.ToString("one", "two"));
		}

		[Test]
		[ExpectedException(typeof(Castle.MicroKernel.ComponentNotFoundException))]
		public void ClientContainerConsumingRemoteGenericComponentWhichDoesNotExistOnServer()
		{
			clientDomain.DoCallBack(ClientContainerConsumingRemoteGenericComponentWhichDoesNotExistOnServerCallback);
		}

		public void ClientContainerConsumingRemoteGenericComponentWhichDoesNotExistOnServerCallback()
		{
			var clientContainer = CreateRemoteContainer(clientDomain,
			                                            ConfigHelper.ResolveConfigPath(
			                                            	"Facilities/Remoting/Configs/client_kernelgenericcomponent.xml"));

			var service = clientContainer.Resolve<GenericToStringServiceImpl<String>>();
		}

		protected override String GetServerConfigFile()
		{
			return ConfigHelper.ResolveConfigPath("Facilities/Remoting/Configs/server_kernelgenericcomponent.xml");
		}
	}
}

#endif