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
	using System.Collections;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Resolvers;
	using Castle.MicroKernel.SubSystems.Naming;

	/// <summary>
	/// Summary description for DefaultHandler.
	/// </summary>
	[Serializable]
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
		/// <returns></returns>
		public override object Resolve(CreationContext context)
		{
			if (!context.HasAdditionalParameters)
			{
				if (CurrentState != HandlerState.Valid && !CanResolvePendingDependencies(context))
				{
					AssertNotWaitingForDependency();
				}
			}

			using(CreationContext.ResolutionContext resCtx = context.EnterResolutionContext(this))
			{
			    object instance = lifestyleManager.Resolve(context);

				resCtx.Burden.SetRootInstance(instance, this, ComponentModel.LifecycleSteps.HasDecommissionSteps);

				context.ReleasePolicy.Track(instance, resCtx.Burden);

				return instance;
			}
		}

		private bool CanResolvePendingDependencies(CreationContext context)
		{
			// detect circular dependencies
			if (context.HandlerIsCurrentlyBeingResolved(this))
				return false;

			foreach (var dependency in DependenciesByService.Values.ToArray())
			{
				// a self-dependency is not allowed
				var handler = Kernel.GetHandler(dependency.TargetType);
				if (handler == this)
					return false;

				// ask the kernel
				if (Kernel.HasComponent(dependency.TargetType)) continue;

				// let's try to lazy load the dependency...
				if (!LazyLoadComponent(dependency.DependencyKey, dependency.TargetType))
					return false;
				// and see if we can have it this time around
				// if the previous call returned true we always should
				if (!Kernel.HasComponent(dependency.TargetType))
					return false;
			}
			return DependenciesByKey.Count == 0;
		}

		/// <summary>
		/// disposes the component instance (or recycle it)
		/// </summary>
		/// <param name="instance"></param>
		/// <returns>true if destroyed</returns>
		public override bool Release(object instance)
		{
			return lifestyleManager.Release(instance);
		}

		protected void AssertNotWaitingForDependency()
		{
			if (CurrentState == HandlerState.WaitingDependency)
			{
				String message = String.Format("Can't create component '{1}' " +
					"as it has dependencies to be satisfied. {0}",
					ObtainDependencyDetails(new ArrayList()), ComponentModel.Name);

				throw new HandlerException(message);
			}
		}

		private bool LazyLoadComponent(string dependencyKey, Type targetType)
		{
			if (dependencyKey == null && targetType == null)
			{
				throw new ArgumentException("At least one - dependencyKey or targetType must not be a null reference.");
			}

			foreach (var loader in Kernel.ResolveAll<ILazyComponentLoader>())
			{
				var registration = loader.Load(dependencyKey, targetType);
				if (registration != null)
				{
					registration.Register(Kernel);
					return true;
				}
			}
			return false;
		}
	}
}
