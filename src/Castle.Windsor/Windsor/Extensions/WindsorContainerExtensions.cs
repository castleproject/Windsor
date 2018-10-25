// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor
{
	using System;
	using System.Collections.Generic;

	using Castle.MicroKernel;

	public static class WindsorContainerExtensions
	{
		/// <summary>
		/// Returns a component instance by the service.
		/// </summary>
		public static object Resolve(this IWindsorContainer container, Type service, IEnumerable<KeyValuePair<string, object>> arguments)
		{
			return container.Kernel.Resolve(service, new Arguments().AddNamed(arguments));
		}

		/// <summary>
		/// Returns a component instance by the key.
		/// </summary>
		public static object Resolve(this IWindsorContainer container, string key, Type service, IEnumerable<KeyValuePair<string, object>> arguments)
		{
			return container.Kernel.Resolve(key, service, new Arguments().AddNamed(arguments));
		}

		/// <summary>
		/// Returns a component instance by the service.
		/// </summary>
		public static T Resolve<T>(this IWindsorContainer container, IEnumerable<KeyValuePair<string, object>> arguments)
		{
			return (T)container.Kernel.Resolve(typeof(T), new Arguments().AddNamed(arguments));
		}

		/// <summary>
		/// Returns a component instance by the key.
		/// </summary>
		public static T Resolve<T>(this IWindsorContainer container, string key, IEnumerable<KeyValuePair<string, object>> arguments)
		{
			return container.Kernel.Resolve<T>(key, new Arguments().AddNamed(arguments));
		}

		/// <summary>
		/// Resolve all valid components that match this type by passing dependencies as arguments.
		/// </summary>
		public static Array ResolveAll(this IWindsorContainer container, Type service, IEnumerable<KeyValuePair<string, object>> arguments)
		{
			return container.Kernel.ResolveAll(service, new Arguments().AddNamed(arguments));
		}

		/// <summary>
		/// Resolve all valid components that match this type.
		/// </summary>
		public static T[] ResolveAll<T>(this IWindsorContainer container, IEnumerable<KeyValuePair<string, object>> arguments)
		{
			return (T[])container.ResolveAll(typeof(T), new Arguments().AddNamed(arguments));
		}
	}
}