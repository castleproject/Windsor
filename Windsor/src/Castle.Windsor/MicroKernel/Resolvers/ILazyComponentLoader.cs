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

namespace Castle.MicroKernel.Resolvers
{
	using System;
	using System.Collections;

	using Castle.MicroKernel.Registration;

	/// <summary>
	///   Provides lazy registration capabilities to the container.
	/// </summary>
	/// <remarks>
	///   When a component is requested from a container and it was not registered, 
	///   container loads up all registered implementers of this interface and asks 
	///   them in turn whether they can provide that component, until it finds one that will.
	/// </remarks>
	public interface ILazyComponentLoader
	{
		/// <summary>
		///   Used by container to allow the loader to register component for given <paramref name = "name" /> and <paramref
		///    name = "service" /> to the container at the time when it is requested
		/// </summary>
		/// <param name = "name">Name of the requested component or null</param>
		/// <param name = "service">Type of requested service or null</param>
		/// <param name = "arguments">User supplied arguments or null</param>
		/// <returns>Registration that registers component for given key and/or service or null.</returns>
		/// <remarks>
		///   While either key or service can be null reference it is guaranteed that at least one of them will not be null.
		///   When implementer opts in to provide the requested component (by returning not-null registration) it is required
		///   to register component for requested key/service combination (when one of the elements is null, it should be ignored as well).
		///   When implementer does not want to register the requested component it must return null.
		/// </remarks>
		IRegistration Load(string name, Type service, IDictionary arguments);
	}
}