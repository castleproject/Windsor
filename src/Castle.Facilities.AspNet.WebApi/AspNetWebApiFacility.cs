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

namespace Castle.Facilities.AspNet.WebApi
{
	using System;
	using System.Web.Http;
	using System.Web.Http.Dispatcher;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;

	public class AspNetWebApiFacility : AbstractFacility
	{
		private bool isSelfHosted;
		private bool initialised = false;
		private HttpConfiguration httpConfiguration = null;
		private Action<IKernel, Type> beforeControllerResolved;
		private Action<IKernel, object> afterControllerReleased;
		private AspNetWebApiControllerActivator controllerActivator;
		private AspNetWebApiDependencyResolver dependencyResolver;

		public new IKernel Kernel => base.Kernel;
		public bool AutoCreateLifestyleScopes { get; private set; }

		public AspNetWebApiControllerActivator ControllerActivator => controllerActivator;
		public AspNetWebApiDependencyResolver DependencyResolver => dependencyResolver;

		protected override void Init()
		{
			if (!isSelfHosted)
			{
				controllerActivator = new AspNetWebApiControllerActivator(Kernel, this.AutoCreateLifestyleScopes);
				(httpConfiguration ?? GlobalConfiguration.Configuration).Services.Replace(typeof(IHttpControllerActivator), controllerActivator);
			}
			else
			{
				dependencyResolver = new AspNetWebApiDependencyResolver(Kernel);
				(httpConfiguration ?? GlobalConfiguration.Configuration).DependencyResolver = dependencyResolver;
			}

			SubscribeBeforeControllerCreatedEvent();
			SubscribeAfterControllerReleaseEvent();

			initialised = true;
		}

		public AspNetWebApiFacility BeforeControllerResolved(Action<IKernel, Type> beforeResolvedCallback)
		{
			beforeControllerResolved = beforeResolvedCallback;
			if (initialised)
			{
				SubscribeBeforeControllerCreatedEvent();
			}
			return this;
		}

		public AspNetWebApiFacility AfterControllerReleased(Action<IKernel, object> afterReleasedCallback)
		{
			afterControllerReleased = afterReleasedCallback;
			if (initialised)
			{
				SubscribeAfterControllerReleaseEvent();
			}
			return this;
		}

		public AspNetWebApiFacility UsingConfiguration(HttpConfiguration httpConfiguration)
		{
			this.httpConfiguration = httpConfiguration;
			return this;
		}

		public AspNetWebApiFacility UsingSelfHosting()
		{
			isSelfHosted = true;
			return this;
		}

		public AspNetWebApiFacility WithLifestyleScopedPerWebRequest()
		{
			AutoCreateLifestyleScopes = true;
			controllerActivator?.AutoCreateLifestyleScopes();
			return this;
		}

		private void SubscribeBeforeControllerCreatedEvent()
		{
			if (beforeControllerResolved != null && controllerActivator != null)
				controllerActivator.BeforeControllerResolved += beforeControllerResolved;
			if (beforeControllerResolved != null && dependencyResolver != null)
				dependencyResolver.BeforeControllerResolved += beforeControllerResolved;
		}

		private void SubscribeAfterControllerReleaseEvent()
		{
			if (afterControllerReleased != null && controllerActivator != null)
				controllerActivator.AfterControllerReleased += afterControllerReleased;
			if (afterControllerReleased != null && dependencyResolver != null)
				dependencyResolver.AfterControllerReleased += afterControllerReleased;
		}

	}
}
