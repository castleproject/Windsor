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

namespace CastleTests
{
	using Castle.Core.Configuration;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using CastleTests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class DefaultDependnecyResolverTestCase : AbstractContainerTestCase
	{
		[Test]
		public void DependencyChain_each_registered_separately()
		{
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain9>());
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain8>());
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain7>());
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain6>());
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain5>());
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain4>());
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain3>());
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain2>());
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain1>());
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>());

			var customer = (CustomerChain1)Kernel.Resolve<ICustomer>();
			Assert.IsInstanceOf(typeof(CustomerChain9), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain8), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain7), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain6), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain5), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain4), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain3), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain2), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain1), customer);
			var lastCustomer = customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerImpl), lastCustomer);
		}

		[Test]
		public void DependencyChain_registered_all_at_once()
		{
			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerChain9>(),
			                Component.For<ICustomer>().ImplementedBy<CustomerChain8>(),
			                Component.For<ICustomer>().ImplementedBy<CustomerChain7>(),
			                Component.For<ICustomer>().ImplementedBy<CustomerChain6>(),
			                Component.For<ICustomer>().ImplementedBy<CustomerChain5>(),
			                Component.For<ICustomer>().ImplementedBy<CustomerChain4>(),
			                Component.For<ICustomer>().ImplementedBy<CustomerChain3>(),
			                Component.For<ICustomer>().ImplementedBy<CustomerChain2>(),
			                Component.For<ICustomer>().ImplementedBy<CustomerChain1>(),
			                Component.For<ICustomer>().ImplementedBy<CustomerImpl>());

			var customer = (CustomerChain1)Kernel.Resolve<ICustomer>();
			Assert.IsInstanceOf(typeof(CustomerChain9), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain8), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain7), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain6), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain5), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain4), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain3), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain2), customer);
			customer = (CustomerChain1)customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerChain1), customer);
			var lastCustomer = customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerImpl), lastCustomer);
		}

		[Test]
		public void FactoryPattern()
		{
			Kernel.Register(Component.For<DefaultSpamServiceWithConstructor>().Named("spamservice"),
			                Component.For<DefaultMailSenderService>().Named("mailsender"),
			                Component.For<DefaultTemplateEngine>().Named("templateengine"),
			                Component.For<ComponentFactory>().Named("factory"));

			var factory = Kernel.Resolve<ComponentFactory>("factory");

			Assert.IsNotNull(factory);

			var spamservice =
				(DefaultSpamServiceWithConstructor)factory.Create("spamservice");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void ResolvingConcreteClassThroughConstructor()
		{
			Kernel.Register(Component.For<DefaultSpamServiceWithConstructor>(),
			                Component.For<DefaultMailSenderService>(),
			                Component.For<DefaultTemplateEngine>());

			var spamservice = Kernel.Resolve<DefaultSpamServiceWithConstructor>();

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void ResolvingConcreteClassThroughProperties()
		{
			Kernel.Register(Component.For<DefaultSpamService>(),
			                Component.For<DefaultMailSenderService>(),
			                Component.For<DefaultTemplateEngine>());

			var spamservice = Kernel.Resolve<DefaultSpamService>();

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void ResolvingPrimitivesThroughProperties()
		{
			var config = new MutableConfiguration("component");

			var parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("name", "hammett"));
			parameters.Children.Add(new MutableConfiguration("address", "something"));
			parameters.Children.Add(new MutableConfiguration("age", "25"));

			Kernel.ConfigurationStore.AddComponentConfiguration("customer", config);

			Kernel.Register(Component.For<ICustomer>().ImplementedBy<CustomerImpl>().Named("customer"));

			var customer = Kernel.Resolve<ICustomer>("customer");

			Assert.IsNotNull(customer);
			Assert.AreEqual("hammett", customer.Name);
			Assert.AreEqual("something", customer.Address);
			Assert.AreEqual(25, customer.Age);
		}

		[Test]
		public void Resolving_by_name_is_case_insensitive()
		{
			Kernel.Register(Component.For<DefaultSpamServiceWithConstructor>().Named("spamService"));
			Kernel.Register(Component.For<DefaultMailSenderService>().Named("mailSender"));
			Kernel.Register(Component.For<DefaultTemplateEngine>().Named("templateEngine"));

			var spamservice = Kernel.Resolve<DefaultSpamServiceWithConstructor>("spamSERVICE");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void Service_override_by_name_is_case_insensitive()
		{
			Kernel.Register(Component.For<DefaultSpamServiceWithConstructor>().Named("spamService"));
			Kernel.Register(Component.For<DefaultMailSenderService>().Named("someMailSender"));
			Kernel.Register(Component.For<DefaultTemplateEngine>().Named("templateEngine")
			                	.DependsOn(ServiceOverride.ForKey("mailSENDER").Eq("SOMEmailSenDeR")));

			var spamservice = Kernel.Resolve<DefaultSpamServiceWithConstructor>("spamSERVICE");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		[ExpectedException(typeof(HandlerException))]
		public void UnresolvedDependencies()
		{
			Kernel.Register(Component.For<DefaultSpamServiceWithConstructor>(),
			                Component.For<DefaultTemplateEngine>());

			Kernel.Resolve<DefaultSpamServiceWithConstructor>();
		}
	}
}