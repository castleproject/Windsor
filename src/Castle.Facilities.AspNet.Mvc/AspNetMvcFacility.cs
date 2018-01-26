// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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

using System.Reflection;

namespace Castle.Facilities.AspNet.Mvc
{
	using System;
	using System.Collections.Generic;
	using System.Web.Mvc;
	using System.Web.SessionState;

	using Castle.Facilities.AspNet.Mvc.Exceptions;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;

	public class AspNetMvcFacility : AbstractFacility
	{
		private bool initialised = false;
		private AspNetMvcControllerFactory controllerFactory;
		private Action<IKernel, Type> beforeControllerResolved;
		private Action<IKernel, object> afterControllerReleased;
		private readonly List<Assembly> controllerAssemblies = new List<Assembly>();

		public new IKernel Kernel => base.Kernel;
		public bool AutoCreateLifestyleScopes { get; private set; }
		public SessionStateBehavior SessionState { get; private set; } = SessionStateBehavior.Default;
		public AspNetMvcControllerFactory ControllerFactory => controllerFactory;

		protected override void Init()
		{
			ThrowIfControllerAssemblyWasNotSet();

			controllerFactory = new AspNetMvcControllerFactory(this.Kernel, this.AutoCreateLifestyleScopes, this.SessionState, controllerAssemblies);
			ControllerBuilder.Current.SetControllerFactory(controllerFactory);

			SubscribeBeforeControllerCreatedEvent();
			SubscribeAfterControllerReleaseEvent();

			initialised = true;
		}

		public AspNetMvcFacility AddControllerAssembly<T>()
		{
			controllerAssemblies.Add(typeof(T).Assembly);
			return this;
		}

		public AspNetMvcFacility BeforeControllerResolved(Action<IKernel, Type> beforeResolvedCallback)
		{
			this.beforeControllerResolved = beforeResolvedCallback;
			if (initialised) SubscribeBeforeControllerCreatedEvent();
			return this;
		}

		public AspNetMvcFacility AfterControllerReleased(Action<IKernel, object> afterReleasedCallback)
		{
			this.afterControllerReleased = afterReleasedCallback;
			if (initialised) SubscribeAfterControllerReleaseEvent();
			return this;
		}

		public AspNetMvcFacility WithLifestyleScopedPerWebRequest()
		{
			this.AutoCreateLifestyleScopes = true;
			controllerFactory?.AutoCreateLifestyleScopes();
			return this;
		}

		private void SubscribeBeforeControllerCreatedEvent()
		{
			if (beforeControllerResolved != null)
			{
				controllerFactory.BeforeControllerResolved += beforeControllerResolved;
			}
		}

		private void SubscribeAfterControllerReleaseEvent()
		{
			if (afterControllerReleased != null)
			{
				controllerFactory.AfterControllerReleased += afterControllerReleased;
			}
		}

		private void ThrowIfControllerAssemblyWasNotSet()
		{
			if (controllerAssemblies.Count == 0)
			{
				throw new ControllerAssemblyWasNotSetException("The controller assembly was not set. Did you set it up when adding the facility? Try using 'container.AddFacility<AspNetMvcFacility>(x => x.AddControllerAssembly<MvcApplication>());'");
			}
		}
	}
}
