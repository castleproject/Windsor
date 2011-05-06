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

	using Castle.Core;

	/// <summary>
	///   Factory for creating <see cref = "ComponentRegistration" /> objects. Use static methods on the class to fluently build registration.
	/// </summary>
	public static class Component
	{
		/// <summary>
		///   Creates a component registration for the <paramref name = "serviceType" />
		/// </summary>
		/// <param name = "serviceType">Type of the service.</param>
		/// <returns>The component registration.</returns>
		public static ComponentRegistration For(Type serviceType)
		{
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType",
				                                "The argument was null. Check that the assembly "
				                                + "is referenced and the type available to your application.");
			}

			return new ComponentRegistration(serviceType);
		}

		/// <summary>
		///   Creates a component registration for the <paramref name = "serviceTypes" />
		/// </summary>
		/// <param name = "serviceTypes">Types of the service.</param>
		/// <returns>The component registration.</returns>
		/// B
		public static ComponentRegistration For(params Type[] serviceTypes)
		{
			if (serviceTypes.Length == 0)
			{
				throw new ArgumentException("At least one service type must be supplied");
			}
			return new ComponentRegistration(serviceTypes);
		}

		/// <summary>
		///   Creates a component registration for the <paramref name = "serviceTypes" />
		/// </summary>
		/// <param name = "serviceTypes">Types of the service.</param>
		/// <returns>The component registration.</returns>
		public static ComponentRegistration For(IEnumerable<Type> serviceTypes)
		{
			ComponentRegistration registration = null;

			foreach (var serviceType in serviceTypes)
			{
				if (registration == null)
				{
					registration = For(serviceType);
				}
				else
				{
					registration.Forward(serviceType);
				}
			}

			if (registration == null)
			{
				throw new ArgumentException("At least one service type must be supplied");
			}

			return registration;
		}

		/// <summary>
		///   Creates a component registration for the service type.
		/// </summary>
		/// <typeparam name = "TService">The service type.</typeparam>
		/// <returns>The component registration.</returns>
		public static ComponentRegistration<TService> For<TService>()
			where TService : class
		{
			return new ComponentRegistration<TService>();
		}

		/// <summary>
		///   Creates a component registration for the service types.
		/// </summary>
		/// <typeparam name = "TService1">The primary service type.</typeparam>
		/// <typeparam name = "TService2">The forwarded type.</typeparam>
		/// <returns>The component registration.</returns>
		public static ComponentRegistration<TService1> For<TService1, TService2>()
			where TService1 : class
		{
			return new ComponentRegistration<TService1>().Forward<TService2>();
		}

		/// <summary>
		///   Creates a component registration for the service types.
		/// </summary>
		/// <typeparam name = "TService1">The primary service type.</typeparam>
		/// <typeparam name = "TService2">The first forwarded type.</typeparam>
		/// <typeparam name = "TService3">The second forwarded type.</typeparam>
		/// <returns>The component registration.</returns>
		public static ComponentRegistration<TService1> For<TService1, TService2, TService3>()
			where TService1 : class
		{
			return new ComponentRegistration<TService1>().Forward<TService2, TService3>();
		}

		/// <summary>
		///   Creates a component registration for the service types.
		/// </summary>
		/// <typeparam name = "TService1">The primary service type.</typeparam>
		/// <typeparam name = "TService2">The first forwarded type.</typeparam>
		/// <typeparam name = "TService3">The second forwarded type.</typeparam>
		/// <typeparam name = "TService4">The third forwarded type.</typeparam>
		/// <returns>The component registration.</returns>
		public static ComponentRegistration<TService1> For<TService1, TService2, TService3, TService4>()
			where TService1 : class
		{
			return new ComponentRegistration<TService1>().Forward<TService2, TService3, TService4>();
		}

		/// <summary>
		///   Creates a component registration for the service types.
		/// </summary>
		/// <typeparam name = "TService1">The primary service type.</typeparam>
		/// <typeparam name = "TService2">The first forwarded type.</typeparam>
		/// <typeparam name = "TService3">The second forwarded type.</typeparam>
		/// <typeparam name = "TService4">The third forwarded type.</typeparam>
		/// <typeparam name = "TService5">The fourth forwarded type.</typeparam>
		/// <returns>The component registration.</returns>
		public static ComponentRegistration<TService1> For<TService1, TService2, TService3, TService4, TService5>()
			where TService1 : class
		{
			return new ComponentRegistration<TService1>().Forward<TService2, TService3, TService4, TService5>();
		}

		/// <summary>
		///   Helper method for filtering components based on presence of an Attribute.
		/// </summary>
		/// <typeparam name = "TAttribute"></typeparam>
		/// <param name = "type"></param>
		/// <returns></returns>
		/// <example>
		///   container.Register(
		///   Classes.FromThisAssembly()
		///   .Where(Component.HasAttribute&lt;UserAttribute&gt;) );
		/// </example>
		public static bool HasAttribute<TAttribute>(Type type) where TAttribute : Attribute
		{
			return Attribute.IsDefined(type, typeof(TAttribute));
		}

		/// <summary>
		///   Helper method for filtering components based on presence of an Attribute and value of predicate on that attribute.
		/// </summary>
		/// <typeparam name = "TAttribute"></typeparam>
		/// <param name = "filter"></param>
		/// <returns></returns>
		/// <example>
		///   container.Register(
		///   Classes.FromThisAssembly()
		///   .Where(Component.HasAttribute&lt;UserAttribute&gt;(u => u.SomeFlag)) );
		/// </example>
		public static Predicate<Type> HasAttribute<TAttribute>(Predicate<TAttribute> filter) where TAttribute : Attribute
		{
			return type => HasAttribute<TAttribute>(type) &&
			               filter((TAttribute)Attribute.GetCustomAttribute(type, typeof(TAttribute)));
		}

		/// <summary>
		///   Determines if the component is a Castle component, that is - if it has a <see cref = "CastleComponentAttribute" />.
		/// </summary>
		/// <returns>true if the service is a Castle Component.</returns>
		/// <remarks>
		///   This method is usually used as argument for <see cref = "BasedOnDescriptor.If" /> method.
		/// </remarks>
		public static bool IsCastleComponent(Type type)
		{
			return HasAttribute<CastleComponentAttribute>(type);
		}

		/// <summary>
		///   Creates a predicate to check if a component is in a namespace.
		/// </summary>
		/// <param name = "namespace">The namespace.</param>
		/// <returns>true if the component type is in the namespace.</returns>
		public static Predicate<Type> IsInNamespace(string @namespace)
		{
			return IsInNamespace(@namespace, false);
		}

		/// <summary>
		///   Creates a predicate to check if a component is in a namespace.
		/// </summary>
		/// <param name = "namespace">The namespace.</param>
		/// <param name = "includeSubnamespaces">If set to true, will also include types from subnamespaces.</param>
		/// <returns>true if the component type is in the namespace.</returns>
		public static Predicate<Type> IsInNamespace(string @namespace, bool includeSubnamespaces)
		{
			if (includeSubnamespaces)
			{
				return type => type.Namespace == @namespace ||
				               type.Namespace.StartsWith(@namespace + ".");
			}

			return type => type.Namespace == @namespace;
		}

		/// <summary>
		///   Creates a predicate to check if a component shares a namespace with another.
		/// </summary>
		/// <param name = "type">The component type to test namespace against.</param>
		/// <returns>true if the component is in the same namespace.</returns>
		public static Predicate<Type> IsInSameNamespaceAs(Type type)
		{
			return IsInNamespace(type.Namespace);
		}

		/// <summary>
		///   Creates a predicate to check if a component shares a namespace with another.
		/// </summary>
		/// <param name = "type">The component type to test namespace against.</param>
		/// <param name = "includeSubnamespaces">If set to true, will also include types from subnamespaces.</param>
		/// <returns>true if the component is in the same namespace.</returns>
		public static Predicate<Type> IsInSameNamespaceAs(Type type, bool includeSubnamespaces)
		{
			return IsInNamespace(type.Namespace, includeSubnamespaces);
		}

		/// <summary>
		///   Creates a predicate to check if a component shares a namespace with another.
		/// </summary>
		/// <typeparam name = "T">The component type to test namespace against.</typeparam>
		/// <returns>true if the component is in the same namespace.</returns>
		public static Predicate<Type> IsInSameNamespaceAs<T>() where T : class
		{
			return IsInSameNamespaceAs(typeof(T));
		}

		/// <summary>
		///   Creates a predicate to check if a component shares a namespace with another.
		/// </summary>
		/// <typeparam name = "T">The component type to test namespace against.</typeparam>
		/// <param name = "includeSubnamespaces">If set to true, will also include types from subnamespaces.</param>
		/// <returns>true if the component is in the same namespace.</returns>
		public static Predicate<Type> IsInSameNamespaceAs<T>(bool includeSubnamespaces) where T : class
		{
			return IsInSameNamespaceAs(typeof(T), includeSubnamespaces);
		}
	}
}