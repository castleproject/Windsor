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

namespace Castle.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Threading;

	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.MicroKernel;

	/// <summary>
	///   Represents the collection of information and meta information collected about a component.
	/// </summary>
	[Serializable]
	public sealed class ComponentModel : GraphNode
	{
		public const string GenericImplementationMatchingStrategy = "generic.matching";

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly ConstructorCandidateCollection constructors = new ConstructorCandidateCollection();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly LifecycleConcernsCollection lifecycle = new LifecycleConcernsCollection();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly List<Type> services = new List<Type>(4);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ComponentName componentName;

		[NonSerialized]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private IDictionary customDependencies;

		/// <summary>
		///   Dependencies the kernel must resolve
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private DependencyModelCollection dependencies;

		[NonSerialized]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private IDictionary extendedProperties;

		/// <summary>
		///   Interceptors associated
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private InterceptorReferenceCollection interceptors;

		/// <summary>
		///   External parameters
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ParameterModelCollection parameters;

		/// <summary>
		///   All potential properties that can be setted by the kernel
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private PropertySetCollection properties;

		/// <summary>
		///   Constructs a ComponentModel
		/// </summary>
		public ComponentModel(ComponentName name, ICollection<Type> services, Type implementation, IDictionary extendedProperties)
		{
			componentName = Must.NotBeNull(name, "name");
			Implementation = Must.NotBeNull(implementation, "implementation");
			this.extendedProperties = extendedProperties;
			services = Must.NotBeEmpty(services, "services");
			foreach (var type in services)
			{
				AddService(type);
			}
		}

		public ComponentModel()
		{
		}

		public ComponentName ComponentName
		{
			get { return componentName; }
			internal set { componentName = Must.NotBeNull(value, "value"); }
		}

		/// <summary>
		///   Gets or sets the configuration.
		/// </summary>
		/// <value> The configuration. </value>
		public IConfiguration Configuration { get; set; }

		/// <summary>
		///   Gets the constructors candidates.
		/// </summary>
		/// <value> The constructors. </value>
		[DebuggerDisplay("Count = {constructors.Count}")]
		public ConstructorCandidateCollection Constructors
		{
			get { return constructors; }
		}

		/// <summary>
		///   Gets or sets the custom component activator.
		/// </summary>
		/// <value> The custom component activator. </value>
		public Type CustomComponentActivator { get; set; }

		/// <summary>
		///   Gets the custom dependencies.
		/// </summary>
		/// <value> The custom dependencies. </value>
		[DebuggerDisplay("Count = {customDependencies.Count}")]
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
		/// <value> The custom lifestyle. </value>
		public Type CustomLifestyle { get; set; }

		/// <summary>
		///   Dependencies are kept within constructors and properties. Others dependencies must be registered here, so the kernel (as a matter of fact the handler) can check them
		/// </summary>
		[DebuggerDisplay("Count = {dependencies.dependencies.Count}")]
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
		/// <value> The extended properties. </value>
		[DebuggerDisplay("Count = {extendedProperties.Count}")]
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

		public bool HasClassServices
		{
			get { return services.First().IsClass; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public bool HasCustomDependencies
		{
			get
			{
				var value = customDependencies;
				return value != null && value.Count > 0;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public bool HasInterceptors
		{
			get
			{
				var value = interceptors;
				return value != null && value.HasInterceptors;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
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
		/// <value> The implementation. </value>
		public Type Implementation { get; set; }

		/// <summary>
		///   Gets or sets the strategy for inspecting public properties on the components
		/// </summary>
		public PropertiesInspectionBehavior InspectionBehavior { get; set; }

		/// <summary>
		///   Gets the interceptors.
		/// </summary>
		/// <value> The interceptors. </value>
		[DebuggerDisplay("Count = {interceptors.list.Count}")]
		public InterceptorReferenceCollection Interceptors
		{
			get
			{
				var value = interceptors;
				if (value != null)
				{
					return value;
				}
				value = new InterceptorReferenceCollection(this);
				var originalValue = Interlocked.CompareExchange(ref interceptors, value, null);
				return originalValue ?? value;
			}
		}

		/// <summary>
		///   Gets the lifecycle steps.
		/// </summary>
		/// <value> The lifecycle steps. </value>
		[DebuggerDisplay(
			"Count = {(lifecycle.commission != null ? lifecycle.commission.Count : 0) + (lifecycle.decommission != null ? lifecycle.decommission.Count : 0)}"
			)]
		public LifecycleConcernsCollection Lifecycle
		{
			get { return lifecycle; }
		}

		/// <summary>
		///   Gets or sets the lifestyle type.
		/// </summary>
		/// <value> The type of the lifestyle. </value>
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
		/// <value> The parameters. </value>
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
		/// <value> The properties. </value>
		[DebuggerDisplay("Count = {properties.Count}")]
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
		/// <value> <c>true</c> if generic arguments are required; otherwise, <c>false</c> . </value>
		public bool RequiresGenericArguments { get; set; }

		[DebuggerDisplay("Count = {services.Count}")]
		public IEnumerable<Type> Services
		{
			get { return services; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal ParameterModelCollection ParametersInternal
		{
			get { return parameters; }
		}

		/// <summary>
		///   Adds constructor dependency to this <see cref="ComponentModel" />
		/// </summary>
		/// <param name="constructor"> </param>
		public void AddConstructor(ConstructorCandidate constructor)
		{
			Constructors.Add(constructor);
			constructor.Dependencies.ForEach(Dependencies.Add);
		}

		/// <summary>
		///   Adds property dependency to this <see cref="ComponentModel" />
		/// </summary>
		/// <param name="property"> </param>
		public void AddProperty(PropertySet property)
		{
			Properties.Add(property);
			Dependencies.Add(property.Dependency);
		}

		/// <summary>
		///   Add service to be exposed by this <see cref="ComponentModel" />
		/// </summary>
		/// <param name="type"> </param>
		public void AddService(Type type)
		{
			if (type == null)
			{
				return;
			}
			if (type.IsPrimitiveType())
			{
				throw new ArgumentException(
					string.Format("Type {0} can not be used as a service. only classes, and interfaces can be exposed as a service.",
					              type));
			}

			ComponentServicesUtil.AddService(services, type);
		}

		/// <summary>
		///   Requires the selected property dependencies.
		/// </summary>
		/// <param name="selectors"> The property selector. </param>
		public void Requires(params Predicate<PropertySet>[] selectors)
		{
			foreach (var property in Properties)
			{
				if (selectors.Any(s => s(property)))
				{
					property.Dependency.IsOptional = false;
				}
			}
		}

		/// <summary>
		///   Requires the property dependencies of type <typeparamref name="D" /> .
		/// </summary>
		/// <typeparam name="D"> The dependency type. </typeparam>
		public void Requires<D>() where D : class
		{
			Requires(p => p.Dependency.TargetItemType == typeof (D));
		}

		public override string ToString()
		{
			var services = Services.ToArray();
			if (services.Length == 1 && services[0] == Implementation)
			{
				return Implementation.ToCSharpString();
			}

			string value;
			if (Implementation == typeof (LateBoundComponent))
			{
				value = string.Format("late bound {0}", services[0].ToCSharpString());
			}
			else if (Implementation == null)
			{
				value = "no impl / " + services[0].ToCSharpString();
			}
			else
			{
				value = string.Format("{0} / {1}", Implementation.ToCSharpString(), services[0].ToCSharpString());
			}
			if (services.Length > 1)
			{
				value += string.Format(" and {0} other services", services.Length - 1);
			}
			return value;
		}
	}
}