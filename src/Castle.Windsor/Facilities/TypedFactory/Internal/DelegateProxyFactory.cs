// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Facilities.TypedFactory.Internal
{
	using System;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.DynamicProxy.Generators;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;

	public class DelegateProxyFactory : IProxyFactoryExtension
	{
		public object Generate(IProxyBuilder builder, ProxyGenerationOptions options, IInterceptor[] interceptors, ComponentModel model,
		                       CreationContext context)
		{
			var targetDelegateType = context.RequestedType;
			var type = GetProxyType(builder, targetDelegateType);
			var instance = GetProxyInstance(type, interceptors);
			var method = GetInvokeDelegate(instance, targetDelegateType);
			return method;
		}

		private object GetInvokeDelegate(object instance, Type targetDelegateType)
		{
			return Delegate.CreateDelegate(targetDelegateType, instance, "Invoke");
		}

		private object GetProxyInstance(Type type, IInterceptor[] interceptors)
		{
			return type.CreateInstance<object>(null, interceptors);
		}

		private Type GetProxyType(IProxyBuilder builder, Type targetDelegateType)
		{
			var scope = builder.ModuleScope;
			var logger = builder.Logger;
			var generator = new DelegateProxyGenerator(scope, targetDelegateType)
			{
				Logger = logger
			};
			return generator.GetProxyType();
		}
	}
}