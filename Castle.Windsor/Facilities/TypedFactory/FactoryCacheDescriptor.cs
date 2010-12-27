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
	using Castle.Facilities.TypedFactory.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	public class FactoryCacheDescriptor<TFactory> : ComponentDescriptor<TFactory>
	{
		protected internal override void ApplyToModel(IKernel kernel, ComponentModel model)
		{
			var map = new Dictionary<MethodInfo, FactoryMethod>();
			foreach (var service in model.AllServices)
			{
				BuildHandlersMap(service, map);
			}

			model.ExtendedProperties[TypedFactoryFacility.FactoryMapCacheKey] = map;
		}

		private void BuildHandlersMap(Type service, Dictionary<MethodInfo, FactoryMethod> map)
		{
			if (service == null)
			{
				return;
			}

			if (service.Equals(typeof(IDisposable)))
			{
				var method = service.GetMethods().Single();
				map[method] = FactoryMethod.Dispose;
				return;
			}

			var methods = service.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			foreach (var method in methods)
			{
				if (IsReleaseMethod(method))
				{
					map[method] = FactoryMethod.Release;
					continue;
				}
				map[method] = FactoryMethod.Resolve;
			}

			foreach (var @interface in service.GetInterfaces())
			{
				BuildHandlersMap(@interface, map);
			}
		}

		private bool IsReleaseMethod(MethodInfo methodInfo)
		{
			return methodInfo.ReturnType == typeof(void);
		}
	}
}