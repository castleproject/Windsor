using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel;

namespace Castle.Facilities.AspNet.WebApi
{
	public class AspNetWebApiDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver
	{
		private readonly IKernel kernel;

		public event Action<IKernel, Type> BeforeControllerResolved;
		public event Action<IKernel, object> AfterControllerReleased;

		public AspNetWebApiDependencyResolver(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public object GetService(Type serviceType)
		{
			// This is not supported, you have not way of telling when the component will be released.
			return null;
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			// This is not supported, you have not way of telling when the component will be released.
			return Enumerable.Empty<object>();
		}

		public System.Web.Http.Dependencies.IDependencyScope BeginScope()
		{
			var scope = new AspNetWebApiDependencyScope(this, kernel);
			return scope;
		}

		public virtual void OnAfterControllerReleased(IKernel kernel, object controllerInstance)
		{
			AfterControllerReleased?.Invoke(kernel, controllerInstance);
		}

		public virtual void OnBeforeControllerResolved(IKernel kernel, Type controllerType)
		{
			BeforeControllerResolved?.Invoke(kernel, controllerType);
		}

		public void Dispose()
		{
			this.kernel.Dispose();
		}
	}
}