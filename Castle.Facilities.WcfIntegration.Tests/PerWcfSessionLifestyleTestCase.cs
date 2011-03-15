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
	using System.Linq;
	using System.ServiceModel;

	using Castle.Core;
	using Castle.Facilities.WcfIntegration.Tests.Behaviors;
	using Castle.Facilities.WcfIntegration.Tests.Components;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class PerWcfSessionLifestyleTestCase
	{
		[SetUp]
		public void SetUp()
		{
			windsorContainer = new WindsorContainer()
				.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero)
				.Register(
				Component.For<ServiceHostListener>(),
				Component.For<CollectingInterceptor>(),
				Component.For<UnitOfworkEndPointBehavior>(),
				Component.For<NetDataContractFormatBehavior>(),
				Component.For<IOne>().ImplementedBy<One>().LifeStyle.PerWcfSession().Interceptors(
					InterceptorReference.ForType<CollectingInterceptor>()).Anywhere,
				Component.For<ITwo>().ImplementedBy<Two>().LifeStyle.PerWcfSession().Interceptors(
					InterceptorReference.ForType<CollectingInterceptor>()).Anywhere,
				Component.For<IServiceWithSession>().ImplementedBy<ServiceWithSession>().LifeStyle.Transient
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
			ServiceWithSession.InstanceCount = 0;
		}

		private IWindsorContainer windsorContainer;
		private IServiceWithSession client;

		private IServiceWithSession CreateClient()
		{
			return ChannelFactory<IServiceWithSession>.CreateChannel(
				new NetTcpBinding { PortSharingEnabled = true }, new EndpointAddress("net.tcp://localhost/Operations"));
		}

		[Test]
		public void Services_should_be_reused_among_calls_within_session()
		{
			client.Initiating("start ");
			client.Operation1("one ");
			client.Operation1("and again");
			client.Operation2("two ");
			client.Operation2("and two again");
			client.Terminating();
			var invocations = windsorContainer.GetService<CollectingInterceptor>()
				.AllInvocations
				.GroupBy(i => i.InvocationTarget)
				.ToArray();
			Assert.AreEqual(2, invocations.Length);
			var one = invocations[0].Key as One;
			var two = invocations[1].Key as Two;
			Assert.AreEqual("start one and again", one.Arg);
			Assert.AreEqual("two and two again", two.Arg);
		}

		[Test]
		public void Services_should_not_be_shared_between_two_subsequent_sessions()
		{
			client.Initiating("Client 1");
			client.Operation1(" Run Forrest run!");
			client.Terminating();
			client = CreateClient();
			client.Initiating("Client 2");
			client.Operation1(" welcomes you.");
			client.Terminating();
			var interceptor = windsorContainer.GetService<CollectingInterceptor>();
			var invocations = interceptor
				.AllInvocations
				.GroupBy(i => i.InvocationTarget)
				.ToArray();

			Assert.AreEqual(6, ServiceWithSession.InstanceCount);
			Assert.AreEqual(2, invocations.Length);
			var one1 = invocations[0].Key as One;
			var one2 = invocations[1].Key as One;
			Assert.AreEqual("Client 1 Run Forrest run!", one1.Arg);
			Assert.AreEqual("Client 2 welcomes you.", one2.Arg);
		}

		[Test]
		public void Services_should_not_be_shared_between_two_concurrent_sessions()
		{
			var client2 = CreateClient();
			client.Initiating("Client 1");
			client.Operation1(" Run Forrest run!");
			client2.Initiating("Client 2");
			client2.Operation1(" welcomes you.");
			client.Terminating();
			client2.Terminating();
			var interceptor = windsorContainer.GetService<CollectingInterceptor>();
			var invocations = interceptor
				.AllInvocations
				.GroupBy(i => i.InvocationTarget)
				.ToArray();

			Assert.AreEqual(6, ServiceWithSession.InstanceCount);
			Assert.AreEqual(2, invocations.Length);
			var one1 = invocations[0].Key as One;
			var one2 = invocations[1].Key as One;
			Assert.AreEqual("Client 1 Run Forrest run!", one1.Arg);
			Assert.AreEqual("Client 2 welcomes you.", one2.Arg);
		}
	}
}