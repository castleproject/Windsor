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

namespace Castle.MicroKernel.Tests.DependencyResolving
{
    using System;
    using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Configuration;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	/// <summary>
	/// This test case ensures that the resolving event
	/// is fired properly.
	/// </summary>
	[TestFixture]
	public class EventTests
	{
		private IKernel kernel;

		private ComponentModel expectedClient;
		private List<DependencyModel> expectedModels;

		#region Setup / Teardown

		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
			kernel.DependencyResolving += new DependencyDelegate(AssertEvent);
		}

		[TearDown]
		public void TearDown()
		{
			kernel.Dispose();
		}

		#endregion

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

			kernel.Register(Component.For(typeof (ICustomer)).ImplementedBy(typeof (CustomerImpl)).Named("customer"));

			expectedClient = kernel.GetHandler("customer").ComponentModel;
			expectedModels = new List<DependencyModel>();
			foreach (PropertySet prop in kernel.GetHandler("customer").ComponentModel.Properties)
			{
				expectedModels.Add(prop.Dependency);
			}

			var customer = kernel.Resolve<ICustomer>("customer");

			Assert.IsNotNull(customer);
		}

		[Test]
		public void ResolvingConcreteClassThroughProperties()
		{
			kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));
			kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			var mailservice = kernel.Resolve<DefaultMailSenderService>("mailsender");
			DefaultTemplateEngine templateengine = kernel.Resolve<DefaultTemplateEngine>("templateengine");

			Assert.IsNotNull(mailservice);
			Assert.IsNotNull(templateengine);

			expectedClient = kernel.GetHandler("spamservice").ComponentModel;
			expectedModels = new List<DependencyModel>();
			foreach(PropertySet prop in kernel.GetHandler("spamservice").ComponentModel.Properties)
			{
				expectedModels.Add(prop.Dependency);
			}

			DefaultSpamService spamservice = (DefaultSpamService) kernel.Resolve<DefaultSpamService>("spamservice");

			Assert.IsNotNull(spamservice);
		}

		[Test]
		public void ResolvingConcreteClassThroughConstructor()
		{
			kernel.Register(Component.For(typeof(DefaultSpamServiceWithConstructor)).Named("spamservice"));
			kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			DefaultMailSenderService mailservice = (DefaultMailSenderService) kernel.Resolve<DefaultMailSenderService>("mailsender");
			DefaultTemplateEngine templateengine = (DefaultTemplateEngine) kernel.Resolve<DefaultTemplateEngine>("templateengine");

			Assert.IsNotNull(mailservice);
			Assert.IsNotNull(templateengine);

			expectedClient = kernel.GetHandler("spamservice").ComponentModel;
			expectedModels =
				new List<DependencyModel>(kernel.GetHandler("spamservice").ComponentModel.Constructors.FewerArgumentsCandidate.Dependencies);

			DefaultSpamServiceWithConstructor spamservice =
				(DefaultSpamServiceWithConstructor) kernel.Resolve<DefaultSpamServiceWithConstructor>("spamservice");

			Assert.IsNotNull(spamservice);
		}

		private void AssertEvent(Castle.Core.ComponentModel client, Castle.Core.DependencyModel model, object dependency)
		{
			bool ok = false;
			Assert.AreEqual(expectedClient, client);
			foreach(DependencyModel expectedModel in expectedModels)
			{
				if (expectedModel.Equals(model))
				{
					ok = true;
					break;
				}
			}
			Assert.IsTrue(ok);
			Assert.IsNotNull(dependency);
		}
	}
}
