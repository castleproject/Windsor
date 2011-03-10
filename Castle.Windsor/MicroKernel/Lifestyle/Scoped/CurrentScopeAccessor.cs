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

	using Castle.Core;
	using Castle.MicroKernel.Context;

	public interface IScopeRootSelector
	{
		IHandler Select(IHandler[] resolutionStack);
	}

	public class CurrentScopeAccessor : ICurrentScopeAccessor, IScopeRootSelector
	{
		private readonly ComponentModel componentModel;
		private readonly IScopeManager scopeManager;
		private readonly Type scopeRoot;

		public CurrentScopeAccessor(IScopeManager scopeManager, ComponentModel componentModel)
		{
			this.scopeManager = scopeManager;
			this.componentModel = componentModel;
			scopeRoot = (Type)componentModel.ExtendedProperties["castle-scope-root"];
		}

		public ScopeStash GetScope(CreationContext context, bool required = true)
		{
			if (scopeRoot == null)
			{
				return GetScopeExplicit(required);
			}
			return GetScopeImplicit(required, context);
		}

		public IHandler Select(IHandler[] resolutionStack)
		{
			return resolutionStack.FirstOrDefault(h => h.ComponentModel.Implementation == scopeRoot);
		}

		private ScopeStash GetScopeExplicit(bool required)
		{
			var scope = scopeManager.CurrentScopeStash;
			if (scope == null && required)
			{
				throw new InvalidOperationException("Scope was not available. Did you forget to call container.BeginScope()?");
			}
			return scope;
		}

		private ScopeStash GetScopeImplicit(bool required, CreationContext context)
		{
			var selected = context.SelectScopeRoot(this);
			if (selected == null && required)
			{
				throw new InvalidOperationException(string.Format("Scope was not available for '{0}'. Did you forget to call container.BeginScope()?", componentModel.Name));
			}
			return GetStash(selected);
		}

		private ScopeStash GetStash(CreationContext.ResolutionContext selected)
		{
			var stash = (ScopeStash)selected.Context.GetContextualProperty("castle-scope-stash");
			if (stash == null)
			{
				stash = new ScopeStash();
				selected.Context.SetContextualProperty("castle-scope-stash", stash);
				selected.Burden.Released += burden => stash.Dispose();
				selected.Burden.RequiresDecommission = true;
			}
			return stash;
		}
	}
}