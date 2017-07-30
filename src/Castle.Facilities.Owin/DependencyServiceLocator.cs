#if NET45

namespace Castle.Facilities.Owin
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Windsor;

	internal static class DependencyServiceLocator
	{
		public static IWindsorContainer Container;

		public static object GetService(Type serviceType)
		{
			if (Container.Kernel.HasComponent(serviceType))
				return Container.Resolve(serviceType);
			return null;
		}

		public static IEnumerable<object> GetServices(Type serviceType)
		{
			if (Container.Kernel.HasComponent(serviceType))
				return Container.ResolveAll(serviceType).Cast<object>().ToList();
			return Enumerable.Empty<object>();
		}
	}
}

#endif
