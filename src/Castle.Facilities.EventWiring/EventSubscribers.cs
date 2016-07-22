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
	using System.Collections.Generic;
	using System.Linq.Expressions;

	using Castle.Core;

	/// <summary>
	///   Collects information about subscribers for given event
	/// </summary>
	public class EventSubscribers
	{
		private readonly List<EventSubscriber> subscribers = new List<EventSubscriber>(3);

		internal EventSubscriber[] Subscribers
		{
			get { return subscribers.ToArray(); }
		}

		public EventSubscribers To<TSubscriber>(string subscriberComponentName, Expression<Action<TSubscriber>> methodHandler)
		{
			subscribers.Add(EventSubscriber.Named(subscriberComponentName).HandledBy(methodHandler));
			return this;
		}

		public EventSubscribers To<TSubscriber>(Expression<Action<TSubscriber>> methodHandler)
		{
			return To(ComponentName.DefaultNameFor(typeof(TSubscriber)), methodHandler);
		}

		public EventSubscribers To<TSubscriber>(string methodHandler)
		{
			return To(ComponentName.DefaultNameFor(typeof(TSubscriber)), methodHandler);
		}

		public EventSubscribers To<TSubscriber>()
		{
			return To(ComponentName.DefaultNameFor(typeof(TSubscriber)));
		}

		public EventSubscribers To(string subscriberComponentName, string methodHandler)
		{
			subscribers.Add(EventSubscriber.Named(subscriberComponentName).HandledBy(methodHandler));
			return this;
		}

		public EventSubscribers To(string subscriberComponentName)
		{
			subscribers.Add(EventSubscriber.Named(subscriberComponentName));
			return this;
		}
	}
}