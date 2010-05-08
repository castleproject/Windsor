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
	using System;
	using System.Reflection;

	using Castle.MicroKernel.Registration;

	public static class EventWiringRegistrationExtensions
	{
		public static EventPublisher<TPublisher> PublishEvent<TPublisher>(this ComponentRegistration<TPublisher> registration, Action<TPublisher> eventSubscribtion)
		{
			var eventName = GetEventName(eventSubscribtion);
			return new EventPublisher<TPublisher>(registration, eventName);
		}

		private static string GetEventName<TPublisher>(Action<TPublisher> eventSubscribtion)
		{
			string eventName;
			try
			{
				var calledMethod = new NaiveMethodNameExtractor(eventSubscribtion).CalledMethod;
				if (calledMethod == null || (eventName = ExtractEventName(calledMethod)) == null)
				{
					throw new InvalidOperationException();
				}
			}
			catch (Exception)
			{

				throw new ArgumentException(
					"Delegate given was not a method subscribption delegate. Please use something similar to: 'publisher => publisher += null'. " +
					"If you did, than it's probably a bug. Please use the other overload and specify name of the event as string.");
			}
			return eventName;
		}

		private static string ExtractEventName(MethodBase calledMethod)
		{
			var methodName = calledMethod.Name;
			if (methodName.StartsWith("add_"))
			{
				return methodName.Substring("add_".Length);
			}
			if (methodName.StartsWith("remove_"))
			{
				return methodName.Substring("remove_".Length);
			}
			return null;
		}
	}
}