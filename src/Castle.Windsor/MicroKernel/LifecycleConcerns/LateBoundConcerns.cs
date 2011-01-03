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

namespace Castle.MicroKernel.LifecycleConcerns
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
#if DOTNET40
	using System.Collections.Concurrent;

#else
	using Castle.Core.Internal;
#endif

	/// <summary>
	///   Lifetime concern that works for components that don't have their actual type determined upfront
	/// </summary>
	[Serializable]
	public class LateBoundConcerns : ICommissionConcern, IDecommissionConcern
	{
		private IDictionary<Type, ILifecycleConcern> concerns;
#if DOTNET40
		private ConcurrentDictionary<Type, List<ILifecycleConcern>> concernsCache;
#else
		private IDictionary<Type, List<ILifecycleConcern>> concernsCache;
		private readonly Lock cacheLock = Lock.Create();

#endif

		public bool HasConcerns
		{
			get { return concerns != null; }
		}

		public void AddConcern<TForType>(ILifecycleConcern lifecycleConcern)
		{
			if (concerns == null)
			{
				concerns = new Dictionary<Type, ILifecycleConcern>(2);
#if DOTNET40
				concernsCache = new ConcurrentDictionary<Type, List<ILifecycleConcern>>(2, 2);
#else
				concernsCache = new Dictionary<Type, List<ILifecycleConcern>>(2);
#endif
			}
			concerns.Add(typeof(TForType), lifecycleConcern);
		}

		public void Apply(ComponentModel model, object component)
		{
			var componentConcerns = GetComponentConcerns(component.GetType());
			if (componentConcerns == null)
			{
				return;
			}
			componentConcerns.ForEach(c => c.Apply(model, component));
		}

		private List<ILifecycleConcern> BuildConcernCache(Type type)
		{
			var componentConcerns = new List<ILifecycleConcern>(concerns.Count);
			foreach (var concern in concerns)
			{
				if (concern.Key.IsAssignableFrom(type))
				{
					componentConcerns.Add(concern.Value);
				}
			}
			if (componentConcerns.Count > 0)
			{
				return componentConcerns;
			}
			return null;
		}

		private List<ILifecycleConcern> GetComponentConcerns(Type type)
		{
#if DOTNET40
			return concernsCache.GetOrAdd(type, BuildConcernCache);
#else
			List<ILifecycleConcern> componentConcerns;
			using(var @lock = cacheLock.ForReadingUpgradeable())
			{
				if (concernsCache.TryGetValue(type, out componentConcerns))
				{
					return componentConcerns;
				}

				@lock.Upgrade();
				if (concernsCache.TryGetValue(type, out componentConcerns))
				{
					return componentConcerns;
				}

				componentConcerns = BuildConcernCache(type);
				concernsCache.Add(type, componentConcerns);
				return componentConcerns;
			}
#endif
		}
	}
}