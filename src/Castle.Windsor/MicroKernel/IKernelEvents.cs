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

	using Castle.Core;
	using Castle.Windsor;

	/// <summary>
	///   Represents a delegate which holds basic information about a component.
	/// </summary>
	/// <param name = "key">Key which identifies the component</param>
	/// <param name = "handler">handler that holds this component and is capable of 
	///   creating an instance of it.
	/// </param>
	public delegate void ComponentDataDelegate(String key, IHandler handler);

	/// <summary>
	///   Represents a delegate which holds basic information about a component
	///   and its instance.
	/// </summary>
	/// <param name = "model">Component meta information</param>
	/// <param name = "instance">Component instance</param>
	public delegate void ComponentInstanceDelegate(ComponentModel model, object instance);

	/// <summary>
	///   Represents a delegate which holds the information about the 
	///   component
	/// </summary>
	public delegate void ComponentModelDelegate(ComponentModel model);

	/// <summary>
	///   Represents a delegate which holds the information about a service.
	/// </summary>
	public delegate void ServiceDelegate(Type service);

	/// <summary>
	///   Represents a delegate which holds a handler
	/// </summary>
	/// <param name = "handler">handler that holds a component and is capable of 
	///   creating an instance of it.
	/// </param>
	/// <param name = "stateChanged"></param>
	public delegate void HandlerDelegate(IHandler handler, ref bool stateChanged);

	public delegate void HandlersChangedDelegate(ref bool stateChanged);

	/// <summary>
	///   Represents a delegate which holds dependency
	///   resolving information.
	/// </summary>
	public delegate void DependencyDelegate(ComponentModel client, DependencyModel model, Object dependency);

	/// <summary>
	///   Summary description for IKernelEvents.
	/// </summary>
	public interface IKernelEvents
	{
		/// <summary>
		///   Event fired when a new component is registered 
		///   on the kernel.
		/// </summary>
		event ComponentDataDelegate ComponentRegistered;

		/// <summary>
		///   Event fired after the ComponentModel is created.
		///   Allows customizations that may affect the handler.
		/// </summary>
		event ComponentModelDelegate ComponentModelCreated;

		/// <summary>
		///   Event fired when the kernel was added as child of
		///   another kernel.
		/// </summary>
		event EventHandler AddedAsChildKernel;

		/// <summary>
		///   Event fired when the kernel was removed from being a child
		///   of another kernel.
		/// </summary>
		event EventHandler RemovedAsChildKernel;

		/// <summary>
		///   Event fired before the component is created.
		/// </summary>
		event ComponentInstanceDelegate ComponentCreated;

		/// <summary>
		///   Event fired when a component instance destroyed.
		/// </summary>
		event ComponentInstanceDelegate ComponentDestroyed;

		/// <summary>
		///   Event fired when a new handler is registered 
		///   (it might be in a valid or waiting dependency state)
		/// </summary>
		event HandlerDelegate HandlerRegistered;

		/// <summary>
		///   Event fired when a new handler is registered 
		///   (it might be in a valid or waiting dependency state)
		/// </summary>
		event HandlersChangedDelegate HandlersChanged;

		/// <summary>
		///   Event fired when a dependency is being resolved,
		///   it allows the dependency to be changed,
		///   but the client ComponentModel must not be changed.
		/// </summary>
		event DependencyDelegate DependencyResolving;

		/// <summary>
		///   Event fired when registration / installation process is completed.
		///   That is when container is about to exit<see cref = "IKernel.Register" /> method. This event is raised once regardless of how many components were registered.
		///   If the <see cref = "IKernel.Register" /> is called by <see cref = "IWindsorContainer.Install" /> the event is raised when that method exits.
		/// </summary>
		event EventHandler RegistrationCompleted;

		/// <summary>
		///   Event fired when a collection is being resolved (via <see cref = "IKernel.ResolveAll(System.Type)" /> or another overload) and the collection is empty.
		///   Implementors would usually log that fact or potentially throw an exception (especially in development).
		/// </summary>
		event ServiceDelegate EmptyCollectionResolving;
	}
}