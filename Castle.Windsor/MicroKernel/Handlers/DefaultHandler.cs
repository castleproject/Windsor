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

namespace Castle.MicroKernel.Handlers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	/// <summary>
	///   Summary description for DefaultHandler.
	/// </summary>
	[Serializable]
	public class DefaultHandler : AbstractHandler
	{
		/// <summary>
		///   Initializes a new instance of the <see cref = "DefaultHandler" /> class.
		/// </summary>
		/// <param name = "model"></param>
		public DefaultHandler(ComponentModel model) : base(model)
		{
		}

		/// <summary>
		///   disposes the component instance (or recycle it)
		/// </summary>
		/// <param name = "burden"></param>
		/// <returns>true if destroyed</returns>
		public override bool ReleaseCore(Burden burden)
		{
			return lifestyleManager.Release(burden.Instance);
		}

		protected void AssertNotWaitingForDependency()
		{
			if (CurrentState == HandlerState.WaitingDependency)
			{
				var message = String.Format("Can't create component '{1}' " +
				                            "as it has dependencies to be satisfied. {0}",
				                            ObtainDependencyDetails(new List<object>()), ComponentModel.Name);

				throw new HandlerException(message);
			}
		}

		protected override object Resolve(CreationContext context, bool instanceRequired)
		{
			Burden burden;
			return ResolveCore(context, false, instanceRequired, out burden);
		}

		/// <summary>
		///   Returns an instance of the component this handler
		///   is responsible for
		/// </summary>
		/// <param name = "context"></param>
		/// <param name = "requiresDecommission"></param>
		/// <param name = "instanceRequired"></param>
		/// <param name="burden"></param>
		/// <returns></returns>
		protected object ResolveCore(CreationContext context, bool requiresDecommission, bool instanceRequired, out Burden burden)
		{
			if (CanResolvePendingDependencies(context) == false)
			{
				if (instanceRequired == false)
				{
					burden = null;
					return null;
				}

				AssertNotWaitingForDependency();
			}
			using (var ctx = context.EnterResolutionContext(this, HasDecomission(requiresDecommission)))
			{
				var instance = lifestyleManager.Resolve(context, context.ReleasePolicy);
				burden = ctx.Burden;
				return instance;
			}
		}

		private bool CanResolvePendingDependencies(CreationContext context)
		{
			if (CurrentState == HandlerState.Valid)
			{
				return true;
			}
			// detect circular dependencies
			if (IsBeingResolvedInContext(context))
			{
				return context.HasAdditionalArguments;
			}
			var canResolveAll = true;
			foreach (var dependency in DependenciesByService.Values.ToArray())
			{
				// a self-dependency is not allowed
				var handler = Kernel.LoadHandlerByType(dependency.DependencyKey, dependency.TargetItemType, context.AdditionalArguments);
				if (handler == this || handler == null)
				{
					canResolveAll = false;
					break;
				}
			}
			return (canResolveAll && DependenciesByKey.Count == 0) || context.HasAdditionalArguments;
		}

		private bool HasDecomission(bool track)
		{
			return track || ComponentModel.Lifecycle.HasDecommissionConcerns;
		}
	}
}