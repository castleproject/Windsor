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

namespace Castle.Facilities.EventWiring.Tests
{
	using Castle.Facilities.EventWiring.Tests.Model;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class FluentRegistrationTestCase
	{
		private IWindsorContainer container;

		[SetUp]
		public void SetUp()
		{
			container = new WindsorContainer();
			container.AddFacility<EventWiringFacility>();
		}

		[Test]
		public void Single_publisher_single_subscriber_single_event()
		{
			container.Register(
				Component.For<SimplePublisher>()
					.PublishEvent(p => p.Event += null,
					              x => x.To<SimpleListener>("foo", l => l.OnPublish(null, null))),
				Component.For<SimpleListener>().Named("foo"));

			var subscriber = container.Resolve<SimpleListener>("foo");
			var publisher = container.Resolve<SimplePublisher>();

			publisher.Trigger();

			Assert.IsTrue(subscriber.Listened);
			Assert.AreSame(publisher, subscriber.Sender);
		}

		[Test]
		public void Can_specify_event_as_string()
		{
			container.Register(
				Component.For<SimplePublisher>()
					.PublishEvent("Event",
								  x => x.To<SimpleListener>("foo", l => l.OnPublish(null, null))),
				Component.For<SimpleListener>().Named("foo"));

			var subscriber = container.Resolve<SimpleListener>("foo");
			var publisher = container.Resolve<SimplePublisher>();

			publisher.Trigger();

			Assert.IsTrue(subscriber.Listened);
			Assert.AreSame(publisher, subscriber.Sender);
		}

		[Test]
		public void Can_specify_handler_as_string()
		{
			container.Register(
				Component.For<SimplePublisher>()
					.PublishEvent("Event",
								  x => x.To("foo", "OnPublish")),
				Component.For<SimpleListener>().Named("foo"));

			var subscriber = container.Resolve<SimpleListener>("foo");
			var publisher = container.Resolve<SimplePublisher>();

			publisher.Trigger();

			Assert.IsTrue(subscriber.Listened);
			Assert.AreSame(publisher, subscriber.Sender);
		}

		[Test]
		public void Not_specifying_handler_name_uses_OnEVENTNAME_method()
		{
			container.Register(
				Component.For<SimplePublisher>()
					.PublishEvent(p => p.Event += null,
					              x => x.To("foo")),
				Component.For<ListenerWithOnEventMethod>().Named("foo"));

			var subscriber = container.Resolve<ListenerWithOnEventMethod>("foo");
			var publisher = container.Resolve<SimplePublisher>();

			publisher.Trigger();

			Assert.IsTrue(subscriber.Listened);
			Assert.AreSame(publisher, subscriber.Sender);
		}

		[Test]
		public void Can_specify_multiple_subscribers()
		{
			container.Register(
				Component.For<SimplePublisher>()
					.PublishEvent(p => p.Event += null,
					              x => x.To("foo")
					                   	.To<SimpleListener>("bar", l => l.OnPublish(null, null))),
				Component.For<ListenerWithOnEventMethod>().Named("foo"),
				Component.For<SimpleListener>().Named("bar"));

			var subscriber1 = container.Resolve<ListenerWithOnEventMethod>("foo");
			var subscriber2 = container.Resolve<SimpleListener>("bar");
			var publisher = container.Resolve<SimplePublisher>();

			publisher.Trigger();

			Assert.IsTrue(subscriber1.Listened);
			Assert.AreSame(publisher, subscriber1.Sender);
			Assert.IsTrue(subscriber2.Listened);
			Assert.AreSame(publisher, subscriber2.Sender);
		}

		[Test]
		public void Can_publish_static_events()
		{
			container.Register(
				Component.For<SimplePublisher>()
					.PublishEvent(p => SimplePublisher.StaticEvent += null,
					              x => x.To<SimpleListener>("bar", l => l.OnPublish(null, null))),
				Component.For<SimpleListener>().Named("bar"));

			var listener = container.Resolve<SimpleListener>("bar");
			var publisher = container.Resolve<SimplePublisher>();

			publisher.StaticTrigger();

			Assert.IsTrue(listener.Listened);
			Assert.AreSame(publisher, listener.Sender);
		}

		[Test]
		public void Can_specify_multiple_events()
		{
			container.Register(
				Component.For<SimplePublisher>()
					.PublishEvent(p => p.Event += null,
					              x => x.To("foo"))
					.PublishEvent(p => SimplePublisher.StaticEvent += null,
					              x => x.To<SimpleListener>("bar", l => l.OnPublish(null, null))),
				Component.For<ListenerWithOnEventMethod>().Named("foo"),
				Component.For<SimpleListener>().Named("bar"));

			var subscriber1 = container.Resolve<ListenerWithOnEventMethod>("foo");
			var subscriber2 = container.Resolve<SimpleListener>("bar");
			var publisher = container.Resolve<SimplePublisher>();

			publisher.Trigger();

			Assert.IsTrue(subscriber1.Listened);
			Assert.AreSame(publisher, subscriber1.Sender);

			publisher.StaticTrigger();

			Assert.IsTrue(subscriber2.Listened);
			Assert.AreSame(publisher, subscriber2.Sender);
		}
	}
}