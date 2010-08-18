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

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.Collections.Specialized;
	using System.Reflection;
	using Castle.Facilities.TypedFactory;
	using Castle.MicroKernel;

	public class WcfClientFactorySelector : ITypedFactoryComponentSelector
	{
		public TypedFactoryComponent SelectComponent(MethodInfo method, Type type, object[] arguments)
		{
			return new ClientComponent(method.ReturnType, arguments);
		}

		#region Nested Class: ClientComponent

		class ClientComponent : TypedFactoryComponent
		{
			private readonly object[] arguments;

			public ClientComponent(Type componentType, object[] arguments)
				: base(null, componentType, null)
            {
				this.arguments = arguments;
            }

			public override object Resolve(IKernel kernel)
			{
				string key = null;
				var argument = arguments[0];

				if (arguments.Length == 2)
				{
					key = (string)argument;
					argument = arguments[1];
				}
				else if (argument is string)
				{
					return kernel.Resolve((string)argument, ComponentType);
				}

				if (argument is Uri)
				{
					argument = WcfEndpoint.At((Uri)argument);
				}

				var args = new HybridDictionary();
				args.Add(Guid.NewGuid().ToString(), argument);

				if (key == null)
				{
					return kernel.Resolve(ComponentType, args);
				}

				return kernel.Resolve(key, ComponentType, args);
			}
		}

		#endregion
	}
}
