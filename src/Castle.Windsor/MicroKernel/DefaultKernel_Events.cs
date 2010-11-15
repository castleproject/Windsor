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
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Security;

	using Castle.Core;

	/// <summary>
	/// Default implementation of <see cref="IKernel"/>. 
	/// This implementation is complete and also support a kernel 
	/// hierarchy (sub containers).
	/// </summary>
#if (SILVERLIGHT)
	public partial class DefaultKernel : IKernel, IKernelEvents
#else

	public partial class DefaultKernel
#endif
	{
		private static readonly object HandlerRegisteredEvent = new object();
		private static readonly object HandlersChangedEvent = new object();
		private static readonly object ComponentRegisteredEvent = new object();
		private static readonly object ComponentUnregisteredEvent = new object();
		private static readonly object ComponentCreatedEvent = new object();
		private static readonly object ComponentDestroyedEvent = new object();
		private static readonly object AddedAsChildKernelEvent = new object();
		private static readonly object ComponentModelCreatedEvent = new object();
		private static readonly object DependencyResolvingEvent = new object();
		private static readonly object RemovedAsChildKernelEvent = new object();
		private static readonly object RegistrationCompletedEvent = new object();

		private readonly object handlersChangedLock = new object();
		private bool handlersChanged;
		private volatile bool handlersChangedDeferred;

#if (!SILVERLIGHT)
		[NonSerialized]
#endif
			private readonly IDictionary<object, Delegate> events = new Dictionary<object, Delegate>();

#if !SILVERLIGHT
#if DOTNET40
		[SecurityCritical]
#endif
		public override object InitializeLifetimeService()
		{
			return null;
		}
#endif

		public event HandlerDelegate HandlerRegistered
		{
			add { AddHandler(HandlerRegisteredEvent, value); }
			remove { RemoveHandler(HandlerRegisteredEvent, value); }
		}

		public event HandlersChangedDelegate HandlersChanged
		{
			add { AddHandler(HandlersChangedEvent, value); }
			remove { RemoveHandler(HandlersChangedEvent, value); }
		}

		public event ComponentDataDelegate ComponentRegistered
		{
			add { AddHandler(ComponentRegisteredEvent, value); }
			remove { RemoveHandler(ComponentRegisteredEvent, value); }
		}

		public event ComponentDataDelegate ComponentUnregistered
		{
			add { AddHandler(ComponentUnregisteredEvent, value); }
			remove { RemoveHandler(ComponentUnregisteredEvent, value); }
		}

		public event ComponentInstanceDelegate ComponentCreated
		{
			add { AddHandler(ComponentCreatedEvent, value); }
			remove { RemoveHandler(ComponentCreatedEvent, value); }
		}

		public event ComponentInstanceDelegate ComponentDestroyed
		{
			add { AddHandler(ComponentDestroyedEvent, value); }
			remove { RemoveHandler(ComponentDestroyedEvent, value); }
		}

		public event EventHandler AddedAsChildKernel
		{
			add { AddHandler(AddedAsChildKernelEvent, value); }
			remove { RemoveHandler(AddedAsChildKernelEvent, value); }
		}

		event EventHandler IKernelEventsInternal.RegistrationCompleted
		{
			add { AddHandler(RegistrationCompletedEvent, value); }
			remove { RemoveHandler(RegistrationCompletedEvent, value); }
		}

		public event EventHandler RemovedAsChildKernel
		{
			add { AddHandler(RemovedAsChildKernelEvent, value); }
			remove { RemoveHandler(RemovedAsChildKernelEvent, value); }
		}

		public event ComponentModelDelegate ComponentModelCreated
		{
			add { AddHandler(ComponentModelCreatedEvent, value); }
			remove { RemoveHandler(ComponentModelCreatedEvent, value); }
		}

		public event DependencyDelegate DependencyResolving
		{
			add { AddHandler(DependencyResolvingEvent, value); }
			remove { RemoveHandler(DependencyResolvingEvent, value); }
		}

		protected virtual void RaiseComponentRegistered(String key, IHandler handler)
		{
			var eventDelegate = GetEventHandlers<ComponentDataDelegate>(ComponentRegisteredEvent);
			if (eventDelegate != null)
			{
				eventDelegate(key, handler);
			}
		}

		protected virtual void RaiseComponentUnregistered(String key, IHandler handler)
		{
			var eventDelegate = GetEventHandlers<ComponentDataDelegate>(ComponentUnregisteredEvent);
			if (eventDelegate != null)
			{
				eventDelegate(key, handler);
			}
		}

		protected virtual void RaiseComponentCreated(ComponentModel model, object instance)
		{
			var eventDelegate = GetEventHandlers<ComponentInstanceDelegate>(ComponentCreatedEvent);
			if (eventDelegate != null)
			{
				eventDelegate(model, instance);
			}
		}

		protected virtual void RaiseComponentDestroyed(ComponentModel model, object instance)
		{
			var eventDelegate = GetEventHandlers<ComponentInstanceDelegate>(ComponentDestroyedEvent);
			if (eventDelegate != null)
			{
				eventDelegate(model, instance);
			}
		}

		protected virtual void RaiseAddedAsChildKernel()
		{
			var eventDelegate = GetEventHandlers<EventHandler>(AddedAsChildKernelEvent);
			if (eventDelegate != null)
			{
				eventDelegate(this, EventArgs.Empty);
			}
		}

		protected virtual void RaiseRemovedAsChildKernel()
		{
			var eventDelegate = GetEventHandlers<EventHandler>(RemovedAsChildKernelEvent);
			if (eventDelegate != null)
			{
				eventDelegate(this, EventArgs.Empty);
			}
		}

		protected virtual void RaiseComponentModelCreated(ComponentModel model)
		{
			var eventDelegate = GetEventHandlers<ComponentModelDelegate>(ComponentModelCreatedEvent);
			if (eventDelegate != null)
			{
				eventDelegate(model);
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

		private void DoActualRaisingOfHandlersChanged()
		{
			var stateChanged = true;
			while (stateChanged)
			{
				stateChanged = false;
				var eventDelegate = GetEventHandlers<HandlersChangedDelegate>(HandlersChangedEvent);
				if (eventDelegate != null)
				{
					eventDelegate(ref stateChanged);
				}
			}
		}

		public virtual void RaiseHandlerRegistered(IHandler handler)
		{
			var stateChanged = true;

			while (stateChanged)
			{
				stateChanged = false;
				var eventDelegate = GetEventHandlers<HandlerDelegate>(HandlerRegisteredEvent);
				if (eventDelegate != null)
				{
					eventDelegate(handler, ref stateChanged);
				}
			}
		}

		protected virtual void RaiseDependencyResolving(ComponentModel client, DependencyModel model, Object dependency)
		{
			var eventDelegate = GetEventHandlers<DependencyDelegate>(DependencyResolvingEvent);
			if (eventDelegate != null)
			{
				eventDelegate(client, model, dependency);
			}
		}

		public IDisposable OptimizeDependencyResolution()
		{
			if (handlersChangedDeferred)
			{
				return null;
			}

			handlersChangedDeferred = true;

			return new OptimizeDependencyResolutionDisposable(this);
		}

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

		private void RaiseRegistrationCompleted()
		{
			var eventDelegate = GetEventHandlers<EventHandler>(RegistrationCompletedEvent);
			if (eventDelegate != null)
			{
				eventDelegate(this, EventArgs.Empty);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected void AddHandler(object key, Delegate value)
		{
			Delegate @event;
			if (events.TryGetValue(key, out @event))
			{
				events[key] = Delegate.Combine(@event, value);
				return;
			}

			events.Add(key, Delegate.Combine(value, null));
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected void RemoveHandler(object key, Delegate value)
		{
			Delegate @event;
			if (events.TryGetValue(key, out @event))
			{
				events[key] = Delegate.Remove(@event, value);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected TEventHandler GetEventHandlers<TEventHandler>(object key) where TEventHandler : class
			// where TEventHandler : Delegate <-- this is illegal :/
		{
			Delegate handlers;
			events.TryGetValue(key, out handlers);
			return handlers as TEventHandler;
		}
	}
}