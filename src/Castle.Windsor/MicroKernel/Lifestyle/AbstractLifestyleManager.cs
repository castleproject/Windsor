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

namespace Castle.MicroKernel.Lifestyle
{
	using System;
	using System.Diagnostics;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	/// <summary>
	///   Base implementation of <see cref="ILifestyleManager"/>
	/// </summary>
	[Serializable]
	public abstract class AbstractLifestyleManager : ILifestyleManager
	{
		private IComponentActivator componentActivator;
		private IKernel kernel;
		private ComponentModel model;

		protected IComponentActivator ComponentActivator
		{
			get { return componentActivator; }
		}

		protected IKernel Kernel
		{
			get { return kernel; }
		}

		protected ComponentModel Model
		{
			get { return model; }
		}

		/// <summary>
		///   Invoked when the container gets disposed. The container will not call it multiple times in multithreaded environments.
		///   However it may be called at the same time when some out of band release mechanism is in progress. Resolving those potential
		///   issues is the task of implementors
		/// </summary>
		public abstract void Dispose();

		public virtual void Init(IComponentActivator componentActivator, IKernel kernel, ComponentModel model)
		{
			this.componentActivator = componentActivator;
			this.kernel = kernel;
			this.model = model;
		}

		public virtual bool Release(object instance)
		{
			componentActivator.Destroy(instance);
			return true;
		}

		public virtual object Resolve(CreationContext context, IReleasePolicy releasePolicy)
		{
			var burden = CreateInstance(context, false);
			Track(burden, releasePolicy);
			return burden.Instance;
		}

		protected virtual Burden CreateInstance(CreationContext context, bool trackedExternally)
		{
			var burden = context.CreateBurden(ComponentActivator, trackedExternally);

			var instance = componentActivator.Create(context, burden);
			Debug.Assert(ReferenceEquals(instance, burden.Instance));
			return burden;
		}

		protected virtual void Track(Burden burden, IReleasePolicy releasePolicy)
		{
			if (burden.RequiresPolicyRelease)
			{
				releasePolicy.Track(burden.Instance, burden);
			}
		}
	}
}