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
	using System.Reflection;
	using System.ServiceModel;
	using System.ServiceModel.Description;

	using Castle.Facilities.WcfIntegration.Demo;
	using Castle.Facilities.WcfIntegration.Tests.Behaviors;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class ConfigFixture
	{
		private WindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
			container.AddFacility<WcfFacility>(f => f.CloseTimeout = TimeSpan.Zero);
		}

		[TearDown]
		public void TearDown()
		{
			container.Dispose();
		}

		[Test]
		public void Can_resolve_component_based_solely_on_standard_wcf_config()
		{
			var client = container.Resolve<IAmUsingWindsor>("WSHttpBinding_IAmUsingWindsor");
			Assert.IsInstanceOf<IClientChannel>(client);
		}

		[Test]
		public void Can_use_component_based_solely_on_standard_wcf_config()
		{
			container.Register(
				Component.For<IServiceBehavior>()
					.Instance(new ServiceDebugBehavior()
					{
						IncludeExceptionDetailInFaults = true
					}),
				Component.For<NetDataContractFormatBehavior>()
					.Attribute("scope").Eq(WcfExtensionScope.Explicit),
				Component.For<IAmUsingWindsor>().ImplementedBy<UsingWindsor>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel()
					)
				);


			var client = container.Resolve<IAmUsingWindsor>("WSHttpBinding_IAmUsingWindsor");
			var valueFromWindsorConfig = client.GetValueFromWindsorConfig();
			Assert.AreEqual(42, valueFromWindsorConfig);
		}

		[Test]
		public void Can_call_asynchronousely_component_based_solely_on_standard_wcf_config()
		{
			container.Register(
				Component.For<IServiceBehavior>()
					.Instance(new ServiceDebugBehavior()
					{
						IncludeExceptionDetailInFaults = true
					}),
				Component.For<NetDataContractFormatBehavior>()
					.Attribute("scope").Eq(WcfExtensionScope.Explicit),
				Component.For<IAmUsingWindsor>().ImplementedBy<UsingWindsor>()
					.DependsOn(new { number = 42 })
					.AsWcfService(new DefaultServiceModel()
					)
				);


			var client = container.Resolve<IAmUsingWindsor>("WSHttpBinding_IAmUsingWindsor");
			var asyncCall = client.BeginWcfCall(c => c.GetValueFromWindsorConfig());
			var valueFromWindsorConfig = asyncCall.End();
			Assert.AreEqual(42, valueFromWindsorConfig);
		}

		[Test]
		public void Resolving_service_with_invalid_name_causes_exception()
		{
			Assert.Throws(typeof(TargetInvocationException),
			              () => container.Resolve<IAmUsingWindsor>("NoSuchValueInConfig"));

		}
	}
}