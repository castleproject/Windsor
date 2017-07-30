#if NET45

namespace Castle.Facilities.Owin.WebHost
{
	using System;
	using System.Collections.Generic;

	using Castle.Facilities.Owin.WebHost.Lifestyles;

	internal class MsWebApiDependencyScope : System.Web.Http.Dependencies.IDependencyScope
	{
		public MsWebApiDependencyScope()
		{
			MsSystemWebHttpContextScopeAccessor.RequireScope();
		}

		public object GetService(Type serviceType)
		{
			return DependencyServiceLocator.GetService(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return DependencyServiceLocator.GetServices(serviceType);
		}

		public void Dispose()
		{
			MsSystemWebHttpContextScopeAccessor.ReleaseScope();
		}
	}
}

#endif