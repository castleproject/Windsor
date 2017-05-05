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
	using System.Linq.Expressions;

	public class EventSubscriber
	{
		private readonly string subscriberComponentName;
		private string eventHandler;

		private EventSubscriber(string subscriberComponentName)
		{
			this.subscriberComponentName = subscriberComponentName;
		}

		public string EventHandler
		{
			get { return eventHandler; }
		}

		public string SubscriberComponentName
		{
			get { return subscriberComponentName; }
		}

		public EventSubscriber HandledBy(string eventHandlerMethodName)
		{
			eventHandler = eventHandlerMethodName;
			return this;
		}

		public EventSubscriber HandledBy<THandler>(Expression<Action<THandler>> methodHandler)
		{
			eventHandler = ExtractMethodName(methodHandler);
			return this;
		}

		private string ExtractMethodName<THandler>(Expression<Action<THandler>> methodHandler)
		{
			var expression = methodHandler.Body as MethodCallExpression;
			if (expression != null)
			{
				return expression.Method.Name;
			}
			throw new ArgumentException(
				"Couldn't extract method to handle the event from given expression. Expression should point to method that ought to handle subscribed event, something like: 's => s.HandleClick(null, null)'.");
		}

		public static EventSubscriber Named(string subscriberComponentName)
		{
			return new EventSubscriber(subscriberComponentName);
		}
	}
}