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