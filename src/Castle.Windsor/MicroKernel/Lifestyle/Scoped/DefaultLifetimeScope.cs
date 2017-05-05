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

	using Castle.Core;

	public class DefaultLifetimeScope : ILifetimeScope
	{
		private static readonly Action<Burden> emptyOnAfterCreated = delegate { };
		private readonly Action<Burden> onAfterCreated;
		private readonly IScopeCache scopeCache;

		public DefaultLifetimeScope(IScopeCache scopeCache = null, Action<Burden> onAfterCreated = null)
		{
			this.scopeCache = scopeCache ?? new ScopeCache();
			this.onAfterCreated = onAfterCreated ?? emptyOnAfterCreated;
		}

		public void Dispose()
		{
			var disposableCache = scopeCache as IDisposable;
			if (disposableCache != null)
			{
				disposableCache.Dispose();
			}
		}

		public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
		{
			var burden = scopeCache[model];
			if (burden == null)
			{
				scopeCache[model] = burden = createInstance(onAfterCreated);
			}
			return burden;
		}
	}
}