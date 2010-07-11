// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.ComponentActivator
{
	using System;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	public class FactoryMethodActivator<T> : DefaultComponentActivator, IDependencyAwareActivator
	{
		private readonly Func<IKernel,ComponentModel, CreationContext, T> creator;

		public FactoryMethodActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction) 
			: base(model, kernel, onCreation, onDestruction)
		{
			creator = Model.ExtendedProperties["factoryMethodDelegate"] as Func<IKernel, ComponentModel, CreationContext, T>;
			if (creator == null)
			{
				throw new ComponentActivatorException(
					string.Format(
						"{0} received misconfigured component model for {1}. Are you sure you registered this component with 'UsingFactoryMethod'?",
						GetType().Name, Model));
			}
		}

		protected override object Instantiate(CreationContext context)
		{
			return creator(Kernel, Model, context);
		}
		
		public bool CanProvideRequiredDependencies(ComponentModel component)
		{
			// the factory will take care of providing all dependencies.
			return true;
		}
	}
}
