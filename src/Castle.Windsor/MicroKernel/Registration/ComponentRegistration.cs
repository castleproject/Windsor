// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.LifecycleConcerns;
	using Castle.MicroKernel.Lifestyle.Scoped;
	using Castle.MicroKernel.ModelBuilder;
	using Castle.MicroKernel.ModelBuilder.Descriptors;
	using Castle.MicroKernel.Registration.Interceptor;
	using Castle.MicroKernel.Registration.Lifestyle;
	using Castle.MicroKernel.Registration.Proxy;

	/// <summary>
	/// Registration for a single type as a component with the kernel.
	///     <para />
	/// You can create a new registration with the <see cref = "Component" /> factory.
	/// </summary>
	/// <typeparam name = "TService"> The service type </typeparam>
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
		/// Initializes a new instance of the <see cref = "ComponentRegistration{TService}" /> class.
		/// </summary>
		public ComponentRegistration() : this(typeof(TService))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref = "ComponentRegistration{TService}" /> class.
		/// </summary>
		public ComponentRegistration(params Type[] services)
		{
			Forward(services);
		}

		/// <summary>
		/// The concrete type that implements the service.
		///     <para />
		/// To set the implementation, use <see cref = "ImplementedBy(System.Type)" /> .
		/// </summary>
		/// <value> The implementation of the service. </value>
		public Type Implementation
		{
			get { return implementation; }
		}

		/// <summary>
		/// Set the lifestyle of this component. For example singleton and transient (also known as 'factory').
		/// </summary>
		/// <value> The with lifestyle. </value>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public LifestyleGroup<TService> LifeStyle
		{
			get { return new LifestyleGroup<TService>(this); }
		}

		/// <summary>
		/// The name of the component. Will become the key for the component in the kernel.
		///     <para />
		/// To set the name, use <see cref = "Named" /> .
		///     <para />
		/// If not set, the <see cref = "Type.FullName" /> of the <see cref = "Implementation" /> will be used as the key to register the component.
		/// </summary>
		/// <value> The name. </value>
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
		/// Set proxy for this component.
		/// </summary>
		/// <value> The proxy. </value>
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
		/// Set a custom <see cref = "IComponentActivator" /> which creates and destroys the component.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> Activator<TActivator>() where TActivator : IComponentActivator
		{
			return AddAttributeDescriptor("componentActivatorType", typeof(TActivator).AssemblyQualifiedName);
		}

		/// <summary>
		/// Adds the attribute descriptor.
		/// </summary>
		/// <param name = "key"> The key. </param>
		/// <param name = "value"> The value. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> AddAttributeDescriptor(string key, string value)
		{
			AddDescriptor(new AttributeDescriptor<TService>(key, value));
			return this;
		}

		/// <summary>
		/// Adds the descriptor.
		/// </summary>
		/// <param name = "descriptor"> The descriptor. </param>
		/// <returns> </returns>
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
		/// Creates an attribute descriptor.
		/// </summary>
		/// <param name = "key"> The attribute key. </param>
		/// <returns> </returns>
		public AttributeKeyDescriptor<TService> Attribute(string key)
		{
			return new AttributeKeyDescriptor<TService>(this, key);
		}

		/// <summary>
		/// Apply more complex configuration to this component registration.
		/// </summary>
		/// <param name = "configNodes"> The config nodes. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> Configuration(params Node[] configNodes)
		{
			return AddDescriptor(new ConfigurationDescriptor(configNodes));
		}

		/// <summary>
		/// Apply more complex configuration to this component registration.
		/// </summary>
		/// <param name = "configuration"> The configuration <see cref = "MutableConfiguration" /> . </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> Configuration(IConfiguration configuration)
		{
			return AddDescriptor(new ConfigurationDescriptor(configuration));
		}

		/// <summary>
		/// Defines additional dependencies for the component. Those can be any of <see cref = "ServiceOverride" />, <see cref = "Property" /> and <see cref = "Parameter" />. Use the static methods on
		///     <see cref = "Dependency" /> class to define the dependencies. See the example attached.
		/// </summary>
		/// <param name = "dependency"> The dependency. </param>
		/// <returns> </returns>
		/// <example>
		/// Artificial example showing how to specify a service override. See other methods on <see cref = "Dependency" /> class for more options.
		///     <code>DependsOn(Dependency.OnComponent(typeof(IRepository), typeof(IntranetRepository)));</code>
		/// </example>
		public ComponentRegistration<TService> DependsOn(Dependency dependency)
		{
			return DependsOn(new[] { dependency });
		}

		/// <summary>
		/// Defines additional dependencies for the component. Those can be any combibation of <see cref = "ServiceOverride" />, <see cref = "Property" /> and <see cref = "Parameter" />. Use the static methods
		/// on <see cref = "Dependency" /> class to define the dependencies. See the example attached.
		/// </summary>
		/// <param name = "dependencies"> The dependencies. </param>
		/// <returns> </returns>
		/// <example>
		/// Artificial example showing how to specify three different dependencies. If any of the methods shown is not self explanatory consult its documentation.
		///     <code>DependsOn(Dependency.OnAppSettingsValue("connectionString", "intranet-connection-string"),
		/// 		Dependency.OnComponent(typeof(IRepository), typeof(IntranetRepository)),
		/// 		Dependency.OnValue("applicationName", "My Application"));</code>
		/// </example>
		public ComponentRegistration<TService> DependsOn(params Dependency[] dependencies)
		{
			if (dependencies == null || dependencies.Length == 0)
			{
				return this;
			}
			var serviceOverrides = new List<ServiceOverride>(dependencies.Length);
			var properties = new List<Property>(dependencies.Length);
			var parameters = new List<Parameter>(dependencies.Length);
			foreach (var dependency in dependencies)
			{
				if (dependency.Accept(properties))
				{
					continue;
				}
				if (dependency.Accept(parameters))
				{
					continue;
				}
				if (dependency.Accept(serviceOverrides))
				{
					continue;
				}
			}

			if (serviceOverrides.Count > 0)
			{
				AddDescriptor(new ServiceOverrideDescriptor(serviceOverrides.ToArray()));
			}
			if (properties.Count > 0)
			{
				AddDescriptor(new CustomDependencyDescriptor(properties.ToArray()));
			}

			if (parameters.Count > 0)
			{
				AddDescriptor(new ParametersDescriptor(parameters.ToArray()));
			}
			return this;
		}

		/// <summary>
		/// Uses a dictionary of key/value pairs, to specify custom dependencies.
		/// </summary>
		public ComponentRegistration<TService> DependsOn(Arguments dependencies)
		{
			return AddDescriptor(new CustomDependencyDescriptor(dependencies));
		}

		/// <summary>
		/// Uses a dictionary of key/value pairs, to specify custom dependencies.
		/// </summary>
		public ComponentRegistration<TService> DependsOn(IDictionary dependencies)
		{
			var arguments = new Arguments();
			foreach (DictionaryEntry item in dependencies)
			{
				arguments.Add(item.Key, item.Value);
			}
			return DependsOn(arguments);
		}

		/// <summary>
		/// Uses an (anonymous) object as a dictionary, to specify custom dependencies.
		/// </summary>
		public ComponentRegistration<TService> DependsOn(object dependenciesAsAnonymousType)
		{
			return DependsOn(new ReflectionBasedDictionaryAdapter(dependenciesAsAnonymousType));
		}

		/// <summary>
		/// Allows custom dependencies to by defined dyncamically. Calling this overload is synonymous to using <see cref = "DynamicParameters(Castle.MicroKernel.Registration.DynamicParametersDelegate)" />
		/// </summary>
		/// <param name = "resolve"> The delegate used for providing dynamic parameters. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> DependsOn(DynamicParametersDelegate resolve)
		{
			return DynamicParameters((k, c, d) =>
			{
				resolve(k, d);
				return null;
			});
		}

		/// <summary>
		/// Allows custom dependencies to by defined dynamically with releasing capability. Calling this overload is synonymous to using
		///     <see cref = "DynamicParameters(Castle.MicroKernel.Registration.DynamicParametersResolveDelegate)" />
		/// </summary>
		/// <param name = "resolve"> The delegate used for providing dynamic parameters. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> DependsOn(DynamicParametersResolveDelegate resolve)
		{
			return DynamicParameters((k, c, d) => resolve(k, d));
		}

		/// <summary>
		/// Allows custom dependencies to by defined dynamically with releasing capability. Calling this overload is synonymous to using
		///     <see cref = "DynamicParameters(Castle.MicroKernel.Registration.DynamicParametersWithContextResolveDelegate)" />
		/// </summary>
		/// <param name = "resolve"> The delegate used for providing dynamic parameters. </param>
		/// <returns> </returns>
		/// <remarks>
		/// Use <see cref = "CreationContext" /> when resolving components from <see cref = "IKernel" /> in order to detect cycles.
		/// </remarks>
		public ComponentRegistration<TService> DependsOn(DynamicParametersWithContextResolveDelegate resolve)
		{
			AddDescriptor(new DynamicParametersDescriptor(resolve));
			return this;
		}

		/// <summary>
		/// Allows custom dependencies to by defined dynamically.
		/// </summary>
		/// <param name = "resolve"> The delegate used for providing dynamic parameters. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> DynamicParameters(DynamicParametersDelegate resolve)
		{
			return DynamicParameters((k, c, d) =>
			{
				resolve(k, d);
				return null;
			});
		}

		/// <summary>
		/// Allows custom dependencies to by defined dynamically with releasing capability.
		/// </summary>
		/// <param name = "resolve"> The delegate used for providing dynamic parameters. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> DynamicParameters(DynamicParametersResolveDelegate resolve)
		{
			return DynamicParameters((k, c, d) => resolve(k, d));
		}

		/// <summary>
		/// Allows custom dependencies to by defined dynamically with releasing capability.
		/// </summary>
		/// <param name = "resolve"> The delegate used for providing dynamic parameters. </param>
		/// <returns> </returns>
		/// <remarks>
		/// Use <see cref = "CreationContext" /> when resolving components from <see cref = "IKernel" /> in order to detect cycles.
		/// </remarks>
		public ComponentRegistration<TService> DynamicParameters(DynamicParametersWithContextResolveDelegate resolve)
		{
			AddDescriptor(new DynamicParametersDescriptor(resolve));
			return this;
		}

		/// <summary>
		/// Sets <see cref = "ComponentModel.ExtendedProperties" /> for this component.
		/// </summary>
		/// <param name = "properties"> The extended properties. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> ExtendedProperties(params Property[] properties)
		{
			return AddDescriptor(new ExtendedPropertiesDescriptor(properties));
		}

		/// <summary>
		/// Sets <see cref = "ComponentModel.ExtendedProperties" /> for this component.
		/// </summary>
		/// <param name = "property"> The extended properties. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> ExtendedProperties(Property property)
		{
			return ExtendedProperties(new[] { property });
		}

		/// <summary>
		/// Sets <see cref = "ComponentModel.ExtendedProperties" /> for this component.
		/// </summary>
		/// <param name = "anonymous"> The extendend properties as key/value pairs. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> ExtendedProperties(object anonymous)
		{
			return AddDescriptor(new ExtendedPropertiesDescriptor(new ReflectionBasedDictionaryAdapter(anonymous)));
		}

		/// <summary>
		/// Adds <paramref name = "types" /> as additional services to be exposed by this component.
		/// </summary>
		/// <param name = "types"> The types to forward. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> Forward(params Type[] types)
		{
			return Forward((IEnumerable<Type>)types);
		}

		/// <summary>
		/// Adds <typeparamref name = "TService2" /> as additional service to be exposed by this component.
		/// </summary>
		/// <typeparam name = "TService2"> The forwarded type. </typeparam>
		/// <returns> The component registration. </returns>
		public ComponentRegistration<TService> Forward<TService2>()
		{
			return Forward(new[] { typeof(TService2) });
		}

		/// <summary>
		/// Adds <typeparamref name = "TService2" /> and <typeparamref name = "TService3" /> as additional services to be exposed by this component.
		/// </summary>
		/// <typeparam name = "TService2"> The first forwarded type. </typeparam>
		/// <typeparam name = "TService3"> The second forwarded type. </typeparam>
		/// <returns> The component registration. </returns>
		public ComponentRegistration<TService> Forward<TService2, TService3>()
		{
			return Forward(new[] { typeof(TService2), typeof(TService3) });
		}

		/// <summary>
		/// Adds <typeparamref name = "TService2" /> , <typeparamref name = "TService3" /> and <typeparamref name = "TService4" /> as additional services to be exposed by this component.
		/// </summary>
		/// <typeparam name = "TService2"> The first forwarded type. </typeparam>
		/// <typeparam name = "TService3"> The second forwarded type. </typeparam>
		/// <typeparam name = "TService4"> The third forwarded type. </typeparam>
		/// <returns> The component registration. </returns>
		public ComponentRegistration<TService> Forward<TService2, TService3, TService4>()
		{
			return Forward(new[] { typeof(TService2), typeof(TService3), typeof(TService4) });
		}

		/// <summary>
		/// Adds <typeparamref name = "TService2" /> , <typeparamref name = "TService3" /> , <typeparamref name = "TService4" /> and <typeparamref name = "TService5" /> as additional services to be exposed by
		/// this component.
		/// </summary>
		/// <typeparam name = "TService2"> The first forwarded type. </typeparam>
		/// <typeparam name = "TService3"> The second forwarded type. </typeparam>
		/// <typeparam name = "TService4"> The third forwarded type. </typeparam>
		/// <typeparam name = "TService5"> The fourth forwarded type. </typeparam>
		/// <returns> The component registration. </returns>
		public ComponentRegistration<TService> Forward<TService2, TService3, TService4, TService5>()
		{
			return Forward(new[] { typeof(TService2), typeof(TService3), typeof(TService4), typeof(TService5) });
		}

		/// <summary>
		/// Adds <paramref name = "types" /> as additional services to be exposed by this component.
		/// </summary>
		/// <param name = "types"> The types to forward. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> Forward(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				ComponentServicesUtil.AddService(potentialServices, type);
			}
			return this;
		}

		/// <summary>
		/// Sets the concrete type that implements the service to <typeparamref name = "TImpl" /> .
		///     <para />
		/// If not set, the class service type or first registered interface will be used as the implementation for this component.
		/// </summary>
		/// <typeparam name = "TImpl"> The type that is the implementation for the service. </typeparam>
		/// <returns> </returns>
		public ComponentRegistration<TService> ImplementedBy<TImpl>() where TImpl : TService
		{
			return ImplementedBy(typeof(TImpl));
		}

		/// <summary>
		/// Sets the concrete type that implements the service to <paramref name = "type" /> .
		///     <para />
		/// If not set, the class service type or first registered interface will be used as the implementation for this component.
		/// </summary>
		/// <param name = "type"> The type that is the implementation for the service. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> ImplementedBy(Type type)
		{
			return ImplementedBy(type, null, null);
		}

		/// <summary>
		/// Sets the concrete type that implements the service to <paramref name = "type" /> .
		///     <para />
		/// If not set, the class service type or first registered interface will be used as the implementation for this component.
		/// </summary>
		/// <param name = "type"> The type that is the implementation for the service. </param>
		/// <param name = "genericImplementationMatchingStrategy"> Provides ability to close open generic service. Ignored when registering closed or non-generic component. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> ImplementedBy(Type type, IGenericImplementationMatchingStrategy genericImplementationMatchingStrategy)
		{
			return ImplementedBy(type, genericImplementationMatchingStrategy, null);
		}

		/// <summary>
		/// Sets the concrete type that implements the service to <paramref name = "type" /> .
		///     <para />
		/// If not set, the class service type or first registered interface will be used as the implementation for this component.
		/// </summary>
		/// <param name = "type"> The type that is the implementation for the service. </param>
		/// <param name = "genericServiceStrategy"> Provides ability to select if open generic component supports particular closed version of a service. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> ImplementedBy(Type type, IGenericServiceStrategy genericServiceStrategy)
		{
			return ImplementedBy(type, null, genericServiceStrategy);
		}

		/// <summary>
		/// Sets the concrete type that implements the service to <paramref name = "type" /> .
		///     <para />
		/// If not set, the class service type or first registered interface will be used as the implementation for this component.
		/// </summary>
		/// <param name = "type"> The type that is the implementation for the service. </param>
		/// <param name = "genericImplementationMatchingStrategy"> Provides ability to close open generic service. Ignored when registering closed or non-generic component. </param>
		/// <param name = "genericServiceStrategy"> Provides ability to select if open generic component supports particular closed version of a service. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> ImplementedBy(Type type, IGenericImplementationMatchingStrategy genericImplementationMatchingStrategy, IGenericServiceStrategy genericServiceStrategy)
		{
			if (implementation != null && implementation != typeof(LateBoundComponent))
			{
				var message = String.Format("This component has already been assigned implementation {0}",
				                            implementation.FullName);
				throw new ComponentRegistrationException(message);
			}

			implementation = type;
			if (genericImplementationMatchingStrategy != null)
			{
				ExtendedProperties(Property.ForKey(Constants.GenericImplementationMatchingStrategy).Eq(genericImplementationMatchingStrategy));
			}
			if (genericServiceStrategy != null)
			{
				ExtendedProperties(Property.ForKey(Constants.GenericServiceStrategy).Eq(genericServiceStrategy));
			}
			return this;
		}

		/// <summary>
		/// Assigns an existing instance as the component for this registration.
		/// </summary>
		/// <param name = "instance"> The component instance. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> Instance(TService instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}
			return ImplementedBy(instance.GetType())
				.Activator<ExternalInstanceActivator>()
				.ExtendedProperties(Property.ForKey("instance").Eq(instance));
		}

		/// <summary>
		/// Set the interceptors for this component.
		/// </summary>
		/// <param name = "interceptors"> The interceptors. </param>
		/// <returns> </returns>
		public InterceptorGroup<TService> Interceptors(params InterceptorReference[] interceptors)
		{
			return new InterceptorGroup<TService>(this, interceptors);
		}

		/// <summary>
		/// Set the interceptors for this component.
		/// </summary>
		/// <param name = "interceptors"> The interceptors. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> Interceptors(params Type[] interceptors)
		{
			var references = interceptors.ConvertAll(t => new InterceptorReference(t));
			return AddDescriptor(new InterceptorDescriptor(references));
		}

		/// <summary>
		/// Set the interceptor for this component.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> Interceptors<TInterceptor>() where TInterceptor : IInterceptor
		{
			return AddDescriptor(new InterceptorDescriptor(new[] { new InterceptorReference(typeof(TInterceptor)) }));
		}

		/// <summary>
		/// Set the interceptor for this component.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> Interceptors<TInterceptor1, TInterceptor2>()
			where TInterceptor1 : IInterceptor
			where TInterceptor2 : IInterceptor
		{
			return Interceptors<TInterceptor1>().Interceptors<TInterceptor2>();
		}

		/// <summary>
		/// Set the interceptor for this component.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> Interceptors(params string[] keys)
		{
			var interceptors = keys.ConvertAll(InterceptorReference.ForKey);
			return AddDescriptor(new InterceptorDescriptor(interceptors));
		}

		/// <summary>
		/// Sets component lifestyle to specified one.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestyleCustom(Type customLifestyleType)
		{
			return LifeStyle.Custom(customLifestyleType);
		}

		/// <summary>
		/// Sets component lifestyle to specified one.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestyleCustom<TLifestyleManager>()
			where TLifestyleManager : ILifestyleManager, new()
		{
			return LifeStyle.Custom<TLifestyleManager>();
		}

		/// <summary>
		/// Sets component lifestyle to per thread.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestylePerThread()
		{
			return LifeStyle.PerThread;
		}

		/// <summary>
		/// Sets component lifestyle to scoped per explicit scope. If <paramref name = "scopeAccessorType" /> is provided, it will be used to access scope for the component. Otherwise the default scope accessor
		/// will be used.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestyleScoped(Type scopeAccessorType = null)
		{
			return LifeStyle.Scoped(scopeAccessorType);
		}

		/// <summary>
		/// Sets component lifestyle to scoped per explicit scope.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestyleScoped<TScopeAccessor>() where TScopeAccessor : IScopeAccessor, new()
		{
			return LifestyleScoped(typeof(TScopeAccessor));
		}

		/// <summary>
		/// Sets component lifestyle to scoped per farthest component on the resolution stack where implementation type is assignable to <typeparamref name = "TBaseForRoot" /> .
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestyleBoundTo<TBaseForRoot>() where TBaseForRoot : class
		{
			return LifeStyle.BoundTo<TBaseForRoot>();
		}

		/// <summary>
		/// Sets component lifestyle to scoped per nearest component on the resolution stack where implementation type is assignable to <typeparamref name = "TBaseForRoot" /> .
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestyleBoundToNearest<TBaseForRoot>() where TBaseForRoot : class
		{
			return LifeStyle.BoundToNearest<TBaseForRoot>();
		}

		/// <summary>
		/// Sets component lifestyle to scoped per scope determined by <paramref name = "scopeRootBinder" />
		/// </summary>
		/// <param name = "scopeRootBinder"> Custom algorithm for selection which component higher up the resolution stack should be the root of the lifetime scope for current component's instances. The delegate
		/// will be invoked when current component is about to be resolved and will be passed set of handlers to components higher up the resolution stack. It ought to return one which it designages as the root
		/// which shall scope the lifetime of current component's instance, or <c>null</c> </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestyleBoundTo(Func<IHandler[], IHandler> scopeRootBinder)
		{
			return LifeStyle.BoundTo(scopeRootBinder);
		}

		/// <summary>
		/// Sets component lifestyle to pooled. If <paramref name = "initialSize" /> or <paramref name = "maxSize" /> are not set default values will be used.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestylePooled(int? initialSize = null, int? maxSize = null)
		{
			return LifeStyle.PooledWithSize(initialSize, maxSize);
		}

		/// <summary>
		/// Sets component lifestyle to singleton.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestyleSingleton()
		{
			return LifeStyle.Singleton;
		}

		/// <summary>
		/// Sets component lifestyle to transient.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> LifestyleTransient()
		{
			return LifeStyle.Transient;
		}

		/// <summary>
		/// Set a name of this registration. This is required if you have multiple components for a given service and want to be able to resolve some specific ones. Then you'd provide the name so that Windsor
		/// knows which one of the bunch you know. Otherwise don't bother setting the name.
		///     <para />
		/// If not set, the <see cref = "Type.FullName" /> of the <see cref = "Implementation" /> will be used as the key to register the component.
		/// </summary>
		/// <param name = "name"> The name of this registration. </param>
		/// <returns> </returns>
		/// <remarks>
		/// Names have to be globally unique in the scope of the container.
		/// </remarks>
		public ComponentRegistration<TService> Named(String name)
		{
			if (this.name != null)
			{
				var message = String.Format("This component has already been assigned name '{0}'", this.name.Name);
				throw new ComponentRegistrationException(message);
			}
			if (name == null)
			{
				return this;
			}

			this.name = new ComponentName(name, true);
			return this;
		}

		/// <summary>
		/// This method as opposed to <see cref = "Named" /> should be used by tools like facilities when the name is not provided by the user, but autogenerated and user has no interest in seing this name, for
		/// example in diagnostics reports. Set a name of this registration. This is required if you have multiple components for a given service and want to be able to resolve some specific ones. Then you'd
		/// provide the name so that Windsor knows which one of the bunch you know. Otherwise don't bother setting the name.
		///     <para />
		/// If not set, the <see cref = "Type.FullName" /> of the <see cref = "Implementation" /> will be used as the key to register the component.
		/// </summary>
		/// <param name = "name"> The name of this registration. </param>
		/// <returns> </returns>
		/// <remarks>
		/// Names have to be globally unique in the scope of the container.
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
		/// Stores a set of <see cref = "LifecycleActionDelegate{T}" /> which will be invoked when the component is created and before it's returned from the container.
		/// </summary>
		/// <param name = "actions"> A set of actions to be executed right after the component is created and before it's returned from the container. </param>
		public ComponentRegistration<TService> OnCreate(params Action<TService>[] actions)
		{
			if (actions != null && actions.Length != 0)
			{
				return OnCreate(actions.ConvertAll(a => new LifecycleActionDelegate<TService>((_, o) => a(o))));
			}
			return this;
		}

		/// <summary>
		/// Stores a set of <see cref = "LifecycleActionDelegate{T}" /> which will be invoked when the component is created and before it's returned from the container.
		/// </summary>
		/// <param name = "actions"> A set of actions to be executed right after the component is created and before it's returned from the container. </param>
		public ComponentRegistration<TService> OnCreate(params LifecycleActionDelegate<TService>[] actions)
		{
			if (actions != null && actions.Length != 0)
			{
				var action = (LifecycleActionDelegate<TService>)Delegate.Combine(actions);
				AddDescriptor(new OnCreateComponentDescriptor<TService>(action));
			}
			return this;
		}

		/// <summary>
		/// Stores a set of <see cref = "LifecycleActionDelegate{T}" /> which will be invoked when the component is created and before it's returned from the container.
		/// </summary>
		/// <param name = "actions"> A set of actions to be executed right after the component is created and before it's returned from the container. </param>
		public ComponentRegistration<TService> OnDestroy(params Action<TService>[] actions)
		{
			if (actions != null && actions.Length != 0)
			{
				return OnDestroy(actions.ConvertAll(a => new LifecycleActionDelegate<TService>((_, o) => a(o))));
			}
			return this;
		}

		/// <summary>
		/// Stores a set of <see cref = "LifecycleActionDelegate{T}" /> which will be invoked when the component is destroyed which means when it's released or it's lifetime scope ends. Notice that usage of this
		/// method will cause instances of the component to be tracked, even if they wouldn't be otherwise.
		/// </summary>
		/// <param name = "actions"> A set of actions to be executed when the component is destroyed. </param>
		public ComponentRegistration<TService> OnDestroy(params LifecycleActionDelegate<TService>[] actions)
		{
			if (actions != null && actions.Length != 0)
			{
				var action = (LifecycleActionDelegate<TService>)Delegate.Combine(actions);
				AddDescriptor(new OnDestroyComponentDescriptor<TService>(action));
			}
			return this;
		}

		/// <summary>
		/// Services that are already present in the container will be skipped. If no new service is left the registration will not happen at all.
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> OnlyNewServices()
		{
			registerNewServicesOnly = true;
			return this;
		}

		/// <summary>
		/// With the overwrite.
		/// </summary>
		/// <returns> </returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ComponentRegistration<TService> OverWrite()
		{
			overwrite = true;
			return this;
		}

		/// <summary>
		/// Sets the interceptor selector for this component.
		/// </summary>
		/// <param name = "selector"> </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> SelectInterceptorsWith(IInterceptorSelector selector)
		{
			return SelectInterceptorsWith(s => s.Instance(selector));
		}

		/// <summary>
		/// Sets the interceptor selector for this component.
		/// </summary>
		/// <param name = "selector"> </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> SelectInterceptorsWith(Action<ItemRegistration<IInterceptorSelector>> selector)
		{
			var registration = new ItemRegistration<IInterceptorSelector>();
			selector.Invoke(registration);
			return AddDescriptor(new InterceptorSelectorDescriptor(registration.Item));
		}

		/// <summary>
		/// Uses a factory to instantiate the component
		/// </summary>
		/// <typeparam name = "TFactory"> Factory type. This factory has to be registered in the kernel. </typeparam>
		/// <typeparam name = "TServiceImpl"> Implementation type. </typeparam>
		/// <param name = "factory"> Factory invocation </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> UsingFactory<TFactory, TServiceImpl>(Func<TFactory, TServiceImpl> factory)
			where TServiceImpl : TService
		{
			return UsingFactoryMethod(kernel => factory.Invoke(kernel.Resolve<TFactory>()));
		}

		/// <summary>
		/// Uses a factory method to instantiate the component.
		/// </summary>
		/// <typeparam name = "TImpl"> Implementation type </typeparam>
		/// <param name = "factoryMethod"> Factory method </param>
		/// <param name = "managedExternally"> When set to <c>true</c> container will not assume ownership of this component, will not track it not apply and lifecycle concerns to it. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> UsingFactoryMethod<TImpl>(Func<TImpl> factoryMethod,
		                                                                 bool managedExternally = false)
			where TImpl : TService
		{
			return UsingFactoryMethod((k, m, c) => factoryMethod(), managedExternally);
		}

		/// <summary>
		/// Uses a factory method to instantiate the component.
		/// </summary>
		/// <typeparam name = "TImpl"> Implementation type </typeparam>
		/// <param name = "factoryMethod"> Factory method </param>
		/// <param name = "managedExternally"> When set to <c>true</c> container will not assume ownership of this component, will not track it not apply and lifecycle concerns to it. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> UsingFactoryMethod<TImpl>(Func<IKernel, TImpl> factoryMethod,
		                                                                 bool managedExternally = false)
			where TImpl : TService
		{
			return UsingFactoryMethod((k, m, c) => factoryMethod(k), managedExternally);
		}

		/// <summary>
		/// Uses a factory method to instantiate the component.
		/// </summary>
		/// <typeparam name = "TImpl"> Implementation type </typeparam>
		/// <param name = "factoryMethod"> Factory method </param>
		/// <param name = "managedExternally"> When set to <c>true</c> container will not assume ownership of this component, will not track it not apply and lifecycle concerns to it. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> UsingFactoryMethod<TImpl>(
			Func<IKernel, ComponentModel, CreationContext, TImpl> factoryMethod,
			bool managedExternally = false)
			where TImpl : TService
		{
			Activator<FactoryMethodActivator<TImpl>>()
				.ExtendedProperties(Property.ForKey("factoryMethodDelegate").Eq(factoryMethod));

			if (managedExternally)
			{
				ExtendedProperties(Property.ForKey("factory.managedExternally").Eq(managedExternally));
			}

			if (implementation == null &&
			    (potentialServices.First().GetTypeInfo().IsClass == false || potentialServices.First().GetTypeInfo().IsSealed == false))
			{
				implementation = typeof(LateBoundComponent);
			}
			return this;
		}

		/// <summary>
		/// Uses a factory method to instantiate the component.
		/// </summary>
		/// <typeparam name = "TImpl"> Implementation type </typeparam>
		/// <param name = "factoryMethod"> Factory method </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> UsingFactoryMethod<TImpl>(Func<IKernel, CreationContext, TImpl> factoryMethod)
			where TImpl : TService
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
				services.RemoveAll(kernel.HasComponent);
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
			return list.ToArray();
		}

		private bool SkipRegistration(IKernelInternal internalKernel, ComponentModel componentModel)
		{
			return ifComponentRegisteredIgnore && internalKernel.HasComponent(componentModel.Name);
		}

		/// <summary>
		/// Registers this component with the <see cref = "IKernel" /> .
		/// </summary>
		/// <param name = "kernel"> The kernel. </param>
		void IRegistration.Register(IKernelInternal kernel)
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

			var componentModel = kernel.ComponentModelBuilder.BuildModel(GetContributors(services));
			if (SkipRegistration(kernel, componentModel))
			{
				kernel.Logger.Info("Skipping registration of " + componentModel.Name);
				return;
			}
			kernel.AddCustomComponent(componentModel);
		}

		/// <summary>
		/// Overrides default behavior by making the current component the default for every service it exposes. The <paramref name = "serviceFilter" /> allows user to narrow down the number of services which
		/// should be make defaults.
		/// </summary>
		/// <param name = "serviceFilter"> Invoked for each service exposed by given component if returns <c>true</c> this component will be the default for that service. </param>
		/// <returns> </returns>
		/// <remarks>
		/// When specified for multiple components for any given service the one registered after will override the one selected before.
		/// </remarks>
		public ComponentRegistration<TService> IsDefault(Predicate<Type> serviceFilter)
		{
			if (serviceFilter == null)
			{
				throw new ArgumentNullException(nameof(serviceFilter));
			}
			var properties = new Property(Constants.DefaultComponentForServiceFilter, serviceFilter);
			return ExtendedProperties(properties);
		}

		/// <summary>
		/// Overrides default behavior by making the current component the default for every service it exposes.
		/// </summary>
		/// <returns> </returns>
		/// <remarks>
		/// When specified for multiple components for any given service the one registered after will override the one selected before.
		/// </remarks>
		public ComponentRegistration<TService> IsDefault()
		{
			return IsDefault(_ => true);
		}

		/// <summary>
		/// Overrides default behavior by making the current component the fallback for every service it exposes that <paramref name = "serviceFilter" /> returns <c>true</c> for. That is if another,
		/// non-fallback, component will be registered exposing any of these same services as this component, that other component will take precedence over this one, regardless of order in which they are
		/// registered.
		/// </summary>
		/// <param name = "serviceFilter"> Invoked for each service exposed by given component if returns <c>true</c> this component will be the fallback for that service. </param>
		public ComponentRegistration<TService> IsFallback(Predicate<Type> serviceFilter)
		{
			if (serviceFilter == null)
			{
				throw new ArgumentNullException(nameof(serviceFilter));
			}
			var properties = new Property(Constants.FallbackComponentForServiceFilter, serviceFilter);
			return ExtendedProperties(properties);
		}

		/// <summary>
		/// Overrides default behavior by making the current component the fallback for every service it exposes. That is if another, non-fallback, component will be registered exposing any of the same services
		/// as this component, that other component will take precedence over this one, regardless of order in which they are registered
		/// </summary>
		/// <returns> </returns>
		public ComponentRegistration<TService> IsFallback()
		{
			return IsFallback(_ => true);
		}

		/// <summary>
		/// Filters (settable) properties of the component's implementation type to ignore.
		/// </summary>
		/// <param name = "propertySelector"> Predicate finding properties to ignore. If it returns <c>true</c> the property will not be added to <see cref = "ComponentModel.Properties" /> collection and Windsor
		/// will never try to set it. </param>
		public ComponentRegistration<TService> PropertiesIgnore(Func<PropertyInfo, bool> propertySelector)
		{
			return PropertiesIgnore((_, p) => propertySelector(p));
		}

		/// <summary>
		/// Filters (settable) properties of the component's implementation type to expose in the container as mandatory dependencies
		/// </summary>
		/// <param name = "propertySelector"> Predicate finding properties. If it returns <c>true</c> the property will be added to <see cref = "ComponentModel.Properties" /> collection and Windsor will make it
		/// a mandatory dependency. </param>
		public ComponentRegistration<TService> PropertiesRequire(Func<PropertyInfo, bool> propertySelector)
		{
			return PropertiesRequire((_, p) => propertySelector(p));
		}

		/// <summary>
		/// Filters (settable) properties of the component's implementation type to ignore.
		/// </summary>
		/// <param name = "propertySelector"> Predicate finding properties to ignore. If it returns <c>true</c> the property will not be added to <see cref = "ComponentModel.Properties" /> collection and Windsor
		/// will never try to set it. </param>
		public ComponentRegistration<TService> PropertiesIgnore(Func<ComponentModel, PropertyInfo, bool> propertySelector)
		{
			return AddDescriptor(new DelegatingModelDescriptor(builder: (k, c) =>
			{
				var filters = StandardPropertyFilters.GetPropertyFilters(c, createIfMissing: true);
				filters.Add(StandardPropertyFilters.IgnoreSelected(propertySelector));
			}));
		}

		/// <summary>
		/// Filters (settable) properties of the component's implementation type to expose in the container as mandatory dependencies
		/// </summary>
		/// <param name = "propertySelector"> Predicate finding properties. If it returns <c>true</c> the property will be added to <see cref = "ComponentModel.Properties" /> collection and Windsor will make it
		/// a mandatory dependency. </param>
		public ComponentRegistration<TService> PropertiesRequire(Func<ComponentModel, PropertyInfo, bool> propertySelector)
		{
			return AddDescriptor(new DelegatingModelDescriptor(builder: (k, c) =>
			{
				var filters = StandardPropertyFilters.GetPropertyFilters(c, createIfMissing: true);
				filters.Add(StandardPropertyFilters.RequireSelected(propertySelector));
			}));
		}

		/// <summary>
		/// Filters (settable) properties of the component's implementation type to expose in the container and specifies if matched properties are considered mandatory.
		/// </summary>
		/// <param name = "filter"> Rules for deciding whether given properties are exposed in the container or ignored and if they are mandatory, that is Windsor will only successfully resole the component if
		/// it can provide value for all of these properties. </param>
		/// <returns> </returns>
		public ComponentRegistration<TService> Properties(PropertyFilter filter)
		{
			return AddDescriptor(new DelegatingModelDescriptor(builder: (k, c) =>
			{
				var filters = StandardPropertyFilters.GetPropertyFilters(c, createIfMissing: true);
				filters.Add(StandardPropertyFilters.Create(filter));
			}));
		}
	}
}