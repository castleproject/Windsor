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

namespace Castle.Facilities.WcfIntegration
{
	using System;
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
				if (arguments.Length == 1)
				{
					var argument = arguments[0];

					if (argument is string)
					{
						return kernel.Resolve((string)argument, ComponentType);
					}
					else if (argument is IWcfClientModel || argument is IWcfEndpoint)
					{
						return kernel.Resolve(ComponentType, new { argument });
					}
					else if (argument is Uri)
					{
						var endpoint = WcfEndpoint.At((Uri)argument);
						return kernel.Resolve(ComponentType, new { endpoint });
					}
				}
				return null;
			}
		}

		#endregion
	}
}
