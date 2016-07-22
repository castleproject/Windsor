// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

	using Castle.Core;
	using Castle.MicroKernel.Context;

	public class CreationContextScopeAccessor : IScopeAccessor
	{
		private const string ScopeStash = "castle.scope-stash";
		private readonly ComponentModel componentModel;
		private readonly Func<IHandler[], IHandler> scopeRootSelector;

		public CreationContextScopeAccessor(ComponentModel componentModel, Func<IHandler[], IHandler> scopeRootSelector)
		{
			this.componentModel = componentModel;
			this.scopeRootSelector = scopeRootSelector;
		}

		public void Dispose()
		{
		}

		public ILifetimeScope GetScope(CreationContext context)
		{
			var selected = context.SelectScopeRoot(scopeRootSelector);
			if (selected == null)
			{
				throw new InvalidOperationException(
					string.Format(
						"Scope was not available for '{0}'. No component higher up in the resolution stack met the criteria specified for scoping the component. This usually indicates a bug in custom scope root selector or means that the component is being resolved in a unforseen context (a.k.a - it's probably a bug in how the dependencies in the application are wired).",
						componentModel.Name));
			}
			var stash = (DefaultLifetimeScope)selected.GetContextualProperty(ScopeStash);
			if (stash == null)
			{
				DefaultLifetimeScope newStash = null;
				newStash = new DefaultLifetimeScope(new ScopeCache(), burden =>
				{
					if (burden.RequiresDecommission)
					{
						selected.Burden.RequiresDecommission = true;
						selected.Burden.GraphReleased += delegate { newStash.Dispose(); };
					}
				});
				selected.SetContextualProperty(ScopeStash, newStash);
				stash = newStash;
			}
			return stash;
		}

		public static IHandler DefaultScopeRootSelector<TBaseForRoot>(IHandler[] resolutionStack)
		{
			return resolutionStack.FirstOrDefault(h => typeof(TBaseForRoot).IsAssignableFrom(h.ComponentModel.Implementation));
		}

		public static IHandler NearestScopeRootSelector<TBaseForRoot>(IHandler[] resolutionStack)
		{
			return resolutionStack.LastOrDefault(h => typeof(TBaseForRoot).IsAssignableFrom(h.ComponentModel.Implementation));
		}
	}
}