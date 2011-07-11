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
	using Castle.Core;

	public class CurrentScope : ILifetimeScope
	{
		private readonly Burden rootBurden;
		private readonly ScopeCache scopeCache = new ScopeCache();

		public CurrentScope(Burden rootBurden)
		{
			this.rootBurden = rootBurden;
		}

		public void Dispose()
		{
		}

		public Burden GetCachedInstance(ComponentModel instance, ScopedInstanceActivationCallback createInstance)
		{
			var burden = scopeCache[instance];
			if (burden == null)
			{
				scopeCache[instance] = burden = createInstance(OnAfterCreated);
			}
			return burden;
		}

		private void OnAfterCreated(Burden burden)
		{
			if (burden.RequiresDecommission)
			{
				rootBurden.RequiresDecommission = true;
				rootBurden.GraphReleased += delegate { scopeCache.Dispose(); };
			}
		}
	}
}