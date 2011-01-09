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

namespace Castle.MicroKernel.Registration
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;

	/// <summary>
	///   Delegate for custom registration configuration.
	/// </summary>
	/// <param name = "registration">The component registration.</param>
	/// <returns>Not used.</returns>
	public delegate object ConfigureDelegate(ComponentRegistration registration);

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
		///   Gets the type all types must be based on.
		/// </summary>
		internal Type InternalBasedOn
		{
			get { return basedOn; }
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
		public BasedOnDescriptor BasedOn<T>()
		{
			return from.BasedOn<T>();
		}

		/// <summary>
		///   Returns the descriptor for accepting a new type.
		/// </summary>
		/// <param name = "basedOn">The base type.</param>
		/// <returns>The descriptor for the type.</returns>
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
		public BasedOnDescriptor Where(Predicate<Type> accepted)
		{
			return from.Where(accepted);
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
			if(serviceTypes.Count == 0 && defaults.Service != null)
			{
				serviceTypes = new[] { defaults.Service };
			}
			var registration = Component.For(serviceTypes);
			registration.ImplementedBy(type);

			foreach (var configurer in configurers)
			{
				configurer.Apply(registration);
			}
			if (String.IsNullOrEmpty(registration.Name) &&
				String.IsNullOrEmpty(defaults.Key) == false)
			{
				registration.Named(defaults.Key);
			}

			if (!kernel.HasComponent(registration.Name))
			{
				kernel.Register(registration);
			}

			return true;
		}

		private bool Accepts(Type type, out Type[] baseTypes)
		{
			baseTypes = null;
			return type.IsClass && !type.IsAbstract
			       && IsBasedOn(type, out baseTypes)
			       && ExecuteIfCondition(type)
			       && !ExecuteUnlessCondition(type);
		}

		private bool ExecuteIfCondition(Type type)
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

		private bool ExecuteUnlessCondition(Type type)
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

		private bool IsBasedOn(Type type, out Type[] baseTypes)
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

		#region IRegistration Members

		void IRegistration.Register(IKernel kernel)
		{
			((IRegistration)from).Register(kernel);
		}

		#endregion
	}
}