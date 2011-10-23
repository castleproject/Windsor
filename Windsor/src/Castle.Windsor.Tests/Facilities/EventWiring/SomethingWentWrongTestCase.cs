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

namespace CastleTests.Facilities.EventWiring
{
	using System;

	using Castle.Facilities.EventWiring;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;

	using CastleTests.Facilities.EventWiring.Model;

	using NUnit.Framework;

	[TestFixture]
	public class SomethingWentWrongTestCase : AbstractContainerTestCase
	{
		protected override void AfterContainerCreated()
		{
			Container.AddFacility<EventWiringFacility>();
		}

		[Test]
		public void Helpful_exception_is_thrown_when_publishing_to_non_existing_subscribers()
		{
			Container.Register(Component.For<SimplePublisher>()
			                   	.PublishEvent("Event",
			                   	              x => x.To<SimpleListener>()
			                   	                   	.To("nonExistingListener")));

			var exception = Assert.Throws<HandlerException>(() => Container.Resolve<SimplePublisher>());

			var message =
				string.Format(
					"Can't create component 'CastleTests.Facilities.EventWiring.Model.SimplePublisher' as it has dependencies to be satisfied.{0}{0}'CastleTests.Facilities.EventWiring.Model.SimplePublisher' is waiting for the following dependencies:{0}- Component 'CastleTests.Facilities.EventWiring.Model.SimpleListener' (via override) which was not found. Did you forget to register it or misspelled the name? If the component is registered and override is via type make sure it doesn't have non-default name assigned explicitly or override the dependency via name.{0}- Component 'nonExistingListener' (via override) which was not found. Did you forget to register it or misspelled the name? If the component is registered and override is via type make sure it doesn't have non-default name assigned explicitly or override the dependency via name.{0}",
					Environment.NewLine);

			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Helpful_exception_is_thrown_when_susbcribing_to_a_non_existing_event()
		{
			Container.Register(Component.For<SimplePublisher>().PublishEvent("NonExistingEvent", x => x.To<SimpleListener>("OnPublish")),
			                   Component.For<SimpleListener>());

			var exception = Assert.Throws<EventWiringException>(() => Container.Resolve<SimplePublisher>());
			var message =
				"Could not find event 'NonExistingEvent' on component 'CastleTests.Facilities.EventWiring.Model.SimplePublisher'. Make sure you didn't misspell the name.";
			Assert.AreEqual(message, exception.Message);
		}

		[Test]
		public void Helpful_exception_is_thrown_when_susbcribing_with_a_non_existing_method()
		{
			Container.Register(Component.For<SimplePublisher>().PublishEvent("Event", x => x.To<SimpleListener>("NonExistingHandlerMethod")),
			                   Component.For<SimpleListener>());

			var exception = Assert.Throws<EventWiringException>(() => Container.Resolve<SimplePublisher>());
			var message =
				"Could not find method 'NonExistingHandlerMethod' on component 'CastleTests.Facilities.EventWiring.Model.SimpleListener' to handle event 'Event' published by component 'CastleTests.Facilities.EventWiring.Model.SimplePublisher'. Make sure you didn't misspell the name.";
			Assert.AreEqual(message, exception.Message);
		}
	}
}