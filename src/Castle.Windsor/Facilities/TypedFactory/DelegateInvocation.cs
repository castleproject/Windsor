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
	using System.Reflection;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;

	public class DelegateInvocation
	{
		internal static readonly MethodInfo InvokeToken = typeof(DelegateInvocation).GetMethod("Invoke");

		private readonly IKernel kernel;
		private readonly ITypedFactoryComponentSelector selector;
		private readonly MethodInfo invoke;
		private readonly Type serviceType;

		public DelegateInvocation(IKernel kernel, ITypedFactoryComponentSelector selector, MethodInfo invoke,Type serviceType)
		{
			this.kernel = kernel;
			this.selector = selector;
			this.invoke = invoke;
			this.serviceType = serviceType;
		}

		public object Invoke(object[] arguments)
		{
			var component = selector.SelectComponent(invoke, serviceType, arguments);
			if (component == null)
			{
				throw new FacilityException(
					string.Format(
						"Selector {0} didn't select any component for delegate {1}. This usually signifies a bug in the selector.",
						selector,
						invoke.DeclaringType));
			}
			return component.Resolve(kernel);
		}
	}
}