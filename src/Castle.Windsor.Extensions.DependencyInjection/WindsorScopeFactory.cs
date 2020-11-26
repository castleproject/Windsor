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


namespace Castle.Windsor.Extensions.DependencyInjection
{
	using System;

	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.Windsor;
	using Castle.Windsor.Extensions.DependencyInjection.Interfaces.Scope;
	using Castle.Windsor.Extensions.DependencyInjection.Scope;
	
	using Microsoft.Extensions.DependencyInjection;

	internal class WindsorScopeFactory : IServiceScopeFactory
	{
		private readonly IWindsorContainer scopeFactoryContainer;

		public WindsorScopeFactory(IWindsorContainer container)
		{
			scopeFactoryContainer = container;
		}

		public IServiceScope CreateScope()
		{
			var rootScope = scopeFactoryContainer.Kernel.Resolve<IExtensionContainerScope>();
			if (rootScope == null)
			{
				throw new ArgumentNullException(nameof(rootScope), "ExtensionContainerScope does not exist in the  kernel");
			}

			ExtensionContainerScope parent = null;
			if (rootScope.Current != null)
			{
				parent = rootScope.Current.Parent;
			}
			var scope = rootScope.BeginScope(parent);
			//since WindsorServiceProvider is scoped, this gives us new instance
			var provider = scopeFactoryContainer.Resolve<IServiceProvider>();
			return new ServiceScope(scope, provider);
		}
	}
}