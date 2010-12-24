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

namespace Castle.MicroKernel.Lifestyle.Pool
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Internal;

	[Serializable]
	public class DefaultPool : IPool, IDisposable
	{
		private readonly Stack<object> available = new Stack<object>();
		private readonly IComponentActivator componentActivator;
		private readonly List<object> inUse = new List<object>();
		private readonly int initialSize;
		private readonly IKernel kernel;
		private readonly int maxsize;
		private readonly Lock rwlock = Lock.Create();
		private bool evicting;
		private bool initialized;

		public DefaultPool(int initialSize, int maxsize, IComponentActivator componentActivator, IKernel kernel)
		{
			this.initialSize = initialSize;
			this.maxsize = maxsize;
			this.componentActivator = componentActivator;
			this.kernel = kernel;
		}

		public virtual void Dispose()
		{
			evicting = true;

			foreach (var instance in available)
			{
				kernel.ReleaseComponent(instance);
			}
		}

		public virtual bool Release(object instance)
		{
			using (rwlock.ForWriting())
			{
				if (inUse.Contains(instance) == false)
				{
					return evicting;
				}

				inUse.Remove(instance);

				if (available.Count < maxsize)
				{
					if (instance is IRecyclable)
					{
						(instance as IRecyclable).Recycle();
					}

					available.Push(instance);
					return false;
				}
				// Pool is full
				componentActivator.Destroy(instance);
				return true;
			}
		}

		public virtual object Request(Func<object> createCallback)
		{
			object instance;

			using (rwlock.ForWriting())
			{
				if (!initialized)
				{
					Intitialize(createCallback);
				}
				if (available.Count != 0)
				{
					instance = available.Pop();

					if (instance == null)
					{
						throw new PoolException("Invalid instance on the pool stack");
					}
				}
				else
				{
					instance = createCallback.Invoke();

					if (instance == null)
					{
						throw new PoolException("Activator didn't return a valid instance");
					}
				}

				inUse.Add(instance);
			}

			return instance;
		}

		protected virtual void Intitialize(Func<object> createCallback)
		{
			var tempInstance = new List<object>();

			for (var i = 0; i < initialSize; i++)
			{
				tempInstance.Add(createCallback.Invoke());
			}

			for (var i = 0; i < initialSize; i++)
			{
				Release(tempInstance[i]);
			}
			initialized = true;
		}
	}
}