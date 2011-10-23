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

namespace Castle.Facilities.TypedFactory
{
	using Castle.Core;
	using Castle.Facilities.TypedFactory.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;

	public class DelegateFactoryActivator : AbstractComponentActivator, IDependencyAwareActivator
	{
		private readonly IProxyFactoryExtension proxyFactory = new DelegateProxyFactory();

		public DelegateFactoryActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
		}

		public bool CanProvideRequiredDependencies(ComponentModel component)
		{
			return true;
		}

		public bool IsManagedExternally(ComponentModel component)
		{
			return false;
		}

		protected override object InternalCreate(CreationContext context)
		{
			var instance = Kernel.ProxyFactory.Create(proxyFactory, Kernel, Model, context);
			ApplyDecommissionConcerns(instance);
			return instance;
		}

		protected override void InternalDestroy(object instance)
		{
			ApplyDecommissionConcerns(instance);
		}
	}
}