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

namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.Threading;

	using Castle.Core.Internal;

	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle.Scoped;

	public class ScopedLifestyleManager : AbstractLifestyleManager
	{
		private IScopeAccessor accessor;

		public ScopedLifestyleManager()
			: this(new LifetimeScopeAccessor())
		{
		}

		public ScopedLifestyleManager(IScopeAccessor accessor)
		{
			this.accessor = accessor;
		}

		public override void Dispose()
		{
			var scope = Interlocked.Exchange(ref accessor, null);
			if (scope != null)
			{
				scope.Dispose();
			}
		}

		public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
		{
			var scope = GetScope(context);
			var burden = scope.GetCachedInstance(Model, afterCreated =>
			{
				var localBurden = base.CreateInstance(context, trackedExternally: true);
				afterCreated(localBurden);
				Track(localBurden, releasePolicy);
				return localBurden;
			});
			return burden.Instance;
		}

		private ILifetimeScope GetScope(CreationContext context)
		{
			var localScope = accessor;
			if (localScope == null)
			{
				throw new ObjectDisposedException("Scope was already disposed. This is most likely a bug in the calling code.");
			}
			var scope = localScope.GetScope(context);
			if(scope == null)
			{
				throw new ComponentResolutionException(
					string.Format(
						"Could not obtain scope for component {0}. This is most likely either a bug in custom {1} or you're trying to access scoped component outside of the scope (like a per-web-request component outside of web request etc)",
						Model.Name,
						typeof(IScopeAccessor).ToCSharpString()),
					Model);
			}
			return scope;
		}
	}
}