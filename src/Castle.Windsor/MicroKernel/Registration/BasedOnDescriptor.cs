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
	using System.Collections.Generic;
	using System.ComponentModel;

	using Castle.Core;

	/// <summary>
	///   Describes how to register a group of related types.
	/// </summary>
	public class BasedOnDescriptor : IRegistration
	{
		private readonly Type basedOn;
		private readonly List<ConfigureDescriptor> configurers;
		private readonly FromDescriptor from;
		private readonly ServiceDescriptor service;
		private Predicate<Type> ifFilter;
		private Predicate<Type> unlessFilter;

		/// <summary>
		///   Initializes a new instance of the BasedOnDescriptor.
		/// </summary>
		internal BasedOnDescriptor(Type basedOn, FromDescriptor from)
		{
			this.basedOn = basedOn;
			this.from = from;
			service = new ServiceDescriptor(this);
			configurers = new List<ConfigureDescriptor>();
		}

		/// <summary>
		///   Gets the service descriptor.
		/// </summary>
		public ServiceDescriptor WithService
		{
			get { return service; }
		}

		/// <summary>
		///   Allows a type to be registered multiple times.
		/// </summary>
		public FromDescriptor AllowMultipleMatches()
		{
			return from.AllowMultipleMatches();
		}

		/// <summary>
		///   Returns the descriptor for accepting a new type.
		/// </summary>
		/// <typeparam name = "T">The base type.</typeparam>
		/// <returns>The descriptor for the type.</returns>
		[Obsolete("Calling this method resets registration. If that's what you want, start anew, with AllTypes...")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public BasedOnDescriptor BasedOn<T>()
		{
			return from.BasedOn<T>();
		}

		/// <summary>
		///   Returns the descriptor for accepting a new type.
		/// </summary>
		/// <param name = "basedOn">The base type.</param>
		/// <returns>The descriptor for the type.</returns>
		[Obsolete("Calling this method resets registration. If that's what you want, start anew, with AllTypes...")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public BasedOnDescriptor BasedOn(Type basedOn)
		{
			return from.BasedOn(basedOn);
		}

		/// <summary>
		///   Allows customized configurations of each matching type.
		/// </summary>
		/// <param name = "configurer">The configuration action.</param>
		/// <returns></returns>
		public BasedOnDescriptor Configure(Action<ComponentRegistration> configurer)
		{
			var config = new ConfigureDescriptor(this, configurer);
			configurers.Add(config);
			return this;
		}

		/// <summary>
		///   Allows customized configurations of each matching type.
		/// </summary>
		/// <param name = "configurer">The configuration action.</param>
		/// <returns></returns>
		public BasedOnDescriptor Configure(ConfigureDelegate configurer)
		{
			return Configure(delegate(ComponentRegistration registration) { configurer(registration); });
		}

		/// <summary>
		///   Allows customized configurations of each matching type that is 
		///   assignable to
		///   <typeparamref name = "T" />
		///   .
		/// </summary>
		/// <typeparam name = "T">The type assignable from.</typeparam>
		/// <param name = "configurer">The configuration action.</param>
		/// <returns></returns>
		public BasedOnDescriptor ConfigureFor<T>(Action<ComponentRegistration> configurer)
		{
			var config = new ConfigureDescriptor(this, typeof(T), configurer);
			configurers.Add(config);
			return this;
		}

		/// <summary>
		///   Allows customized configurations of each matching type that is 
		///   assignable to
		///   <typeparamref name = "T" />
		///   .
		/// </summary>
		/// <typeparam name = "T">The type assignable from.</typeparam>
		/// <param name = "configurer">The configuration action.</param>
		/// <returns></returns>
		public BasedOnDescriptor ConfigureFor<T>(ConfigureDelegate configurer)
		{
			return ConfigureFor<T>(delegate(ComponentRegistration registration) { configurer(registration); });
		}

		/// <summary>
		///   Assigns a conditional predication which must be satisfied.
		/// </summary>
		/// <param name = "ifFilter">The predicate to satisfy.</param>
		/// <returns></returns>
		public BasedOnDescriptor If(Predicate<Type> ifFilter)
		{
			this.ifFilter += ifFilter;
			return this;
		}

		/// <summary>
		///   Assigns a conditional predication which must not be satisfied.
		/// </summary>
		/// <param name = "unlessFilter">The predicate not to satisify.</param>
		/// <returns></returns>
		public BasedOnDescriptor Unless(Predicate<Type> unlessFilter)
		{
			this.unlessFilter += unlessFilter;
			return this;
		}

		/// <summary>
		///   Returns the descriptor for accepting a type based on a condition.
		/// </summary>
		/// <param name = "accepted">The accepting condition.</param>
		/// <returns>The descriptor for the type.</returns>
		[Obsolete("Calling this method resets registration. If that's what you want, start anew, with AllTypes...")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public BasedOnDescriptor Where(Predicate<Type> accepted)
		{
			return from.Where(accepted);
		}

		/// <summary>
		///   Uses all interfaces implemented by the type (or its base types) as well as their base interfaces.
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor WithServiceAllInterfaces()
		{
			return WithService.AllInterfaces();
		}

		/// <summary>
		///   Uses the base type matched on.
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor WithServiceBase()
		{
			return WithService.Base();
		}

		/// <summary>
		///   Uses all interfaces that have names matched by implementation type name.
		///   Matches Foo to IFoo, SuperFooExtended to IFoo and IFooExtended etc
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor WithServiceDefaultInterfaces()
		{
			return WithService.DefaultInterfaces();
		}

		/// <summary>
		///   Uses the first interface of a type. This method has non-deterministic behavior when type implements more than one interface!
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor WithServiceFirstInterface()
		{
			return WithService.FirstInterface();
		}

		/// <summary>
		///   Uses <paramref name = "implements" /> to lookup the sub interface.
		///   For example: if you have IService and 
		///   IProductService : ISomeInterface, IService, ISomeOtherInterface.
		///   When you call FromInterface(typeof(IService)) then IProductService
		///   will be used. Useful when you want to register _all_ your services
		///   and but not want to specify all of them.
		/// </summary>
		/// <param name = "implements"></param>
		/// <returns></returns>
		public BasedOnDescriptor WithServiceFromInterface(Type implements)
		{
			return WithService.FromInterface(implements);
		}

		/// <summary>
		///   Uses base type to lookup the sub interface.
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor WithServiceFromInterface()
		{
			return WithService.FromInterface();
		}

		/// <summary>
		///   Assigns a custom service selection strategy.
		/// </summary>
		/// <param name = "selector"></param>
		/// <returns></returns>
		public BasedOnDescriptor WithServiceSelect(ServiceDescriptor.ServiceSelector selector)
		{
			return WithService.Select(selector);
		}

		/// <summary>
		///   Uses the type itself.
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor WithServiceSelf()
		{
			return WithService.Self();
		}

		/// <summary>
		///   Assigns the supplied service types.
		/// </summary>
		/// <param name = "types"></param>
		/// <returns></returns>
		public BasedOnDescriptor WithServices(IEnumerable<Type> types)
		{
			return WithService.Select(types);
		}

		/// <summary>
		///   Assigns the supplied service types.
		/// </summary>
		/// <param name = "types"></param>
		/// <returns></returns>
		public BasedOnDescriptor WithServices(params Type[] types)
		{
			return WithService.Select(types);
		}

		protected virtual bool Accepts(Type type, out Type[] baseTypes)
		{
			baseTypes = null;
			return type.IsClass && !type.IsAbstract
			       && IsBasedOn(type, out baseTypes)
			       && ExecuteIfCondition(type)
			       && !ExecuteUnlessCondition(type);
		}

		protected bool ExecuteIfCondition(Type type)
		{
			if (ifFilter == null)
			{
				return true;
			}

			foreach (Predicate<Type> filter in ifFilter.GetInvocationList())
			{
				if (filter(type) == false)
				{
					return false;
				}
			}

			return true;
		}

		protected bool ExecuteUnlessCondition(Type type)
		{
			if (unlessFilter == null)
			{
				return false;
			}
			foreach (Predicate<Type> filter in unlessFilter.GetInvocationList())
			{
				if (filter(type))
				{
					return true;
				}
			}
			return false;
		}

		protected bool IsBasedOn(Type type, out Type[] baseTypes)
		{
			if (basedOn.IsAssignableFrom(type))
			{
				baseTypes = new[] { basedOn };
				return true;
			}
			if (basedOn.IsGenericTypeDefinition)
			{
				if (basedOn.IsInterface)
				{
					return IsBasedOnGenericInterface(type, out baseTypes);
				}
				return IsBasedOnGenericClass(type, out baseTypes);
			}
			baseTypes = new[] { basedOn };
			return false;
		}

		internal bool TryRegister(Type type, IKernel kernel)
		{
			Type[] baseTypes;

			if (!Accepts(type, out baseTypes))
			{
				return false;
			}
			var defaults = CastleComponentAttribute.GetDefaultsFor(type);
			var serviceTypes = service.GetServices(type, baseTypes);
			if (serviceTypes.Count == 0 && defaults.Services.Length > 0)
			{
				serviceTypes = defaults.Services;
			}
			var registration = Component.For(serviceTypes);
			registration.ImplementedBy(type);

			foreach (var configurer in configurers)
			{
				configurer.Apply(registration);
			}
			if (String.IsNullOrEmpty(registration.Name) && !String.IsNullOrEmpty(defaults.Name))
			{
				registration.Named(defaults.Name);
			}
			else
			{
				registration.RegisterOptionally();
			}
			kernel.Register(registration);
			return true;
		}

		private bool IsBasedOnGenericClass(Type type, out Type[] baseTypes)
		{
			while (type != null)
			{
				if (type.IsGenericType &&
				    type.GetGenericTypeDefinition() == basedOn)
				{
					baseTypes = new[] { type };
					return true;
				}

				type = type.BaseType;
			}
			baseTypes = null;
			return false;
		}

		private bool IsBasedOnGenericInterface(Type type, out Type[] baseTypes)
		{
			var types = new List<Type>(4);
			foreach (var @interface in type.GetInterfaces())
			{
				if (@interface.IsGenericType &&
				    @interface.GetGenericTypeDefinition() == basedOn)
				{
					if (@interface.ReflectedType == null &&
					    @interface.ContainsGenericParameters)
					{
						types.Add(@interface.GetGenericTypeDefinition());
					}
					else
					{
						types.Add(@interface);
					}
				}
			}
			baseTypes = types.ToArray();
			return baseTypes.Length > 0;
		}

		void IRegistration.Register(IKernel kernel)
		{
			((IRegistration)from).Register(kernel);
		}
	}
}