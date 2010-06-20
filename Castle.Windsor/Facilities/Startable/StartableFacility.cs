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

namespace Castle.Facilities.Startable
{
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.Windsor;

	public class StartableFacility : AbstractFacility
	{
		private bool optimizeForSingleInstall;
		private readonly List<IHandler> waitList = new List<IHandler>();
		private ITypeConverter converter;

		// Don't check the waiting list while this flag is set as this could result in
		// duplicate singletons.
		private bool inStart;
		private bool disableException;

		/// <summary>
		/// This method changes behavior of the facility. Deferred mode should be used when you
		/// have single call to <see cref="IWindsorContainer.Install"/> and register all your components there.
		/// Enabling this mode will optimize the behavior of the facility so that it will wait 'till the end of
		/// intallation and only after all <see cref="IWindsorInstaller"/>s were ran it will instantiate and
		/// start all the startable components. If the <paramref name="tryStart"/> is set to <c>false</c> an
		/// exception will be thrown is a startable component can't be instantiated and started. This will
		/// help you fail fast and diagnose issues quickly.
		/// </summary>
		/// <param name="tryStart"></param>
		public void DeferredStart(bool tryStart = false)
		{
			optimizeForSingleInstall = true;
			disableException = tryStart;
		}

		protected override void Init()
		{
			converter = (ITypeConverter)Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);
			Kernel.ComponentModelBuilder.AddContributor(new StartableContributor(converter));
			if(optimizeForSingleInstall)
			{
				Kernel.HandlersChanged += StartAll;
				Kernel.ComponentRegistered += CacheForStart;
				return;
			}
			Kernel.ComponentRegistered +=OnComponentRegistered;
		}

		private void CacheForStart(string key, IHandler handler)
		{
			if (IsStartable(handler))
			{
				waitList.Add(handler);
			}
		}

		private void StartAll(ref bool statechanged)
		{
			var array = waitList.ToArray();
			waitList.Clear();
			foreach (var handler in array)
			{
				if (disableException == false)
				{
					Start(handler);
					continue;
				}

				if (TryStart(handler) == false)
				{
					AddHandlerToWaitingList(handler);
				}
			}
		}

		private void Start(IHandler handler)
		{
			handler.Resolve(CreationContext.Empty);
		}

		private void OnComponentRegistered(String key, IHandler handler)
		{
			var startable = IsStartable(handler);
			if (startable)
			{
				if (TryStart(handler) == false)
				{
					AddHandlerToWaitingList(handler);
				}
			}

			CheckWaitingList();
		}

		private bool IsStartable(IHandler handler)
		{
			var isStartable = (bool?)handler.ComponentModel.ExtendedProperties["startable"];
			return isStartable ?? false;
		}

		private void OnHandlerStateChanged(object source, EventArgs args)
		{
			CheckWaitingList();
		}

		private void AddHandlerToWaitingList(IHandler handler)
		{
			waitList.Add(handler);
			handler.OnHandlerStateChanged += OnHandlerStateChanged;
		}

		/// <summary>
		/// For each new component registered,
		/// some components in the WaitingDependency
		/// state may have became valid, so we check them
		/// </summary>
		private void CheckWaitingList()
		{
			if (!inStart)
			{
				var handlers = waitList.ToArray();
				foreach (IHandler handler in handlers)
				{
					if (TryStart(handler))
					{
						waitList.Remove(handler);
						handler.OnHandlerStateChanged -= OnHandlerStateChanged;
					}
				}
			}
		}

		/// <summary>
		/// Request the component instance
		/// </summary>
		/// <param name="handler"></param>
		private bool TryStart(IHandler handler)
		{
			try
			{
				inStart = true;
				return handler.TryResolve(CreationContext.Empty) != null;
			}
			finally
			{
				inStart = false;
			}
		}
	}
}
