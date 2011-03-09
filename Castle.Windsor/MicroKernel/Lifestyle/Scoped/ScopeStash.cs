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
	using System.Collections.Generic;
	using System.Linq;

	public class ScopeStash : IDisposable
	{
		// NOTE: does that need to be thread safe?
		private readonly IDictionary<object, Burden> cache = new Dictionary<object, Burden>();

		public void AddComponent(object id, Burden componentBurden)
		{
			cache.Add(id, componentBurden);
		}

		public Burden GetComponentBurden(object id)
		{
			Burden burden;
			cache.TryGetValue(id, out burden);
			return burden;
		}

		public void Dispose()
		{
			var burdens = cache.Values.Reverse().ToArray();
			cache.Clear();
			foreach (var burden in burdens)
			{
				burden.Release();
			}
		}
	}
}