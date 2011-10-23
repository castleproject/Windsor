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

namespace Castle.Facilities.EventWiring
{
	using System;
	using System.Collections;
	using System.Reflection;

	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;

	///<summary>
	///  Facility to allow components to dynamically subscribe to events offered by 
	///  other components. We call the component that offers events publishers and 
	///  the components that uses them, subscribers.
	///</summary>
	///<remarks>
	///  A component that wish to subscribe to an event must use the external configuration
	///  adding a node <c>subscribers</c> on the publisher. This node can have multiple entries using the 
	///  <c>subscriber</c> node.
	///</remarks>
	///<example>
	///  <para>This example shows two simple components: one is the event publisher and the other is the 
	///    subscriber. The subscription will be done by the facility, using the publisher associated configuration.</para>
	///  <para>The Publisher class:</para>
	///  <code>
	///    public class SimplePublisher
	///    {
	///    public event PublishEventHandler Event;
	///
	///    public void Trigger()
	///    {
	///    if (Event != null)
	///    {
	///    Event(this, new EventArgs()); 
	///    }
	///    }
	///    }
	///  </code>
	///  <para>The Subscriber class:</para>
	///  <code>
	///    public class SimpleListener
	///    {
	///    private bool _listened;
	///    private object _sender;
	/// 
	///    public void OnPublish(object sender, EventArgs e)
	///    {
	///    _sender = sender; 
	///    _listened = sender != null;
	///    }
	/// 
	///    public bool Listened
	///    {
	///    get { return _listened;	}
	///    }
	/// 
	///    public object Sender
	///    {
	///    get { return _sender; }
	///    }
	///    }
	///  </code>
	///  <para>The configuration file:</para>
	///  <code>
	///    <![CDATA[
	/// <?xml version="1.0" encoding="utf-8" ?>
	/// <configuration>
	/// 	<facilities>
	/// 		<facility 
	/// 			id="event.wiring"
	/// 			type="Castle.Facilities.EventWiring.EventWiringFacility, Castle.Windsor" />
	/// 	</facilities>
	/// 
	/// 	<components>
	/// 		<component 
	/// 			id="SimpleListener" 
	/// 			type="Castle.Facilities.EventWiring.Tests.Model.SimpleListener, Castle.Facilities.EventWiring.Tests" />
	/// 
	/// 		<component 
	/// 			id="SimplePublisher" 
	/// 			type="Castle.Facilities.EventWiring.Tests.Model.SimplePublisher, Castle.Facilities.EventWiring.Tests" >
	/// 			<subscribers>
	/// 				<subscriber id="SimpleListener" event="Event" handler="OnPublish"/>
	/// 			</subscribers>
	/// 		</component>
	/// 	</components>
	/// </configuration>
	/// ]]>
	///  </code>
	///</example>
	public class EventWiringFacility : AbstractFacility
	{
		internal const string SubscriberList = "evts.subscriber.list";

		/// <summary>
		///   Overridden. Initializes the facility, subscribing to the <see cref = "IKernelEvents.ComponentModelCreated" />,
		///   <see cref = "IKernelEvents.ComponentCreated" />, <see cref = "IKernelEvents.ComponentDestroyed" /> Kernel events.
		/// </summary>
		protected override void Init()
		{
			Kernel.ComponentModelBuilder.AddContributor(new EventWiringInspector());
			Kernel.ComponentCreated += OnComponentCreated;
			Kernel.ComponentDestroyed += OnComponentDestroyed;
		}

		private bool IsPublisher(ComponentModel model)
		{
			return model.ExtendedProperties[SubscriberList] != null;
		}

		/// <summary>
		///   Checks if the component we're dealing is a publisher. If it is, 
		///   iterates the subscribers starting them and wiring the events.
		/// </summary>
		/// <param name = "model">The component model.</param>
		/// <param name = "instance">The instance representing the component.</param>
		/// <exception cref = "EventWiringException">When the subscriber is not found
		///   <br /> or <br />
		///   The handler method isn't found
		///   <br /> or <br />
		///   The event isn't found
		/// </exception>
		private void OnComponentCreated(ComponentModel model, object instance)
		{
			if (IsPublisher(model))
			{
				WirePublisher(model, instance);
			}
		}

		private void OnComponentDestroyed(ComponentModel model, object instance)
		{
			// TODO: Remove Listener
		}

		private void StartAndWirePublisherSubscribers(ComponentModel model, object publisher)
		{
			var subscribers = (IDictionary)model.ExtendedProperties[SubscriberList];

			if (subscribers == null)
			{
				return;
			}

			foreach (DictionaryEntry subscriberInfo in subscribers)
			{
				var subscriberKey = (string)subscriberInfo.Key;

				var wireInfoList = (IList)subscriberInfo.Value;

				var handler = Kernel.GetHandler(subscriberKey);

				AssertValidHandler(handler, subscriberKey);

				object subscriberInstance;

				try
				{
					subscriberInstance = Kernel.Resolve<object>(subscriberKey);
				}
				catch (Exception ex)
				{
					throw new EventWiringException("Failed to start subscriber " + subscriberKey, ex);
				}

				var publisherType = publisher.GetType();

				foreach (WireInfo wireInfo in wireInfoList)
				{
					var eventName = wireInfo.EventName;

					//TODO: Caching of EventInfos.
					var eventInfo = publisherType.GetEvent(eventName,
					                                       BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					if (eventInfo == null)
					{
						throw new EventWiringException(
							string.Format("Could not find event '{0}' on component '{1}'. Make sure you didn't misspell the name.", eventName, model.Name));
					}

					var handlerMethod = subscriberInstance.GetType().GetMethod(wireInfo.Handler,
					                                                           BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

					if (handlerMethod == null)
					{
						throw new EventWiringException(
							string.Format(
								"Could not find method '{0}' on component '{1}' to handle event '{2}' published by component '{3}'. Make sure you didn't misspell the name.",
								wireInfo.Handler,
								subscriberKey,
								eventName,
								model.Name));
					}

					var delegateHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType, subscriberInstance, wireInfo.Handler);

					eventInfo.AddEventHandler(publisher, delegateHandler);
				}
			}
		}

		private void WirePublisher(ComponentModel model, object publisher)
		{
			StartAndWirePublisherSubscribers(model, publisher);
		}

		private static void AssertValidHandler(IHandler handler, string subscriberKey)
		{
			if (handler == null)
			{
				throw new EventWiringException("Publisher tried to start subscriber " + subscriberKey + " that was not found");
			}

			if (handler.CurrentState == HandlerState.WaitingDependency)
			{
				throw new EventWiringException("Publisher tried to start subscriber " + subscriberKey + " that is waiting for a dependency");
			}
		}
	}

	/// <summary>
	///   Represents the information about an event.
	/// </summary>
	internal class WireInfo
	{
		private readonly String eventName;

		private readonly String handler;

		/// <summary>
		///   Initializes a new instance of the <see cref = "WireInfo" /> class.
		/// </summary>
		/// <param name = "eventName">Name of the event.</param>
		/// <param name = "handler">The name of the handler method.</param>
		public WireInfo(string eventName, string handler)
		{
			this.eventName = eventName;
			this.handler = handler;
		}

		/// <summary>
		///   Gets the name of the event.
		/// </summary>
		/// <value>The name of the event.</value>
		public string EventName
		{
			get { return eventName; }
		}

		/// <summary>
		///   Gets the handler method name.
		/// </summary>
		/// <value>The handler.</value>
		public string Handler
		{
			get { return handler; }
		}

		/// <summary>
		///   Determines whether the specified <see cref = "T:System.Object"></see> is equal to the current <see
		///    cref = "T:System.Object"></see>.
		/// </summary>
		/// <param name = "obj">The <see cref = "T:System.Object"></see> to compare with the current <see cref = "T:System.Object"></see>.</param>
		/// <returns>
		///   true if the specified <see cref = "T:System.Object"></see> is equal to the current <see cref = "T:System.Object"></see>; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}

			var wireInfo = obj as WireInfo;
			if (wireInfo == null)
			{
				return false;
			}

			if (!Equals(eventName, wireInfo.eventName))
			{
				return false;
			}

			if (!Equals(handler, wireInfo.handler))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		///   Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		///   A hash code for the current <see cref = "T:System.Object"></see>.
		/// </returns>
		public override int GetHashCode()
		{
			return eventName.GetHashCode() + 29*handler.GetHashCode();
		}
	}
}