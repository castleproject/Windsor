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

namespace Castle.Facilities.EventWiring
{
	using Castle.Core.Configuration;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	public class EventWiringDescriptor<T>:ComponentDescriptor<T>
	{
		private readonly string eventName;
		private readonly EventSubscriber[] subscribers;

		protected internal override void ApplyToConfiguration(IKernel kernel, IConfiguration configuration)
		{
			var node = GetSubscribersNode(configuration);
			foreach (var eventSubscriber in subscribers)
			{
				var child = Child.ForName("subscriber").Eq(
					Attrib.ForName("id").Eq(eventSubscriber.SubscriberComponentName),
					Attrib.ForName("event").Eq(eventName),
					Attrib.ForName("handler").Eq(EventHandlerMethodName(eventSubscriber)));
				child.ApplyTo(node);
			}
		}

		private string EventHandlerMethodName(EventSubscriber eventSubscriber)
		{
			return eventSubscriber.EventHandler ?? ("On" + eventName);
		}

		private IConfiguration GetSubscribersNode(IConfiguration configuration)
		{
			var node = configuration.Children["subscribers"];
			if(node==null)
			{
				node = new MutableConfiguration("subscribers");
				configuration.Children.Add(node);
			}
			return node;
		}

		public EventWiringDescriptor(string eventName, EventSubscriber[] subscribers)
		{
			this.eventName = eventName;
			this.subscribers = subscribers;
		}
	}
}