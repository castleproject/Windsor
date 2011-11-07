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

	using Castle.Core.Internal;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;

	/// <summary>
	///   The <c>IKernel</c> interface exposes all the functionality
	///   the MicroKernel implements.
	/// </summary>
	/// <remarks>
	///   It allows you to register components and
	///   request them by their name or the services they expose.
	///   It also allow you to register facilities and subsystem, thus 
	///   augmenting the functionality exposed by the kernel alone to fit 
	///   your needs.
	///   <seealso cref = "IFacility" />
	///   <seealso cref = "ISubSystem" />
	/// </remarks>
	public partial interface IKernel : IKernelEvents, IDisposable
	{
		/// <summary>
		///   Returns the implementation of <see cref = "IComponentModelBuilder" />
		/// </summary>
		IComponentModelBuilder ComponentModelBuilder { get; }

		/// <summary>
		///   Gets or sets the implementation of <see cref = "IConfigurationStore" />
		/// </summary>
		IConfigurationStore ConfigurationStore { get; set; }

		/// <summary>
		///   Graph of components and interactions.
		/// </summary>
		GraphNode[] GraphNodes { get; }

		/// <summary>
		///   Returns the implementation of <see cref = "IHandlerFactory" />
		/// </summary>
		IHandlerFactory HandlerFactory { get; }

		/// <summary>
		///   Returns the parent kernel
		/// </summary>
		IKernel Parent { get; set; }

		/// <summary>
		///   Gets or sets the implementation of <see cref = "IProxyFactory" />
		///   allowing different strategies for proxy creation.
		/// </summary>
		IProxyFactory ProxyFactory { get; set; }

		/// <summary>
		///   Gets or sets the implementation for <see cref = "IReleasePolicy" />
		/// </summary>
		IReleasePolicy ReleasePolicy { get; set; }

		/// <summary>
		///   Returns the implementation for <see cref = "IDependencyResolver" />
		/// </summary>
		IDependencyResolver Resolver { get; }

		/// <summary>
		///   Support for kernel hierarchy
		/// </summary>
		/// <param name = "kernel"></param>
		void AddChildKernel(IKernel kernel);

		/// <summary>
		///   Adds a <see cref = "IFacility" /> to the kernel.
		/// </summary>
		/// <param name = "facility"></param>
		/// <returns></returns>
		IKernel AddFacility(IFacility facility);

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the kernel.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <returns></returns>
		IKernel AddFacility<T>() where T : IFacility, new();

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the kernel.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "onCreate">The callback for creation.</param>
		/// <returns></returns>
		IKernel AddFacility<T>(Action<T> onCreate)
			where T : IFacility, new();

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
		///   Adds (or replaces) an <see cref = "ISubSystem" />
		/// </summary>
		/// <param name = "name"></param>
		/// <param name = "subsystem"></param>
		void AddSubSystem(String name, ISubSystem subsystem);

		/// <summary>
		///   Return handlers for components that 
		///   implements the specified service. 
		///   The check is made using IsAssignableFrom
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		IHandler[] GetAssignableHandlers(Type service);

		/// <summary>
		///   Returns the facilities registered on the kernel.
		/// </summary>
		/// <returns></returns>
		IFacility[] GetFacilities();

		/// <summary>
		///   Returns the <see cref = "IHandler" />
		///   for the specified component name.
		/// </summary>
		/// <param name = "name"></param>
		/// <returns></returns>
		IHandler GetHandler(String name);

		/// <summary>
		///   Returns the <see cref = "IHandler" />
		///   for the specified service.
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		IHandler GetHandler(Type service);

		/// <summary>
		///   Return handlers for components that 
		///   implements the specified service.
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		IHandler[] GetHandlers(Type service);

		/// <summary>
		///   Returns an implementation of <see cref = "ISubSystem" />
		///   for the specified name. 
		///   <seealso cref = "SubSystemConstants" />
		/// </summary>
		/// <param name = "name"></param>
		/// <returns></returns>
		ISubSystem GetSubSystem(String name);

		/// <summary>
		///   Returns <c>true</c> if a component with given <paramref name = "name" /> was registered, otherwise <c>false</c>.
		/// </summary>
		/// <param name = "name"></param>
		/// <returns></returns>
		bool HasComponent(String name);

		/// <summary>
		///   Returns true if the specified service was registered
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		bool HasComponent(Type service);

		/// <summary>
		///   Registers the components with the <see cref = "IKernel" />. The instances of <see cref = "IRegistration" /> are produced by fluent registration API.
		///   Most common entry points are <see cref = "Component.For{TService}" /> method to register a single type or (recommended in most cases) 
		///   <see cref = "AllTypes.FromThisAssembly" />.
		///   Let the Intellisense drive you through the fluent API past those entry points. For details see the documentation at http://j.mp/WindsorApi
		/// </summary>
		/// <example>
		///   <code>
		///     kernel.Register(Component.For&lt;IService&gt;().ImplementedBy&lt;DefaultService&gt;().LifestyleTransient());
		///   </code>
		/// </example>
		/// <example>
		///   <code>
		///     kernel.Register(Classes.FromThisAssembly().BasedOn&lt;IService&gt;().WithServiceDefaultInterfaces().Configure(c => c.LifestyleTransient()));
		///   </code>
		/// </example>
		/// <param name = "registrations">The component registrations created by <see cref = "Component.For{TService}" />, <see
		///    cref = "AllTypes.FromThisAssembly" /> or different entry method to the fluent API.</param>
		/// <returns>The kernel.</returns>
		IKernel Register(params IRegistration[] registrations);

		/// <summary>
		///   Releases a component instance. This allows
		///   the kernel to execute the proper decommission 
		///   lifecycles on the component instance.
		/// </summary>
		/// <param name = "instance"></param>
		void ReleaseComponent(object instance);

		/// <summary>
		///   Remove child kernel
		/// </summary>
		/// <param name = "kernel"></param>
		void RemoveChildKernel(IKernel kernel);
	}
}