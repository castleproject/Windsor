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

	using NUnit.Framework;

	[TestFixture]
	public class DefaultDependnecyResolverTestCase : AbstractContainerTestCase
	{
		[Test]
		public void DependencyChain_each_registered_separately()
		{
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain9)).Named("Customer9"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain8)).Named("Customer8"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain7)).Named("Customer7"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain6)).Named("Customer6"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain5)).Named("Customer5"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain4)).Named("Customer4"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain3)).Named("Customer3"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain2)).Named("Customer2"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain1)).Named("Customer1"));
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerImpl)).Named("Customer"));

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
			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain9)).Named("Customer9"),
			                Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain8)).Named("Customer8"),
			                Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain7)).Named("Customer7"),
			                Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain6)).Named("Customer6"),
			                Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain5)).Named("Customer5"),
			                Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain4)).Named("Customer4"),
			                Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain3)).Named("Customer3"),
			                Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain2)).Named("Customer2"),
			                Component.For<ICustomer>().ImplementedBy(typeof(CustomerChain1)).Named("Customer1"),
			                Component.For<ICustomer>().ImplementedBy(typeof(CustomerImpl)).Named("Customer"));

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
			Kernel.Register(Component.For(typeof(DefaultSpamServiceWithConstructor)).Named("spamservice"));
			Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			Kernel.Register(Component.For(typeof(ComponentFactory)).Named("factory"));

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
			Kernel.Register(Component.For<DefaultSpamServiceWithConstructor>().Named("spamservice"));
			Kernel.Register(Component.For<DefaultMailSenderService>().Named("mailsender"));
			Kernel.Register(Component.For<DefaultTemplateEngine>().Named("templateengine"));

			var spamservice = Kernel.Resolve<DefaultSpamServiceWithConstructor>("spamservice");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void ResolvingConcreteClassThroughProperties()
		{
			Kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));
			Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			var spamservice = Kernel.Resolve<DefaultSpamService>("spamservice");

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

			Kernel.Register(Component.For<ICustomer>().ImplementedBy(typeof(CustomerImpl)).Named("customer"));

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
			Kernel.Register(Component.For(typeof(DefaultSpamServiceWithConstructor)).Named("spamservice"));
			Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			var spamservice = Kernel.Resolve<DefaultSpamService>("spamservice");
		}
	}
}