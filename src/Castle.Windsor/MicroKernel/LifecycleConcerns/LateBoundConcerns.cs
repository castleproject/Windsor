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
#if !(DOTNET35 || SILVERLIGHT)
	using System.Collections.Concurrent;
#else
	using Castle.Core.Internal;
#endif

	/// <summary>
	///   Lifetime concern that works for components that don't have their actual type determined upfront
	/// </summary>
	[Serializable]
	public abstract class LateBoundConcerns<TConcern>
	{
		private IDictionary<Type, TConcern> concerns;
#if !(DOTNET35 || SILVERLIGHT)
		private ConcurrentDictionary<Type, List<TConcern>> concernsCache;
#else
		private IDictionary<Type, List<TConcern>> concernsCache;
		private readonly Lock cacheLock = Lock.Create();

#endif

		public bool HasConcerns
		{
			get { return concerns != null; }
		}

		public void AddConcern<TForType>(TConcern lifecycleConcern)
		{
			if (concerns == null)
			{
				concerns = new Dictionary<Type, TConcern>(2);
#if !(DOTNET35 || SILVERLIGHT)
				concernsCache = new ConcurrentDictionary<Type, List<TConcern>>(2, 2);
#else
				concernsCache = new Dictionary<Type, List<TConcern>>(2);
#endif
			}
			concerns.Add(typeof(TForType), lifecycleConcern);
		}

		public abstract void Apply(ComponentModel model, object component);

		private List<TConcern> BuildConcernCache(Type type)
		{
			var componentConcerns = new List<TConcern>(concerns.Count);
			foreach (var concern in concerns)
			{
				if (concern.Key.IsAssignableFrom(type))
				{
					componentConcerns.Add(concern.Value);
				}
			}
			return componentConcerns;
		}

		protected List<TConcern> GetComponentConcerns(Type type)
		{
#if !(DOTNET35 || SILVERLIGHT)
			return concernsCache.GetOrAdd(type, BuildConcernCache);
#else
			List<TConcern> componentConcerns;
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