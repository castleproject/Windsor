#if NET45

namespace Castle.Facilities.Owin
{
	using System;
	using System.Collections.Generic;

	internal class MsWebApiDependencyResolver<T> : System.Web.Http.Dependencies.IDependencyResolver where T : System.Web.Http.Dependencies.IDependencyScope, new()
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
			return new T();
		}

		public void Dispose()
		{
		}
	}
}

#endif