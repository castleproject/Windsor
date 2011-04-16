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
	using Castle.Core.Internal;
	using Castle.MicroKernel.Context;

	public class CurrentScopeAccessor : ICurrentScopeAccessor
	{
		private readonly ComponentModel componentModel;
		private readonly IScopeManager scopeManager;
		private readonly IScopeRootSelector selector;

		public CurrentScopeAccessor(IScopeManager scopeManager, ComponentModel componentModel)
		{
			this.scopeManager = scopeManager;
			this.componentModel = componentModel;
			var baseType = (Type)componentModel.ExtendedProperties[Constants.ScopeRoot];
			if (baseType != null)
			{
				selector = new BasedOnTypeScopeRootSelector(baseType);
			}
		}

		public IScopeCache GetScopeCache(CreationContext context, bool required = true)
		{
			if (selector == null)
			{
				return GetScopeExplicit(required);
			}
			return GetScopeImplicit(required, context);
		}

		private IScopeCache GetCache(CreationContext.ResolutionContext selected)
		{
			var stash = (IScopeCache)selected.Context.GetContextualProperty("castle-scope-stash");
			if (stash == null)
			{
				var newStash = new ScopeCache();
				var decorator = new ScopeCacheDecorator(newStash);
				decorator.OnInserted += (_, b) =>
				{
					if (b.RequiresDecommission)
					{
						selected.Burden.RequiresDecommission = true;
						selected.Burden.GraphReleased += burden => newStash.Dispose();
					}
				};
				selected.Context.SetContextualProperty("castle-scope-stash", newStash);
				stash = decorator;
			}
			return stash;
		}

		private IScopeCache GetScopeExplicit(bool required)
		{
			var scope = scopeManager.CurrentScopeCache;
			if (scope == null && required)
			{
				throw new InvalidOperationException("Scope was not available. Did you forget to call container.BeginScope()?");
			}
			return scope;
		}

		private IScopeCache GetScopeImplicit(bool required, CreationContext context)
		{
			var selected = context.SelectScopeRoot(selector);
			if (selected == null && required)
			{
				throw new InvalidOperationException(string.Format("Scope was not available for '{0}'. Did you forget to call container.BeginScope()?", componentModel.Name));
			}
			return GetCache(selected);
		}
	}
}