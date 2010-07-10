// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

	using Castle.Facilities.WcfIntegration.Tests.Behaviors;
	using Castle.Facilities.WcfIntegration.Tests.Components;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class PerWcfOperationLifestyleTestCase
	{
		[SetUp]
		public void SetUp()
		{
			windsorContainer = new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
				Component.For<ServiceHostListener>(),
				Component.For<UnitOfworkEndPointBehavior>(),
				Component.For<NetDataContractFormatBehavior>(),
				Component.For<IOne>().ImplementedBy<One>().LifeStyle.PerWcfOperation(),
				Component.For<HasOne>().LifeStyle.PerWcfOperation(),
				Component.For<IServiceWithDependencies>().ImplementedBy<ServiceWithDependencies>().LifeStyle.Transient
					.Named("Operations")
					.AsWcfService(new DefaultServiceModel().AddEndpoints(
					       	WcfEndpoint.BoundTo(new NetTcpBinding { PortSharingEnabled = true })
					       		.At("net.tcp://localhost/Operations")
					       	)
					)
				);

			client = CreateClient();
		}

		[TearDown]
		public void TearDown()
		{
			windsorContainer.Dispose();
			ServiceWithDependencies.Dependencies.Clear();
		}

		private IWindsorContainer windsorContainer;
		private IServiceWithDependencies client;

		private IServiceWithDependencies CreateClient()
		{
			return ChannelFactory<IServiceWithDependencies>.CreateChannel(
				new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
		}

		[Test]
		public void Dependencies_should_be_reused_among_services_within_call()
		{
			client.OperationOne();
			Assert.AreEqual(2, ServiceWithDependencies.Dependencies.Count);
			var one = ServiceWithDependencies.Dependencies[0] as IOne;
			var hasOne = ServiceWithDependencies.Dependencies[1] as HasOne;
			Assert.AreSame(one, hasOne.One);
		}

		[Test]
		public void Dependencies_should_not_reused_among_between_calls()
		{
			client.OperationOne();
			client.OperationOne();
			Assert.AreEqual(4, ServiceWithDependencies.Dependencies.Count);
			var one1 = ServiceWithDependencies.Dependencies[0] as IOne;
			var one2 = ServiceWithDependencies.Dependencies[2] as IOne;
			Assert.AreNotSame(one1, one2);
		}
	}
}