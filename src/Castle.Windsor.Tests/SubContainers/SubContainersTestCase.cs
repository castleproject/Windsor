// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace CastleTests.SubContainers
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor.Tests.Components;

	using CastleTests;
	using CastleTests.Components;

	using NUnit.Framework;

	/// <summary>
	///   Summary description for SubContainersTestCase.
	/// </summary>
	[TestFixture]
	public class SubContainersTestCase : AbstractContainerTestCase
	{
		/// <summary>
		///   collects events in an array list, used for ensuring we are cleaning up the parent kernel
		///   event subscriptions correctly.
		/// </summary>
		private class EventsCollector
		{
			public const string Added = "added";
			public const string Removed = "removed";

			private readonly List<string> events;
			private readonly object expectedSender;

			public EventsCollector(object expectedSender)
			{
				this.expectedSender = expectedSender;
				events = new List<string>();
			}

			public List<string> Events
			{
				get { return events; }
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

		[Test]
		public void AddChildKernelToTwoParentsThrowsException()
		{
			var expectedMessage = "You can not change the kernel parent once set, use the RemoveChildKernel and AddChildKernel methods together to achieve this.";

			IKernel kernel2 = new DefaultKernel();

			IKernel subkernel = new DefaultKernel();

			Kernel.AddChildKernel(subkernel);
			Assert.AreEqual(Kernel, subkernel.Parent);

			var exception = Assert.Throws<KernelException>(() => kernel2.AddChildKernel(subkernel));
			Assert.AreEqual(exception.Message, expectedMessage);
		}

		[Test]
		public void ChildDependenciesIsSatisfiedEvenWhenComponentTakesLongToBeAddedToParentContainer()
		{
			var container = new DefaultKernel();
			var childContainer = new DefaultKernel();

			container.AddChildKernel(childContainer);
			childContainer.Register(Component.For(typeof(UsesIEmptyService)).Named("component"));

			container.Register(
				Component.For(typeof(IEmptyService)).ImplementedBy(typeof(EmptyServiceA)).Named("service1"));

			childContainer.Resolve<UsesIEmptyService>();
		}

		[Test]
		public void ChildDependenciesSatisfiedAmongContainers()
		{
			IKernel subkernel = new DefaultKernel();

			Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));
			Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));

			Kernel.AddChildKernel(subkernel);
			subkernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));

			var spamservice = subkernel.Resolve<DefaultSpamService>("spamservice");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void ChildKernelFindsAndCreateParentComponent()
		{
			IKernel subkernel = new DefaultKernel();

			Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			Kernel.AddChildKernel(subkernel);

			Assert.IsTrue(subkernel.HasComponent(typeof(DefaultTemplateEngine)));
			Assert.IsNotNull(subkernel.Resolve<DefaultTemplateEngine>());
		}

		[Test]
		public void ChildKernelOverloadsParentKernel1()
		{
			var instance1 = new DefaultTemplateEngine();
			var instance2 = new DefaultTemplateEngine();

			// subkernel added with already registered components that overload parent components.

			IKernel subkernel = new DefaultKernel();
			subkernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance1));
			Assert.AreEqual(instance1, subkernel.Resolve<DefaultTemplateEngine>("engine"));

			Kernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance2));
			Assert.AreEqual(instance2, Kernel.Resolve<DefaultTemplateEngine>("engine"));

			Kernel.AddChildKernel(subkernel);
			Assert.AreEqual(instance1, subkernel.Resolve<DefaultTemplateEngine>("engine"));
			Assert.AreEqual(instance2, Kernel.Resolve<DefaultTemplateEngine>("engine"));
		}

		[Test]
		public void ChildKernelOverloadsParentKernel2()
		{
			var instance1 = new DefaultTemplateEngine();
			var instance2 = new DefaultTemplateEngine();

			IKernel subkernel = new DefaultKernel();
			Kernel.AddChildKernel(subkernel);

			// subkernel added first, then populated with overloaded components after

			Kernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance2));
			Assert.AreEqual(instance2, Kernel.Resolve<DefaultTemplateEngine>("engine"));
			Assert.AreEqual(instance2, subkernel.Resolve<DefaultTemplateEngine>("engine"));

			subkernel.Register(Component.For<DefaultTemplateEngine>().Named("engine").Instance(instance1));
			Assert.AreEqual(instance1, subkernel.Resolve<DefaultTemplateEngine>("engine"));
			Assert.AreEqual(instance2, Kernel.Resolve<DefaultTemplateEngine>("engine"));
		}

		[Test]
		public void DependenciesSatisfiedAmongContainers()
		{
			IKernel subkernel = new DefaultKernel();

			Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			Kernel.AddChildKernel(subkernel);

			subkernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));

			var spamservice = subkernel.Resolve<DefaultSpamService>("spamservice");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void DependenciesSatisfiedAmongContainersUsingEvents()
		{
			IKernel subkernel = new DefaultKernel();

			subkernel.Register(Component.For(typeof(DefaultSpamServiceWithConstructor)).Named("spamservice"));

			Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			Kernel.AddChildKernel(subkernel);

			var spamservice =
				subkernel.Resolve<DefaultSpamServiceWithConstructor>("spamservice");

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.MailSender);
			Assert.IsNotNull(spamservice.TemplateEngine);
		}

		[Test]
		public void ParentKernelFindsAndCreateChildComponent()
		{
			IKernel subkernel = new DefaultKernel();

			subkernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			Kernel.AddChildKernel(subkernel);

			Assert.IsFalse(Kernel.HasComponent(typeof(DefaultTemplateEngine)));

			Assert.Throws<ComponentNotFoundException>(() => Kernel.Resolve<DefaultTemplateEngine>());
		}

		[Test]
		public void RemoveChildKernelCleansUp()
		{
			IKernel subkernel = new DefaultKernel();
			var eventCollector = new EventsCollector(subkernel);
			subkernel.RemovedAsChildKernel += eventCollector.RemovedAsChildKernel;
			subkernel.AddedAsChildKernel += eventCollector.AddedAsChildKernel;

			Kernel.AddChildKernel(subkernel);
			Assert.AreEqual(Kernel, subkernel.Parent);
			Assert.AreEqual(1, eventCollector.Events.Count);
			Assert.AreEqual(EventsCollector.Added, eventCollector.Events[0]);

			Kernel.RemoveChildKernel(subkernel);
			Assert.IsNull(subkernel.Parent);
			Assert.AreEqual(2, eventCollector.Events.Count);
			Assert.AreEqual(EventsCollector.Removed, eventCollector.Events[1]);
		}

		[Test]
		public void RemovingChildKernelUnsubscribesFromParentEvents()
		{
			IKernel subkernel = new DefaultKernel();
			var eventCollector = new EventsCollector(subkernel);
			subkernel.RemovedAsChildKernel += eventCollector.RemovedAsChildKernel;
			subkernel.AddedAsChildKernel += eventCollector.AddedAsChildKernel;

			Kernel.AddChildKernel(subkernel);
			Kernel.RemoveChildKernel(subkernel);
			Kernel.AddChildKernel(subkernel);
			Kernel.RemoveChildKernel(subkernel);

			Assert.AreEqual(4, eventCollector.Events.Count);
			Assert.AreEqual(EventsCollector.Added, eventCollector.Events[0]);
			Assert.AreEqual(EventsCollector.Removed, eventCollector.Events[1]);
			Assert.AreEqual(EventsCollector.Added, eventCollector.Events[2]);
			Assert.AreEqual(EventsCollector.Removed, eventCollector.Events[3]);
		}

		[Test]
		[Ignore(
			"Support for this was removed due to issues with scoping (SimpleComponent1 would become visible from parent container)."
			)]
		public void Requesting_parent_component_with_child_dependency_from_child_component()
		{
			var subkernel = new DefaultKernel();
			Kernel.AddChildKernel(subkernel);

			Kernel.Register(Component.For<UsesSimpleComponent1>());
			subkernel.Register(Component.For<SimpleComponent1>());

			subkernel.Resolve<UsesSimpleComponent1>();
		}

		[Test]
		public void Parent_component_will_NOT_have_dependencies_from_child()
		{
			Kernel.Register(Component.For<DefaultTemplateEngine>(),
			                Component.For<DefaultSpamService>());

			var child = new DefaultKernel();
			Kernel.AddChildKernel(child);

			child.Register(Component.For<DefaultMailSenderService>());

			var spamservice = child.Resolve<DefaultSpamService>();

			Assert.IsNotNull(spamservice);
			Assert.IsNotNull(spamservice.TemplateEngine);
			Assert.IsNull(spamservice.MailSender);
		}

		[Test]
		public void Singleton_withNonSingletonDependencies_doesNotReResolveDependencies()
		{
			Kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice"));
			Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));

			var subkernel1 = new DefaultKernel();
			subkernel1.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));
			Kernel.AddChildKernel(subkernel1);

			var subkernel2 = new DefaultKernel();
			subkernel2.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine")
				                    .LifeStyle.Is(LifestyleType.Transient));
			Kernel.AddChildKernel(subkernel2);

			var templateengine1 = subkernel1.Resolve<DefaultTemplateEngine>("templateengine");
			var spamservice1 = subkernel1.Resolve<DefaultSpamService>("spamservice");

			Assert.IsNull(spamservice1.TemplateEngine);

			var templateengine2 = subkernel2.Resolve<DefaultTemplateEngine>("templateengine");
			var spamservice2 = subkernel2.Resolve<DefaultSpamService>("spamservice");

			Assert.AreSame(spamservice1, spamservice2);
		}

		[Test]
		[Ignore(
			"Support for this was removed due to issues with scoping (SimpleComponent1 would become visible from parent container)."
			)]
		public void Three_level_hierarchy([Values(0, 1, 2)] int parentComponentContainer,
		                                  [Values(0, 1, 2)] int childComponentContainer)
		{
			var subKernel = new DefaultKernel();
			var subSubKernel = new DefaultKernel();
			Kernel.AddChildKernel(subKernel);
			subKernel.AddChildKernel(subSubKernel);
			var containers = new[]
			{
				Kernel,
				subKernel,
				subSubKernel
			};

			containers[parentComponentContainer].Register(Component.For<UsesSimpleComponent1>());
			containers[childComponentContainer].Register(Component.For<SimpleComponent1>());

			subSubKernel.Resolve<UsesSimpleComponent1>();
		}

		[Test]
		[Bug("IOC-345")]
		public void Do_NOT_UseChildComponentsForParentDependenciesWhenRequestedFromChild()
		{
			IKernel subkernel = new DefaultKernel();

			Kernel.Register(Component.For(typeof(DefaultSpamService)).Named("spamservice").LifeStyle.Is(LifestyleType.Transient));
			Kernel.Register(Component.For(typeof(DefaultMailSenderService)).Named("mailsender"));
			Kernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			Kernel.AddChildKernel(subkernel);
			subkernel.Register(Component.For(typeof(DefaultTemplateEngine)).Named("templateengine"));

			var templateengine = Kernel.Resolve<DefaultTemplateEngine>("templateengine");
			var sub_templateengine = subkernel.Resolve<DefaultTemplateEngine>("templateengine");

			var spamservice = subkernel.Resolve<DefaultSpamService>("spamservice");
			Assert.AreNotEqual(spamservice.TemplateEngine, sub_templateengine);
			Assert.AreEqual(spamservice.TemplateEngine, templateengine);

			spamservice = Kernel.Resolve<DefaultSpamService>("spamservice");
			Assert.AreNotEqual(spamservice.TemplateEngine, sub_templateengine);
			Assert.AreEqual(spamservice.TemplateEngine, templateengine);
		}

		[Test]
		[Bug("IOC-325")]
		public void TryResolvingViaChildKernelShouldNotThrowException()
		{
			using (var childKernel = new DefaultKernel())
			{
				Kernel.Register(Component.For<BookStore>());
				Kernel.AddChildKernel(childKernel);
				var handler = childKernel.GetHandler(typeof(BookStore));

				// Assert setup invariant
				Assert.IsInstanceOf<ParentHandlerWrapper>(handler);

				Assert.DoesNotThrow(() => handler.TryResolve(CreationContext.CreateEmpty()));
			}
		}
	}
}