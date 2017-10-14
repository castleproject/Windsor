using System;
using System.Collections.Generic;
using System.Linq;

using Castle.MicroKernel.Lifestyle;

namespace Castle.Facilities.AspNet.WebApi
{
	internal class AspNetWebApiDependencyScope : System.Web.Http.Dependencies.IDependencyScope
	{
		private readonly IDisposable scope;
		private readonly ISupportFacility facilitySupport;
		private readonly AspNetWebApiDependencyResolver parentResolver;
		private readonly List<object> resolvedInstances = new List<object>();

		public AspNetWebApiDependencyScope(AspNetWebApiDependencyResolver parentResolver, ISupportFacility facilitySupport)
		{
			this.parentResolver = parentResolver;
			this.facilitySupport = facilitySupport;
			this.scope = facilitySupport.Kernel.BeginScope();
		}

		public object GetService(Type serviceType)
		{
			parentResolver.OnBeforeControllerResolved(facilitySupport.Kernel, serviceType);
			var instance = facilitySupport.Kernel.Resolve(serviceType);
			resolvedInstances.Add(instance);
			return instance;
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			parentResolver.OnBeforeControllerResolved(facilitySupport.Kernel, serviceType);
			var instances = facilitySupport.Kernel.ResolveAll(serviceType).Cast<object>();
			resolvedInstances.AddRange(instances);
			return instances.ToList();
		}

		public void Dispose()
		{
			try
			{
				foreach (var instance in resolvedInstances)
				{
					facilitySupport.Kernel.ReleaseComponent(instance);
					parentResolver.OnAfterControllerReleased(facilitySupport.Kernel, instance);
				}
			}
			finally
			{
				scope.Dispose();
			}
		}
	}
}