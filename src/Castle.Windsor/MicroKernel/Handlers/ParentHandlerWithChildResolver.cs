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
	using System.Collections.ObjectModel;
	using System.Linq;

	using Castle.Core;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Resolvers;

    /// <summary>
	///   Redirects resolution to the main resolver, and if not found uses
	///   the parent handler.
	/// </summary>
	public class ParentHandlerWithChildResolver : IHandler, IDisposable
	{
		private readonly ISubDependencyResolver childResolver;
        private readonly IHandler parentHandler;
        private IKernelInternal kernel;

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

		public void Dispose()
		{
			Dispose(true);
		}

		public virtual void Init(IKernelInternal kernel)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException("kernel");
            }

            this.kernel = kernel;
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
            if (parentHandler.CurrentState == HandlerState.WaitingDependency && parentHandler is IExposeDependencyInfo)
            {
                // The parent handler's root `Resolve` method will fail due to the current state of the handler being 
                // WaitingDependency. Here, we attempt to "pre-load" any outstanding dependencies from the child
                // resolver to get around this state.
                var inspector = new GetMissingDependencyInspector();
                ((IExposeDependencyInfo)parentHandler).ObtainDependencyDetails(inspector);
                var dependencyResolver = new DefaultDependencyResolver();
                dependencyResolver.Initialize(kernel, (client, model, dependency) => { });

                var dependencies = from d in inspector.MissingDependencies
                                   let instance = dependencyResolver.Resolve(context, childResolver, parentHandler.ComponentModel, d)
                                   select new { Key = d.DependencyKey, Instance = instance };

                foreach (var dependency in dependencies)
                {
                    context.AdditionalArguments.Add(dependency.Key, dependency.Instance);
                }
            }

            return parentHandler.Resolve(context);
        }

		public bool Supports(Type service)
		{
			return parentHandler.Supports(service);
		}

		public object TryResolve(CreationContext context)
		{
			return parentHandler.TryResolve(context);
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

        class GetMissingDependencyInspector : IDependencyInspector
        {
            public IList<DependencyModel> MissingDependencies { get; private set; }

            void IDependencyInspector.Inspect(IHandler handler, DependencyModel[] missingDependencies, IKernel kernel)
            {
                MissingDependencies = new ReadOnlyCollection<DependencyModel>(missingDependencies);
            }
        }
	}
}