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

namespace Castle.MicroKernel.Registration
{
	using Castle.Core;
	using Castle.MicroKernel.LifecycleConcerns;

	public class OnDestroyComponentDescriptor<S> : ComponentDescriptor<S>
		where S : class
	{
		private readonly LifecycleActionDelegate<S>[] actions;

		public OnDestroyComponentDescriptor(LifecycleActionDelegate<S>[] actions)
		{
			this.actions = actions;
		}

		protected internal override void ApplyToModel(IKernel kernel, ComponentModel model)
		{
			if (actions == null)
			{
				return;
			}
			model.Lifecycle.Add(new OnCreatedConcern<S>(actions, kernel));
		}
	}
}