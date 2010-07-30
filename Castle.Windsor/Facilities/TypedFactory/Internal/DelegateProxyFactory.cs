namespace Castle.Facilities.TypedFactory.Internal
{
	using System;

	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.DynamicProxy.Generators;
	using Castle.MicroKernel;

	public class DelegateProxyFactory : IProxyFactoryExtension
	{
		private readonly Type targetDelegateType;

		public DelegateProxyFactory(Type targetDelegateType)
		{
			this.targetDelegateType = targetDelegateType;
		}

		public object Generate(IProxyBuilder builder, ProxyGenerationOptions options, IInterceptor[] interceptors)
		{
			var type = GetProxyType(builder);
			var instance = GetProxyInstance(type,interceptors);
			var method = GetInvokeDelegate(instance);
			return method;
		}

		private object GetInvokeDelegate(object instance)
		{
			return Delegate.CreateDelegate(targetDelegateType, instance, "Invoke");
		}

		private object GetProxyInstance(Type type, IInterceptor[] interceptors)
		{
			return type.CreateInstance<object>(null, interceptors);
		}

		private Type GetProxyType(IProxyBuilder builder)
		{
			var scope = builder.ModuleScope;
			var logger = builder.Logger;
			var generator = new DelegateProxyGenerator(scope, targetDelegateType)
			{
				Logger = logger
			};
			return generator.GetProxyType(builder);
		}
	}
}