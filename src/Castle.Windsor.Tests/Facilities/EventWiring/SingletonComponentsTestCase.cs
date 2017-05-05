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


#if (!SILVERLIGHT)

namespace CastleTests.Facilities.EventWiring
{
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests;
	using Castle.XmlFiles;

	using CastleTests.Facilities.EventWiring.Model;

	using NUnit.Framework;

	[TestFixture]
	public class SingletonComponentsTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.Install(Configuration.FromXml(Xml.Embedded("EventWiringFacility/singleton.config")));
		}

		[Test]
		public void MultiEvents()
		{
			var listener = Container.Resolve<MultiListener>("MultiListener");
			var publisherThatListens = Container.Resolve<PublisherListener>("PublisherListener");

			publisherThatListens.Trigger1();
			Assert.IsTrue(listener.Listened);
			Assert.AreSame(publisherThatListens, listener.Sender);

			listener.Reset();
			Assert.IsFalse(publisherThatListens.Listened);
			Assert.IsNull(publisherThatListens.Sender);

			var anotherPublisher = Container.Resolve<SimplePublisher>("SimplePublisher");

			anotherPublisher.Trigger();
			Assert.IsTrue(publisherThatListens.Listened);
			Assert.AreSame(anotherPublisher, publisherThatListens.Sender);

			listener.Reset();
			Assert.IsFalse(listener.Listened);
			Assert.IsNull(listener.Sender);

			var publisher = Container.Resolve<MultiPublisher>("MultiPublisher");
			publisher.Trigger1();
			Assert.IsTrue(listener.Listened);
			Assert.AreSame(publisher, listener.Sender);

			listener.Reset();
			Assert.IsFalse(listener.Listened);
			Assert.IsNull(listener.Sender);

			publisher.Trigger2();
			Assert.IsTrue(listener.Listened);
			Assert.AreSame(publisher, listener.Sender);
		}

		[Test]
		public void TriggerSimple()
		{
			var publisher = Container.Resolve<SimplePublisher>("SimplePublisher");
			var listener = Container.Resolve<SimpleListener>("SimpleListener");

			Assert.IsFalse(listener.Listened);
			Assert.IsNull(listener.Sender);

			publisher.Trigger();

			Assert.IsTrue(listener.Listened);
			Assert.AreSame(publisher, listener.Sender);
		}

		[Test]
		public void TriggerStaticEvent()
		{
			var publisher = Container.Resolve<SimplePublisher>("SimplePublisher");
			var listener = Container.Resolve<SimpleListener>("SimpleListener2");

			Assert.IsFalse(listener.Listened);
			Assert.IsNull(listener.Sender);

			publisher.StaticTrigger();

			Assert.IsTrue(listener.Listened);
			Assert.AreSame(publisher, listener.Sender);
		}

		[Test]
		public void TriggerThenResolve()
		{
			var publisher = Container.Resolve<SimplePublisher>("SimplePublisher");

			publisher.Trigger();

			var listener = Container.Resolve<SimpleListener>("SimpleListener");
			Assert.IsTrue(listener.Listened);
			Assert.AreSame(publisher, listener.Sender);
		}
	}
}

#endif