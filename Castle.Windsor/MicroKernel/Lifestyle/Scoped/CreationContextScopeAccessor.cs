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
	using Castle.MicroKernel.Context;

	public class CreationContextScopeAccessor : IScopeAccessor
	{
		private readonly ComponentModel componentModel;
		private readonly IScopeRootSelector selector;

		public CreationContextScopeAccessor(ComponentModel componentModel, IScopeRootSelector selector)
		{
			this.componentModel = componentModel;
			this.selector = selector;
		}

		public void Dispose()
		{
		}

		public ILifetimeScope GetScope(CreationContext context)
		{
			var selected = context.SelectScopeRoot(selector);
			if (selected == null)
			{
				throw new InvalidOperationException(string.Format("Scope was not available for '{0}'. Did you forget to call container.BeginScope()?", componentModel.Name));
			}
			var stash = (CurrentScope)selected.Context.GetContextualProperty("castle.scope-stash");
			if (stash == null)
			{
				var newStash = new CurrentScope(selected.Burden);
				selected.Context.SetContextualProperty("castle.scope-stash", newStash);
				stash = newStash;
			}
			return stash;
		}
	}
}