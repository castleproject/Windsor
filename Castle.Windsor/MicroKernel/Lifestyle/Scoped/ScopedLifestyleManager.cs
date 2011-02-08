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
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Lifestyle;

	public class ScopedLifestyleManager : AbstractLifestyleManager
	{
		private bool evicting;
		private IScopeManager manager;

		public override void Dispose()
		{
			var current = GetCurrentScope();
			if (current == null)
			{
				return;
			}

			var instance = current.GetComponent(this);
			if (instance == null)
			{
				return;
			}

			Evict(instance);
		}

		public override void Init(IComponentActivator componentActivator, IKernel kernel, ComponentModel model)
		{
			base.Init(componentActivator, kernel, model);

			manager = kernel.GetSubSystem("scope") as IScopeManager;
			if (manager == null)
			{
				throw new InvalidOperationException("Scope Subsystem not found.  Did you forget to add it?");
			}
		}

		public override bool Release(object instance)
		{
			if (evicting == false)
			{
				return false;
			}

			return base.Release(instance);
		}

		public override object Resolve(CreationContext context, IReleasePolicy releasePolicy)
		{
			var scope = GetCurrentScope();
			if (scope == null)
			{
				throw new InvalidOperationException("Not in a scope.  Did you forget to call BeginScope?");
			}

			if (scope.HasComponent(this))
			{
				return scope.GetComponent(this);
			}

			var component = base.Resolve(context, releasePolicy);
			scope.AddComponent(this, component);
			return component;
		}

		internal void Evict(object instance)
		{
			using (new EvictionScope(this))
			{
				Kernel.ReleaseComponent(instance);
			}
		}

		private LifestyleScope GetCurrentScope()
		{
			return manager.CurrentScope;
		}

		private class EvictionScope : IDisposable
		{
			private readonly ScopedLifestyleManager owner;

			public EvictionScope(ScopedLifestyleManager owner)
			{
				this.owner = owner;
				this.owner.evicting = true;
			}

			public void Dispose()
			{
				owner.evicting = false;
			}
		}
	}
}