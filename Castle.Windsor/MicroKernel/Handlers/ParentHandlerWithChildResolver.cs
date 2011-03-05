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
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel.Context;

	/// <summary>
	///   Redirects resolution to the main resolver, and if not found uses
	///   the parent handler.
	/// </summary>
	public class ParentHandlerWithChildResolver : IHandler, IDisposable
	{
		private readonly ISubDependencyResolver childResolver;
		private readonly IHandler parentHandler;

		/// <summary>
		///   Initializes a new instance of the <see cref = "ParentHandlerWithChildResolver" /> class.
		/// </summary>
		/// <param name = "parentHandler">The parent handler.</param>
		/// <param name = "childResolver">The child resolver.</param>
		public ParentHandlerWithChildResolver(IHandler parentHandler, ISubDependencyResolver childResolver)
		{
			if (parentHandler == null)
			{
				throw new ArgumentNullException("parentHandler");
			}
			if (childResolver == null)
			{
				throw new ArgumentNullException("childResolver");
			}

			this.parentHandler = parentHandler;
			parentHandler.OnHandlerStateChanged += RaiseHandlerStateChanged;
			this.childResolver = childResolver;
		}

		public virtual ComponentModel ComponentModel
		{
			get { return parentHandler.ComponentModel; }
		}

		public virtual HandlerState CurrentState
		{
			get { return parentHandler.CurrentState; }
		}

		public IEnumerable<Type> Services
		{
			get { return ComponentModel.Services; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public virtual void AddCustomDependencyValue(object key, object value)
		{
			parentHandler.AddCustomDependencyValue(key, value);
		}

		public virtual bool HasCustomParameter(object key)
		{
			return parentHandler.HasCustomParameter(key);
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

		public virtual void RemoveCustomDependencyValue(object key)
		{
			parentHandler.RemoveCustomDependencyValue(key);
		}

		public virtual object Resolve(CreationContext context)
		{
			return parentHandler.Resolve(context);
		}

		public object TryResolve(CreationContext context)
		{
			return parentHandler.Resolve(context);
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
					parentHandler.OnHandlerStateChanged -= RaiseHandlerStateChanged;
				}
			}
		}

		protected virtual void RaiseHandlerStateChanged(object s, EventArgs e)
		{
			if (OnHandlerStateChanged != null)
			{
				OnHandlerStateChanged(s, e);
			}
		}

		public event HandlerStateDelegate OnHandlerStateChanged;
	}
}