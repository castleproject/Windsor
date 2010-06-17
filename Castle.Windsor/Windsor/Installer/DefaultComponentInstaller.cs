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

namespace Castle.Windsor.Installer
{
	using System.Diagnostics;
	using System.Linq;
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.Core.Resource;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.MicroKernel.SubSystems.Conversion;
	using Castle.Windsor.Configuration.Interpreters;

	/// <summary>
	/// Default <see cref="IComponentsInstaller"/> implementation.
	/// </summary>
	public class DefaultComponentInstaller : IComponentsInstaller
	{
		private string assemblyName;

		#region IComponentsInstaller Members

		/// <summary>
		/// Perform installation.
		/// </summary>
		/// <param name="container">Target container</param>
		/// <param name="store">Configuration store</param>
		public void SetUp(IWindsorContainer container, IConfigurationStore store)
		{
			var converter = container.Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey) as IConversionManager;
			SetUpInstallers(store.GetInstallers(), container, converter);
			SetUpComponents(store.GetBootstrapComponents(), container, converter);
			SetUpFacilities(store.GetFacilities(), container, converter);
			SetUpComponents(store.GetComponents(), container, converter);
#if !SILVERLIGHT
			SetUpChildContainers(store.GetConfigurationForChildContainers(), container);
#endif
		}

		#endregion

		protected virtual void SetUpInstallers(IConfiguration[] installers, IWindsorContainer container,
		                                       IConversionManager converter)
		{
			var instances = new Dictionary<Type, IWindsorInstaller>();
			foreach (var installer in installers)
			{
				AddInstaller(installer, instances, converter);
			}

			if (instances.Count != 0)
			{
				container.Install(instances.Values.ToArray());
			}
		}

		private void AddInstaller(IConfiguration installer, Dictionary<Type, IWindsorInstaller> cache,
		                          IConversionManager conversionManager)
		{
			var typeName = installer.Attributes["type"];
			if (string.IsNullOrEmpty(typeName) == false)
			{
				var type = conversionManager.PerformConversion(typeName, typeof(Type)) as Type;
				AddInstaller(cache, type);
				return;
			}

			// TODO: extract this so that it has as much common with FromAssembly. as possible
			assemblyName = installer.Attributes["assembly"];
			if (string.IsNullOrEmpty(assemblyName) == false)
			{
				AddAssemblyInstallers(cache, assemblyName);
				return;
			}

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
				GetAssemblyInstallers(cache, assembly);
			}
		}

		private void AddAssemblyInstallers(Dictionary<Type, IWindsorInstaller> cache, string assemblyName)
		{
			var assembly = ReflectionUtil.GetAssemblyNamed(assemblyName);
			GetAssemblyInstallers(cache, assembly);
		}

		private void GetAssemblyInstallers(Dictionary<Type, IWindsorInstaller> cache, Assembly assembly)
		{
			var types = assembly.GetExportedTypes();
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
			       typeof(IWindsorInstaller).IsAssignableFrom(type);
		}

		private void AddInstaller(Dictionary<Type, IWindsorInstaller> cache, Type type)
		{
			if (cache.ContainsKey(type) == false)
			{
				var installerInstance = ReflectionUtil.CreateInstance<IWindsorInstaller>(type);
				cache.Add(type, installerInstance);
			}
		}

		protected virtual void SetUpFacilities(IConfiguration[] configurations, IWindsorContainer container,
		                                       IConversionManager converter)
		{
			foreach (IConfiguration facility in configurations)
			{
				var id = facility.Attributes["id"];
				var typeName = facility.Attributes["type"];
				if (string.IsNullOrEmpty(typeName))
				{
					continue;
				}

				var type = ObtainType(typeName, converter);

				var facilityInstance = ReflectionUtil.CreateInstance<IFacility>(type);

				Debug.Assert(id != null);
				Debug.Assert(facilityInstance != null);

				container.AddFacility(id, facilityInstance);
			}
		}

		protected virtual void SetUpComponents(IConfiguration[] configurations, IWindsorContainer container,
		                                       IConversionManager converter)
		{
			foreach (IConfiguration component in configurations)
			{
				var id = component.Attributes["id"];

				var typeName = component.Attributes["type"];
				var serviceTypeName = component.Attributes["service"];

				if (string.IsNullOrEmpty(typeName))
				{
					continue;
				}

				var type = ObtainType(typeName, converter);
				var service = type;

				if (!string.IsNullOrEmpty(serviceTypeName))
				{
					service = ObtainType(serviceTypeName, converter);
				}

				AssertImplementsService(id, service, type);

				Debug.Assert(id != null);
				Debug.Assert(type != null);
				Debug.Assert(service != null);

				container.Register(Component.For(service).ImplementedBy(type).Named(id));
				SetUpComponentForwardedTypes(container.Kernel as IKernelInternal, component, typeName, id, converter);
			}
		}

		private void AssertImplementsService(string id, Type service, Type type)
		{
			if (service.IsGenericTypeDefinition)
			{
				type = type.MakeGenericType(service.GetGenericArguments());
			}
			if (!service.IsAssignableFrom(type))
			{
				var message = string.Format("Could not set up component '{0}'. Type '{1}' does not implement service '{2}'", id,
				                            type.AssemblyQualifiedName, service.AssemblyQualifiedName);
				throw new Exception(message);
			}
		}

		private void SetUpComponentForwardedTypes(IKernelInternal kernel, IConfiguration component, string typeName, string id,
		                                          IConversionManager converter)
		{
			if (kernel == null)
			{
				return;
			}
			var forwardedTypes = component.Children["forwardedTypes"];
			if (forwardedTypes == null)
			{
				return;
			}

			var forwarded = new List<Type>();
			foreach (var forwardedType in forwardedTypes.Children
				.Where(c => c.Name.Equals("add", StringComparison.InvariantCultureIgnoreCase)))
			{
				var forwardedServiceTypeName = forwardedType.Attributes["service"];
				try
				{
					forwarded.Add(ObtainType(forwardedServiceTypeName, converter));
				}
				catch (Exception e)
				{
					throw new Exception(
						string.Format("Component {0}-{1} defines invalid forwarded type.", id ?? string.Empty, typeName), e);
				}
			}

			foreach (var forwadedType in forwarded)
			{
				kernel.RegisterHandlerForwarding(forwadedType, id);
			}
		}

#if !SILVERLIGHT
		private static void SetUpChildContainers(IConfiguration[] configurations, IWindsorContainer parentContainer)
		{
			foreach (IConfiguration childContainerConfig in configurations)
			{
				var id = childContainerConfig.Attributes["name"];

				Debug.Assert(id != null);

				new WindsorContainer(id, parentContainer,
				                     new XmlInterpreter(new StaticContentResource(childContainerConfig.Value)));
			}
		}
#endif

		private static Type ObtainType(String typeName, IConversionManager converter)
		{
			return (Type)converter.PerformConversion(typeName, typeof(Type));
		}
	}
}