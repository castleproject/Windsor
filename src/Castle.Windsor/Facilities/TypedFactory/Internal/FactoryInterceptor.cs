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
	using Castle.Core.Interceptor;
	using Castle.DynamicProxy;
	using Castle.MicroKernel;

	/// <summary>
	///   Legacy interceptor for old impl. of the facility.
	/// </summary>
	[Transient]
	public class FactoryInterceptor : IInterceptor, IOnBehalfAware
	{
		private readonly IKernel kernel;
		private FactoryEntry entry;

		public FactoryInterceptor(IKernel kernel)
		{
			this.kernel = kernel;
		}

		public void Intercept(IInvocation invocation)
		{
			var name = invocation.Method.Name;
			var args = invocation.Arguments;
			if (name.Equals(entry.CreationMethod))
			{
				if (args.Length == 0 || args[0] == null)
				{
					invocation.ReturnValue = kernel.Resolve(invocation.Method.ReturnType);
					return;
				}
				var key = (String)args[0];
				invocation.ReturnValue = kernel.Resolve<object>(key);
				return;
			}

			if (name.Equals(entry.DestructionMethod))
			{
				if (args.Length == 1)
				{
					kernel.ReleaseComponent(args[0]);
					invocation.ReturnValue = null;
					return;
				}
			}

			invocation.Proceed();
		}

		public void SetInterceptedComponentModel(ComponentModel target)
		{
			entry = (FactoryEntry)target.ExtendedProperties["typed.fac.entry"];
		}
	}
}