// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Extensions.DependencyInjection.Scope
{
	using System;
	using Castle.Core.Logging;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle.Scoped;

	internal class ExtensionContainerRootScopeAccessor : IScopeAccessor
	{
		public ILifetimeScope GetScope(CreationContext context)
		{
			if (ExtensionContainerScopeCache.Current?.RootScope == null)
			{
				//TODO: if we have a way from context to retrieve the instance of container/kernel we could use it to get
				//a reference of the correct root scope with a call to WindsorServiceProviderFactoryBase.GetSingleRootScope
				//but since in this version we cannot determine the container from this context. 

				//In this version we have the limit to have only one container registered in the dependency injection chain of
				//.NET core, so we call a different method that gives us the single root scope.				
				var scope = WindsorServiceProviderFactoryBase.GetSingleRootScope();

				if (scope == null)
				{
					throw new InvalidOperationException($"{nameof(ExtensionContainerRootScopeAccessor)}: We are trying to access a ROOT scope null for requested type {context.RequestedType}");
				}

				return scope;
			}
			return ExtensionContainerScopeCache.Current?.RootScope;
		}

		public void Dispose()
		{
		}
	}
}
