// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Facilities.TypedFactory
{
	using System.Collections.Generic;

	using Castle.Core.Interceptor;

	/// <summary>
	/// resolves componet selected by given <see cref="ITypedFactoryComponentSelector"/> from the container
	/// </summary>
	public class Resolve : ITypedFactoryMethod
	{
		private readonly IKernel kernel;
		private readonly ITypedFactoryComponentSelector selector;
		public Resolve(IKernel kernel, ITypedFactoryComponentSelector selector)
		{
			this.kernel = kernel;
			this.selector = selector;
		}

		public void Invoke(IInvocation invocation)
		{
			var component = selector.SelectComponent(invocation.Method, invocation.TargetType);
			var arguments = GetArguments(invocation);
			if (component.First == null)
			{
				invocation.ReturnValue = kernel.Resolve(component.Second, arguments);
				return;
			}

			if (component.Second == null)
			{
				invocation.ReturnValue = kernel.Resolve(component.First, arguments);
				return;
			}

			invocation.ReturnValue = kernel.Resolve(component.First, component.Second, arguments);
		}

		private Dictionary<string, object> GetArguments(IInvocation invocation)
		{
			var arguments = new Dictionary<string, object>();
			var parameters = invocation.Method.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				arguments.Add(parameters[i].Name, invocation.GetArgumentValue(i));
			}
			return arguments;
		}
	}
}