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

namespace Castle.MicroKernel.ComponentActivator
{
	using Castle.Core;
	using Castle.MicroKernel.Context;

	public class ExternalInstanceActivator : AbstractComponentActivator, IDependencyAwareActivator
	{
		public ExternalInstanceActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
		}

		public bool CanProvideRequiredDependencies(ComponentModel component)
		{
			//we already have an instance so we don't need to provide any dependencies at all
			return true;
		}

		public bool IsManagedExternally(ComponentModel component)
		{
			return true;
		}

		protected override object InternalCreate(CreationContext context)
		{
			return Model.ExtendedProperties["instance"];
		}

		protected override void InternalDestroy(object instance)
		{
			// Nothing to do
		}
	}
}