namespace Castle.Windsor.Tests.Proxy
{
	using System;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Interceptor;
	using Castle.DynamicProxy;

	using NUnit.Framework;

	public class OnBehalfAwareInterceptorSelector : IInterceptorSelector, IOnBehalfAware
	{
		public static ComponentModel target;

		public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
		{
			Assert.IsNotNull(target);
			return interceptors;
		}

		public void SetInterceptedComponentModel(ComponentModel target)
		{
			OnBehalfAwareInterceptorSelector.target = target;
		}
	}
}