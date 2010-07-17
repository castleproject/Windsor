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

#if (!SILVERLIGHT)

namespace Castle.Windsor.Tests.Facilities.Remoting
{
	using System;
	using System.Runtime.Remoting;

	using Castle.Windsor;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture]
	[Serializable]
	public class ServerClientContainerTestCase : AbstractRemoteTestCase
	{
		public override void Init()
		{
			base.Init();

			serverClient = AppDomainFactory.Create("serverClient");
			serverClientContainer = GetRemoteContainer(serverClient,
			                                           ConfigHelper.ResolveConfigPath(
			                                           	"Facilities/Remoting/Configs/server_client_kernelcomponent.xml"));
		}

		public override void Terminate()
		{
			serverClientContainer.Dispose();
			AppDomain.Unload(serverClient);

			base.Terminate();
		}

		private AppDomain serverClient;

		private IWindsorContainer serverClientContainer;

		protected override String GetServerConfigFile()
		{
			return ConfigHelper.ResolveConfigPath("Facilities/Remoting/Configs/server_kernelcomponent.xml");
		}

		public void ClientContainerConsumingRemoteComponentCallback()
		{
			var clientContainer = CreateRemoteContainer(clientDomain,
			                                            ConfigHelper.ResolveConfigPath(
			                                            	"Facilities/Remoting/Configs/client_12134_kernelcomponent.xml"));

			var service = clientContainer.Resolve<ICalcService>("calc.service.c1");

			Assert.IsTrue(RemotingServices.IsTransparentProxy(service));
			Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(service));

			Assert.AreEqual(10, service.Sum(7, 3));
		}

		public void ServerClientContainerConsumingRemoteComponentCallback()
		{
			var serverAsClient = GetRemoteContainer(serverClient,
			                                        ConfigHelper.ResolveConfigPath(
			                                        	"Facilities/Remoting/Configs/server_client_kernelcomponent.xml"));

			var service = serverAsClient.Resolve<ICalcService>();

			Assert.IsTrue(RemotingServices.IsTransparentProxy(service));
			Assert.IsTrue(RemotingServices.IsObjectOutOfAppDomain(service));

			Assert.AreEqual(10, service.Sum(7, 3));
		}

		[Test]
		public void ClientContainerConsumingRemoteComponent()
		{
			clientDomain.DoCallBack(ClientContainerConsumingRemoteComponentCallback);
		}

		[Test]
		public void ServerClientContainerConsumingRemoteComponent()
		{
			serverClient.DoCallBack(ServerClientContainerConsumingRemoteComponentCallback);
		}
	}
}

#endif