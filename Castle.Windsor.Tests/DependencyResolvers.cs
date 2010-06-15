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

namespace Castle.MicroKernel.Tests
{
	using Castle.Core.Configuration;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	/// <summary>
	/// Summary description for DependencyResolvers.
	/// </summary>
	[TestFixture]
	public class DependencyResolvers
	{
		private IKernel kernel;

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[Test]
		public void ResolvingPrimitivesThroughProperties()
		{
			MutableConfiguration config = new MutableConfiguration("component");

			MutableConfiguration parameters = new MutableConfiguration("parameters");
			config.Children.Add(parameters);

			parameters.Children.Add(new MutableConfiguration("name", "hammett"));
			parameters.Children.Add(new MutableConfiguration("address", "something"));
			parameters.Children.Add(new MutableConfiguration("age", "25"));

			kernel.ConfigurationStore.AddComponentConfiguration("customer", config);

			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl)).Named("customer"));

			ICustomer customer = (ICustomer) kernel["customer"];

			Assert.IsNotNull(customer);
			Assert.AreEqual("hammett", customer.Name);
			Assert.AreEqual("something", customer.Address);
			Assert.AreEqual(25, customer.Age);
		}

		[Test]
		public void ResolvingConcreteClassThroughProperties()
		{
			kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));
			kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			DefaultSpamService spamservice = (DefaultSpamService) kernel["spamservice"];

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void ResolvingConcreteClassThroughConstructor()
		{
			kernel.Register(Component.For<DefaultSpamServiceWithConstructor>().Named("spamservice"));
			kernel.Register(Component.For<DefaultMailSenderService>().Named("mailsender"));
			kernel.Register(Component.For<DefaultTemplateEngine>().Named("templateengine"));

			var spamservice = kernel.Resolve<DefaultSpamServiceWithConstructor>("spamservice");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void Resolving_by_name_is_case_insensitive()
		{
			kernel.Register(Component.For<DefaultSpamServiceWithConstructor>().Named("spamService"));
			kernel.Register(Component.For<DefaultMailSenderService>().Named("mailSender"));
			kernel.Register(Component.For<DefaultTemplateEngine>().Named("templateEngine"));

			var spamservice = kernel.Resolve<DefaultSpamServiceWithConstructor>("spamSERVICE");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void Service_override_by_name_is_case_insensitive()
		{
			kernel.Register(Component.For<DefaultSpamServiceWithConstructor>().Named("spamService"));
			kernel.Register(Component.For<DefaultMailSenderService>().Named("someMailSender"));
			kernel.Register(Component.For<DefaultTemplateEngine>().Named("templateEngine")
			                	.DependsOn(ServiceOverride.ForKey("mailSENDER").Eq("SOMEmailSenDeR")));

			var spamservice = kernel.Resolve<DefaultSpamServiceWithConstructor>("spamSERVICE");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}
		[Test]
		[ExpectedException(typeof(HandlerException))]
		public void UnresolvedDependencies()
		{
			kernel.Register(Component.For(typeof(DefaultSpamServiceWithConstructor)).Named("spamservice"));
			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			DefaultSpamService spamservice = (DefaultSpamService) kernel["spamservice"];
		}

		[Test]
		public void FactoryPattern()
		{
			kernel.Register(Component.For(typeof(DefaultSpamServiceWithConstructor)).Named("spamservice"));
			kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			kernel.Register(Component.For(typeof(ComponentFactory)).Named("factory"));

			ComponentFactory factory = (ComponentFactory) kernel["factory"];

			Assert.IsNotNull(factory);

			DefaultSpamServiceWithConstructor spamservice =
				(DefaultSpamServiceWithConstructor) factory.Create("spamservice");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void DependencyChain()
		{
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerChain9)).Named("Customer9"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerChain8)).Named("Customer8"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerChain7)).Named("Customer7"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerChain6)).Named("Customer6"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerChain5)).Named("Customer5"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerChain4)).Named("Customer4"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerChain3)).Named("Customer3"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerChain2)).Named("Customer2"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerChain1)).Named("Customer1"));
			kernel.Register(Component.For(typeof(ICustomer)).ImplementedBy(typeof(CustomerImpl)).Named("Customer"));

			CustomerChain1 customer = (CustomerChain1) kernel[typeof(ICustomer)];
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
			ICustomer lastCustomer = customer.CustomerBase;
			Assert.IsInstanceOf(typeof(CustomerImpl), lastCustomer);
		}
	}
}
