#if NET45

namespace Castle.Facilities.Owin.WebHost
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;

	internal class MsMvcDependencyResolver : IDependencyResolver
	{
		public object GetService(Type serviceType)
		{
			return DependencyServiceLocator.GetService(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return DependencyServiceLocator.GetServices(serviceType).ToList();
		}
	}
}

#endif
