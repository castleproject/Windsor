

#if (!SILVERLIGHT)
namespace Castle.Facilities.Remoting.Tests
{
	using System;
	using Castle.Facilities.Remoting.TestComponents;
	using Castle.Windsor;
	using Castle.Windsor.Tests;

	using NUnit.Framework;

	[TestFixture, Serializable]
	public class ConfigurableRegistrationTestCase : AbstractRemoteTestCase
	{
		protected override string GetServerConfigFile()
		{
			return ConfigHelper.ResolveConfigPath("Facilities/Remoting/Configs/server_confreg_clientactivated.xml");
		}

		[Test]
		public void ClientContainerConsumingRemoteComponents()
		{
			clientDomain.DoCallBack(new CrossAppDomainDelegate(ClientContainerConsumingRemoteComponentsCallback));
		}

		public void ClientContainerConsumingRemoteComponentsCallback()
		{
			IWindsorContainer clientContainer = CreateRemoteContainer(clientDomain, ConfigHelper.ResolveConfigPath("Facilities/Remoting/Configs/client_confreg_clientactivated.xml"));

			Assert.IsNotNull(clientContainer.Kernel.ResolveAll<ICalcService>());
		}
	}
}
#endif
