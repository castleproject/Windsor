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

namespace Castle.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;

	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.MicroKernel;

	/// <summary>
	///   Represents the collection of information and
	///   meta information collected about a component.
	/// </summary>
	[Serializable]
	public sealed class ComponentModel : GraphNode
	{
		public const string SkipRegistration = "skip.registration";

		/// <summary>
		///   All available constructors
		/// </summary>
		private readonly ConstructorCandidateCollection constructors = new ConstructorCandidateCollection();

		private readonly ICollection<Type> interfaceServices = new HashSet<Type>();
		private readonly ICollection<Type> classServices = new SortedSet<Type>(new TypeByInheritanceDepthMostSpecificFirstComparer());

		/// <summary>
		///   Steps of lifecycle
		/// </summary>
		private readonly LifecycleConcernsCollection lifecycle = new LifecycleConcernsCollection();

		private readonly ComponentName componentName;

		/// <summary>
		///   /// Custom dependencies///
		/// </summary>
		[NonSerialized]
		private IDictionary customDependencies;

		/// <summary>
		///   Dependencies the kernel must resolve
		/// </summary>
		private DependencyModelCollection dependencies;

		/// <summary>
		///   Extended properties
		/// </summary>
		[NonSerialized]
		private IDictionary extendedProperties;

		/// <summary>
		///   Interceptors associated
		/// </summary>
		private InterceptorReferenceCollection interceptors;

		/// <summary>
		///   External parameters
		/// </summary>
		private ParameterModelCollection parameters;

		/// <summary>
		///   All potential properties that can be setted by the kernel
		/// </summary>
		private PropertySetCollection properties;

		/// <summary>
		///   Constructs a ComponentModel
		/// </summary>
		public ComponentModel(ComponentName name, ICollection<Type> services, Type implementation, IDictionary extendedProperties)
		{
			componentName = name;
			Implementation = implementation;
			LifestyleType = LifestyleType.Undefined;
			InspectionBehavior = PropertiesInspectionBehavior.Undefined;
			this.extendedProperties = extendedProperties;
			foreach (var type in services)
			{
				AddService(type);
			}
		}

		public IEnumerable<Type> AllServices
		{
			get { return classServices.Concat(interfaceServices); }
		}

		public ComponentName ComponentName
		{
			get { return componentName; }
		}

		/// <summary>
		///   Gets or sets the configuration.
		/// </summary>
		/// <value>The configuration.</value>
		public IConfiguration Configuration { get; set; }

		/// <summary>
		///   Gets the constructors candidates.
		/// </summary>
		/// <value>The constructors.</value>
		public ConstructorCandidateCollection Constructors
		{
			get { return constructors; }
		}

		/// <summary>
		///   Gets or sets the custom component activator.
		/// </summary>
		/// <value>The custom component activator.</value>
		public Type CustomComponentActivator { get; set; }

		/// <summary>
		///   Gets the custom dependencies.
		/// </summary>
		/// <value>The custom dependencies.</value>
		public IDictionary CustomDependencies
		{
			get
			{
				var value = customDependencies;
				if (value != null)
				{
					return value;
				}
				value = new Arguments();
				var originalValue = Interlocked.CompareExchange(ref customDependencies, value, null);
				return originalValue ?? value;
			}
		}

		/// <summary>
		///   Gets or sets the custom lifestyle.
		/// </summary>
		/// <value>The custom lifestyle.</value>
		public Type CustomLifestyle { get; set; }

		/// <summary>
		///   Dependencies are kept within constructors and
		///   properties. Others dependencies must be 
		///   registered here, so the kernel (as a matter 
		///   of fact the handler) can check them
		/// </summary>
		public DependencyModelCollection Dependencies
		{
			get
			{
				var value = dependencies;
				if (value != null)
				{
					return value;
				}
				value = new DependencyModelCollection();
				var originalValue = Interlocked.CompareExchange(ref dependencies, value, null);
				return originalValue ?? value;
			}
		}

		/// <summary>
		///   Gets or sets the extended properties.
		/// </summary>
		/// <value>The extended properties.</value>
		public IDictionary ExtendedProperties
		{
			get
			{
				var value = extendedProperties;
				if (value != null)
				{
					return value;
				}
				value = new Arguments();
				var originalValue = Interlocked.CompareExchange(ref extendedProperties, value, null);
				return originalValue ?? value;
			}
		}

		public bool HasCustomDependencies
		{
			get
			{
				var value = customDependencies;
				return value != null && value.Count > 0;
			}
		}

		public bool HasInterceptors
		{
			get
			{
				var value = interceptors;
				return value != null && value.HasInterceptors;
			}
		}

		public bool HasParameters
		{
			get
			{
				var value = parameters;
				return value != null && value.Count > 0;
			}
		}

		/// <summary>
		///   Gets or sets the component implementation.
		/// </summary>
		/// <value>The implementation.</value>
		public Type Implementation { get; set; }

		/// <summary>
		///   Gets or sets the strategy for
		///   inspecting public properties 
		///   on the components
		/// </summary>
		public PropertiesInspectionBehavior InspectionBehavior { get; set; }

		/// <summary>
		///   Gets the interceptors.
		/// </summary>
		/// <value>The interceptors.</value>
		public InterceptorReferenceCollection Interceptors
		{
			get
			{
				var value = interceptors;
				if (value != null)
				{
					return value;
				}
				value = new InterceptorReferenceCollection(Dependencies);
				var originalValue = Interlocked.CompareExchange(ref interceptors, value, null);
				return originalValue ?? value;
			}
		}

		/// <summary>
		///   Gets the interface services exposed.
		/// </summary>
		/// <value>The service.</value>
		public IEnumerable<Type> InterfaceServices
		{
			get { return interfaceServices; }
		}

		/// <summary>
		///   Gets the class services exposed
		/// </summary>
		/// <value>The service.</value>
		public IEnumerable<Type> ClassServices
		{
			get { return classServices; }
		}

		/// <summary>
		///   Gets the lifecycle steps.
		/// </summary>
		/// <value>The lifecycle steps.</value>
		public LifecycleConcernsCollection Lifecycle
		{
			get { return lifecycle; }
		}

		/// <summary>
		///   Gets or sets the lifestyle type.
		/// </summary>
		/// <value>The type of the lifestyle.</value>
		public LifestyleType LifestyleType { get; set; }

		/// <summary>
		///   Sets or returns the component key
		/// </summary>
		public string Name
		{
			get { return componentName.Name; }
			set { componentName.SetName(value); }
		}

		/// <summary>
		///   Gets the parameter collection.
		/// </summary>
		/// <value>The parameters.</value>
		public ParameterModelCollection Parameters
		{
			get
			{
				var value = parameters;
				if (value != null)
				{
					return value;
				}
				value = new ParameterModelCollection();
				var originalValue = Interlocked.CompareExchange(ref parameters, value, null);
				return originalValue ?? value;
			}
		}

		/// <summary>
		///   Gets the properties set.
		/// </summary>
		/// <value>The properties.</value>
		public PropertySetCollection Properties
		{
			get
			{
				var value = properties;
				if (value != null)
				{
					return value;
				}
				value = new PropertySetCollection();
				var originalValue = Interlocked.CompareExchange(ref properties, value, null);
				return originalValue ?? value;
			}
		}

		/// <summary>
		///   Gets or sets a value indicating whether the component requires generic arguments.
		/// </summary>
		/// <value>
		///   <c>true</c> if generic arguments are required; otherwise, <c>false</c>.
		/// </value>
		public bool RequiresGenericArguments { get; set; }

		public void AddService(Type type)
		{
			if (type == null)
			{
				return;
			}
			if (type.IsClass)
			{
				classServices.Add(type);
				return;
			}
			if (!type.IsInterface)
			{
				throw new ArgumentException(
					string.Format("Type {0} is not a class nor an interface, and those are the only values allowed.", type));
			}
			interfaceServices.Add(type);
		}

		/// <summary>
		///   Requires the selected property dependencies.
		/// </summary>
		/// <param name = "selectors">The property selector.</param>
		public void Requires(params Predicate<PropertySet>[] selectors)
		{
			foreach (var property in Properties)
			{
				foreach (var select in selectors)
				{
					if (select(property))
					{
						property.Dependency.IsOptional = false;
						break;
					}
				}
			}
		}

		/// <summary>
		///   Requires the property dependencies of type <typeparamref name = "D" />.
		/// </summary>
		/// <typeparam name = "D">The dependency type.</typeparam>
		public void Requires<D>() where D : class
		{
			Requires(p => p.Dependency.TargetItemType == typeof(D));
		}

		public override string ToString()
		{
			var services = AllServices.ToArray();
			if(services.Length == 1 && services.Single() == Implementation)
			{
				return Implementation.Name;
			}

			string value;
			if(Implementation == typeof(LateBoundComponent))
			{

				value = string.Format("late bound {0}", services[0].Name);
			}
			else if(Implementation == null)
			{

				value = "no impl / "+ services[0].Name;
			}
			else
			{
				value = string.Format("{0} / {1}", Implementation.Name, services[0].Name);
			}
			if (services.Length > 1)
			{
				value += string.Format(" and {0} other services", services.Length - 1);
			}
			return value;
		}
	}
} 