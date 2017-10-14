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
	using System.Net.Http;
	using System.Web;
	using System.Web.Http.Controllers;
	using System.Web.Http.Dispatcher;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle;
	using Castle.Facilities.AspNet.WebApi.Exceptions;

	public class AspNetWebApiControllerActivator : IHttpControllerActivator
	{
		public const string ContextScopeKey = "castle-windsor-facility-aspnet-webapi-scope";

		private readonly ISupportFacility facilitySupport;

		public event Action<IKernel, Type> BeforeControllerResolved;
		public event Action<IKernel, object> AfterControllerReleased;

		public AspNetWebApiControllerActivator(ISupportFacility facilitySupport)
		{
			this.facilitySupport = facilitySupport;
		}

		public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
		{
			BeginScopedLifestyleIfRequired();
			OnBeforeControllerResolved(controllerType);
			try
			{
				var controllerInstance = (IHttpController)facilitySupport.Kernel.Resolve(controllerType);
				request.RegisterForDispose(new AspNetWebApiControllerDeactivator(() =>
				{
					facilitySupport.Kernel.ReleaseComponent(controllerInstance);
					DisposeScopedLifestyleIfRequired();
					OnAfterControllerReleased(controllerInstance);
				}));
				return controllerInstance;
			}
			catch (InvalidOperationException err)
			{
				throw new LifestyleScopesPotentiallyNotEnabledException("If you are having trouble with scoped lifestyles, please try 'container.AddFacility<AspNetWebApiFacility>(x => x.UsingConfiguration(config).WithLifestyleScopedPerWebRequest())'", err);
			}
		}

		protected virtual void OnAfterControllerReleased(object controllerInstance)
		{
			AfterControllerReleased?.Invoke(facilitySupport.Kernel, controllerInstance);
		}

		protected virtual void OnBeforeControllerResolved(Type controllerType)
		{
			BeforeControllerResolved?.Invoke(facilitySupport.Kernel, controllerType);
		}

		private void BeginScopedLifestyleIfRequired()
		{
			if (facilitySupport.AutoCreateLifestyleScopes)
			{
				ThrowIfHttpContextIsNull();
				HttpContext.Current.Items[ContextScopeKey] = facilitySupport.Kernel.BeginScope();
			}
		}

		private void DisposeScopedLifestyleIfRequired()
		{
			if (facilitySupport.AutoCreateLifestyleScopes)
			{
				(HttpContext.Current.Items[ContextScopeKey] as IDisposable)?.Dispose();
			}
		}

		private void ThrowIfHttpContextIsNull()
		{
			if (HttpContext.Current == null)
			{
				throw new InvalidSelfHostConfigurationException($"Looks like HttpContext.Current is null, please try container.AddFacility<AspNetWebApiFacility>(x => x.UsingConfiguration(config).UsingSelfHosting()) instead. If you are using scoped lifestyles you need container.AddFacility<AspNetWebApiFacility>(x => x.UsingConfiguration(config).UsingSelfHosting().WithLifestyleScopedPerWebRequest()).");
			}
		}
	}
}