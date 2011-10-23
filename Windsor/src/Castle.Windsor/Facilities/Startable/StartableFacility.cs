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

namespace Castle.Facilities.Startable
{
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.Windsor;

	public class StartableFacility : AbstractFacility
	{
		private readonly List<IHandler> waitList = new List<IHandler>();
		private ITypeConverter converter;

		// Don't check the waiting list while this flag is set as this could result in
		// duplicate singletons.
		private bool disableException;
		private bool inStart;
		private bool optimizeForSingleInstall;

		/// <summary>
		///   This method changes behavior of the facility. Deferred mode should be used when you
		///   have single call to <see cref = "IWindsorContainer.Install" /> and register all your components there.
		///   Enabling this mode will optimize the behavior of the facility so that it will wait 'till the end of
		///   installation and only after all <see cref = "IWindsorInstaller" />s were ran it will instantiate and
		///   start all the startable components. An exception will be thrown if a startable component can't be 
		///   instantiated and started. This will help you fail fast and diagnose issues quickly. If you don't want
		///   the exception to be thrown and you prefer the component to fail silently, use <see cref = "DeferredTryStart" /> method instead.
		/// </summary>
		/// <remarks>
		///   It is recommended to use this method over <see cref = "DeferredTryStart" /> method.
		/// </remarks>
		public void DeferredStart()
		{
			optimizeForSingleInstall = true;
			disableException = false;
		}

		/// <summary>
		///   This method changes behavior of the facility. Deferred mode should be used when you
		///   have single call to <see cref = "IWindsorContainer.Install" /> and register all your components there.
		///   Enabling this mode will optimize the behavior of the facility so that it will wait 'till the end of
		///   installation and only after all <see cref = "IWindsorInstaller" />s were ran it will instantiate and
		///   start all the startable components. No exception will be thrown if a startable component can't be 
		///   instantiated and started. If you'd rather fail fast and diagnose issues quickly, use <see cref = "DeferredStart" /> method instead.
		/// </summary>
		/// <remarks>
		///   It is recommended to use <see cref = "DeferredStart" /> method over this method.
		/// </remarks>
		public void DeferredTryStart()
		{
			optimizeForSingleInstall = true;
			disableException = true;
		}

		protected override void Init()
		{
			converter = Kernel.GetConversionManager();
			Kernel.ComponentModelBuilder.AddContributor(new StartableContributor(converter));
			if (optimizeForSingleInstall)
			{
				Kernel.RegistrationCompleted += StartAll;
				Kernel.ComponentRegistered += CacheForStart;
				return;
			}
			Kernel.ComponentRegistered += OnComponentRegistered;
		}

		private void AddHandlerToWaitingList(IHandler handler)
		{
			waitList.Add(handler);
		}

		private void CacheForStart(string key, IHandler handler)
		{
			if (IsStartable(handler))
			{
				waitList.Add(handler);
			}
		}

		/// <summary>
		///   For each new component registered,
		///   some components in the WaitingDependency
		///   state may have became valid, so we check them
		/// </summary>
		private void CheckWaitingList()
		{
			if (!inStart)
			{
				var handlers = waitList.ToArray();
				foreach (var handler in handlers)
				{
					if (TryStart(handler))
					{
						waitList.Remove(handler);
					}
				}
			}
		}

		private bool IsStartable(IHandler handler)
		{
			var startable = handler.ComponentModel.ExtendedProperties["startable"];
			var isStartable = (bool?)startable;
			return isStartable.GetValueOrDefault();
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

		private void Start(IHandler handler)
		{
			handler.Resolve(CreationContext.CreateEmpty());
		}

		private void StartAll(object sender, EventArgs e)
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

		/// <summary>
		///   Request the component instance
		/// </summary>
		/// <param name = "handler"></param>
		private bool TryStart(IHandler handler)
		{
			try
			{
				inStart = true;
				return handler.TryResolve(CreationContext.CreateEmpty()) != null;
			}
			finally
			{
				inStart = false;
			}
		}
	}
}