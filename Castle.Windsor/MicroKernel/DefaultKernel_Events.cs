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

namespace Castle.MicroKernel
{
	using System;
	using System.Security;

	using Castle.Core;

	/// <summary>
	///   Default implementation of <see cref = "IKernel" />. 
	///   This implementation is complete and also support a kernel 
	///   hierarchy (sub containers).
	/// </summary>
	public partial class DefaultKernel
	{
		private readonly object handlersChangedLock = new object();
		private bool handlersChanged;
		private volatile bool handlersChangedDeferred;

#if !SILVERLIGHT
#if DOTNET40
		[SecurityCritical]
#endif
		public override object InitializeLifetimeService()
		{
			return null;
		}
#endif

		public IDisposable OptimizeDependencyResolution()
		{
			if (handlersChangedDeferred)
			{
				return null;
			}

			handlersChangedDeferred = true;

			return new OptimizeDependencyResolutionDisposable(this);
		}

		public virtual void RaiseHandlerRegistered(IHandler handler)
		{
			var stateChanged = true;

			while (stateChanged)
			{
				stateChanged = false;
				HandlerRegistered(handler, ref stateChanged);
			}
		}

		public virtual void RaiseHandlersChanged()
		{
			if (handlersChangedDeferred)
			{
				lock (handlersChangedLock)
				{
					handlersChanged = true;
				}

				return;
			}

			DoActualRaisingOfHandlersChanged();
		}

		protected virtual void RaiseAddedAsChildKernel()
		{
			AddedAsChildKernel(this, EventArgs.Empty);
		}

		protected virtual void RaiseComponentCreated(ComponentModel model, object instance)
		{
			ComponentCreated(model, instance);
		}

		protected virtual void RaiseComponentDestroyed(ComponentModel model, object instance)
		{
			ComponentDestroyed(model, instance);
		}

		protected virtual void RaiseComponentModelCreated(ComponentModel model)
		{
			ComponentModelCreated(model);
		}

		protected virtual void RaiseComponentRegistered(String key, IHandler handler)
		{
			ComponentRegistered(key, handler);
		}

		protected virtual void RaiseDependencyResolving(ComponentModel client, DependencyModel model, Object dependency)
		{
			DependencyResolving(client, model, dependency);
		}

		protected virtual void RaiseRegistrationCompleted()
		{
			RegistrationCompleted(this, EventArgs.Empty);
		}

		protected virtual void RaiseRemovedAsChildKernel()
		{
			RemovedAsChildKernel(this, EventArgs.Empty);
		}

		private void DoActualRaisingOfHandlersChanged()
		{
			var stateChanged = true;
			while (stateChanged)
			{
				stateChanged = false;
				HandlersChanged(ref stateChanged);
			}
		}

		public event HandlerDelegate HandlerRegistered = delegate { };

		public event HandlersChangedDelegate HandlersChanged = delegate { };

		public event ComponentDataDelegate ComponentRegistered = delegate { };

		public event ComponentInstanceDelegate ComponentCreated = delegate { };

		public event ComponentInstanceDelegate ComponentDestroyed = delegate { };

		public event EventHandler AddedAsChildKernel = delegate { };

		public event EventHandler ContainerDisposed = delegate { };

		public event EventHandler RegistrationCompleted = delegate { };

		public event EventHandler RemovedAsChildKernel = delegate { };

		public event ComponentModelDelegate ComponentModelCreated = delegate { };

		public event DependencyDelegate DependencyResolving = delegate { };

		private class OptimizeDependencyResolutionDisposable : IDisposable
		{
			private readonly DefaultKernel kernel;

			public OptimizeDependencyResolutionDisposable(DefaultKernel kernel)
			{
				this.kernel = kernel;
			}

			public void Dispose()
			{
				lock (kernel.handlersChangedLock)
				{
					try
					{
						if (kernel.handlersChanged == false)
						{
							return;
						}

						kernel.DoActualRaisingOfHandlersChanged();
						kernel.RaiseRegistrationCompleted();
						kernel.handlersChanged = false;
					}
					finally
					{
						kernel.handlersChangedDeferred = false;
					}
				}
			}
		}

		private void RaiseContainerDisposed()
		{
			ContainerDisposed(this, EventArgs.Empty);
		}
	}
}