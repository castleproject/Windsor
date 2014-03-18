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

namespace Castle.Windsor.Installer
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.Core.Resource;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.Windsor.Configuration.Interpreters;

	/// <summary>
	///   Default <see cref = "IComponentsInstaller" /> implementation.
	/// </summary>
	public class DefaultComponentInstaller : IComponentsInstaller
	{
		private string assemblyName;

		/// <summary>
		///   Perform installation.
		/// </summary>
		/// <param name = "container">Target container</param>
		/// <param name = "store">Configuration store</param>
		public void SetUp(IWindsorContainer container, IConfigurationStore store)
		{
			var converter = container.Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey) as IConversionManager;
			SetUpInstallers(store.GetInstallers(), container, converter);
			SetUpFacilities(store.GetFacilities(), container, converter);
			SetUpComponents(store.GetComponents(), container, converter);
#if !SILVERLIGHT
			SetUpChildContainers(store.GetConfigurationForChildContainers(), container);
#endif
		}

		protected virtual void SetUpInstallers(IConfiguration[] installers, IWindsorContainer container,
		                                       IConversionManager converter)
		{
			var instances = new Dictionary<Type, IWindsorInstaller>();
			var assemblies = new HashSet<Assembly>();
			foreach (var installer in installers)
			{
				AddInstaller(installer, instances, converter, assemblies);
			}

			if (instances.Count != 0)
			{
				container.Install(instances.Values.ToArray());
			}
		}

		private void AddInstaller(IConfiguration installer, Dictionary<Type, IWindsorInstaller> cache,
		                          IConversionManager conversionManager, ICollection<Assembly> assemblies)
		{
			var typeName = installer.Attributes["type"];
			if (string.IsNullOrEmpty(typeName) == false)
			{
				var type = conversionManager.PerformConversion<Type>(typeName);
				AddInstaller(cache, type);
				return;
			}

			assemblyName = installer.Attributes["assembly"];
			if (string.IsNullOrEmpty(assemblyName) == false)
			{
				var assembly = ReflectionUtil.GetAssemblyNamed(assemblyName);
				if (assemblies.Contains(assembly))
				{
					return;
				}
				assemblies.Add(assembly);

				GetAssemblyInstallers(cache, assembly);
				return;
			}

#if !SILVERLIGHT
			var directory = installer.Attributes["directory"];
			var mask = installer.Attributes["fileMask"];
			var token = installer.Attributes["publicKeyToken"];
			Debug.Assert(directory != null, "directory != null");
			var assemblyFilter = new AssemblyFilter(directory, mask);
			if (token != null)
			{
				assemblyFilter.WithKeyToken(token);
			}

			foreach (var assembly in ReflectionUtil.GetAssemblies(assemblyFilter))
			{
				if (assemblies.Contains(assembly))
				{
					continue;
				}
				assemblies.Add(assembly);
				GetAssemblyInstallers(cache, assembly);
			}
#endif
		}

		private void GetAssemblyInstallers(Dictionary<Type, IWindsorInstaller> cache, Assembly assembly)
		{
			var types = assembly.GetAvailableTypes();
			foreach (var type in InstallerTypes(types))
			{
				AddInstaller(cache, type);
			}
		}

		private IEnumerable<Type> InstallerTypes(IEnumerable<Type> types)
		{
			return types.Where(IsInstaller);
		}

		private bool IsInstaller(Type type)
		{
			return type.IsClass &&
			       type.IsAbstract == false &&
			       type.IsGenericTypeDefinition == false &&
			       type.Is<IWindsorInstaller>();
		}

		private void AddInstaller(Dictionary<Type, IWindsorInstaller> cache, Type type)
		{
			if (cache.ContainsKey(type) == false)
			{
				var installerInstance = type.CreateInstance<IWindsorInstaller>();
				cache.Add(type, installerInstance);
			}
		}

		protected virtual void SetUpFacilities(IConfiguration[] configurations, IWindsorContainer container, IConversionManager converter)
		{
			foreach (var facility in configurations)
			{
				var type = converter.PerformConversion<Type>(facility.Attributes["type"]);
				var facilityInstance = type.CreateInstance<IFacility>();
				Debug.Assert(facilityInstance != null);

				container.AddFacility(facilityInstance);
			}
		}

		private void AssertImplementsService(IConfiguration id, Type service, Type implementation)
		{
			if (service == null)
			{
				return;
			}
			if (service.IsGenericTypeDefinition)
			{
				implementation = implementation.MakeGenericType(service.GetGenericArguments());
			}
			if (!service.IsAssignableFrom(implementation))
			{
				var message = string.Format("Could not set up component '{0}'. Type '{1}' does not implement service '{2}'",
				                            id.Attributes["id"],
				                            implementation.AssemblyQualifiedName,
				                            service.AssemblyQualifiedName);
				throw new ComponentRegistrationException(message);
			}
		}

		protected virtual void SetUpComponents(IConfiguration[] configurations, IWindsorContainer container, IConversionManager converter)
		{
			foreach (var component in configurations)
			{
				var implementation = GetType(converter, component.Attributes["type"]);
				var firstService = GetType(converter, component.Attributes["service"]);
				var services = new HashSet<Type>();
				if (firstService != null)
				{
					services.Add(firstService);
				}
				CollectAdditionalServices(component, converter, services);

				var name = default(string);
				if (implementation != null)
				{
					AssertImplementsService(component, firstService, implementation);
					var defaults = CastleComponentAttribute.GetDefaultsFor(implementation);
					if (defaults.ServicesSpecifiedExplicitly && services.Count == 0)
					{
						defaults.Services.ForEach(s => services.Add(s));
					}
					name = GetName(defaults, component);
				}

				if (services.Count == 0 && implementation == null)
				{
					continue;
				}

				container.Register(Component.For(services).ImplementedBy(implementation).Named(name));
			}
		}

		private static string GetName(CastleComponentAttribute defaults, IConfiguration component)
		{
			if (component.Attributes["id-automatic"] != bool.TrueString)
			{
				return component.Attributes["id"];
			}
			return defaults.Name;
		}

		private Type GetType(IConversionManager converter, string typeName)
		{
			if (typeName == null)
			{
				return null;
			}
			return converter.PerformConversion<Type>(typeName);
		}

		private void CollectAdditionalServices(IConfiguration component, IConversionManager converter, ICollection<Type> services)
		{
			var forwardedTypes = component.Children["forwardedTypes"];
			if (forwardedTypes == null)
			{
				return;
			}

			foreach (var forwardedType in forwardedTypes.Children)
			{
				var forwardedServiceTypeName = forwardedType.Attributes["service"];
				try
				{
					services.Add(converter.PerformConversion<Type>(forwardedServiceTypeName));
				}
				catch (ConverterException e)
				{
					throw new ComponentRegistrationException(
						string.Format("Component {0} defines invalid forwarded type.", component.Attributes["id"]), e);
				}
			}
		}

#if !SILVERLIGHT
		private static void SetUpChildContainers(IConfiguration[] configurations, IWindsorContainer parentContainer)
		{
			foreach (var childContainerConfig in configurations)
			{
				var id = childContainerConfig.Attributes["name"];

				Debug.Assert(id != null);

				new WindsorContainer(id, parentContainer,
				                     new XmlInterpreter(new StaticContentResource(childContainerConfig.Value)));
			}
		}
#endif
	}
}