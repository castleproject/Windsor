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

namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	using Castle.MicroKernel.Context;

	public class ThreadInstanceScope : IInstanceScope
	{
		private readonly IDictionary<int, IDictionary<IComponentActivator, object>> allLookup = new Dictionary<int, IDictionary<IComponentActivator, object>>();

		private readonly IKernelEvents events;

		public ThreadInstanceScope(IKernelEvents events)
		{
			this.events = events;
			events.ContainerDisposed += CleanUp;
		}

		public object GetInstance(CreationContext context, IComponentActivator activator)
		{
			var threadId = Thread.CurrentThread.ManagedThreadId;
			IDictionary<IComponentActivator, object> currentThreadLookup;
			if (allLookup.TryGetValue(threadId,out currentThreadLookup) == false)
			{
				currentThreadLookup = new Dictionary<IComponentActivator, object>();
				lock(allLookup)
				{
					allLookup[threadId] = currentThreadLookup;
				}
			}

			object instance;
			if (!currentThreadLookup.TryGetValue(activator, out instance))
			{
				instance = activator.Create(context);
				currentThreadLookup.Add(activator, instance);
			}

			return instance;
		}

		private void CleanUp(object sender, EventArgs e)
		{
			foreach (var thread in allLookup.Values)
			{
				foreach (var instance in thread)
				{
					instance.Key.Destroy(instance.Value);
				}
			}
			allLookup.Clear();
			events.ContainerDisposed -= CleanUp;
		}
	}
}