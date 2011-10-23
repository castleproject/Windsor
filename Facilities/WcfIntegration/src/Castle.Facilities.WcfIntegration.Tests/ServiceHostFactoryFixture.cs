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

namespace Castle.Facilities.WcfIntegration.Tests
{
	using System;
	using System.ServiceModel;
	using Castle.Facilities.WcfIntegration.Demo;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using NUnit.Framework;

	[TestFixture]
	public class ServiceHostFactoryFixture
	{
		[Test]
		public void CanCreateServiceByName()
		{
			IWindsorContainer windsorContainer = new WindsorContainer()
				.Register(Component.For<IOperations>().ImplementedBy<Operations>().Named("operations"),
						  Component.For<IServiceHostBuilder<DefaultServiceModel>>().ImplementedBy<DefaultServiceHostBuilder>()
						  );

			DefaultServiceHostFactory factory = new DefaultServiceHostFactory(windsorContainer.Kernel);
			ServiceHostBase serviceHost = factory.CreateServiceHost("operations", 
				new Uri[] {new Uri("http://localhost/Foo.svc")});
			Assert.IsNotNull(serviceHost);
		}

		[Test]
		public void CanCreateWindsorHostFactory()
		{
			DefaultServiceHostFactory factory = new DefaultServiceHostFactory(new WindsorContainer().Kernel);
			Assert.IsNotNull(factory);
		}

		[Test, Ignore("This test requires the Castle.Facilities.WcfIntegration.Demo running")]
		public void CanCallHostedService()
		{
			IAmUsingWindsor client = ChannelFactory<IAmUsingWindsor>.CreateChannel(
				new BasicHttpBinding(), new EndpointAddress("http://localhost:27197/UsingWindsorWithoutConfig.svc"));
			Assert.AreEqual(42, client.GetValueFromWindsorConfig());
		}
	}
}