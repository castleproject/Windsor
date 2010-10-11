// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Interceptor;
	using Castle.DynamicProxy;
	using Castle.Facilities.TypedFactory.Internal;
	using Castle.MicroKernel;

	[Transient]
	public class TypedFactoryInterceptor : IInterceptor, IOnBehalfAware, IDisposable
	{
		private readonly IKernel kernel;

		private readonly IDictionary<MethodInfo, ITypedFactoryMethod> methods =
			new Dictionary<MethodInfo, ITypedFactoryMethod>();

		private readonly List<object> trackedComponents = new List<object>();

		private bool disposed;

		private ComponentModel target;

		public TypedFactoryInterceptor(IKernel kernel, ITypedFactoryComponentSelector componentSelector)
		{
			ComponentSelector = componentSelector;
			this.kernel = kernel;
		}

		public ITypedFactoryComponentSelector ComponentSelector { get; private set; }

		public void Dispose()
		{
			disposed = true;
			var components = trackedComponents.ToArray();
			trackedComponents.Clear();
			foreach (var component in components)
			{
				kernel.ReleaseComponent(component);
			}
		}

		public void Intercept(IInvocation invocation)
		{
			if (disposed)
			{
				throw new ObjectDisposedException("this", "The factory was disposed and can no longer be used.");
			}

			ITypedFactoryMethod method;
			if (TryGetMethod(invocation, out method) == false)
			{
				throw new Exception(
					string.Format("Can't find information about factory method {0}. This is most likely a bug. Please report it.",
					              invocation.Method));
			}
			method.Invoke(invocation);
		}

		public void SetInterceptedComponentModel(ComponentModel target)
		{
			this.target = target;
			BuildHandlersMap(this.target.Service);
		}

		protected virtual void BuildHandlersMap(Type service)
		{
			if (service.Equals(typeof(IDisposable)))
			{
				var method = service.GetMethods().Single();
				methods.Add(method, new Dispose(Dispose));
				return;
			}

			foreach (var method in service.GetMethods())
			{
				if (IsReleaseMethod(method))
				{
					methods.Add(method, new Release(kernel));
					continue;
				}
				methods.Add(method, new Resolve(kernel, ComponentSelector, trackedComponents.Add));
			}

			foreach (var @interface in service.GetInterfaces())
			{
				BuildHandlersMap(@interface);
			}
		}

		private bool IsReleaseMethod(MethodInfo methodInfo)
		{
			return methodInfo.ReturnType == typeof(void);
		}

		private bool TryGetMethod(IInvocation invocation, out ITypedFactoryMethod method)
		{
			if (methods.TryGetValue(invocation.Method, out method))
			{
				return true;
			}
			if (invocation.Method.IsGenericMethod == false)
			{
				return false;
			}
			return methods.TryGetValue(invocation.Method.GetGenericMethodDefinition(), out method);
		}
	}
}