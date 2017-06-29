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
	using System.Reflection;

	using Castle.Core;
	using System.Collections.Concurrent;

	/// <summary>
	///   Lifetime concern that works for components that don't have their actual type determined upfront
	/// </summary>
	[Serializable]
	public abstract class LateBoundConcerns<TConcern>
	{
		private IDictionary<Type, TConcern> concerns;
		private ConcurrentDictionary<Type, List<TConcern>> concernsCache;

		public bool HasConcerns
		{
			get { return concerns != null; }
		}

		public void AddConcern<TForType>(TConcern lifecycleConcern)
		{
			if (concerns == null)
			{
				concerns = new Dictionary<Type, TConcern>(2);
				concernsCache = new ConcurrentDictionary<Type, List<TConcern>>(2, 2);
			}
			concerns.Add(typeof(TForType), lifecycleConcern);
		}

		public abstract void Apply(ComponentModel model, object component);

		private List<TConcern> BuildConcernCache(Type type)
		{
			var componentConcerns = new List<TConcern>(concerns.Count);
			foreach (var concern in concerns)
			{
				if (concern.Key.GetTypeInfo().IsAssignableFrom(type))
				{
					componentConcerns.Add(concern.Value);
				}
			}
			return componentConcerns;
		}

		protected List<TConcern> GetComponentConcerns(Type type)
		{
			return concernsCache.GetOrAdd(type, BuildConcernCache);
		}
	}
}