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
	using System.Linq;

	using Castle.DynamicProxy.Generators.Emitters;

	/// <summary>
	///   Describes how to select a types service.
	/// </summary>
	public class ServiceDescriptor
	{
		private readonly BasedOnDescriptor basedOnDescriptor;
		private ServiceSelector serviceSelector;

		internal ServiceDescriptor(BasedOnDescriptor basedOnDescriptor)
		{
			this.basedOnDescriptor = basedOnDescriptor;
		}

		/// <summary>
		///   Uses all interfaces implemented by the type (or its base types) as well as their base interfaces.
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor AllInterfaces()
		{
			return Select((t, b) => t.GetAllInterfaces());
		}

		/// <summary>
		///   Uses the base type matched on.
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor Base()
		{
			return Select((t, b) => b);
		}

		/// <summary>
		///   Uses all interfaces that have names matched by implementation type name.
		///   Matches Foo to IFoo, SuperFooExtended to IFoo and IFooExtended etc
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor DefaultInterfaces()
		{
			return Select((type, @base) =>
			              type.GetAllInterfaces()
			              	.Where(i => type.Name.Contains(GetInterfaceName(i))));
		}

		/// <summary>
		///   Uses the first interface of a type. This method has non-deterministic behavior when type implements more than one interface!
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor FirstInterface()
		{
			return Select((type, @base) =>
			{
				var first = type.GetInterfaces().FirstOrDefault();
				if (first == null)
				{
					return null;
				}

				return new[] { first };
			});
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
		public BasedOnDescriptor FromInterface(Type implements)
		{
			return Select(delegate(Type type, Type[] baseTypes)
			{
				var matches = new HashSet<Type>();
				if (implements != null)
				{
					AddFromInterface(type, implements, matches);
				}
				else
				{
					foreach (var baseType in baseTypes)
					{
						AddFromInterface(type, baseType, matches);
					}
				}

				if (matches.Count == 0)
				{
					foreach (var baseType in baseTypes.Where(t => t != typeof(object)))
					{
						if (baseType.IsAssignableFrom(type))
						{
							matches.Add(baseType);
							break;
						}
					}
				}

				return matches;
			});
		}

		/// <summary>
		///   Uses base type to lookup the sub interface.
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor FromInterface()
		{
			return FromInterface(null);
		}

		/// <summary>
		///   Assigns a custom service selection strategy.
		/// </summary>
		/// <param name = "selector"></param>
		/// <returns></returns>
		public BasedOnDescriptor Select(ServiceSelector selector)
		{
			serviceSelector += selector;
			return basedOnDescriptor;
		}

		/// <summary>
		///   Assigns the supplied service types.
		/// </summary>
		/// <param name = "types"></param>
		/// <returns></returns>
		public BasedOnDescriptor Select(IEnumerable<Type> types)
		{
			return Select(delegate { return types; });
		}

		/// <summary>
		///   Uses the type itself.
		/// </summary>
		/// <returns></returns>
		public BasedOnDescriptor Self()
		{
			return Select((t, b) => new[] { t });
		}

		internal ICollection<Type> GetServices(Type type, Type[] baseType)
		{
			var services = new HashSet<Type>();
			if (serviceSelector != null)
			{
				foreach (ServiceSelector selector in serviceSelector.GetInvocationList())
				{
					var selected = selector(type, baseType);
					if (selected != null)
					{
						foreach (var service in selected.Select(WorkaroundCLRBug))
						{
							services.Add(service);
						}
					}
				}
			}
			return services;
		}

		private void AddFromInterface(Type type, Type implements, ICollection<Type> matches)
		{
			foreach (var @interface in GetTopLevelInterfaces(type))
			{
				if (@interface.GetInterface(implements.FullName, false) != null)
				{
					matches.Add(@interface);
				}
			}
		}

		private string GetInterfaceName(Type @interface)
		{
			var name = @interface.Name;
			if ((name.Length > 1 && name[0] == 'I') && char.IsUpper(name[1]))
			{
				return name.Substring(1);
			}
			return name;
		}

		private IEnumerable<Type> GetTopLevelInterfaces(Type type)
		{
			var interfaces = type.GetInterfaces();
			var topLevel = new List<Type>(interfaces);

			foreach (var @interface in interfaces)
			{
				foreach (var parent in @interface.GetInterfaces())
				{
					topLevel.Remove(parent);
				}
			}

			return topLevel;
		}

		/// <summary>
		///   This is a workaround for a CLR bug in
		///   which GetInterfaces() returns interfaces
		///   with no implementations.
		/// </summary>
		/// <param name = "serviceType">Type of the service.</param>
		/// <returns></returns>
		private static Type WorkaroundCLRBug(Type serviceType)
		{
			if (!serviceType.IsInterface)
			{
				return serviceType;
			}
			// This is a workaround for a CLR bug in
			// which GetInterfaces() returns interfaces
			// with no implementations.
			if (serviceType.IsGenericType && serviceType.ReflectedType == null)
			{
				var shouldUseGenericTypeDefinition = false;
				foreach (var argument in serviceType.GetGenericArguments())
				{
					shouldUseGenericTypeDefinition |= argument.IsGenericParameter;
				}
				if (shouldUseGenericTypeDefinition)
				{
					return serviceType.GetGenericTypeDefinition();
				}
			}
			return serviceType;
		}

		public delegate IEnumerable<Type> ServiceSelector(Type type, Type[] baseTypes);
	}
}