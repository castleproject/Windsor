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
	using Castle.MicroKernel.Context;

	[Serializable]
	public class DefaultPool : IPool, IDisposable
	{
		private readonly Stack<Burden> available;
		private readonly IComponentActivator componentActivator;
		private readonly Dictionary<object, Burden> inUse = new Dictionary<object, Burden>();
		private readonly int initialSize;
		private readonly int maxsize;
		private readonly Lock rwlock = Lock.Create();
		private bool initialized;

		public DefaultPool(int initialSize, int maxsize, IComponentActivator componentActivator)
		{
			available = new Stack<Burden>(initialSize);
			this.initialSize = initialSize;
			this.maxsize = maxsize;
			this.componentActivator = componentActivator;
		}

		public virtual void Dispose()
		{
			foreach (var burden in available)
			{
				burden.Release();
			}
			inUse.Clear();
			available.Clear();

		}

		public virtual bool Release(object instance)
		{
			using (rwlock.ForWriting())
			{
				Burden burden;
				if (inUse.TryGetValue(instance, out burden) == false)
				{
					return false;
				}
				inUse.Remove(instance);

				if (available.Count < maxsize)
				{
					if (instance is IRecyclable)
					{
						(instance as IRecyclable).Recycle();
					}

					available.Push(burden);
					return false;
				}
				// Pool is full
				componentActivator.Destroy(instance);
				return true;
			}
		}

		public virtual object Request(CreationContext context, Func<CreationContext, Burden> creationCallback)
		{
			using (rwlock.ForWriting())
			{
				if (!initialized)
				{
					Intitialize(creationCallback, context);
				}

				Burden burden;
				if (available.Count != 0)
				{
					burden = available.Pop();
					context.AttachExistingBurden(burden);
				}
				else
				{
					burden = creationCallback.Invoke(context);
				}
				try
				{
					inUse.Add(burden.Instance, burden);
				}
				catch (NullReferenceException)
				{
					throw new PoolException("creationCallback didn't return a valid burden");
				}
				catch (ArgumentNullException)
				{
					throw new PoolException("burden returned by creationCallback does not have root instance associated with it (its Instance property is null).");
				}
				return burden.Instance;
			}
		}

		protected virtual void Intitialize(Func<CreationContext, Burden> createCallback, CreationContext c)
		{
			initialized = true;
			for (var i = 0; i < initialSize; i++)
			{
				var burden = createCallback(c);
				available.Push(burden);
			}
		}
	}
}