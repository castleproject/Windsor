// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Tests.SubContainers
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	/// <summary>
	/// Summary description for SubContainersTestCase.
	/// </summary>
	[TestFixture]
	public class SubContainersTestCase
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
		public void ChildDependenciesSatisfiedAmongContainers()
		{
			IKernel subkernel = new DefaultKernel();

			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));
			kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));

			kernel.AddChildKernel(subkernel);
			subkernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));

			DefaultSpamService spamservice = (DefaultSpamService) subkernel["spamservice"];

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void ChildDependenciesIsSatisfiedEvenWhenComponentTakesLongToBeAddedToParentContainer()
		{
			DefaultKernel container = new DefaultKernel();
			DefaultKernel childContainer = new DefaultKernel();

			container.AddChildKernel(childContainer);
			((IKernel)childContainer).Register(Component.For(typeof(UsesIEmptyService)).Named("component"));

			((IKernel)container).Register(Component.For(typeof(IEmptyService)).ImplementedBy(typeof(EmptyService)).Named("service1"));

			UsesIEmptyService comp = (UsesIEmptyService) childContainer[typeof(UsesIEmptyService)];
		}

		[Test]
		public void SameLevelDependenciesSatisfied()
		{
			IKernel child = new DefaultKernel();

			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));
			kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));

			kernel.AddChildKernel(child);
			
			child.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));

			DefaultSpamService spamservice = (DefaultSpamService) child["spamservice"];

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void UseChildComponentsForParentDependenciesWhenRequestedFromChild()
		{
			IKernel subkernel = new DefaultKernel();

			kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice").LifeStyle.Is(LifestyleType.Transient));
			kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			kernel.AddChildKernel(subkernel);
			subkernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			DefaultTemplateEngine templateengine = (DefaultTemplateEngine) kernel["templateengine"];
			DefaultTemplateEngine sub_templateengine = (DefaultTemplateEngine) subkernel["templateengine"];

			DefaultSpamService spamservice = (DefaultSpamService) subkernel["spamservice"];
			Assert.AreNotEqual(spamservice.TemplateEngine, templateengine);
			Assert.AreEqual(spamservice.TemplateEngine, sub_templateengine);

			spamservice = (DefaultSpamService) kernel["spamservice"];
			Assert.AreNotEqual(spamservice.TemplateEngine, sub_templateengine);
			Assert.AreEqual(spamservice.TemplateEngine, templateengine);

			kernel.RemoveComponent("templateengine");
			spamservice = (DefaultSpamService) kernel["spamservice"];
			Assert.IsNull(spamservice.TemplateEngine);
		}

		[Test]
		[Ignore("Support for this was removed due to issues with scoping (SimpleComponent1 would become visible from parent container).")]
		public void Requesting_parent_component_with_child_dependency_from_child_component()
		{
			var subkernel = new DefaultKernel();
			kernel.AddChildKernel(subkernel);

			kernel.Register(Component.For<UsesSimpleComponent1>());
			subkernel.Register(Component.For<SimpleComponent1>());


			subkernel.Resolve<UsesSimpleComponent1>();
		}

		[Test]
		[Ignore("Support for this was removed due to issues with scoping (SimpleComponent1 would become visible from parent container).")]
		public void Three_level_hierarchy([Values(0, 1, 2)] int parentComponentContainer, [Values(0, 1, 2)] int childComponentContainer)
		{
			var subKernel = new DefaultKernel();
			var subSubKernel = new DefaultKernel();
			kernel.AddChildKernel(subKernel);
			subKernel.AddChildKernel(subSubKernel);
			var containers = new[]
			{
				kernel,
				subKernel,
				subSubKernel
			};

			containers[parentComponentContainer].Register(Component.For<UsesSimpleComponent1>());
			containers[childComponentContainer].Register(Component.For<SimpleComponent1>());


			subSubKernel.Resolve<UsesSimpleComponent1>();
		}

		[Test]
		public void Singleton_WithNonSingletonDependencies_DoesNotReResolveDependencies()
		{
			kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));
			kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));

			IKernel subkernel1 = new DefaultKernel();
			subkernel1.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));
			kernel.AddChildKernel(subkernel1);

			IKernel subkernel2 = new DefaultKernel();
			subkernel2.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine").LifeStyle.Is(LifestyleType.Transient));
			kernel.AddChildKernel(subkernel2);

			DefaultTemplateEngine templateengine1 = (DefaultTemplateEngine) subkernel1["templateengine"];
			DefaultSpamService spamservice1 = (DefaultSpamService) subkernel1["spamservice"];
			Assert.IsNotNull(spamservice1);
			Assert.AreEqual(spamservice1.TemplateEngine.Key, templateengine1.Key);

			DefaultTemplateEngine templateengine2 = (DefaultTemplateEngine) subkernel2["templateengine"];
			DefaultSpamService spamservice2 = (DefaultSpamService) subkernel2["spamservice"];
			Assert.IsNotNull(spamservice2);
			Assert.AreEqual(spamservice1, spamservice2);
			Assert.AreEqual(spamservice1.TemplateEngine.Key, templateengine1.Key);
			Assert.AreNotEqual(spamservice2.TemplateEngine.Key, templateengine2.Key);
		}

		[Test]
		public void DependenciesSatisfiedAmongContainers()
		{
			IKernel subkernel = new DefaultKernel();

			kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			kernel.AddChildKernel(subkernel);

			subkernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));

			DefaultSpamService spamservice = (DefaultSpamService) subkernel["spamservice"];

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void DependenciesSatisfiedAmongContainersUsingEvents()
		{
			IKernel subkernel = new DefaultKernel();

			subkernel.Register(Component.For(typeof(DefaultSpamServiceWithConstructor)).Named("spamservice"));

			kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			kernel.AddChildKernel(subkernel);

			DefaultSpamServiceWithConstructor spamservice =
				(DefaultSpamServiceWithConstructor) subkernel["spamservice"];

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void ChildKernelFindsAndCreateParentComponent()
		{
			IKernel subkernel = new DefaultKernel();

			kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			kernel.AddChildKernel(subkernel);


			Assert.IsTrue(subkernel.HasComponent(typeof(DefaultTemplateEngine)));
			Assert.IsNotNull(subkernel[typeof(DefaultTemplateEngine)]);
		}

		[Test]
		[ExpectedException(typeof(ComponentNotFoundException))]
		public void ParentKernelFindsAndCreateChildComponent()
		{
			IKernel subkernel = new DefaultKernel();

			subkernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			kernel.AddChildKernel(subkernel);


			Assert.IsFalse(kernel.HasComponent(typeof(DefaultTemplateEngine)));
			object engine = kernel[typeof(DefaultTemplateEngine)];
		}

		[Test]
		public void ChildKernelOverloadsParentKernel1()
		{
			DefaultTemplateEngine instance1 = new DefaultTemplateEngine();
			DefaultTemplateEngine instance2 = new DefaultTemplateEngine();

			// subkernel added with already registered components that overload parent components.

			IKernel subkernel = new DefaultKernel();
			subkernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance1));
			Assert.AreEqual(instance1, subkernel["engine"]);

            kernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance2));
			Assert.AreEqual(instance2, kernel["engine"]);

			kernel.AddChildKernel(subkernel);
			Assert.AreEqual(instance1, subkernel["engine"]);
			Assert.AreEqual(instance2, kernel["engine"]);
		}

		[Test]
		public void ChildKernelOverloadsParentKernel2()
		{
			DefaultTemplateEngine instance1 = new DefaultTemplateEngine();
			DefaultTemplateEngine instance2 = new DefaultTemplateEngine();

			IKernel subkernel = new DefaultKernel();
			kernel.AddChildKernel(subkernel);

			// subkernel added first, then populated with overloaded components after

            kernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance2));
			Assert.AreEqual(instance2, kernel["engine"]);
			Assert.AreEqual(instance2, subkernel["engine"]);

            subkernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance1));
			Assert.AreEqual(instance1, subkernel["engine"]);
			Assert.AreEqual(instance2, kernel["engine"]);
		}

		[Test]
		public void RemoveChildKernelCleansUp()
		{
			IKernel subkernel = new DefaultKernel();
			EventsCollector eventCollector = new EventsCollector(subkernel);
			subkernel.RemovedAsChildKernel += new EventHandler(eventCollector.RemovedAsChildKernel);
			subkernel.AddedAsChildKernel += new EventHandler(eventCollector.AddedAsChildKernel);

			kernel.AddChildKernel(subkernel);
			Assert.AreEqual(kernel, subkernel.Parent);
			Assert.AreEqual(1, eventCollector.Events.Count);
			Assert.AreEqual(EventsCollector.Added, eventCollector.Events[0]);

			kernel.RemoveChildKernel(subkernel);
			Assert.IsNull(subkernel.Parent);
			Assert.AreEqual(2, eventCollector.Events.Count);
			Assert.AreEqual(EventsCollector.Removed, eventCollector.Events[1]);
		}

		[Test]
		[ExpectedException(typeof(KernelException),
			ExpectedMessage = "You can not change the kernel parent once set, use the RemoveChildKernel and AddChildKernel methods together to achieve this."
			)]
		public void AddChildKernelToTwoParentsThrowsException()
		{
			IKernel kernel2 = new DefaultKernel();

			IKernel subkernel = new DefaultKernel();

			kernel.AddChildKernel(subkernel);
			Assert.AreEqual(kernel, subkernel.Parent);

			kernel2.AddChildKernel(subkernel);
		}

		[Test]
		public void RemovingChildKernelUnsubscribesFromParentEvents()
		{
			IKernel subkernel = new DefaultKernel();
			EventsCollector eventCollector = new EventsCollector(subkernel);
			subkernel.RemovedAsChildKernel += new EventHandler(eventCollector.RemovedAsChildKernel);
			subkernel.AddedAsChildKernel += new EventHandler(eventCollector.AddedAsChildKernel);

			kernel.AddChildKernel(subkernel);
			kernel.RemoveChildKernel(subkernel);
			kernel.AddChildKernel(subkernel);
			kernel.RemoveChildKernel(subkernel);

			Assert.AreEqual(4, eventCollector.Events.Count);
			Assert.AreEqual(EventsCollector.Added, eventCollector.Events[0]);
			Assert.AreEqual(EventsCollector.Removed, eventCollector.Events[1]);
			Assert.AreEqual(EventsCollector.Added, eventCollector.Events[2]);
			Assert.AreEqual(EventsCollector.Removed, eventCollector.Events[3]);
		}

		/// <summary>
		/// collects events in an array list, used for ensuring we are cleaning up the parent kernel
		/// event subscriptions correctly.
		/// </summary>
		private class EventsCollector
		{
			public const string Added = "added";
			public const string Removed = "removed";

			private List<string> events;
			private object expectedSender;

			public List<string> Events
			{
				get { return events; }
			}

			public EventsCollector(object expectedSender)
			{
				this.expectedSender = expectedSender;
				events = new List<string>();
			}

			public void AddedAsChildKernel(object sender, EventArgs e)
			{
				Assert.AreEqual(expectedSender, sender);
				events.Add(Added);
			}

			public void RemovedAsChildKernel(object sender, EventArgs e)
			{
				Assert.AreEqual(expectedSender, sender);
				events.Add(Removed);
			}
		}


	}
}
