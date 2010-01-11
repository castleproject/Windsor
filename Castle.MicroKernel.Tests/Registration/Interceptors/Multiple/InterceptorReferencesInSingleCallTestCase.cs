using System.Collections.Generic;
using Castle.Core;
using Castle.MicroKernel.Registration;

namespace Castle.MicroKernel.Tests.Registration.Interceptors.Multiple
{
	public class InterceptorReferencesAnywhereInSingleCallTestCase : InterceptorsTestCaseBase
	{
		protected override IRegistration RegisterInterceptors<S>(ComponentRegistration<S> registration)
		{
			return
				registration.Interceptors(InterceptorReference.ForType(typeof (TestInterceptor1)),
				                          InterceptorReference.ForType(typeof (TestInterceptor2))).Anywhere;
		}

		protected override IEnumerable<InterceptorReference> GetExpectedInterceptorsInCorrectOrder()
		{
			yield return InterceptorReference.ForType<TestInterceptor1>();
			yield return InterceptorReference.ForType<TestInterceptor2>();
		}
	}

	public class InterceptorReferencesWithPositionInSingleCallTestCase1 : InterceptorsTestCaseBase
	{
		protected override IRegistration RegisterInterceptors<S>(ComponentRegistration<S> registration)
		{
			return
				registration.Interceptors(InterceptorReference.ForType(typeof(TestInterceptor1)),
										  InterceptorReference.ForType(typeof(TestInterceptor2))).First;
		}

		protected override IEnumerable<InterceptorReference> GetExpectedInterceptorsInCorrectOrder()
		{
			yield return InterceptorReference.ForType<TestInterceptor2>();
			yield return InterceptorReference.ForType<TestInterceptor1>();
		}
	}

	public class InterceptorReferencesWithPositionInSingleCallTestCase2 : InterceptorsTestCaseBase
	{
		protected override IRegistration RegisterInterceptors<S>(ComponentRegistration<S> registration)
		{
			return
				registration.Interceptors(InterceptorReference.ForType(typeof(TestInterceptor1)),
										  InterceptorReference.ForType(typeof(TestInterceptor2))).Last;
		}

		protected override IEnumerable<InterceptorReference> GetExpectedInterceptorsInCorrectOrder()
		{
			yield return InterceptorReference.ForType<TestInterceptor1>();
			yield return InterceptorReference.ForType<TestInterceptor2>();
		}
	}

	public class InterceptorReferencesWithPositionInSingleCallTestCase3 : InterceptorsTestCaseBase
	{
		protected override IRegistration RegisterInterceptors<S>(ComponentRegistration<S> registration)
		{
			return
				registration.Interceptors(InterceptorReference.ForType(typeof(TestInterceptor1)),
										  InterceptorReference.ForType(typeof(TestInterceptor2))).AtIndex(0);
		}

		protected override IEnumerable<InterceptorReference> GetExpectedInterceptorsInCorrectOrder()
		{
			yield return InterceptorReference.ForType<TestInterceptor2>();
			yield return InterceptorReference.ForType<TestInterceptor1>();
		}
	}
}
