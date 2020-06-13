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
	
	using Castle.Windsor.Extensions.DependencyInjection.Extensions;
	using Castle.Windsor.Extensions.DependencyInjection.Scope;
	
	using Microsoft.Extensions.DependencyInjection;

	public class WindsorServiceProviderFactory : IServiceProviderFactory<IWindsorContainer>
	{
		private readonly ExtensionContainerRootScope rootScope;

		public WindsorServiceProviderFactory()
		{
			rootScope = ExtensionContainerRootScope.BeginRootScope();
		}
		
		public IWindsorContainer CreateBuilder(IServiceCollection services)
		{
			var container = services.CreateContainer(this);
			return container;
		}

		public IServiceProvider CreateServiceProvider(IWindsorContainer container)
		{
			return container.Resolve<IServiceProvider>();
		}
	}
}