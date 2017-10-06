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
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Web.SessionState;

	using Castle.Facilities.AspNet.Mvc.Exceptions;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Lifestyle;

	public class AspNetMvcControllerFactory : IControllerFactory
	{
		private const string ContextScopeKey = "castle-windsor-facility-aspnet-mvc-scope";

		private readonly IKernel kernel;
		private bool autoCreateLifestyleScopes;
		private SessionStateBehavior sessionStateBehaviour;
        private IEnumerable<Type> controllerCandidateTypes;

        public event Action<IKernel, Type> BeforeControllerResolved;
		public event Action<IKernel, object> AfterControllerReleased;

		public AspNetMvcControllerFactory(IKernel kernel, bool autoCreateLifestyleScopes, SessionStateBehavior sessionStateBehaviour, List<Assembly> controllerAssemblies)
		{
			this.kernel = kernel;
			this.sessionStateBehaviour = sessionStateBehaviour;
			this.autoCreateLifestyleScopes = autoCreateLifestyleScopes;
			this.controllerCandidateTypes = controllerAssemblies.SelectMany(x => x.ExportedTypes).ToList();
		}

		public IController CreateController(RequestContext requestContext, string controllerName)
		{
			var adjustedControllerName = $"{controllerName}Controller";
			var controllerType = FindControllerType(adjustedControllerName);
			BeginScopedLifestyleIfRequired();
			OnBeforeControllerResolved(controllerType);
			try
			{
				return (IController) kernel.Resolve(controllerType);
			}
			catch (InvalidOperationException err)
			{
				throw new LifestyleScopesPotentiallyNotEnabledException("If you are having trouble with scoped lifestyles, please try 'container.AddFacility<AspNetMvcFacility>(x => x.AddControllerAssembly<MvcApplication>().WithLifestyleScopedPerWebRequest())'", err);
			}
		}

		public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
		{
			return sessionStateBehaviour;
		}

		public void ReleaseController(IController controller)
		{
			kernel.ReleaseComponent(controller);
			DisposeScopedLifestyleIfRequired();
			OnAfterControllerReleased(controller);
		}

		public void AutoCreateLifestyleScopes()
		{
			this.autoCreateLifestyleScopes = true;
		}

		public void SetSessionStateBehaviour(SessionStateBehavior sessionState)
		{
			this.sessionStateBehaviour = sessionState;
		}

		protected virtual void OnAfterControllerReleased(object controllerInstance)
		{
			AfterControllerReleased?.Invoke(kernel, controllerInstance);
		}

		protected virtual void OnBeforeControllerResolved(Type controllerType)
		{
			BeforeControllerResolved?.Invoke(kernel, controllerType);
		}

		private Type FindControllerType(string controllerName)
		{
			foreach (var type in this.controllerCandidateTypes)
			{
				if (type.Name == controllerName)
				{
					return type;
				}
			}

			throw new MissingControllerRegistrationException($"We could not locate the controller '{controllerName}', did you forget to register it?");
		}

		private void BeginScopedLifestyleIfRequired()
		{
			if (autoCreateLifestyleScopes)
			{
				HttpContext.Current.Items[ContextScopeKey] = kernel.BeginScope();
			}
		}

		private void DisposeScopedLifestyleIfRequired()
		{
			if (autoCreateLifestyleScopes)
			{
				(HttpContext.Current.Items[ContextScopeKey] as IDisposable)?.Dispose();
				HttpContext.Current.Items.Remove(ContextScopeKey);
			}
		}

		
	}
}