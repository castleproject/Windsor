// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
	using System.Runtime.CompilerServices;
	using System.Runtime.Serialization;
	using System.Security;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.ModelBuilder;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Releasers;
	using Castle.MicroKernel.Resolvers;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.MicroKernel.SubSystems.Naming;
	using Castle.MicroKernel.SubSystems.Resource;
#if !SILVERLIGHT
	using Castle.Windsor.Experimental.Debugging;
#endif

	/// <summary>
	///   Default implementation of <see cref = "IKernel" />. 
	///   This implementation is complete and also support a kernel 
	///   hierarchy (sub containers).
	/// </summary>
#if !SILVERLIGHT
	[Serializable]
	[DebuggerTypeProxy(typeof(KernelDebuggerProxy))]
	public partial class DefaultKernel : MarshalByRefObject, IKernel, IKernelEvents, IKernelInternal,
	                                     IDeserializationCallback, IKernelEventsInternal
#else
	public partial class DefaultKernel : IKernel, IKernelEvents, IKernelInternal, IKernelEventsInternal
#endif
	{
		[ThreadStatic]
		private static CreationContext currentCreationContext;

		[ThreadStatic]
		private static bool isCheckingLazyLoaders;

		/// <summary>
		///   List of sub containers.
		/// </summary>
		private readonly List<IKernel> childKernels = new List<IKernel>();

		/// <summary>
		///   List of <see cref = "IFacility" /> registered.
		/// </summary>
		private readonly List<IFacility> facilities = new List<IFacility>();

		/// <summary>
		///   The implementation of <see cref = "IHandlerFactory" />
		/// </summary>
		private readonly IHandlerFactory handlerFactory;

		/// <summary>
		///   The dependency resolver.
		/// </summary>
		private readonly IDependencyResolver resolver;

		/// <summary>
		///   Map of subsystems registered.
		/// </summary>
		private readonly Dictionary<string, ISubSystem> subsystems =
			new Dictionary<string, ISubSystem>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		///   The implementation of <see cref = "IComponentModelBuilder" />
		/// </summary>
		private IComponentModelBuilder modelBuilder;

		/// <summary>
		///   The parent kernel, if exists.
		/// </summary>
		private IKernel parentKernel;

		/// <summary>
		///   Holds the implementation of <see cref = "IProxyFactory" />
		/// </summary>
		private IProxyFactory proxyFactory;

		/// <summary>
		///   Implements a policy to control component's
		///   disposal that the user forgot.
		/// </summary>
		private IReleasePolicy releasePolicy;

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

			releasePolicy = new LifecycledComponentsReleasePolicy();
			handlerFactory = new DefaultHandlerFactory(this);
			modelBuilder = new DefaultComponentModelBuilder(this);
			this.proxyFactory = proxyFactory;
			this.resolver = resolver;
			resolver.Initialize(this, RaiseDependencyResolving);
		}

		/// <summary>
		///   Constructs a DefaultKernel with the specified
		///   implementation of <see cref = "IProxyFactory" />
		/// </summary>
		public DefaultKernel(IProxyFactory proxyFactory)
			: this(new DefaultDependencyResolver(),proxyFactory)
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

			AddHandler(HandlerRegisteredEvent, (Delegate)info.GetValue("HandlerRegisteredEvent", typeof(Delegate)));
		}
#endif

		public IComponentModelBuilder ComponentModelBuilder
		{
			get { return modelBuilder; }
			set { modelBuilder = value; }
		}

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

				var handlers = NamingSubSystem.GetHandlers();
				foreach (var handler in handlers)
				{
					nodes[index++] = handler.ComponentModel;
				}

				return nodes;
			}
		}

		public IHandlerFactory HandlerFactory
		{
			get { return handlerFactory; }
		}

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

		public IProxyFactory ProxyFactory
		{
			get { return proxyFactory; }
			set { proxyFactory = value; }
		}

		public virtual IReleasePolicy ReleasePolicy
		{
			get { return releasePolicy; }
			set { releasePolicy = value; }
		}

		public IDependencyResolver Resolver
		{
			get { return resolver; }
		}

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

			info.AddValue("HandlerRegisteredEvent", GetEventHandlers<HandlerDelegate>(HandlerRegisteredEvent));
		}
#endif
		
		/// <summary>
		///   Starts the process of component disposal.
		/// </summary>
		public virtual void Dispose()
		{
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

		// NOTE: this is from IKernelInternal
		public virtual void AddCustomComponent(ComponentModel model)
		{
			if (model == null) throw new ArgumentNullException("model");

			RaiseComponentModelCreated(model);
			IHandler handler = HandlerFactory.Create(model);

			object skipRegistration = model.ExtendedProperties[ComponentModel.SkipRegistration];

			if (skipRegistration != null)
			{
				RegisterHandler(model.Name, handler, (bool)skipRegistration);
			}
			else
			{
				RegisterHandler(model.Name, handler);
			}
		}

		public virtual IKernel AddFacility(String key, IFacility facility)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (facility == null)
			{
				throw new ArgumentNullException("facility");
			}

			facility.Init(this, ConfigurationStore.GetFacilityConfiguration(key));

			facilities.Add(facility);
			return this;
		}

		public IKernel AddFacility<T>(String key) where T : IFacility, new()
		{
			return AddFacility(key, new T());
		}

		public IKernel AddFacility<T>(String key, Action<T> onCreate)
			where T : IFacility, new()
		{
			var facility = new T();
			if (onCreate != null)
			{
				onCreate(facility);
			}
			return AddFacility(key, facility);
		}

		public IKernel AddFacility<T>(String key, Func<T, object> onCreate)
			where T : IFacility, new()
		{
			var facility = new T();
			if (onCreate != null)
			{
				onCreate(facility);
			}
			return AddFacility(key, facility);
		}

		public IKernel AddFacility<T>() where T : IFacility, new()
		{
			return AddFacility<T>(typeof(T).FullName);
		}

		public IKernel AddFacility<T>(Action<T> onCreate)
			where T : IFacility, new()
		{
			var facility = new T();
			if (onCreate != null)
			{
				onCreate(facility);
			}
			return AddFacility(typeof(T).FullName, facility);
		}

		public IKernel AddFacility<T>(Func<T, object> onCreate)
			where T : IFacility, new()
		{
			var facility = new T();
			if (onCreate != null)
			{
				onCreate(facility);
			}
			return AddFacility(typeof(T).FullName, facility);
		}

		public void AddHandlerSelector(IHandlerSelector selector)
		{
			NamingSubSystem.AddHandlerSelector(selector);
		}

		public virtual void AddSubSystem(String key, ISubSystem subsystem)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (subsystem == null)
			{
				throw new ArgumentNullException("subsystem");
			}

			subsystem.Init(this);
			subsystems[key] = subsystem;
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

		public virtual IHandler GetHandler(String key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			var handler = NamingSubSystem.GetHandler(key);

			if (handler == null && Parent != null)
			{
				handler = WrapParentHandler(Parent.GetHandler(key));
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

			// a complete generic type, Foo<Bar>, need to check if Foo<T> is registered
			if (service.IsGenericType && !service.IsGenericTypeDefinition)
			{
				var genericResult = NamingSubSystem.GetHandlers(service.GetGenericTypeDefinition());

				if (result.Length > 0)
				{
					var mergedResult = new IHandler[result.Length + genericResult.Length];
					result.CopyTo(mergedResult, 0);
					genericResult.CopyTo(mergedResult, result.Length);
					result = mergedResult;
				}
				else
				{
					result = genericResult;
				}
			}

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

		public virtual ISubSystem GetSubSystem(String key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			ISubSystem subsystem;
			subsystems.TryGetValue(key, out subsystem);
			return subsystem;
		}

		public virtual bool HasComponent(String key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (NamingSubSystem.Contains(key))
			{
				return true;
			}

			if (Parent != null)
			{
				return Parent.HasComponent(key);
			}

			return false;
		}

		public virtual bool HasComponent(Type serviceType)
		{
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
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
		///   Registers the components described by the <see cref = "ComponentRegistration{S}" />s
		///   with the <see cref = "IKernel" />.
		///   <param name = "registrations">The component registrations.</param>
		///   <returns>The kernel.</returns>
		/// </summary>
		public IKernel Register(params IRegistration[] registrations)
		{
			if (registrations == null)
			{
				throw new ArgumentNullException("registrations");
			}

			using (OptimizeDependencyResolution())
			{
				foreach (var registration in registrations)
				{
					registration.Register(this);
				}
			}

			return this;
		}

		/// <summary>
		///   Associates objects with a component handler,
		///   allowing it to use the specified dictionary
		///   when resolving dependencies
		/// </summary>
		/// <param name = "service"></param>
		/// <param name = "dependencies"></param>
		public void RegisterCustomDependencies(Type service, IDictionary dependencies)
		{
			var handler = GetHandler(service);

			foreach (DictionaryEntry entry in dependencies)
			{
				handler.AddCustomDependencyValue(entry.Key.ToString(), entry.Value);
			}
		}

		/// <summary>
		///   Associates objects with a component handler,
		///   allowing it to use the specified dictionary
		///   when resolving dependencies
		/// </summary>
		/// <param name = "service"></param>
		/// <param name = "dependenciesAsAnonymousType"></param>
		public void RegisterCustomDependencies(Type service, object dependenciesAsAnonymousType)
		{
			RegisterCustomDependencies(service, new ReflectionBasedDictionaryAdapter(dependenciesAsAnonymousType));
		}

		/// <summary>
		///   Associates objects with a component handler,
		///   allowing it to use the specified dictionary
		///   when resolving dependencies
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "dependencies"></param>
		public void RegisterCustomDependencies(String key, IDictionary dependencies)
		{
			var handler = GetHandler(key);

			foreach (DictionaryEntry entry in dependencies)
			{
				handler.AddCustomDependencyValue(entry.Key.ToString(), entry.Value);
			}
		}

		/// <summary>
		///   Associates objects with a component handler,
		///   allowing it to use the specified dictionary
		///   when resolving dependencies
		/// </summary>
		/// <param name = "key"></param>
		/// <param name = "dependenciesAsAnonymousType"></param>
		public void RegisterCustomDependencies(String key, object dependenciesAsAnonymousType)
		{
			RegisterCustomDependencies(key, new ReflectionBasedDictionaryAdapter(dependenciesAsAnonymousType));
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
		///   Returns true if the specified component was
		///   found and could be removed (i.e. no other component depends on it)
		/// </summary>
		/// <param name = "key">The component's key</param>
		/// <returns></returns>
		public virtual bool RemoveComponent(String key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (!NamingSubSystem.Contains(key))
			{
				if (Parent == null)
				{
					return false;
				}

				return Parent.RemoveComponent(key);
			}

			var handler = GetHandler(key);

			if (handler.ComponentModel.Dependers.Length != 0)
			{
				// We can't remove this component as there are
				// others which depends on it

				return false;
			}

			NamingSubSystem.UnRegister(key);

			var service = handler.ComponentModel.Service;
			var assignableHandlers = NamingSubSystem.GetAssignableHandlers(service);
			if (assignableHandlers.Length > 0)
			{
				NamingSubSystem[handler.ComponentModel.Service] = assignableHandlers[0];
			}
			else
			{
				NamingSubSystem.UnRegister(service);
			}

			foreach (ComponentModel model in handler.ComponentModel.Dependents)
			{
				model.RemoveDepender(handler.ComponentModel);
			}

			RaiseComponentUnregistered(key, handler);

			DisposeHandler(handler);

			return true;
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

		///<summary>
		///  Gets the service object of the specified type.
		///</summary>
		///<returns>
		///  A service object of type serviceType.
		///</returns>
		///<param name = "serviceType">An object that specifies the type of service object to get. </param>
		public object GetService(Type serviceType)
		{
			if ((this as IKernelInternal).LazyLoadComponentByType(null, serviceType, null) == false)
			{
				return null;
			}

			return Resolve(serviceType);
		}

		///<summary>
		///  Gets the service object of the specified type.
		///</summary>
		///<returns>
		///  A service object of type serviceType.
		///</returns>
		public T GetService<T>() where T : class
		{
			return (T)GetService(typeof(T));
		}

		protected CreationContext CreateCreationContext(IHandler handler, Type typeToExtractGenericArguments, IDictionary additionalArguments, CreationContext parent)
		{
			return new CreationContext(handler,
			                           ReleasePolicy,
			                           typeToExtractGenericArguments,
			                           additionalArguments,
			                           ConversionSubSystem,
			                           parent);
		}

		protected void DisposeHandler(IHandler handler)
		{
			if (handler == null)
			{
				return;
			}

			if (handler is IDisposable)
			{
				((IDisposable)handler).Dispose();
			}
		}

		protected void RegisterHandler(String key, IHandler handler)
		{
			RegisterHandler(key, handler, false);
		}

		protected void RegisterHandler(String key, IHandler handler, bool skipRegistration)
		{
			if (!skipRegistration)
			{
				NamingSubSystem.Register(key, handler);
			}

			RaiseHandlerRegistered(handler);
			RaiseHandlersChanged();
			RaiseComponentRegistered(key, handler);
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
#if !SILVERLIGHT
			if (Debugger.IsAttached)
			{
				AddSubSystem(SubSystemConstants.DebuggingKey,
				             new DefaultDebuggingSubSystem());
			}
#endif
		}

		protected object ResolveComponent(IHandler handler)
		{
			return ResolveComponent(handler, handler.ComponentModel.Service);
		}

		protected object ResolveComponent(IHandler handler, Type service)
		{
			return ResolveComponent(handler, service, null);
		}

		protected object ResolveComponent(IHandler handler, IDictionary additionalArguments)
		{
			return ResolveComponent(handler, handler.ComponentModel.Service, additionalArguments);
		}

		protected object ResolveComponent(IHandler handler, Type service, IDictionary additionalArguments)
		{
			var parent = currentCreationContext;
			var context = CreateCreationContext(handler, service, additionalArguments, parent);
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

		protected object TryResolveComponent(IHandler handler, Type service, IDictionary additionalArguments)
		{
			var parent = currentCreationContext;
			var context = CreateCreationContext(handler, service, additionalArguments, parent);
			currentCreationContext = context;

			try
			{
				return handler.TryResolve(context);
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
			var nodes = GraphNodes;
			var vertices = TopologicalSortAlgo.Sort(nodes);

			for (var i = 0; i < vertices.Length; i++)
			{
				var model = (ComponentModel)vertices[i];

				// Prevent the removal of a component that belongs 
				// to other container
				if (!NamingSubSystem.Contains(model.Name))
				{
					continue;
				}

				RemoveComponent(model.Name);
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

		private bool LazyLoad(string key, Type service, IDictionary arguments)
		{
			if (isCheckingLazyLoaders)
			{
				return false;
			}

			isCheckingLazyLoaders = true;
			try
			{
				foreach (var loader in ResolveAll<ILazyComponentLoader>())
				{
					var registration = loader.Load(key, service, arguments);
					if (registration != null)
					{
						registration.Register(this);
						return true;
					}
				}
				return false;
			}
			finally
			{
				isCheckingLazyLoaders = false;
			}
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
			parentKernel.ComponentUnregistered += RaiseComponentUnregistered;
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
			parentKernel.ComponentUnregistered -= RaiseComponentUnregistered;
		}

#if !SILVERLIGHT
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			// NOTE: It's pointless to have this method if it doesn't do anything, ain't it?
		}
#endif

		[MethodImpl(MethodImplOptions.Synchronized)]
		bool IKernelInternal.LazyLoadComponentByKey(string key, Type service, IDictionary arguments)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (HasComponent(key))
			{
				return true;
			}

			return LazyLoad(key, service, arguments);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		bool IKernelInternal.LazyLoadComponentByType(string key, Type service, IDictionary arguments)
		{
			if (service == null)
			{
				throw new ArgumentNullException("service");
			}
			if (HasComponent(service))
			{
				return true;
			}

			return LazyLoad(key, service, arguments);
		}

		void IKernelInternal.RegisterHandlerForwarding(Type forwardedType, string name)
		{
			var target = GetHandler(name);
			if (target == null)
			{
				throw new InvalidOperationException("There is no handler named " + name);
			}

			var handler = HandlerFactory.CreateForwarding(target, forwardedType);
			RegisterHandler(name + ", ForwardedType=" + forwardedType.FullName, handler);
		}
	}
}