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

namespace Castle.Windsor
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Installer;

	/// <summary>
	///   The <c>IWindsorContainer</c> interface exposes all the 
	///   functionality the Windsor implements.
	/// </summary>
	public interface IWindsorContainer : IDisposable
	{
		/// <summary>
		///   Returns the inner instance of the MicroKernel
		/// </summary>
		IKernel Kernel { get; }

		/// <summary>
		///   Gets the container's name
		/// </summary>
		/// <remarks>
		///   Only useful when child containers are being used
		/// </remarks>
		/// <value>The container's name.</value>
		string Name { get; }

		/// <summary>
		///   Gets or sets the parent container if this instance
		///   is a sub container.
		/// </summary>
		IWindsorContainer Parent { get; set; }

		/// <summary>
		///   Registers a subcontainer. The components exposed
		///   by this container will be accessible from subcontainers.
		/// </summary>
		/// <param name = "childContainer"></param>
		void AddChildContainer(IWindsorContainer childContainer);

		/// <summary>
		///   Registers a facility within the container.
		/// </summary>
		/// <param name = "facility">The <see cref = "IFacility" /> to add to the container.</param>
		IWindsorContainer AddFacility(IFacility facility);

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the container.
		/// </summary>
		/// <typeparam name = "TFacility">The facility type.</typeparam>
		/// <returns></returns>
		IWindsorContainer AddFacility<TFacility>() where TFacility : IFacility, new();

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the container.
		/// </summary>
		/// <typeparam name = "TFacility">The facility type.</typeparam>
		/// <param name = "onCreate">The callback for creation.</param>
		/// <returns></returns>
		IWindsorContainer AddFacility<TFacility>(Action<TFacility> onCreate)
			where TFacility : IFacility, new();

		/// <summary>
		///   Gets a child container instance by name.
		/// </summary>
		/// <param name = "name">The container's name.</param>
		/// <returns>The child container instance or null</returns>
		IWindsorContainer GetChildContainer(string name);

		/// <summary>
		///   Runs the <paramref name = "installers" /> so that they can register components in the container. For details see the documentation at http://j.mp/WindsorInstall
		/// </summary>
		/// <remarks>
		///   In addition to instantiating and passing every installer inline you can use helper methods on <see
		///    cref = "FromAssembly" /> class to automatically instantiate and run your installers.
		///   You can also use <see cref = "Configuration" /> class to install components and/or run aditional installers specofied in a configuration file.
		/// </remarks>
		/// <returns>The container.</returns>
		/// <example>
		///   <code>
		///     container.Install(new YourInstaller1(), new YourInstaller2(), new YourInstaller3());
		///   </code>
		/// </example>
		/// <example>
		///   <code>
		///     container.Install(FromAssembly.This(), Configuration.FromAppConfig(), new SomeOtherInstaller());
		///   </code>
		/// </example>
		IWindsorContainer Install(params IWindsorInstaller[] installers);

		/// <summary>
		///   Registers the components with the <see cref = "IWindsorContainer" />. The instances of <see cref = "IRegistration" /> are produced by fluent registration API.
		///   Most common entry points are <see cref = "Component.For{TService}" /> method to register a single type or (recommended in most cases) 
		///   <see cref = "Classes.FromAssembly(Assembly)" />.
		///   Let the Intellisense drive you through the fluent API past those entry points. For details see the documentation at http://j.mp/WindsorApi
		/// </summary>
		/// <example>
		///   <code>
		///     container.Register(Component.For&lt;IService&gt;().ImplementedBy&lt;DefaultService&gt;().LifestyleTransient());
		///   </code>
		/// </example>
		/// <example>
		///   <code>
		///     container.Register(Classes.FromThisAssembly().BasedOn&lt;IService&gt;().WithServiceDefaultInterfaces().Configure(c => c.LifestyleTransient()));
		///   </code>
		/// </example>
		/// <param name = "registrations">The component registrations created by <see cref = "Component.For{TService}" />, <see
		///    cref = "Classes.FromAssembly(Assembly)" /> or different entry method to the fluent API.</param>
		/// <returns>The container.</returns>
		IWindsorContainer Register(params IRegistration[] registrations);

		/// <summary>
		///   Releases a component instance
		/// </summary>
		/// <param name = "instance"></param>
		void Release(object instance);

		/// <summary>
		///   Remove a child container
		/// </summary>
		/// <param name = "childContainer"></param>
		void RemoveChildContainer(IWindsorContainer childContainer);

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "service"></param>
		/// <returns></returns>
		object Resolve(String key, Type service);

		/// <summary>
		///   Returns a component instance by the service
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		object Resolve(Type service);

		/// <summary>
		///   Returns a component instance by the service
		/// </summary>
		/// <param name = "service"></param>
		/// <param name = "arguments">Arguments to resolve the service. Please see the factory methods in <see cref="Arguments"/></param>
		/// <returns></returns>
		object Resolve(Type service, Arguments arguments);

		/// <summary>
		///   Returns a component instance by the service
		/// </summary>
		/// <typeparam name = "T">Service type</typeparam>
		/// <returns>The component instance</returns>
		T Resolve<T>();

		/// <summary>
		///   Returns a component instance by the service
		/// </summary>
		/// <typeparam name = "T">Service type</typeparam>
		/// <param name = "arguments">Arguments to resolve the service. Please see the factory methods in <see cref="Arguments"/></param>
		/// <returns>The component instance</returns>
		T Resolve<T>(Arguments arguments);

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
		/// <param name = "arguments">Arguments to resolve the service. Please see the factory methods in <see cref="Arguments"/></param>
		/// <returns>The Component instance</returns>
		T Resolve<T>(string key, Arguments arguments);

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "service"></param>
		/// <param name = "arguments">Arguments to resolve the service. Please see the factory methods in <see cref="Arguments"/></param>
		/// <returns></returns>
		object Resolve(string key, Type service, Arguments arguments);

		/// <summary>
		///   Resolve all valid components that match this type.
		/// </summary>
		/// <typeparam name = "T">The service type</typeparam>
		T[] ResolveAll<T>();

		/// <summary>
		///   Resolve all valid components that match this service
		///   <param name = "service">the service to match</param>
		/// </summary>
		Array ResolveAll(Type service);

		/// <summary>
		///   Resolve all valid components that match this service
		///   <param name = "service">the service to match</param>
		/// <param name = "arguments">Arguments to resolve the service. Please see the factory methods in <see cref="Arguments"/></param>
		/// </summary>
		Array ResolveAll(Type service, Arguments arguments);

		/// <summary>
		///   Resolve all valid components that match this type.
		///   <typeparam name = "T">The service type</typeparam>
		/// <param name = "arguments">Arguments to resolve the service. Please see the factory methods in <see cref="Arguments"/></param>
		/// </summary>
		T[] ResolveAll<T>(Arguments arguments);
	}
}