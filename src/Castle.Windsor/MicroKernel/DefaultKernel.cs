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
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Security;
	using System.Text;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Lifestyle;
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.MicroKernel.ModelBuilder;
	using Castle.MicroKernel.ModelBuilder.Inspectors;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Releasers;
	using Castle.MicroKernel.Resolvers;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.MicroKernel.SubSystems.Naming;
	using Castle.MicroKernel.SubSystems.Resource;
	using Castle.Windsor.Diagnostics;

	/// <summary>
	///   Default implementation of <see cref = "IKernel" />. 
	///   This implementation is complete and also support a kernel 
	///   hierarchy (sub containers).
	/// </summary>
	[Serializable]
#if !SILVERLIGHT
	[DebuggerTypeProxy(typeof(KernelDebuggerProxy))]
	public partial class DefaultKernel : MarshalByRefObject, IKernel, IKernelEvents, IKernelInternal
#else
	public partial class DefaultKernel : IKernel, IKernelEvents, IKernelInternal
#endif
	{
		[ThreadStatic]
		private static CreationContext currentCreationContext;

		[ThreadStatic]
		private static bool isCheckingLazyLoaders;

		private ThreadSafeFlag disposed;

		/// <summary>
		///   List of sub containers.
		/// </summary>
		private readonly List<IKernel> childKernels = new List<IKernel>();

		/// <summary>
		///   List of <see cref = "IFacility" /> registered.
		/// </summary>
		private readonly List<IFacility> facilities = new List<IFacility>();

		/// <summary>
		///   Map of subsystems registered.
		/// </summary>
		private readonly Dictionary<string, ISubSystem> subsystems = new Dictionary<string, ISubSystem>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		///   The parent kernel, if exists.
		/// </summary>
		private IKernel parentKernel;

		private readonly object lazyLoadingLock = new object();

		/// <summary>
		///   Constructs a DefaultKernel with no component
		///   proxy support.
		/// </summary>
		public DefaultKernel() : this(new NotSupportedProxyFactory())
		{
		}

		/// <summary>
		///   Constructs a DefaultKernel with the specified
		///   implementation of <see cref = "IProxyFactory" /> and <see cref = "IDependencyResolver" />
		/// </summary>
		/// <param name = "resolver"></param>
		/// <param name = "proxyFactory"></param>
		public DefaultKernel(IDependencyResolver resolver, IProxyFactory proxyFactory)
		{
			RegisterSubSystems();
			ReleasePolicy = new LifecycledComponentsReleasePolicy(this);
			HandlerFactory = new DefaultHandlerFactory(this);
			ComponentModelBuilder = new DefaultComponentModelBuilder(this);
			ProxyFactory = proxyFactory;
			Resolver = resolver;
			Resolver.Initialize(this, RaiseDependencyResolving);
		}

		/// <summary>
		///   Constructs a DefaultKernel with the specified
		///   implementation of <see cref = "IProxyFactory" />
		/// </summary>
		public DefaultKernel(IProxyFactory proxyFactory)
			: this(new DefaultDependencyResolver(), proxyFactory)
		{
		}

#if !SILVERLIGHT
#if DOTNET40
		[SecurityCritical]
#endif
		public DefaultKernel(SerializationInfo info, StreamingContext context)
		{
			var members = FormatterServices.GetSerializableMembers(GetType(), context);
			var kernelmembers = (object[])info.GetValue("members", typeof(object[]));

			FormatterServices.PopulateObjectMembers(this, members, kernelmembers);

			HandlerRegistered += (HandlerDelegate)info.GetValue("HandlerRegisteredEvent", typeof(Delegate));
		}
#endif

		public IComponentModelBuilder ComponentModelBuilder { get; set; }

		public virtual IConfigurationStore ConfigurationStore
		{
			get { return GetSubSystem(SubSystemConstants.ConfigurationStoreKey) as IConfigurationStore; }
			set { AddSubSystem(SubSystemConstants.ConfigurationStoreKey, value); }
		}

		/// <summary>
		///   Graph of components and interactions.
		/// </summary>
		public GraphNode[] GraphNodes
		{
			get
			{
				var nodes = new GraphNode[NamingSubSystem.ComponentCount];
				var index = 0;

				var handlers = NamingSubSystem.GetAllHandlers();
				foreach (var handler in handlers)
				{
					nodes[index++] = handler.ComponentModel;
				}

				return nodes;
			}
		}

		public IHandlerFactory HandlerFactory { get; private set; }

		public virtual IKernel Parent
		{
			get { return parentKernel; }
			set
			{
				// TODO: should the raise add/removed as child kernel methods be invoked from within the subscriber/unsubscribe methods?

				if (value == null)
				{
					if (parentKernel != null)
					{
						UnsubscribeFromParentKernel();
						RaiseRemovedAsChildKernel();
					}

					parentKernel = null;
				}
				else
				{
					if ((parentKernel != value) && (parentKernel != null))
					{
						throw new KernelException(
							"You can not change the kernel parent once set, use the RemoveChildKernel and AddChildKernel methods together to achieve this.");
					}
					parentKernel = value;
					SubscribeToParentKernel();
					RaiseAddedAsChildKernel();
				}
			}
		}

		public IProxyFactory ProxyFactory { get; set; }

		public IReleasePolicy ReleasePolicy { get; set; }

		public IDependencyResolver Resolver { get; private set; }

		protected IConversionManager ConversionSubSystem
		{
			get { return GetSubSystem(SubSystemConstants.ConversionManagerKey) as IConversionManager; }
		}

		protected INamingSubSystem NamingSubSystem
		{
			get { return GetSubSystem(SubSystemConstants.NamingKey) as INamingSubSystem; }
		}

#if !SILVERLIGHT
#if DOTNET40
		[SecurityCritical]
#endif
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			var members = FormatterServices.GetSerializableMembers(GetType(), context);

			var kernelmembers = FormatterServices.GetObjectData(this, members);

			info.AddValue("members", kernelmembers, typeof(object[]));

			info.AddValue("HandlerRegisteredEvent", HandlerRegistered);
		}
#endif

		/// <summary>
		///   Starts the process of component disposal.
		/// </summary>
		public virtual void Dispose()
		{
			if (!disposed.Signal())
			{
				return;
			}

			DisposeSubKernels();
			TerminateFacilities();
			DisposeComponentsInstancesWithinTracker();
			DisposeHandlers();
			UnsubscribeFromParentKernel();
		}

		public virtual void AddChildKernel(IKernel childKernel)
		{
			if (childKernel == null)
			{
				throw new ArgumentNullException("childKernel");
			}

			childKernel.Parent = this;
			childKernels.Add(childKernel);
		}

		public virtual IHandler AddCustomComponent(ComponentModel model, bool isMetaHandler)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}

			RaiseComponentModelCreated(model);
			return HandlerFactory.Create(model, isMetaHandler);
		}

		// NOTE: this is from IKernelInternal
		public IHandler AddCustomComponent(ComponentModel model)
		{
			return AddCustomComponent(model, false);
		}

		public virtual IKernel AddFacility(IFacility facility)
		{
			if (facility == null)
			{
				throw new ArgumentNullException("facility");
			}
			var facilityType = facility.GetType();
			if (facilities.Any(f => f.GetType() == facilityType))
			{
				throw new ArgumentException(
					string.Format(
						"Facility of type '{0}' has already been registered with the container. Only one facility of a given type can exist in the container.",
						facilityType.FullName));
			}
			facilities.Add(facility);
			facility.Init(this, ConfigurationStore.GetFacilityConfiguration(facilityType.FullName));

			return this;
		}

		public IKernel AddFacility<T>() where T : IFacility, new()
		{
			return AddFacility(new T());
		}

		public IKernel AddFacility<T>(Action<T> onCreate)
			where T : IFacility, new()
		{
			var facility = new T();
			if (onCreate != null)
			{
				onCreate(facility);
			}
			return AddFacility(facility);
		}

		public void AddHandlerSelector(IHandlerSelector selector)
		{
			NamingSubSystem.AddHandlerSelector(selector);
		}

		public void AddHandlersFilter(IHandlersFilter filter)
		{
			NamingSubSystem.AddHandlersFilter(filter);
		}

		public virtual void AddSubSystem(String name, ISubSystem subsystem)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (subsystem == null)
			{
				throw new ArgumentNullException("subsystem");
			}

			subsystem.Init(this);
			subsystems[name] = subsystem;
		}

		/// <summary>
		///   Return handlers for components that 
		///   implements the specified service. 
		///   The check is made using IsAssignableFrom
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		public virtual IHandler[] GetAssignableHandlers(Type service)
		{
			var result = NamingSubSystem.GetAssignableHandlers(service);

			// If a parent kernel exists, we merge both results
			if (Parent != null)
			{
				var parentResult = Parent.GetAssignableHandlers(service);

				if (parentResult.Length > 0)
				{
					var newResult = new IHandler[result.Length + parentResult.Length];
					result.CopyTo(newResult, 0);
					parentResult.CopyTo(newResult, result.Length);
					result = newResult;
				}
			}

			return result;
		}

		/// <summary>
		///   Returns the facilities registered on the kernel.
		/// </summary>
		/// <returns></returns>
		public virtual IFacility[] GetFacilities()
		{
			return facilities.ToArray();
		}

		public virtual IHandler GetHandler(String name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			var handler = NamingSubSystem.GetHandler(name);

			if (handler == null && Parent != null)
			{
				handler = WrapParentHandler(Parent.GetHandler(name));
			}

			return handler;
		}

		public virtual IHandler GetHandler(Type service)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}

			var handler = NamingSubSystem.GetHandler(service);
			if (handler == null && service.IsGenericType)
			{
				handler = NamingSubSystem.GetHandler(service.GetGenericTypeDefinition());
			}

			if (handler == null && Parent != null)
			{
				handler = WrapParentHandler(Parent.GetHandler(service));
			}

			return handler;
		}

		/// <summary>
		///   Return handlers for components that 
		///   implements the specified service.
		/// </summary>
		/// <param name = "service"></param>
		/// <returns></returns>
		public virtual IHandler[] GetHandlers(Type service)
		{
			var result = NamingSubSystem.GetHandlers(service);

			// If a parent kernel exists, we merge both results
			if (Parent != null)
			{
				var parentResult = Parent.GetHandlers(service);

				if (parentResult.Length > 0)
				{
					var newResult = new IHandler[result.Length + parentResult.Length];
					result.CopyTo(newResult, 0);
					parentResult.CopyTo(newResult, result.Length);
					result = newResult;
				}
			}

			return result;
		}

		public virtual ISubSystem GetSubSystem(String name)
		{
			ISubSystem subsystem;
			subsystems.TryGetValue(name, out subsystem);
			return subsystem;
		}

		public virtual bool HasComponent(String name)
		{
			if (name == null)
			{
				return false;
			}

			if (NamingSubSystem.Contains(name))
			{
				return true;
			}

			if (Parent != null)
			{
				return Parent.HasComponent(name);
			}

			return false;
		}

		public virtual bool HasComponent(Type serviceType)
		{
			if (serviceType == null)
			{
				return false;
			}

			if (NamingSubSystem.Contains(serviceType))
			{
				return true;
			}

			if (serviceType.IsGenericType && NamingSubSystem.Contains(serviceType.GetGenericTypeDefinition()))
			{
				return true;
			}

			if (Parent != null)
			{
				return Parent.HasComponent(serviceType);
			}

			return false;
		}

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
		public IKernel Register(params IRegistration[] registrations)
		{
			if (registrations == null)
			{
				throw new ArgumentNullException("registrations");
			}

			var token = OptimizeDependencyResolution();
			foreach (var registration in registrations)
			{
				registration.Register(this);
			}
			if (token != null)
			{
				token.Dispose();
			}
			return this;
		}

		/// <summary>
		///   Releases a component instance. This allows
		///   the kernel to execute the proper decommission
		///   lifecycles on the component instance.
		/// </summary>
		/// <param name = "instance"></param>
		public virtual void ReleaseComponent(object instance)
		{
			if (ReleasePolicy.HasTrack(instance))
			{
				ReleasePolicy.Release(instance);
			}
			else
			{
				if (Parent != null)
				{
					Parent.ReleaseComponent(instance);
				}
			}
		}

		public virtual void RemoveChildKernel(IKernel childKernel)
		{
			if (childKernel == null)
			{
				throw new ArgumentNullException("childKernel");
			}

			childKernel.Parent = null;
			childKernels.Remove(childKernel);
		}

		/// <summary>
		///   Creates an implementation of
		///   <see cref = "ILifestyleManager" />
		///   based
		///   on
		///   <see cref = "LifestyleType" />
		///   and invokes
		///   <see cref = "ILifestyleManager.Init" />
		///   to initialize the newly created manager.
		/// </summary>
		/// <param name = "model"></param>
		/// <param name = "activator"></param>
		/// <returns></returns>
		public virtual ILifestyleManager CreateLifestyleManager(ComponentModel model, IComponentActivator activator)
		{
			ILifestyleManager manager;
			var type = model.LifestyleType;

			switch (type)
			{
				case LifestyleType.Scoped:
					manager = new ScopedLifestyleManager(CreateScopeAccessor(model));
					break;
				case LifestyleType.Bound:
					manager = new ScopedLifestyleManager(CreateScopeAccessorForBoundLifestyle(model));
					break;
				case LifestyleType.Thread:
					manager = new ScopedLifestyleManager(new ThreadScopeAccessor());
					break;
				case LifestyleType.Transient:
					manager = new TransientLifestyleManager();
					break;
#if (!SILVERLIGHT && !CLIENTPROFILE) && SYSTEMWEB
				case LifestyleType.PerWebRequest:
					manager = new ScopedLifestyleManager(new WebRequestScopeAccessor());
					break;
#endif
                case LifestyleType.Custom:
					manager = model.CustomLifestyle.CreateInstance<ILifestyleManager>();

					break;
				case LifestyleType.Pooled:
					var initial = ExtendedPropertiesConstants.Pool_Default_InitialPoolSize;
					var maxSize = ExtendedPropertiesConstants.Pool_Default_MaxPoolSize;

					if (model.ExtendedProperties.Contains(ExtendedPropertiesConstants.Pool_InitialPoolSize))
					{
						initial = (int)model.ExtendedProperties[ExtendedPropertiesConstants.Pool_InitialPoolSize];
					}
					if (model.ExtendedProperties.Contains(ExtendedPropertiesConstants.Pool_MaxPoolSize))
					{
						maxSize = (int)model.ExtendedProperties[ExtendedPropertiesConstants.Pool_MaxPoolSize];
					}

					manager = new PoolableLifestyleManager(initial, maxSize);
					break;
				default:
					//this includes LifestyleType.Undefined, LifestyleType.Singleton and invalid values
					manager = new SingletonLifestyleManager();
					break;
			}

			manager.Init(activator, this, model);

			return manager;
		}

		private static IScopeAccessor CreateScopeAccessor(ComponentModel model)
		{
			var scopeAccessorType = model.GetScopeAccessorType();
			if (scopeAccessorType == null)
			{
				return new LifetimeScopeAccessor();
			}
			return scopeAccessorType.CreateInstance<IScopeAccessor>();
		}

		private IScopeAccessor CreateScopeAccessorForBoundLifestyle(ComponentModel model)
		{
			var selector = (Func<IHandler[], IHandler>)model.ExtendedProperties[Constants.ScopeRootSelector];
			if (selector == null)
			{
				throw new ComponentRegistrationException(
					string.Format("Component {0} has lifestyle {1} but it does not specify mandatory 'scopeRootSelector'.",
					              model.Name, LifestyleType.Bound));
			}

			return new CreationContextScopeAccessor(model, selector);
		}

		public virtual IComponentActivator CreateComponentActivator(ComponentModel model)
		{
			if (model == null)
			{
				throw new ArgumentNullException("model");
			}

			IComponentActivator activator;

			if (model.CustomComponentActivator == null)
			{
				activator = new DefaultComponentActivator(model, this,
				                                          RaiseComponentCreated,
				                                          RaiseComponentDestroyed);
			}
			else
			{
				try
				{
					activator = model.CustomComponentActivator.CreateInstance<IComponentActivator>(new object[]
					{
						model,
						this,
						new ComponentInstanceDelegate(RaiseComponentCreated),
						new ComponentInstanceDelegate(RaiseComponentDestroyed)
					});
				}
				catch (Exception e)
				{
					throw new KernelException("Could not instantiate custom activator", e);
				}
			}

			return activator;
		}

		protected CreationContext CreateCreationContext(IHandler handler, Type requestedType, IDictionary additionalArguments, CreationContext parent,
		                                                IReleasePolicy policy)
		{
			return new CreationContext(handler,
			                           policy,
			                           requestedType,
			                           additionalArguments,
			                           ConversionSubSystem,
			                           parent);
		}

		/// <remarks>
		///   It is the responsibility of the kernel to ensure that handler is only ever disposed once.
		/// </remarks>
		protected void DisposeHandler(IHandler handler)
		{
			var disposable = handler as IDisposable;
			if (disposable == null)
			{
				return;
			}

			disposable.Dispose();
		}

		protected void RegisterHandler(String name, IHandler handler)
		{
			(this as IKernelInternal).RegisterHandler(name, handler, false);
		}

		void IKernelInternal.RegisterHandler(String name, IHandler handler, bool skipRegistration)
		{
			if (skipRegistration == false)
			{
				NamingSubSystem.Register(handler);
			}

			RaiseHandlerRegistered(handler);
			RaiseHandlersChanged();
			RaiseComponentRegistered(name, handler);
		}

		protected virtual void RegisterSubSystems()
		{
			AddSubSystem(SubSystemConstants.ConfigurationStoreKey,
			             new DefaultConfigurationStore());

			AddSubSystem(SubSystemConstants.ConversionManagerKey,
			             new DefaultConversionManager());

			AddSubSystem(SubSystemConstants.NamingKey,
			             new DefaultNamingSubSystem());

			AddSubSystem(SubSystemConstants.ResourceKey,
			             new DefaultResourceSubSystem());

			AddSubSystem(SubSystemConstants.DiagnosticsKey,
			             new DefaultDiagnosticsSubSystem());
		}

		protected object ResolveComponent(IHandler handler, Type service, IDictionary additionalArguments, IReleasePolicy policy)
		{
			Debug.Assert(handler != null, "handler != null");
			var parent = currentCreationContext;
			var context = CreateCreationContext(handler, service, additionalArguments, parent, policy);
			currentCreationContext = context;

			try
			{
				return handler.Resolve(context);
			}
			finally
			{
				currentCreationContext = parent;
			}
		}

		protected virtual IHandler WrapParentHandler(IHandler parentHandler)
		{
			if (parentHandler == null)
			{
				return null;
			}

			var handler = new ParentHandlerWithChildResolver(parentHandler, Resolver);
			handler.Init(this);
			return handler;
		}

		private void DisposeComponentsInstancesWithinTracker()
		{
			ReleasePolicy.Dispose();
		}

		private void DisposeHandlers()
		{
			var vertices = TopologicalSortAlgo.Sort(GraphNodes);

			for (var i = 0; i < vertices.Length; i++)
			{
				var model = (ComponentModel)vertices[i];
				var handler = NamingSubSystem.GetHandler(model.Name);
				DisposeHandler(handler);
			}
		}

		private void DisposeSubKernels()
		{
			foreach (var childKernel in childKernels)
			{
				childKernel.Dispose();
			}
		}

		private void HandlerRegisteredOnParentKernel(IHandler handler, ref bool stateChanged)
		{
			RaiseHandlerRegistered(handler);
		}

		private void HandlersChangedOnParentKernel(ref bool changed)
		{
			RaiseHandlersChanged();
		}

		private void SubscribeToParentKernel()
		{
			if (parentKernel == null)
			{
				return;
			}

			parentKernel.HandlerRegistered += HandlerRegisteredOnParentKernel;
			parentKernel.HandlersChanged += HandlersChangedOnParentKernel;
			parentKernel.ComponentRegistered += RaiseComponentRegistered;
		}

		private void TerminateFacilities()
		{
			foreach (var facility in facilities)
			{
				facility.Terminate();
			}
		}

		private void UnsubscribeFromParentKernel()
		{
			if (parentKernel == null)
			{
				return;
			}

			parentKernel.HandlerRegistered -= HandlerRegisteredOnParentKernel;
			parentKernel.HandlersChanged -= HandlersChangedOnParentKernel;
			parentKernel.ComponentRegistered -= RaiseComponentRegistered;
		}

		IHandler IKernelInternal.LoadHandlerByName(string name, Type service, IDictionary arguments)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			var handler = GetHandler(name);
			if (handler != null)
			{
				return handler;
			}
			lock (lazyLoadingLock)
			{
				handler = GetHandler(name);
				if (handler != null)
				{
					return handler;
				}

				if (isCheckingLazyLoaders)
				{
					return null;
				}

				isCheckingLazyLoaders = true;
				try
				{
					foreach (var loader in ResolveAll<ILazyComponentLoader>())
					{
						var registration = loader.Load(name, service, arguments);
						if (registration != null)
						{
							registration.Register(this);
							return GetHandler(name);
						}
					}
					return null;
				}
				finally
				{
					isCheckingLazyLoaders = false;
				}
			}
		}

		IHandler IKernelInternal.LoadHandlerByType(string name, Type service, IDictionary arguments)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}

			var handler = GetHandler(service);
			if (handler != null)
			{
				return handler;
			}

			lock (lazyLoadingLock)
			{
				handler = GetHandler(service);
				if (handler != null)
				{
					return handler;
				}

				if (isCheckingLazyLoaders)
				{
					return null;
				}

				isCheckingLazyLoaders = true;
				try
				{
					foreach (var loader in ResolveAll<ILazyComponentLoader>())
					{
						var registration = loader.Load(name, service, arguments);
						if (registration != null)
						{
							registration.Register(this);
							return GetHandler(service);
						}
					}
					return null;
				}
				finally
				{
					isCheckingLazyLoaders = false;
				}
			}
		}
	}
}