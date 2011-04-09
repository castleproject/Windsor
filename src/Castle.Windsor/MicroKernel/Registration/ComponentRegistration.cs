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

namespace Castle.MicroKernel.Registration
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.LifecycleConcerns;
	using Castle.MicroKernel.ModelBuilder;
	using Castle.MicroKernel.ModelBuilder.Descriptors;
	using Castle.MicroKernel.Registration.Interceptor;
	using Castle.MicroKernel.Registration.Lifestyle;
	using Castle.MicroKernel.Registration.Proxy;

	/// <summary>
	///   Registration for a single type as a component with the kernel.
	///   <para />
	///   You can create a new registration with the <see cref = "Component" /> factory.
	/// </summary>
	/// <typeparam name = "TService">The service type</typeparam>
	public class ComponentRegistration<TService> : IRegistration
		where TService : class
	{
		private readonly List<IComponentModelDescriptor> descriptors = new List<IComponentModelDescriptor>();
		private readonly List<Type> potentialServices = new List<Type>();

		private bool ifComponentRegisteredIgnore;
		private Type implementation;
		private ComponentName name;
		private bool overwrite;
		private bool registerNewServicesOnly;
		private bool registered;

		/// <summary>
		///   Initializes a new instance of the <see cref = "ComponentRegistration{TService}" /> class.
		/// </summary>
		public ComponentRegistration() : this(typeof(TService))
		{
		}

		/// <summary>
		///   Initializes a new instance of the <see cref = "ComponentRegistration{TService}" /> class.
		/// </summary>
		public ComponentRegistration(params Type[] services)
		{
			Forward(services);
		}

		/// <summary>
		///   The concrete type that implements the service.
		///   <para />
		///   To set the implementation, use <see cref = "ImplementedBy(System.Type)" />.
		/// </summary>
		/// <value>The implementation of the service.</value>
		public Type Implementation
		{
			get { return implementation; }
		}

		/// <summary>
		///   Set the lifestyle of this component.
		///   For example singleton and transient (also known as 'factory').
		/// </summary>
		/// <value>The with lifestyle.</value>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public LifestyleGroup<TService> LifeStyle
		{
			get { return new LifestyleGroup<TService>(this); }
		}

		/// <summary>
		///   The name of the component. Will become the key for the component in the kernel.
		///   <para />
		///   To set the name, use <see cref = "Named" />.
		///   <para />
		///   If not set, the <see cref = "Type.FullName" /> of the <see cref = "Implementation" />
		///   will be used as the key to register the component.
		/// </summary>
		/// <value>The name.</value>
		public String Name
		{
			get
			{
				if (name == null)
				{
					return null;
				}
				return name.Name;
			}
		}

		/// <summary>
		///   Set proxy for this component.
		/// </summary>
		/// <value>The proxy.</value>
		public ProxyGroup<TService> Proxy
		{
			get { return new ProxyGroup<TService>(this); }
		}

		protected internal IList<Type> Services
		{
			get { return potentialServices; }
		}

		protected internal int ServicesCount
		{
			get { return potentialServices.Count; }
		}

		internal bool IsOverWrite
		{
			get { return overwrite; }
		}

		/// <summary>
		///   Marks the components with one or more actors.
		/// </summary>
		/// <param name = "actors">The component actors.</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("If you're using WCF Facility use AsWcfClient/AsWcfService extension methods instead.")]
		public ComponentRegistration<TService> ActAs(params object[] actors)
		{
			foreach (var actor in actors)
			{
				if (actor != null)
				{
					DependsOn(Property.ForKey(Guid.NewGuid().ToString()).Eq(actor));
				}
			}
			return this;
		}

		/// <summary>
		///   Set a custom <see cref = "IComponentActivator" /> which creates and destroys the component.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> Activator<TActivator>() where TActivator : IComponentActivator
		{
			return AddAttributeDescriptor("componentActivatorType", typeof(TActivator).AssemblyQualifiedName);
		}

		/// <summary>
		///   Adds the attribute descriptor.
		/// </summary>
		/// <param name = "key">The key.</param>
		/// <param name = "value">The value.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> AddAttributeDescriptor(string key, string value)
		{
			AddDescriptor(new AttributeDescriptor<TService>(key, value));
			return this;
		}

		/// <summary>
		///   Adds the descriptor.
		/// </summary>
		/// <param name = "descriptor">The descriptor.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> AddDescriptor(IComponentModelDescriptor descriptor)
		{
			descriptors.Add(descriptor);
			var componentDescriptor = descriptor as AbstractOverwriteableDescriptor<TService>;
			if (componentDescriptor != null)
			{
				componentDescriptor.Registration = this;
			}
			return this;
		}

		/// <summary>
		///   Creates an attribute descriptor.
		/// </summary>
		/// <param name = "key">The attribute key.</param>
		/// <returns></returns>
		public AttributeKeyDescriptor<TService> Attribute(string key)
		{
			return new AttributeKeyDescriptor<TService>(this, key);
		}

		/// <summary>
		///   Apply more complex configuration to this component registration.
		/// </summary>
		/// <param name = "configNodes">The config nodes.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> Configuration(params Node[] configNodes)
		{
			return AddDescriptor(new ConfigurationDescriptor(configNodes));
		}

		/// <summary>
		///   Apply more complex configuration to this component registration.
		/// </summary>
		/// <param name = "configuration">The configuration <see cref = "MutableConfiguration" />.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> Configuration(IConfiguration configuration)
		{
			return AddDescriptor(new ConfigurationDescriptor(configuration));
		}

		/// <summary>
		///   Specify custom dependencies using <see cref = "Property.ForKey(string)" /> or <see
		///    cref = "Property.ForKey(System.Type)" />.
		///   <para />
		///   You can pass <see cref = "ServiceOverride" />s to specify the components
		///   this component should be resolved with.
		/// </summary>
		/// <param name = "dependencies">The dependencies.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> DependsOn(params Dependency[] dependencies)
		{
			if (dependencies == null || dependencies.Length == 0)
			{
				return this;
			}

			var serviceOverrides = dependencies.OfType<ServiceOverride>().ToArray();
			if (serviceOverrides.Length > 0)
			{
				AddDescriptor(new ServiceOverrideDescriptor(serviceOverrides));
				dependencies = dependencies.Except(serviceOverrides).ToArray();
			}
			var properties = dependencies.OfType<Property>().ToArray();
			if (properties.Length > 0)
			{
				AddDescriptor(new CustomDependencyDescriptor(properties));
				dependencies = dependencies.Except(properties).ToArray();
			}

			var parameters = dependencies.OfType<Parameter>().ToArray();
			if (parameters.Length > 0)
			{
				AddDescriptor(new ParametersDescriptor(parameters));
				dependencies = dependencies.Except(parameters).ToArray();
			}

			if (dependencies.Length > 0)
			{
				throw new ComponentRegistrationException(string.Format("Unrecognized dependencies: {0} only properties and service overrides are currently supported.",
				                                                       dependencies));
			}
			return this;
		}

		/// <summary>
		///   Uses a dictionary of key/value pairs, to specify custom dependencies.
		///   <para />
		/// </summary>
		/// <param name = "dependencies">The dependencies.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> DependsOn(IDictionary dependencies)
		{
			return AddDescriptor(new CustomDependencyDescriptor(dependencies));
		}

		/// <summary>
		///   Uses an (anonymous) object as a dictionary, to specify custom dependencies.
		///   <para />
		/// </summary>
		/// <param name = "anonymous">The dependencies.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> DependsOn(object anonymous)
		{
			return AddDescriptor(new CustomDependencyDescriptor(new ReflectionBasedDictionaryAdapter(anonymous)));
		}

		/// <summary>
		///   Allows custom dependencies to by defined dyncamically.
		/// </summary>
		/// <param name = "resolve">The delegate used for providing dynamic parameters.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> DependsOn(DynamicParametersDelegate resolve)
		{
			return DynamicParameters((k, c, d) =>
			{
				resolve(k, d);
				return null;
			});
		}

		/// <summary>
		///   Allows custom dependencies to by defined dynamically with releasing capability.
		/// </summary>
		/// <param name = "resolve">The delegate used for providing dynamic parameters.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> DependsOn(DynamicParametersResolveDelegate resolve)
		{
			return DynamicParameters((k, c, d) => resolve(k, d));
		}

		/// <summary>
		///   Allows custom dependencies to by defined dynamically with releasing capability.
		/// </summary>
		/// <param name = "resolve">The delegate used for providing dynamic parameters.</param>
		/// <returns></returns>
		/// <remarks>
		///   Use <see cref = "CreationContext" /> when resolving components from <see cref = "IKernel" /> in order to detect cycles.
		/// </remarks>
		public ComponentRegistration<TService> DependsOn(DynamicParametersWithContextResolveDelegate resolve)
		{
			AddDescriptor(new DynamicParametersDescriptor(resolve));
			return this;
		}

		/// <summary>
		///   Allows custom dependencies to by defined dyncamically.
		/// </summary>
		/// <param name = "resolve">The delegate used for providing dynamic parameters.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> DynamicParameters(DynamicParametersDelegate resolve)
		{
			return DynamicParameters((k, c, d) =>
			{
				resolve(k, d);
				return null;
			});
		}

		/// <summary>
		///   Allows custom dependencies to by defined dynamically with releasing capability.
		/// </summary>
		/// <param name = "resolve">The delegate used for providing dynamic parameters.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> DynamicParameters(DynamicParametersResolveDelegate resolve)
		{
			return DynamicParameters((k, c, d) => resolve(k, d));
		}

		/// <summary>
		///   Allows custom dependencies to by defined dynamically with releasing capability.
		/// </summary>
		/// <param name = "resolve">The delegate used for providing dynamic parameters.</param>
		/// <returns></returns>
		/// <remarks>
		///   Use <see cref = "CreationContext" /> when resolving components from <see cref = "IKernel" /> in order to detect cycles.
		/// </remarks>
		public ComponentRegistration<TService> DynamicParameters(DynamicParametersWithContextResolveDelegate resolve)
		{
			AddDescriptor(new DynamicParametersDescriptor(resolve));
			return this;
		}

		/// <summary>
		///   Sets <see cref = "ComponentModel.ExtendedProperties" /> for this component.
		/// </summary>
		/// <param name = "properties">The extended properties.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> ExtendedProperties(params Property[] properties)
		{
			return AddDescriptor(new ExtendedPropertiesDescriptor(properties));
		}

		/// <summary>
		///   Sets <see cref = "ComponentModel.ExtendedProperties" /> for this component.
		/// </summary>
		/// <param name = "anonymous">The extendend properties as key/value pairs.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> ExtendedProperties(object anonymous)
		{
			return AddDescriptor(new ExtendedPropertiesDescriptor(new ReflectionBasedDictionaryAdapter(anonymous)));
		}

		/// <summary>
		///   Adds <paramref name = "types" /> as additional services to be exposed by this component.
		/// </summary>
		/// <param name = "types">The types to forward.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> Forward(params Type[] types)
		{
			return Forward((IEnumerable<Type>)types);
		}

		/// <summary>
		///   Adds <typeparamref name = "TService2" /> as additional service to be exposed by this component.
		/// </summary>
		/// <typeparam name = "TService2">The forwarded type.</typeparam>
		/// <returns>The component registration.</returns>
		public ComponentRegistration<TService> Forward<TService2>()
		{
			return Forward(new[] { typeof(TService2) });
		}

		/// <summary>
		///   Adds <typeparamref name = "TService2" /> and <typeparamref name = "TService3" /> as additional services to be exposed by this component.
		/// </summary>
		/// <typeparam name = "TService2">The first forwarded type.</typeparam>
		/// <typeparam name = "TService3">The second forwarded type.</typeparam>
		/// <returns>The component registration.</returns>
		public ComponentRegistration<TService> Forward<TService2, TService3>()
		{
			return Forward(new[] { typeof(TService2), typeof(TService3) });
		}

		/// <summary>
		///   Adds <typeparamref name = "TService2" />, <typeparamref name = "TService3" /> and  <typeparamref name = "TService4" /> as additional services to be exposed by this component.
		/// </summary>
		/// <typeparam name = "TService2">The first forwarded type.</typeparam>
		/// <typeparam name = "TService3">The second forwarded type.</typeparam>
		/// <typeparam name = "TService4">The third forwarded type.</typeparam>
		/// <returns>The component registration.</returns>
		public ComponentRegistration<TService> Forward<TService2, TService3, TService4>()
		{
			return Forward(new[] { typeof(TService2), typeof(TService3), typeof(TService4) });
		}

		/// <summary>
		///   Adds <typeparamref name = "TService2" />, <typeparamref name = "TService3" />,<typeparamref name = "TService4" /> and  <typeparamref
		///     name = "TService5" /> as additional services to be exposed by this component.
		/// </summary>
		/// <typeparam name = "TService2">The first forwarded type.</typeparam>
		/// <typeparam name = "TService3">The second forwarded type.</typeparam>
		/// <typeparam name = "TService4">The third forwarded type.</typeparam>
		/// <typeparam name = "TService5">The fourth forwarded type.</typeparam>
		/// <returns>The component registration.</returns>
		public ComponentRegistration<TService> Forward<TService2, TService3, TService4, TService5>()
		{
			return Forward(new[] { typeof(TService2), typeof(TService3), typeof(TService4), typeof(TService5) });
		}

		/// <summary>
		///   Adds <paramref name = "types" /> as additional services to be exposed by this component.
		/// </summary>
		/// <param name = "types">The types to forward.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> Forward(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				ComponentServicesUtil.AddService(potentialServices, type);
			}
			return this;
		}

		/// <summary>
		///   Sets the concrete type that implements the service to <typeparamref name = "TImpl" />.
		///   <para />
		///   If not set, the class service type or first registered interface will be used as the implementation for this component.
		/// </summary>
		/// <typeparam name = "TImpl">The type that is the implementation for the service.</typeparam>
		/// <returns></returns>
		public ComponentRegistration<TService> ImplementedBy<TImpl>() where TImpl : TService
		{
			return ImplementedBy(typeof(TImpl));
		}

		/// <summary>
		///   Sets the concrete type that implements the service to <paramref name = "type" />.
		///   <para />
		///   If not set, the class service type or first registered interface will be used as the implementation for this component.
		/// </summary>
		/// <param name = "type">The type that is the implementation for the service.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> ImplementedBy(Type type)
		{
			return ImplementedBy(type, null);
		}

		/// <summary>
		///   Sets the concrete type that implements the service to <paramref name = "type" />.
		///   <para />
		///   If not set, the class service type or first registered interface will be used as the implementation for this component.
		/// </summary>
		/// <param name = "type">The type that is the implementation for the service.</param>
		/// <param name = "genericImplementationMatchingStrategy">Provides ability to close open generic service. Ignored when registering closed or non-generic component.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> ImplementedBy(Type type, IGenericImplementationMatchingStrategy genericImplementationMatchingStrategy)
		{
			if (implementation != null && implementation != typeof(LateBoundComponent))
			{
				var message = String.Format("This component has already been assigned implementation {0}",
				                            implementation.FullName);
				throw new ComponentRegistrationException(message);
			}

			implementation = type;
			if (genericImplementationMatchingStrategy == null)
			{
				return this;
			}
			return ExtendedProperties(Property.ForKey(ComponentModel.GenericImplementationMatchingStrategy).Eq(genericImplementationMatchingStrategy));
		}

		/// <summary>
		///   Assigns an existing instance as the component for this registration.
		/// </summary>
		/// <param name = "instance">The component instance.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> Instance(TService instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			return ImplementedBy(instance.GetType())
				.Activator<ExternalInstanceActivator>()
				.ExtendedProperties(Property.ForKey("instance").Eq(instance));
		}

		/// <summary>
		///   Set the interceptors for this component.
		/// </summary>
		/// <param name = "interceptors">The interceptors.</param>
		/// <returns></returns>
		public InterceptorGroup<TService> Interceptors(params InterceptorReference[] interceptors)
		{
			return new InterceptorGroup<TService>(this, interceptors);
		}

		/// <summary>
		///   Set the interceptors for this component.
		/// </summary>
		/// <param name = "interceptors">The interceptors.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> Interceptors(params Type[] interceptors)
		{
#if SILVERLIGHT
			var references = interceptors.Select(t => new InterceptorReference(t)).ToArray();
#else
			var references = Array.ConvertAll(interceptors, t => new InterceptorReference(t));
#endif
			return AddDescriptor(new InterceptorDescriptor(references));
		}

		/// <summary>
		///   Set the interceptor for this component.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> Interceptors<TInterceptor>() where TInterceptor : IInterceptor
		{
			return AddDescriptor(new InterceptorDescriptor(new[] { new InterceptorReference(typeof(TInterceptor)) }));
		}

		/// <summary>
		///   Set the interceptor for this component.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> Interceptors<TInterceptor1, TInterceptor2>()
			where TInterceptor1 : IInterceptor
			where TInterceptor2 : IInterceptor
		{
			return Interceptors<TInterceptor1>().Interceptors<TInterceptor2>();
		}

		/// <summary>
		///   Set the interceptor for this component.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> Interceptors(params string[] keys)
		{
#if SILVERLIGHT
			var interceptors = keys.Select(InterceptorReference.ForKey).ToArray();
#else
			var interceptors = Array.ConvertAll(keys, InterceptorReference.ForKey);
#endif
			return AddDescriptor(new InterceptorDescriptor(interceptors));
		}

		/// <summary>
		///   Sets component lifestyle to specified one.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> LifestyleCustom(Type customLifestyleType)
		{
			return LifeStyle.Custom(customLifestyleType);
		}

		/// <summary>
		///   Sets component lifestyle to specified one.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> LifestyleCustom<TLifestyleManager>() where TLifestyleManager : ILifestyleManager, new()
		{
			return LifeStyle.Custom<TLifestyleManager>();
		}

		/// <summary>
		///   Sets component lifestyle to per thread.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> LifestylePerThread()
		{
			return LifeStyle.PerThread;
		}

#if (!SILVERLIGHT)
		/// <summary>
		///   Sets component lifestyle to instance per web request.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> LifestylePerWebRequest()
		{
			return LifeStyle.PerWebRequest;
		}
#endif

		/// <summary>
		///   Sets component lifestyle to pooled. If <paramref name = "initialSize" /> or <paramref name = "maxSize" /> are not set default values will be used.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> LifestylePooled(int? initialSize = null, int? maxSize = null)
		{
			return LifeStyle.PooledWithSize(initialSize, maxSize);
		}

		/// <summary>
		///   Sets component lifestyle to singleton.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> LifestyleSingleton()
		{
			return LifeStyle.Singleton;
		}

		/// <summary>
		///   Sets component lifestyle to transient.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> LifestyleTransient()
		{
			return LifeStyle.Transient;
		}

		/// <summary>
		///   Set a name of this registration. This is required if you have multiple components for a given service and want to be able to resolve some specific ones. Then you'd provide the name so that Windsor knows which one of the bunch you know. Otherwise don't bother setting the name.
		///   <para />
		///   If not set, the <see cref = "Type.FullName" /> of the <see cref = "Implementation" />
		///   will be used as the key to register the component.
		/// </summary>
		/// <param name = "name">The name of this registration.</param>
		/// <returns></returns>
		/// <remarks>
		///   Names have to be globally unique in the scope of the container.
		/// </remarks>
		public ComponentRegistration<TService> Named(String name)
		{
			if (this.name != null)
			{
				var message = String.Format("This component has already been assigned name '{0}'", this.name.Name);
				throw new ComponentRegistrationException(message);
			}

			this.name = new ComponentName(name, true);
			return this;
		}

		/// <summary>
		///   This method as opposed to <see cref = "Named" /> should be used by tools like facilities when the name is not provided by the user, but autogenerated and user has no interest in seing this name, for example in diagnostics reports.
		///   Set a name of this registration. This is required if you have multiple components for a given service and want to be able to resolve some specific ones. Then you'd provide the name so that Windsor knows which one of the bunch you know. Otherwise don't bother setting the name.
		///   <para />
		///   If not set, the <see cref = "Type.FullName" /> of the <see cref = "Implementation" />
		///   will be used as the key to register the component.
		/// </summary>
		/// <param name = "name">The name of this registration.</param>
		/// <returns></returns>
		/// <remarks>
		///   Names have to be globally unique in the scope of the container.
		/// </remarks>
		public ComponentRegistration<TService> NamedAutomatically(String name)
		{
			if (this.name != null)
			{
				var message = String.Format("This component has already been assigned name '{0}'", this.name);
				throw new ComponentRegistrationException(message);
			}

			this.name = new ComponentName(name, false);
			return this;
		}

		/// <summary>
		///   Stores a set of <see cref = "LifecycleActionDelegate{T}" /> which will be invoked when the component
		///   is created and before it's returned from the container.
		/// </summary>
		/// <param name = "actions">A set of actions to be executed right after the component is created and before it's returned from the container.</param>
		public ComponentRegistration<TService> OnCreate(params LifecycleActionDelegate<TService>[] actions)
		{
			if (actions != null && actions.Length != 0)
			{
#if SILVERLIGHT
				var action = actions[0];
				for (int i = 1; i < actions.Length; i++)
				{
					action = (LifecycleActionDelegate<TService>)Delegate.Combine(action, actions[i]);
				}
#else
				var action = (LifecycleActionDelegate<TService>)Delegate.Combine(actions);
#endif
				AddDescriptor(new OnCreateComponentDescriptor<TService>(action));
			}
			return this;
		}

		/// <summary>
		///   Stores a set of <see cref = "LifecycleActionDelegate{T}" /> which will be invoked when the component
		///   is destroyed which means when it's released or it's lifetime scope ends. Notice that usage of this method will cause instsances of the component to be tracked, even if they wouldn't be otherwise.
		/// </summary>
		/// <param name = "actions">A set of actions to be executed when the component is destroyed.</param>
		public ComponentRegistration<TService> OnDestroy(params LifecycleActionDelegate<TService>[] actions)
		{
			if (actions != null && actions.Length != 0)
			{
#if SILVERLIGHT
				var action = actions[0];
				for (int i = 1; i < actions.Length; i++)
				{
					action = (LifecycleActionDelegate<TService>)Delegate.Combine(action, actions[i]);
				}
#else
				var action = (LifecycleActionDelegate<TService>)Delegate.Combine(actions);
#endif
				AddDescriptor(new OnDestroyComponentDescriptor<TService>(action));
			}
			return this;
		}

		/// <summary>
		///   Services that are already present in the container will be skipped. If no new service is left the registration will not happen at all.
		/// </summary>
		/// <returns></returns>
		public ComponentRegistration<TService> OnlyNewServices()
		{
			registerNewServicesOnly = true;
			return this;
		}

		/// <summary>
		///   With the overwrite.
		/// </summary>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ComponentRegistration<TService> OverWrite()
		{
			overwrite = true;
			return this;
		}

		/// <summary>
		///   Set configuration parameters with string or <see cref = "IConfiguration" /> values.
		/// </summary>
		/// <param name = "parameters">The parameters.</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use DependsOn(Dependency.OnConfigValue()) or Dependency.OnValue() instead")]
		public ComponentRegistration<TService> Parameters(params Parameter[] parameters)
		{
			return AddDescriptor(new ParametersDescriptor(parameters));
		}

		/// <summary>
		///   Sets the interceptor selector for this component.
		/// </summary>
		/// <param name = "selector"></param>
		/// <returns></returns>
		public ComponentRegistration<TService> SelectInterceptorsWith(IInterceptorSelector selector)
		{
			return SelectInterceptorsWith(s => s.Instance(selector));
		}

		/// <summary>
		///   Sets the interceptor selector for this component.
		/// </summary>
		/// <param name = "selector"></param>
		/// <returns></returns>
		public ComponentRegistration<TService> SelectInterceptorsWith(Action<ItemRegistration<IInterceptorSelector>> selector)
		{
			var registration = new ItemRegistration<IInterceptorSelector>("interceptor-selector");
			selector.Invoke(registration);
			return AddDescriptor(new InterceptorSelectorDescriptor(registration.Item));
		}

		/// <summary>
		///   Override (some of) the services that this component needs.
		///   Use <see cref = "ServiceOverride.ForKey(string)" /> to create an override.
		///   <para />
		///   Each key represents the service dependency of this component, for example the name of a constructor argument or a property.
		///   The corresponding value is the key of an other component registered to the kernel, and is used to resolve the dependency.
		///   <para />
		///   To specify dependencies which are not services, use <see cref = "DependsOn(Dependency[])" />
		/// </summary>
		/// <param name = "overrides">The service overrides.</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use DependsOn(Dependency.OnComponent()) instead")]
		public ComponentRegistration<TService> ServiceOverrides(params ServiceOverride[] overrides)
		{
			return AddDescriptor(new ServiceOverrideDescriptor(overrides));
		}

		/// <summary>
		///   Override (some of) the services that this component needs, using a dictionary.
		///   <para />
		///   Each key represents the service dependency of this component, for example the name of a constructor argument or a property.
		///   The corresponding value is the key of an other component registered to the kernel, and is used to resolve the dependency.
		///   <para />
		///   To specify dependencies which are not services, use <see cref = "DependsOn(IDictionary)" />
		/// </summary>
		/// <param name = "overrides">The service overrides.</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use DependsOn(Dependency.OnComponent()) instead")]
		public ComponentRegistration<TService> ServiceOverrides(IDictionary overrides)
		{
			return AddDescriptor(new ServiceOverrideDescriptor(overrides));
		}

		/// <summary>
		///   Override (some of) the services that this component needs, using an (anonymous) object as a dictionary.
		///   <para />
		///   Each key represents the service dependency of this component, for example the name of a constructor argument or a property.
		///   The corresponding value is the key of an other component registered to the kernel, and is used to resolve the dependency.
		///   <para />
		///   To specify dependencies which are not services, use <see cref = "DependsOn(object)" />
		/// </summary>
		/// <param name = "anonymous">The service overrides.</param>
		/// <returns></returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use DependsOn(Dependency.OnComponent()) instead")]
		public ComponentRegistration<TService> ServiceOverrides(object anonymous)
		{
			return AddDescriptor(new ServiceOverrideDescriptor(new ReflectionBasedDictionaryAdapter(anonymous)));
		}

		/// <summary>
		///   Uses a factory to instantiate the component
		/// </summary>
		/// <typeparam name = "TFactory">Factory type. This factory has to be registered in the kernel.</typeparam>
		/// <typeparam name = "TServiceImpl">Implementation type.</typeparam>
		/// <param name = "factory">Factory invocation</param>
		/// <returns></returns>
		public ComponentRegistration<TService> UsingFactory<TFactory, TServiceImpl>(Converter<TFactory, TServiceImpl> factory) where TServiceImpl : TService
		{
			return UsingFactoryMethod(kernel => factory.Invoke(kernel.Resolve<TFactory>()));
		}

		/// <summary>
		///   Uses a factory method to instantiate the component.
		/// </summary>
		/// <typeparam name = "TImpl">Implementation type</typeparam>
		/// <param name = "factoryMethod">Factory method</param>
		/// <param name = "managedExternally">When set to <c>true</c> container will not assume ownership of this component, will not track it not apply and lifecycle concerns to it.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> UsingFactoryMethod<TImpl>(Func<TImpl> factoryMethod, bool managedExternally = false) where TImpl : TService
		{
			return UsingFactoryMethod((k, m, c) => factoryMethod(), managedExternally);
		}

		/// <summary>
		///   Uses a factory method to instantiate the component.
		/// </summary>
		/// <typeparam name = "TImpl">Implementation type</typeparam>
		/// <param name = "factoryMethod">Factory method</param>
		/// <param name = "managedExternally">When set to <c>true</c> container will not assume ownership of this component, will not track it not apply and lifecycle concerns to it.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> UsingFactoryMethod<TImpl>(Converter<IKernel, TImpl> factoryMethod, bool managedExternally = false)
			where TImpl : TService
		{
			return UsingFactoryMethod((k, m, c) => factoryMethod(k), managedExternally);
		}

		/// <summary>
		///   Uses a factory method to instantiate the component.
		/// </summary>
		/// <typeparam name = "TImpl">Implementation type</typeparam>
		/// <param name = "factoryMethod">Factory method</param>
		/// <param name = "managedExternally">When set to <c>true</c> container will not assume ownership of this component, will not track it not apply and lifecycle concerns to it.</param>
		/// <returns></returns>
		public ComponentRegistration<TService> UsingFactoryMethod<TImpl>(Func<IKernel, ComponentModel, CreationContext, TImpl> factoryMethod,
		                                                                 bool managedExternally = false)
			where TImpl : TService
		{
			Activator<FactoryMethodActivator<TImpl>>()
				.ExtendedProperties(Property.ForKey("factoryMethodDelegate").Eq(factoryMethod));

			if (managedExternally)
			{
				ExtendedProperties(Property.ForKey("factory.managedExternally").Eq(managedExternally));
			}

			if (implementation == null && (potentialServices.First().IsClass == false || potentialServices.First().IsSealed == false))
			{
				implementation = typeof(LateBoundComponent);
			}
			return this;
		}

		/// <summary>
		///   Uses a factory method to instantiate the component.
		/// </summary>
		/// <typeparam name = "TImpl">Implementation type</typeparam>
		/// <param name = "factoryMethod">Factory method</param>
		/// <returns></returns>
		public ComponentRegistration<TService> UsingFactoryMethod<TImpl>(Func<IKernel, CreationContext, TImpl> factoryMethod) where TImpl : TService
		{
			return UsingFactoryMethod((k, m, c) => factoryMethod(k, c));
		}

		internal void RegisterOptionally()
		{
			ifComponentRegisteredIgnore = true;
		}

		private Type[] FilterServices(IKernel kernel)
		{
			var services = new List<Type>(potentialServices);
			if (registerNewServicesOnly)
			{
#if SILVERLIGHT
				services.ToArray().Where(kernel.HasComponent).ForEach(t => services.Remove(t));
#else
				services.RemoveAll(kernel.HasComponent);
#endif
			}
			return services.ToArray();
		}

		private IComponentModelDescriptor[] GetContributors(Type[] services)
		{
			var list = new List<IComponentModelDescriptor>
			{
				new ServicesDescriptor(services),
				new DefaultsDescriptor(name, implementation),
			};
			list.AddRange(descriptors);
			list.Add(new InterfaceProxyDescriptor());
			return list.ToArray();
		}

		private IKernelInternal GetInternalKernel(IKernel kernel)
		{
			var internalKernel = kernel as IKernelInternal;
			if (internalKernel == null)
			{
				throw new ArgumentException(
					string.Format("The kernel does not implement {0}.", typeof(IKernelInternal)), "kernel");
			}
			return internalKernel;
		}

		private bool SkipRegistration(IKernelInternal internalKernel, ComponentModel componentModel)
		{
			return ifComponentRegisteredIgnore && internalKernel.HasComponent(componentModel.Name);
		}

		/// <summary>
		///   Registers this component with the <see cref = "IKernel" />.
		/// </summary>
		/// <param name = "kernel">The kernel.</param>
		void IRegistration.Register(IKernel kernel)
		{
			if (registered)
			{
				return;
			}
			registered = true;
			var services = FilterServices(kernel);
			if (services.Length == 0)
			{
				return;
			}

			var internalKernel = GetInternalKernel(kernel);
			var componentModel = kernel.ComponentModelFactory.BuildModel(GetContributors(services));
			if (SkipRegistration(internalKernel, componentModel))
			{
				return;
			}
			internalKernel.AddCustomComponent(componentModel);
		}
	}
}