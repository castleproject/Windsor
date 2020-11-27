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

namespace Castle.MicroKernel.Handlers
{
	using System;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	public class ParentHandlerWrapper : IHandler, IDisposable
	{
		private readonly ISubDependencyResolver childResolver;
		private readonly IReleasePolicy parentReleasePolicy;
		private readonly IHandler parentHandler;

		/// <summary>
		///   Initializes a new instance of the <see cref = "ParentHandlerWrapper" /> class.
		/// </summary>
		/// <param name = "parentHandler">The parent handler.</param>
		/// <param name = "childResolver">The child resolver.</param>
		/// <param name="parentReleasePolicy">Release policy of the parent container.</param>
		public ParentHandlerWrapper(IHandler parentHandler, ISubDependencyResolver childResolver, IReleasePolicy parentReleasePolicy)
		{
			if (parentHandler == null)
			{
				throw new ArgumentNullException(nameof(parentHandler));
			}
			if (childResolver == null)
			{
				throw new ArgumentNullException(nameof(childResolver));
			}

			this.parentHandler = parentHandler;
			this.childResolver = childResolver;
			this.parentReleasePolicy = parentReleasePolicy;
		}

		public virtual ComponentModel ComponentModel
		{
			get { return parentHandler.ComponentModel; }
		}

		public virtual HandlerState CurrentState
		{
			get { return parentHandler.CurrentState; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public virtual void Init(IKernelInternal kernel)
		{
		}

		public bool IsBeingResolvedInContext(CreationContext context)
		{
			return (context != null && context.IsInResolutionContext(this)) || parentHandler.IsBeingResolvedInContext(context);
		}

		public virtual bool Release(Burden burden)
		{
			return parentHandler.Release(burden);
		}

		public virtual object Resolve(CreationContext context)
		{
			var releasePolicy = default(IReleasePolicy);
			try
			{
				releasePolicy = context.ReleasePolicy;
				context.ReleasePolicy = parentReleasePolicy;
				return parentHandler.Resolve(context);
			}
			finally
			{
				context.ReleasePolicy = releasePolicy;
			}
		}

		public bool Supports(Type service)
		{
			return parentHandler.Supports(service);
		}

		public bool SupportsAssignable(Type service)
		{
			return parentHandler.SupportsAssignable(service);
		}

		public object TryResolve(CreationContext context)
		{
			var releasePolicy = default(IReleasePolicy);
			try
			{
				releasePolicy = context.ReleasePolicy;
				context.ReleasePolicy = parentReleasePolicy;
				return parentHandler.TryResolve(context);
			}
			finally
			{
				context.ReleasePolicy = releasePolicy;
			}
		}

		public virtual bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
		                               ComponentModel model, DependencyModel dependency)
		{
			var canResolve = false;

			if (contextHandlerResolver != null)
			{
				canResolve = childResolver.CanResolve(context, null, model, dependency);
			}

			if (!canResolve)
			{
				canResolve = parentHandler.CanResolve(context, contextHandlerResolver, model, dependency);
			}

			return canResolve;
		}

		public virtual object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver,
		                              ComponentModel model, DependencyModel dependency)
		{
			var value = childResolver.Resolve(context, null, model, dependency);

			if (value == null)
			{
				value = parentHandler.Resolve(context, contextHandlerResolver, model, dependency);
			}

			return value;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (parentHandler != null)
				{
				}
			}
		}
	}
}