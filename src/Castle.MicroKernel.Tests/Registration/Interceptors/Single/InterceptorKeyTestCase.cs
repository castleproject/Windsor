using System.Collections.Generic;
using Castle.Core;
using Castle.MicroKernel.Registration;

namespace Castle.MicroKernel.Tests.Registration.Interceptors.Single
{
	public class InterceptorKeyTestCase : InterceptorsTestCaseBase
	{
		protected override IRegistration RegisterInterceptors<S>(ComponentRegistration<S> registration)
		{
			return registration.Interceptors("interceptorKey");
		}

		protected override IEnumerable<InterceptorReference> GetExpectedInterceptorsInCorrectOrder()
		{
			yield return InterceptorReference.ForKey("interceptorKey");
		}
	}
}
