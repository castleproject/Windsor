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

namespace Castle.MicroKernel
{
	using System;

	public partial interface IKernel : IKernelEvents, IDisposable
	{
		/// <summary>
		///   Returns the component instance by the service type
		/// </summary>
		object Resolve(Type service);

		/// <summary>
		///   Returns the component instance by the service type
		///   using dynamic arguments
		/// </summary>
		/// <param name = "service"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		object Resolve(Type service, Arguments arguments);

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "service"></param>
		/// <returns></returns>
		object Resolve(String key, Type service);

		/// <summary>
		///   Returns the component instance by the service type
		///   using dynamic arguments
		/// </summary>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		T Resolve<T>(Arguments arguments);

		/// <summary>
		///   Returns the component instance by the component key
		/// </summary>
		/// <returns></returns>
		T Resolve<T>();

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key">Component's key</param>
		/// <typeparam name = "T">Service type</typeparam>
		/// <returns>The Component instance</returns>
		T Resolve<T>(String key);

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <typeparam name = "T">Service type</typeparam>
		/// <param name = "key">Component's key</param>
		/// <param name = "arguments"></param>
		/// <returns>The Component instance</returns>
		T Resolve<T>(string key, Arguments arguments);

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "service"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		object Resolve(string key, Type service, Arguments arguments);

		/// <summary>
		///   Returns all the valid component instances by
		///   the service type
		/// </summary>
		/// <param name = "service">The service type</param>
		Array ResolveAll(Type service);

		/// <summary>
		///   Returns all the valid component instances by
		///   the service type
		/// </summary>
		/// <param name = "service">The service type</param>
		/// <param name = "arguments">Arguments to resolve the services</param>
		Array ResolveAll(Type service, Arguments arguments);

		/// <summary>
		///   Returns component instances that implement TService
		/// </summary>
		/// <typeparam name = "TService"></typeparam>
		/// <returns></returns>
		TService[] ResolveAll<TService>();

		/// <summary>
		///   Returns component instances that implement TService
		/// </summary>
		/// <typeparam name = "TService"></typeparam>
		/// <returns></returns>
		TService[] ResolveAll<TService>(Arguments arguments);
	}
}