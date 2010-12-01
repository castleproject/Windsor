// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

	public class SingletonInstanceScope : IInstanceScope
	{
		private readonly IDictionary<IComponentActivator, object> instanceCache = new Dictionary<IComponentActivator, object>();

		private readonly IKernelEvents events;

		public SingletonInstanceScope(IKernelEvents events)
		{
			this.events = events;
			events.ContainerDisposed += CleanUp;
		}

		private void CleanUp(object sender, EventArgs e)
		{
			foreach (var instance in instanceCache)
			{
				instance.Key.Destroy(instance.Value);
			}
			instanceCache.Clear();
			events.ContainerDisposed -= CleanUp;
		}

		public object GetInstance(CreationContext context, IComponentActivator activator)
		{
			var instanceFromContext = context.GetContextualProperty(activator);
			if (instanceFromContext != null)
			{
				//we've been called recursively, by some dependency from base.Resolve call
				return instanceFromContext;
			}
			object instance;
			if(instanceCache.TryGetValue(activator,out instance))
			{
				return instance;
			}

			lock (activator)
			{
				if (instanceCache.TryGetValue(activator, out instance))
				{
					return instance;
				}
				instance = activator.Create(context);
				instanceCache[activator] = instance;
			}
			return instance;
		}
	}

	/// <summary>
	/// Summary description for SingletonLifestyleManager.
	/// </summary>
	[Serializable]
	public class SingletonLifestyleManager : AbstractLifestyleManager
	{
		private readonly IInstanceScope scope;

		public SingletonLifestyleManager(IInstanceScope scope)
		{
			this.scope = scope;
		}

		public override void Dispose()
		{
		}

		public override object Resolve(CreationContext context)
		{
			return scope.GetInstance(context, ComponentActivator);
		}

		public override bool Release(object instance)
		{
			// Do nothing
			return false;
		}
	}
}
