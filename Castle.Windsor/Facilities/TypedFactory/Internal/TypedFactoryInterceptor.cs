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

namespace Castle.Facilities.TypedFactory.Internal
{
	using System;
#if SILVERLIGHT
	using System.Linq;
#endif
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Interceptor;
	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;

	[Transient]
	public class TypedFactoryInterceptor : IInterceptor, IOnBehalfAware, IDisposable
	{
		private const int SweepFrequency = 100;

		private readonly IKernel kernel;

		private readonly List<WeakReference> resolvedTrackedComponents = new List<WeakReference>();

		private bool disposed;
		private IDictionary<MethodInfo, FactoryMethod> methods;
		private int resolveCount;

		public TypedFactoryInterceptor(IKernel kernel, ITypedFactoryComponentSelector componentSelector)
		{
			ComponentSelector = componentSelector;
			this.kernel = kernel;
		}

		public ITypedFactoryComponentSelector ComponentSelector { get; private set; }

		public void Dispose()
		{
			disposed = true;
			var components = resolvedTrackedComponents.ToArray();
			resolvedTrackedComponents.Clear();
			foreach (var component in components)
			{
				var instance = component.Target;
				if (instance == null)
				{
					continue;
				}
				kernel.ReleaseComponent(instance);
			}
		}

		public void Intercept(IInvocation invocation)
		{
			if (disposed)
			{
				throw new ObjectDisposedException("this", "The factory was disposed and can no longer be used.");
			}

			FactoryMethod method;
			if (TryGetMethod(invocation, out method) == false)
			{
				throw new Exception(
					string.Format("Can't find information about factory method {0}. This is most likely a bug. Please report it.",
					              invocation.Method));
			}
			switch (method)
			{
				case FactoryMethod.Resolve:
					Resolve(invocation);
					break;
				case FactoryMethod.Release:
					Release(invocation);
					break;
				case FactoryMethod.Dispose:
					Dispose();
					break;
			}
		}

		public void SetInterceptedComponentModel(ComponentModel target)
		{
			methods = (IDictionary<MethodInfo, FactoryMethod>)target.ExtendedProperties[TypedFactoryFacility.FactoryMapCacheKey];
			if (methods == null)
			{
				throw new ArgumentException(string.Format("Component {0} is not a typed factory. {1} only works with typed factories.", target.Name, GetType()));
			}
		}

		private void CollectDeadReferences()
		{
			resolveCount++;
			if (resolveCount != SweepFrequency)
			{
				return;
			}

			resolveCount = 0;
			// we go ahead and remove dead references from the list
#if !SILVERLIGHT
			resolvedTrackedComponents.RemoveAll(c => c.IsAlive == false);
#else
			resolvedTrackedComponents.Where(c => c.IsAlive == false).ToList().ForEach(x => resolvedTrackedComponents.Remove(x));
#endif
		}

		private void Release(IInvocation invocation)
		{
			foreach (var argument in invocation.Arguments)
			{
				if (argument == null)
				{
					continue;
				}

				kernel.ReleaseComponent(argument);
			}
		}

		private void Resolve(IInvocation invocation)
		{
			var component = ComponentSelector.SelectComponent(invocation.Method, invocation.TargetType, invocation.Arguments);
			if (component == null)
			{
				throw new FacilityException(
					string.Format(
						"Selector {0} didn't select any component for method {1}. This usually signifies a bug in the selector.",
						ComponentSelector,
						invocation.Method));
			}
			var instance = component.Resolve(kernel);
			if (kernel.ReleasePolicy.HasTrack(instance))
			{
				CollectDeadReferences();
				resolvedTrackedComponents.Add(new WeakReference(instance));
			}
			invocation.ReturnValue = instance;
		}

		private bool TryGetMethod(IInvocation invocation, out FactoryMethod method)
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