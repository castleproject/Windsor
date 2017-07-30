#if NET45

namespace Castle.Facilities.Owin.WebHost
{
	using System;
	using System.Collections.Generic;

	internal class MsWebApiDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver
	{
		public object GetService(Type serviceType)
		{
			return DependencyServiceLocator.GetService(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return DependencyServiceLocator.GetServices(serviceType);
		}

		public System.Web.Http.Dependencies.IDependencyScope BeginScope()
		{
			return new MsWebApiDependencyScope();
		}

		public void Dispose()
		{
		}
	}
}

#endif