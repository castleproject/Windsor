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
	using System.Runtime.Remoting.Messaging;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.Windsor;

	public class CallContextLifetimeScope : ILifetimeScope, ILogicalThreadAffinative
	{
		private const string contextKey = "castle.lifetime-scope";
		private readonly Lock @lock = Lock.Create();
		private readonly CallContextLifetimeScope parentScope;
		private ScopeCache cache = new ScopeCache();

		public CallContextLifetimeScope(IKernel container)
		{
			var parent = ObtainCurrentScope();
			if (parent != null)
			{
				parentScope = parent;
			}
			SetCurrentScope(this);
		}

		public CallContextLifetimeScope(IWindsorContainer container) : this(container.Kernel)
		{
		}

		public void Dispose()
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				if (cache == null)
				{
					return;
				}
				token.Upgrade();
				cache.Dispose();
				cache = null;

				if (parentScope != null)
				{
					CallContext.SetData(contextKey, parentScope);
				}
				else
				{
					CallContext.FreeNamedDataSlot(contextKey);
				}
			}
		}

		public Burden GetCachedInstance(ComponentModel instance, ScopedInstanceActivationCallback createInstance)
		{
			using (var token = @lock.ForReadingUpgradeable())
			{
				var burden = cache[instance];
				if (burden == null)
				{
					token.Upgrade();

					burden = createInstance(delegate { });
					cache[instance] = burden;
				}
				return burden;
			}
		}

		private void SetCurrentScope(CallContextLifetimeScope lifetimeScope)
		{
			CallContext.SetData(contextKey, lifetimeScope);
		}

		public static CallContextLifetimeScope ObtainCurrentScope()
		{
			return (CallContextLifetimeScope)CallContext.GetData(contextKey);
		}
	}
}