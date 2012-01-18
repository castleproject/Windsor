﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

	using Castle.Facilities.WcfIntegration.Data;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	using DataServiceHost = System.Data.Services.DataServiceHost;

	[TestFixture]
	public class DataServiceHostFactoryFixture
	{
		/// <summary>
		///   Verifies that the DataServiceHostFactory can create a 
		///   DataServiceHost instance for a named component.
		/// </summary>
		[Test]
		public void CreatesDataServiceHostFromServiceName()
		{
			var windsorContainer = new WindsorContainer().Register(
				Component.For<IOperations>().ImplementedBy<Operations>().Named("operations"),
				Component.For<IServiceHostBuilder<DataServiceModel>>().ImplementedBy<DataServiceHostBuilder>());

			var factory = new DataServiceHostFactory(windsorContainer.Kernel);

			var serviceLocation = new Uri("http://localhost/Foo.svc");
			var serviceHost = factory.CreateServiceHost("operations", new[] { serviceLocation });

			Assert.That(serviceHost, Is.Not.Null);
			Assert.That(serviceHost, Is.InstanceOf<DataServiceHost>());
			Assert.That(serviceHost.BaseAddresses, Has.Member(serviceLocation));
		}
	}
}