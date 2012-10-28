// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.Context;

#if DOTNET35 || SILVERLIGHT
	using System.Linq;
#endif

	/// <summary>
	/// Abstract implementation of <see cref = "IComponentActivator" />. The implementors must only override the InternalCreate and InternalDestroy methods in order to perform their creation and destruction
	/// logic.
	/// </summary>
	[Serializable]
	public abstract class AbstractComponentActivator : IComponentActivator
	{
		private readonly IKernelInternal kernel;
		private readonly ComponentModel model;
		private readonly ComponentInstanceDelegate onCreation;
		private readonly ComponentInstanceDelegate onDestruction;

		/// <summary>
		/// Constructs an AbstractComponentActivator
		/// </summary>
		protected AbstractComponentActivator(ComponentModel model, IKernelInternal kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
		{
			this.model = model;
			this.kernel = kernel;
			this.onCreation = onCreation;
			this.onDestruction = onDestruction;
		}

		public IKernelInternal Kernel
		{
			get { return kernel; }
		}

		public ComponentModel Model
		{
			get { return model; }
		}

		public ComponentInstanceDelegate OnCreation
		{
			get { return onCreation; }
		}

		public ComponentInstanceDelegate OnDestruction
		{
			get { return onDestruction; }
		}

		protected abstract object InternalCreate(CreationContext context);

		protected abstract void InternalDestroy(object instance);

		public virtual object Create(CreationContext context, Burden burden)
		{
			var instance = InternalCreate(context);
			burden.SetRootInstance(instance);

			onCreation(model, instance);

			return instance;
		}

		public virtual void Destroy(object instance)
		{
			InternalDestroy(instance);

			onDestruction(model, instance);
		}

		protected virtual void ApplyCommissionConcerns(object instance)
		{
			if (Model.Lifecycle.HasCommissionConcerns == false)
			{
				return;
			}

			instance = ProxyUtil.GetUnproxiedInstance(instance);
			if (instance == null)
			{
				// see http://issues.castleproject.org/issue/IOC-332 for details
				throw new NotSupportedException(string.Format("Can not apply commission concerns to component {0} because it appears to be a target-less proxy. Currently those are not supported.", Model.Name));
			}
			ApplyConcerns(Model.Lifecycle.CommissionConcerns, instance);
		}

		protected virtual void ApplyConcerns(IEnumerable<ICommissionConcern> steps, object instance)
		{
			foreach (var concern in steps)
			{
				concern.Apply(Model, instance);
			}
		}

		protected virtual void ApplyConcerns(IEnumerable<IDecommissionConcern> steps, object instance)
		{
			foreach (var concern in steps)
			{
				concern.Apply(Model, instance);
			}
		}

		protected virtual void ApplyDecommissionConcerns(object instance)
		{
			if (Model.Lifecycle.HasDecommissionConcerns == false)
			{
				return;
			}

			instance = ProxyUtil.GetUnproxiedInstance(instance);
			ApplyConcerns(Model.Lifecycle.DecommissionConcerns, instance);
		}
	}
}