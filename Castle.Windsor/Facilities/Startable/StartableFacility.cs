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
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.SubSystems.Conversion;

	public class StartableFacility : AbstractFacility
	{
		private readonly List<IHandler> waitList = new List<IHandler>();
		private readonly HandlerStateDelegate checkWaitingList;
		private ITypeConverter converter;

		// Don't check the waiting list while this flag is set as this could result in
		// duplicate singletons.
		private bool inStart;

		public StartableFacility()
		{
			checkWaitingList = OnHandlerStateChanged;	
		}

		protected override void Init()
		{
			converter = (ITypeConverter) Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);

			Kernel.ComponentModelCreated +=OnComponentModelCreated;
			Kernel.ComponentRegistered +=OnComponentRegistered;
		}

		private void OnComponentModelCreated(ComponentModel model)
		{
			bool startable =
				CheckIfComponentImplementsIStartable(model) || HasStartableAttributeSet(model);

			model.ExtendedProperties["startable"] = startable;

			if (startable)
			{
				model.LifecycleSteps.Add(
					LifecycleStepType.Commission, StartConcern.Instance);
				model.LifecycleSteps.AddFirst(
					LifecycleStepType.Decommission, StopConcern.Instance);
			}
		}

		private void OnComponentRegistered(String key, IHandler handler)
		{
			bool startable = (bool?)handler.ComponentModel.ExtendedProperties["startable"] ?? false;

			if (startable)
			{
				if (TryStart(handler) == false)
				{
					AddHandlerToWaitingList(handler);
				}
			}

			CheckWaitingList();
		}

		private void OnHandlerStateChanged(object source, EventArgs args)
		{
			CheckWaitingList();
		}

		private void AddHandlerToWaitingList(IHandler handler)
		{
			waitList.Add(handler);
			handler.OnHandlerStateChanged += checkWaitingList;
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
						handler.OnHandlerStateChanged -= checkWaitingList;
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

		private static bool CheckIfComponentImplementsIStartable(ComponentModel model)
		{
			return typeof(IStartable).IsAssignableFrom(model.Implementation);
		}

		private bool HasStartableAttributeSet(ComponentModel model)
		{
			bool result = false;

			if (model.Configuration != null)
			{
				String startable = model.Configuration.Attributes["startable"];

				if (startable != null)
				{
					result = (bool) converter.PerformConversion(startable, typeof(bool));
				}
			}

			return result;
		}
	}
}
