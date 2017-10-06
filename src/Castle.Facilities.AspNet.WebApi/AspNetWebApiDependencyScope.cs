using System;
using System.Collections.Generic;
using System.Linq;

using Castle.MicroKernel.Lifestyle;

namespace Castle.Facilities.AspNet.WebApi
{
	using Castle.MicroKernel;

	internal class AspNetWebApiDependencyScope : System.Web.Http.Dependencies.IDependencyScope
	{
		private readonly IKernel kernel;
		private readonly IDisposable scope;
		private readonly AspNetWebApiDependencyResolver parentResolver;
		private readonly List<object> resolvedInstances = new List<object>();

		public AspNetWebApiDependencyScope(AspNetWebApiDependencyResolver parentResolver, IKernel kernel)
		{
			this.parentResolver = parentResolver;
			this.kernel = kernel;
			this.scope = kernel.BeginScope();
		}

		public object GetService(Type serviceType)
		{
			parentResolver.OnBeforeControllerResolved(kernel, serviceType);
			var instance = kernel.Resolve(serviceType);
			resolvedInstances.Add(instance);
			return instance;
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			parentResolver.OnBeforeControllerResolved(kernel, serviceType);
			var instances = kernel.ResolveAll(serviceType).Cast<object>();
			resolvedInstances.AddRange(instances);
			return instances.ToList();
		}

		public void Dispose()
		{
			try
			{
				foreach (var instance in resolvedInstances)
				{
					kernel.ReleaseComponent(instance);
					parentResolver.OnAfterControllerReleased(kernel, instance);
				}
			}
			finally
			{
				scope.Dispose();
			}
		}
	}
}