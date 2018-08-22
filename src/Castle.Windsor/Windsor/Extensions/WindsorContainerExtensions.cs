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

namespace Castle.Windsor.Extensions
{
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel;

	public static class WindsorContainerExtensions
	{
		/// <summary>
		/// Returns a component instance by the service
		/// </summary>
		/// <typeparam name = "T"></typeparam>
		/// <param name = "container"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public static T Resolve<T>(this IWindsorContainer container, IReadOnlyDictionary<string, object> arguments)
		{
			return (T)container.Kernel.Resolve(typeof(T), new Arguments().InsertNamed(arguments));
		}

		/// <summary>
		/// Returns a component instance by the key
		/// </summary>
		/// <param name = "container"></param>
		/// <param name = "key"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public static T Resolve<T>(this IWindsorContainer container, string key, IReadOnlyDictionary<string, object> arguments)
		{
			return container.Kernel.Resolve<T>(key, new Arguments().InsertNamed(arguments));
		}

		/// <summary>
		/// Returns a component instance by the key
		/// </summary>
		/// <param name = "container"></param>
		/// <param name = "key"></param>
		/// <param name = "service"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public static object Resolve(this IWindsorContainer container, string key, Type service, IReadOnlyDictionary<string, object> arguments)
		{
			return container.Kernel.Resolve(key, service, new Arguments().InsertNamed(arguments));
		}

		/// <summary>
		/// Returns a component instance by the service
		/// </summary>
		/// <param name = "container"></param>
		/// <param name = "service"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public static object Resolve(this IWindsorContainer container, Type service, IReadOnlyDictionary<string, object> arguments)
		{
			return container.Kernel.Resolve(service, new Arguments().InsertNamed(arguments));
		}

		/// <summary>
		/// Resolve all valid components that match this type by passing dependencies as arguments.
		/// </summary>
		/// <param name = "container"></param>
		/// <param name="service"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public static Array ResolveAll(this IWindsorContainer container, Type service, IReadOnlyDictionary<string, object> arguments)
		{
			return container.Kernel.ResolveAll(service, new Arguments().InsertNamed(arguments));
		}

		/// <summary>
		/// Resolve all valid components that match this type.
		/// <typeparam name = "T">The service type</typeparam>
		/// <param name="container"></param>
		/// <param name = "arguments">Arguments to resolve the service</param>
		/// </summary>
		public static T[] ResolveAll<T>(this IWindsorContainer container, IReadOnlyDictionary<string, object> arguments)
		{
			return (T[])container.ResolveAll(typeof(T), new Arguments().InsertNamed(arguments));
		}
	}
}