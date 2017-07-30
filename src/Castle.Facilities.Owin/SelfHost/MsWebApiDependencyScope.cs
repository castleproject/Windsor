#if NET45

namespace Castle.Facilities.Owin.SelfHost
{
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel.Lifestyle;

	internal class MsWebApiDependencyScope : System.Web.Http.Dependencies.IDependencyScope
	{
		private IDisposable scope;

		public MsWebApiDependencyScope()
		{
			this.scope = DependencyServiceLocator.Container.BeginScope();
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
			scope.Dispose();
		}
	}
}

#endif