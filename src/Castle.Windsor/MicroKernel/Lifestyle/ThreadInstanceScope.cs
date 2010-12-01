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

	using Castle.MicroKernel.Context;

	public class ThreadInstanceScope : IInstanceScope
	{
		[ThreadStatic]
		private static IDictionary<IComponentActivator, object> perThreadLookup;

		private readonly IKernelEvents events;
		private readonly IDictionary<IComponentActivator, object> perContainerLookup = new Dictionary<IComponentActivator, object>();

		public ThreadInstanceScope(IKernelEvents events)
		{
			this.events = events;
			events.ContainerDisposed += CleanUp;
		}

		public object GetInstance(CreationContext context, IComponentActivator activator)
		{
			if (perThreadLookup == null)
			{
				perThreadLookup = new Dictionary<IComponentActivator, object>();
			}
			object instance;
			if (!perThreadLookup.TryGetValue(activator, out instance))
			{
				instance = activator.Create(context);
				perThreadLookup.Add(activator, instance);
				perContainerLookup.Add(activator, instance);
			}

			return instance;
		}

		private void CleanUp(object sender, EventArgs e)
		{
			foreach (var instance in perContainerLookup)
			{
				instance.Key.Destroy(instance.Value);
				perThreadLookup.Remove(instance.Key);
			}
			perThreadLookup.Clear();
			events.ContainerDisposed -= CleanUp;
		}
	}
}