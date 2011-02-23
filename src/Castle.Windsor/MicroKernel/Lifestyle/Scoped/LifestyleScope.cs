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

namespace Castle.MicroKernel.Lifestyle.Scoped
{
	using System;
	using System.Linq;
	using System.Collections.Generic;

	public class LifestyleScope : IDisposable
	{
		// NOTE: does that need to be thread safe?
		private readonly IDictionary<ScopedLifestyleManager, Burden> cache;

		public LifestyleScope()
		{
			cache = new Dictionary<ScopedLifestyleManager, Burden>();
		}

		public void AddComponent(ScopedLifestyleManager id, Burden componentBurden)
		{
			cache.Add(id, componentBurden);
		}

		public Burden GetComponentBurden(ScopedLifestyleManager id)
		{
			Burden burden;
			cache.TryGetValue(id, out burden);
			return burden;
		}

		public bool HasComponent(ScopedLifestyleManager id)
		{
			return cache.ContainsKey(id);
		}

		public void Dispose()
		{
			foreach (var cacheEntry in cache.Reverse().ToArray())
			{
				cacheEntry.Value.Release();
			}
			cache.Clear();
		}
	}
}