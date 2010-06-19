// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Handlers
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	/// <summary>
	/// Summary description for DefaultHandler.
	/// </summary>
	[DebuggerTypeProxy(typeof(Windsor.Debugging.HandlerDebuggerProxy))]
#if !SILVERLIGHT
	[Serializable]
#endif
	public class DefaultHandler : AbstractHandler
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultHandler"/> class.
		/// </summary>
		/// <param name="model"></param>
		public DefaultHandler(ComponentModel model) : base(model)
		{
		}

		/// <summary>
		/// Returns an instance of the component this handler
		/// is responsible for
		/// </summary>
		/// <param name="context"></param>
		/// <param name="track"></param>
		/// <param name="instanceRequired"></param>
		/// <returns></returns>
		protected override object ResolveCore(CreationContext context, bool track, bool instanceRequired)
		{
			if (!context.HasAdditionalParameters)
			{
				if (CurrentState != HandlerState.Valid && !CanResolvePendingDependencies(context))
				{
					if (!instanceRequired)
					{
						return null;
					}

					AssertNotWaitingForDependency();
				}
			}

			using(CreationContext.ResolutionContext resCtx = context.EnterResolutionContext(this))
			{
				var instance = lifestyleManager.Resolve(context);

				resCtx.Burden.SetRootInstance(instance, this, track || ComponentModel.LifecycleSteps.HasDecommissionSteps);

				context.ReleasePolicy.Track(instance, resCtx.Burden);

				return instance;
			}
		}

		private bool CanResolvePendingDependencies(CreationContext context)
		{
			// detect circular dependencies
			if (IsBeingResolvedInContext(context))
				return false;

			foreach (var dependency in DependenciesByService.Values.ToArray())
			{
				// a self-dependency is not allowed
				var handler = Kernel.GetHandler(dependency.TargetType);
				if (handler == this)
				{
					return false;
				}

				// ask the kernel
				if (Kernel.LazyLoadComponentByType(dependency.DependencyKey, dependency.TargetType) == false)
				{
					return false;
				}
			}
			return DependenciesByKey.Count == 0;
		}

		/// <summary>
		/// disposes the component instance (or recycle it)
		/// </summary>
		/// <param name="instance"></param>
		/// <returns>true if destroyed</returns>
		public override bool ReleaseCore(object instance)
		{
			return lifestyleManager.Release(instance);
		}

		protected void AssertNotWaitingForDependency()
		{
			if (CurrentState == HandlerState.WaitingDependency)
			{
				String message = String.Format("Can't create component '{1}' " +
					"as it has dependencies to be satisfied. {0}",
					ObtainDependencyDetails(new List<object>()), ComponentModel.Name);

				throw new HandlerException(message);
			}
		}
	}
}
