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

namespace Castle.MicroKernel.SubSystems.Naming
{
	using System;

	using Castle.Core;

	/// <summary>
	///   Contract for SubSystem that wishes to keep and coordinate
	///   component registration.
	/// </summary>
	public interface INamingSubSystem : ISubSystem
	{
		/// <summary>
		///   Returns the number of components registered.
		/// </summary>
		int ComponentCount { get; }

		/// <summary>
		///   Register a new component resolver that can take part in the decision
		///   making about which handler to resolve
		/// </summary>
		void AddHandlerSelector(IHandlerSelector selector);

		/// <summary>
		///   Register a new component resolver that can take part in the decision
		///   making about which handler(s) to resolve and in which order
		/// </summary>
		void AddHandlersFilter(IHandlersFilter filter);

		/// <summary>
		///   Returns true if there is a component registered 
		///   for the specified key
		/// </summary>
		/// <param name = "key"></param>
		/// <returns></returns>
		bool Contains(String key);

		/// <summary>
		///   Returns true if there is a component registered 
		///   for the specified service
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		bool Contains(Type service);

		/// <summary>
		///   Returns all <see cref = "IHandler" /> registered.
		/// </summary>
		/// <returns></returns>
		IHandler[] GetAllHandlers();

		/// <summary>
		///   Return <see cref = "IHandler" />s where components are compatible
		///   with the specified service.
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		IHandler[] GetAssignableHandlers(Type service);

		/// <summary>
		///   Returns the <see cref = "IHandler" /> associated with
		///   the specified key.
		/// </summary>
		/// <param name = "key"></param>
		/// <returns></returns>
		IHandler GetHandler(String key);

		/// <summary>
		///   Returns the <see cref = "IHandler" /> associated with
		///   the specified service.
		/// </summary>
		IHandler GetHandler(Type service);

		/// <summary>
		///   Returns an array of <see cref = "IHandler" /> associated with
		///   the specified service.
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		IHandler[] GetHandlers(Type service);

		/// <summary>
		///   Implementors should register the <see cref = "IHandler" /> with all <see cref = "ComponentModel.Services" /> its <see cref="IHandler.ComponentModel"/> exposes.
		///   The handler should also be accessible via unique <see cref = "ComponentModel.Name" /> of its <see
		///    cref = "IHandler.ComponentModel" />.
		/// </summary>
		/// <param name = "handler"></param>
		/// <exception cref = "ComponentRegistrationException">Thrown if the <see cref = "ComponentModel.Name" /> of <paramref
		///    name = "handler" />'s <see cref = "IHandler.ComponentModel" /> is not unique and a handler with the same name has already been registered.</exception>
		void Register(IHandler handler);
	}
}