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

#if NET45

namespace Castle.Facilities.Owin.WebHost
{
	using System;
	using System.Web.Mvc;

	using Castle.Facilities.Owin.WebHost.Lifestyles;

	internal class MsMvcScopedControllerFactory : DefaultControllerFactory
	{

		public MsMvcScopedControllerFactory()
		{
		}

		public MsMvcScopedControllerFactory(IControllerActivator controllerActivator) : base(controllerActivator)
		{
		}

		public override IController CreateController (System.Web.Routing.RequestContext requestContext, string controllerName)
		{
			MsSystemWebHttpContextScopeAccessor.RequireScope();
			return base.CreateController(requestContext, controllerName);
		}

		public override void ReleaseController(IController controller)
		{
			(controller as IDisposable)?.Dispose();
			MsSystemWebHttpContextScopeAccessor.ReleaseScope();
		}
	}
}

#endif