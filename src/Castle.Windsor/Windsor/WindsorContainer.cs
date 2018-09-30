// Copyright 2004-2017 Castle Project - http://www.castleproject.org/
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
	using System.Diagnostics;
	using System.Reflection;
	using System.Text;
	using System.Threading;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.MicroKernel.SubSystems.Resource;
	using Castle.Windsor.Configuration;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.Windsor.Diagnostics;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Proxy;

	/// <summary>
	///   Implementation of <see cref = "IWindsorContainer" />
	///   which delegates to <see cref = "IKernel" /> implementation.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{name,nq}")]
	[DebuggerTypeProxy(typeof(KernelDebuggerProxy))]
	public class WindsorContainer :
#if FEATURE_REMOTING
		MarshalByRefObject,
#endif
		IWindsorContainer
	{
		private const string CastleUnicode = "\uD83C\uDFF0";

		private static int instanceCount = 0;

		private readonly IKernel kernel;
		private readonly string name;
		private readonly IComponentsInstaller installer;
		private IWindsorContainer parent;
		private readonly Dictionary<string, IWindsorContainer> childContainers = new Dictionary<string, IWindsorContainer>(StringComparer.OrdinalIgnoreCase);
		private readonly object childContainersLocker = new object();

		/// <summary>
		///   Constructs a container without any external 
		///   configuration reference
		/// </summary>
		public WindsorContainer() : this(new DefaultKernel(), new DefaultComponentInstaller())
		{
		}

		/// <summary>
		///   Constructs a container using the specified 
		///   <see cref = "IConfigurationStore" /> implementation.
		/// </summary>
		/// <param name = "store">The instance of an <see cref = "IConfigurationStore" /> implementation.</param>
		public WindsorContainer(IConfigurationStore store) : this()
		{
			kernel.ConfigurationStore = store;

			RunInstaller();
		}

		/// <summary>
		///   Constructs a container using the specified 
		///   <see cref = "IConfigurationInterpreter" /> implementation.
		/// </summary>
		/// <param name = "interpreter">The instance of an <see cref = "IConfigurationInterpreter" /> implementation.</param>
		public WindsorContainer(IConfigurationInterpreter interpreter) : this()
		{
			if (interpreter == null)
			{
				throw new ArgumentNullException("interpreter");
			}
			interpreter.ProcessResource(interpreter.Source, kernel.ConfigurationStore, kernel);

			RunInstaller();
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "WindsorContainer" /> class.
		/// </summary>
		/// <param name = "interpreter">The interpreter.</param>
		/// <param name = "environmentInfo">The environment info.</param>
		public WindsorContainer(IConfigurationInterpreter interpreter, IEnvironmentInfo environmentInfo) : this()
		{
			if (interpreter == null)
			{
				throw new ArgumentNullException("interpreter");
			}
			if (environmentInfo == null)
			{
				throw new ArgumentNullException("environmentInfo");
			}

			interpreter.EnvironmentName = environmentInfo.GetEnvironmentName();
			interpreter.ProcessResource(interpreter.Source, kernel.ConfigurationStore, kernel);

			RunInstaller();
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "WindsorContainer" /> class using a
		///   resource pointed to by the parameter. That may be a file, an assembly embedded resource, a UNC path or a config file section.
		///   <para>
		///     Equivalent to the use of <c>new WindsorContainer(new XmlInterpreter(configurationUri))</c>
		///   </para>
		/// </summary>
		/// <param name = "configurationUri">The XML file.</param>
		public WindsorContainer(String configurationUri) : this()
		{
			if (configurationUri == null)
			{
				throw new ArgumentNullException("configurationUri");
			}

			var interpreter = GetInterpreter(configurationUri);
			interpreter.ProcessResource(interpreter.Source, kernel.ConfigurationStore, kernel);

			RunInstaller();
		}

		/// <summary>
		///   Constructs a container using the specified <see cref = "IKernel" />
		///   implementation. Rarely used.
		/// </summary>
		/// <remarks>
		///   This constructs sets the Kernel.ProxyFactory property to
		///   <c>Proxy.DefaultProxyFactory</c>
		/// </remarks>
		/// <param name = "kernel">Kernel instance</param>
		/// <param name = "installer">Installer instance</param>
		public WindsorContainer(IKernel kernel, IComponentsInstaller installer) : this(MakeUniqueName(), kernel, installer)
		{
		}

		/// <summary>
		///   Constructs a container using the specified <see cref = "IKernel" />
		///   implementation. Rarely used.
		/// </summary>
		/// <remarks>
		///   This constructs sets the Kernel.ProxyFactory property to
		///   <c>Proxy.DefaultProxyFactory</c>
		/// </remarks>
		/// <param name = "name">Container's name</param>
		/// <param name = "kernel">Kernel instance</param>
		/// <param name = "installer">Installer instance</param>
		public WindsorContainer(String name, IKernel kernel, IComponentsInstaller installer)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (kernel == null)
			{
				throw new ArgumentNullException("kernel");
			}
			if (installer == null)
			{
				throw new ArgumentNullException("installer");
			}

			this.name = name;
			this.kernel = kernel;
			this.kernel.ProxyFactory = new DefaultProxyFactory();
			this.installer = installer;
		}

		/// <summary>
		///   Constructs with a given <see cref = "IProxyFactory" />.
		/// </summary>
		/// <param name = "proxyFactory">A instance of an <see cref = "IProxyFactory" />.</param>
		public WindsorContainer(IProxyFactory proxyFactory)
		{
			if (proxyFactory == null)
			{
				throw new ArgumentNullException("proxyFactory");
			}

			kernel = new DefaultKernel(proxyFactory);

			installer = new DefaultComponentInstaller();
		}

		/// <summary>
		///   Constructs a container assigning a parent container 
		///   before starting the dependency resolution.
		/// </summary>
		/// <param name = "parent">The instance of an <see cref = "IWindsorContainer" /></param>
		/// <param name = "interpreter">The instance of an <see cref = "IConfigurationInterpreter" /> implementation</param>
		public WindsorContainer(IWindsorContainer parent, IConfigurationInterpreter interpreter) : this()
		{
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}
			if (interpreter == null)
			{
				throw new ArgumentNullException("interpreter");
			}

			parent.AddChildContainer(this);

			interpreter.ProcessResource(interpreter.Source, kernel.ConfigurationStore, kernel);

			RunInstaller();
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "WindsorContainer" /> class.
		/// </summary>
		/// <param name = "name">The container's name.</param>
		/// <param name = "parent">The parent.</param>
		/// <param name = "interpreter">The interpreter.</param>
		public WindsorContainer(string name, IWindsorContainer parent, IConfigurationInterpreter interpreter) : this()
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}
			if (interpreter == null)
			{
				throw new ArgumentNullException("interpreter");
			}

			this.name = name;

			parent.AddChildContainer(this);

			interpreter.ProcessResource(interpreter.Source, kernel.ConfigurationStore, kernel);

			RunInstaller();
		}

		public IComponentsInstaller Installer
		{
			get { return installer; }
		}

		/// <summary>
		///   Returns the inner instance of the MicroKernel
		/// </summary>
		public virtual IKernel Kernel
		{
			get { return kernel; }
		}

		/// <summary>
		///   Gets the container's name
		/// </summary>
		/// <remarks>
		///   Only useful when child containers are being used
		/// </remarks>
		/// <value>The container's name.</value>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		///   Gets or sets the parent container if this instance
		///   is a sub container.
		/// </summary>
		public virtual IWindsorContainer Parent
		{
			get { return parent; }
			set
			{
				if (value == null)
				{
					if (parent != null)
					{
						parent.RemoveChildContainer(this);
						parent = null;
					}
				}
				else
				{
					if (value != parent)
					{
						parent = value;
						parent.AddChildContainer(this);
					}
				}
			}
		}

		protected virtual void RunInstaller()
		{
			if (installer != null)
			{
				installer.SetUp(this, kernel.ConfigurationStore);
			}
		}

		private void Install(IWindsorInstaller[] installers, DefaultComponentInstaller scope)
		{
			using (var store = new PartialConfigurationStore((IKernelInternal)kernel))
			{
				foreach (var windsorInstaller in installers)
				{
					windsorInstaller.Install(this, store);
				}

				scope.SetUp(this, store);
			}
		}

		/// <summary>
		///   Executes Dispose on underlying <see cref = "IKernel" />
		/// </summary>
		public virtual void Dispose()
		{
			Parent = null;
			childContainers.Clear();
			kernel.Dispose();
		}

		/// <summary>
		///   Registers a subcontainer. The components exposed
		///   by this container will be accessible from subcontainers.
		/// </summary>
		/// <param name = "childContainer"></param>
		public virtual void AddChildContainer(IWindsorContainer childContainer)
		{
			if (childContainer == null)
			{
				throw new ArgumentNullException("childContainer");
			}

			if (!childContainers.ContainsKey(childContainer.Name))
			{
				lock (childContainersLocker)
				{
					if (!childContainers.ContainsKey(childContainer.Name))
					{
						kernel.AddChildKernel(childContainer.Kernel);
						childContainers.Add(childContainer.Name, childContainer);
						childContainer.Parent = this;
					}
				}
			}
		}

		/// <summary>
		///   Registers a facility within the container.
		/// </summary>
		/// <param name = "facility"></param>
		public IWindsorContainer AddFacility(IFacility facility)
		{
			kernel.AddFacility(facility);
			return this;
		}

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the container.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <returns></returns>
		public IWindsorContainer AddFacility<T>() where T : IFacility, new()
		{
			kernel.AddFacility<T>();
			return this;
		}

		/// <summary>
		///   Creates and adds an <see cref = "IFacility" /> facility to the container.
		/// </summary>
		/// <typeparam name = "T">The facility type.</typeparam>
		/// <param name = "onCreate">The callback for creation.</param>
		/// <returns></returns>
		public IWindsorContainer AddFacility<T>(Action<T> onCreate)
			where T : IFacility, new()
		{
			kernel.AddFacility(onCreate);
			return this;
		}

		/// <summary>
		///   Gets a child container instance by name.
		/// </summary>
		/// <param name = "name">The container's name.</param>
		/// <returns>The child container instance or null</returns>
		public IWindsorContainer GetChildContainer(string name)
		{
			lock (childContainersLocker)
			{
				IWindsorContainer windsorContainer;
				childContainers.TryGetValue(name, out windsorContainer);
				return windsorContainer;
			}
		}

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
		public IWindsorContainer Install(params IWindsorInstaller[] installers)
		{
			if (installers == null)
			{
				throw new ArgumentNullException("installers");
			}

			if (installers.Length == 0)
			{
				return this;
			}

			var scope = new DefaultComponentInstaller();

			var internalKernel = kernel as IKernelInternal;
			if (internalKernel == null)
			{
				Install(installers, scope);
			}
			else
			{
				var token = internalKernel.OptimizeDependencyResolution();
				Install(installers, scope);
				if (token != null)
				{
					token.Dispose();
				}
			}

			return this;
		}

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
		public IWindsorContainer Register(params IRegistration[] registrations)
		{
			Kernel.Register(registrations);
			return this;
		}

		/// <summary>
		///   Releases a component instance
		/// </summary>
		/// <param name = "instance"></param>
		public virtual void Release(object instance)
		{
			kernel.ReleaseComponent(instance);
		}

		/// <summary>
		///   Removes (unregisters) a subcontainer.  The components exposed by this container
		///   will no longer be accessible to the child container.
		/// </summary>
		/// <param name = "childContainer"></param>
		public virtual void RemoveChildContainer(IWindsorContainer childContainer)
		{
			if (childContainer == null)
			{
				throw new ArgumentNullException("childContainer");
			}

			if (childContainers.ContainsKey(childContainer.Name))
			{
				lock (childContainersLocker)
				{
					if (childContainers.ContainsKey(childContainer.Name))
					{
						kernel.RemoveChildKernel(childContainer.Kernel);
						childContainers.Remove(childContainer.Name);
						childContainer.Parent = null;
					}
				}
			}
		}

		/// <summary>
		///   Returns a component instance by the service
		/// </summary>
		/// <param name = "service"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public virtual object Resolve(Type service, Arguments arguments)
		{
			return kernel.Resolve(service, arguments);
		}

		/// <summary>
		///   Returns a component instance by the service
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		public virtual object Resolve(Type service)
		{
			return kernel.Resolve(service, arguments: null);
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "service"></param>
		/// <returns></returns>
		public virtual object Resolve(String key, Type service)
		{
			return kernel.Resolve(key, service, arguments: null);
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "service"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public virtual object Resolve(string key, Type service, Arguments arguments)
		{
			return kernel.Resolve(key, service, arguments);
		}

		/// <summary>
		///   Returns a component instance by the service
		/// </summary>
		/// <typeparam name = "T"></typeparam>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public T Resolve<T>(Arguments arguments)
		{
			return (T)kernel.Resolve(typeof(T), arguments);
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public virtual T Resolve<T>(string key, Arguments arguments)
		{
			return (T)kernel.Resolve(key, typeof(T), arguments);
		}

		/// <summary>
		///   Returns a component instance by the service
		/// </summary>
		/// <typeparam name = "T"></typeparam>
		/// <returns></returns>
		public T Resolve<T>()
		{
			return (T)kernel.Resolve(typeof(T), arguments: null);
		}

		/// <summary>
		///   Returns a component instance by the key
		/// </summary>
		/// <param name = "key"></param>
		/// <returns></returns>
		public virtual T Resolve<T>(String key)
		{
			return (T)kernel.Resolve(key, typeof(T), arguments: null);
		}

		/// <summary>
		///   Resolve all valid components that match this type.
		/// </summary>
		/// <typeparam name = "T">The service type</typeparam>
		public T[] ResolveAll<T>()
		{
			return (T[])ResolveAll(typeof(T));
		}

		/// <summary>
		///	Resolve all valid components that match this type.
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>
		public Array ResolveAll(Type service)
		{
			return kernel.ResolveAll(service);
		}

		///  <summary>
		/// 	Resolve all valid components that match this type by passing dependencies as arguments.
		///  </summary>
		///  <param name="service"></param>
		/// <param name = "arguments"></param>
		/// <returns></returns>
		public Array ResolveAll(Type service, Arguments arguments)
		{
			return kernel.ResolveAll(service, arguments);
		}

		/// <summary>
		///   Resolve all valid components that match this type.
		///   <typeparam name = "T">The service type</typeparam>
		///   <param name = "arguments">Arguments to resolve the service</param>
		/// </summary>
		public T[] ResolveAll<T>(Arguments arguments)
		{
			return (T[])ResolveAll(typeof(T), arguments);
		}

		private XmlInterpreter GetInterpreter(string configurationUri)
		{
			try
			{
				var resources = (IResourceSubSystem)Kernel.GetSubSystem(SubSystemConstants.ResourceKey);
				var resource = resources.CreateResource(configurationUri);
				return new XmlInterpreter(resource);
			}
			catch (Exception)
			{
				// We fallback to the old behavior
				return new XmlInterpreter(configurationUri);
			}
		}

		private static string MakeUniqueName()
		{
			var sb = new StringBuilder();
#if FEATURE_APPDOMAIN
			sb.Append(AppDomain.CurrentDomain.FriendlyName);
			sb.Append(" ");
#endif
			sb.Append(CastleUnicode);
			sb.Append(" ");
			sb.Append(Interlocked.Increment(ref instanceCount));
			return sb.ToString();
		}
	}
}