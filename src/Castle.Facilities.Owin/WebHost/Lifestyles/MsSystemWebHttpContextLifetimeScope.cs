// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

#if NET45

namespace Castle.Facilities.Owin.WebHost.Lifestyles
{
	using System;

	using Castle.Core;
	using Castle.Core.Internal;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Lifestyle.Scoped;

	internal class MsSystemWebHttpContextLifetimeScope : ILifetimeScope
	{
		private readonly Lock @lock = Lock.Create();
		private ScopeCache cache = new ScopeCache();
		private IDisposable innerCallContextScope;

		public MsSystemWebHttpContextLifetimeScope()
		{
			innerCallContextScope = DependencyServiceLocator.Container.BeginScope();
		}

		public Burden GetCachedInstance(ComponentModel model, ScopedInstanceActivationCallback createInstance)
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				var burden = cache[model];
				if (burden == null)
				{
					token.Upgrade();
					burden = createInstance(delegate { });
					cache[model] = burden;
				}
				return burden;
			}
		}

		public void Dispose()
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				if (cache == null && innerCallContextScope == null) return;
				token.Upgrade();
				cache?.Dispose();
				cache = null;
				innerCallContextScope?.Dispose();
				innerCallContextScope = null;
			}
		}
	}
}

#endif
