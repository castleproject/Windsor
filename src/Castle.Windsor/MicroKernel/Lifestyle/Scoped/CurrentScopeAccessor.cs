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
		private readonly IScopeRootSelector selector;

		public CurrentScopeAccessor(ComponentModel componentModel)
		{
			this.componentModel = componentModel;
			var baseType = (Type)componentModel.ExtendedProperties[Constants.ScopeRoot];
			if (baseType != null)
			{
				selector = new BasedOnTypeScopeRootSelector(baseType);
			}
		}

		public void Dispose()
		{
		}

		private CurrentScope GetScope(CreationContext.ResolutionContext selected)
		{
			var stash = (CurrentScope)selected.Context.GetContextualProperty("castle-scope-stash");
			if (stash == null)
			{
				var newStash = new CurrentScope(new ScopeCache());
				selected.Context.SetContextualProperty("castle-scope-stash", newStash);
				stash = newStash;
			}
			return stash;
		}

		IScope2 ICurrentScopeAccessor.GetScope(CreationContext context, bool required)
		{
			var selected = context.SelectScopeRoot(selector);
			if (selected == null && required)
			{
				throw new InvalidOperationException(string.Format("Scope was not available for '{0}'. Did you forget to call container.BeginScope()?", componentModel.Name));
			}
			return GetScope(selected);
		}
	}
}