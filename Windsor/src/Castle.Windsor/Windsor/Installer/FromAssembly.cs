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
	using System.Reflection;

	using Castle.Core.Internal;
	using Castle.MicroKernel.Registration;

	public class FromAssembly
	{
		/// <summary>
		///   Scans the assembly containing specified type for types implementing <see cref = "IWindsorInstaller" />, instantiates them and returns so that <see
		///    cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Containing(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			var assembly = type.Assembly;
			return Instance(assembly);
		}

		/// <summary>
		///   Scans the assembly containing specified type for types implementing <see cref = "IWindsorInstaller" />, instantiates using given <see
		///    cref = "InstallerFactory" /> and returns so that <see cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Containing(Type type, InstallerFactory installerFactory)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			var assembly = type.Assembly;
			return Instance(assembly, installerFactory);
		}

		/// <summary>
		///   Scans the assembly containing specified type for types implementing <see cref = "IWindsorInstaller" />, instantiates them and returns so that <see
		///    cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Containing<T>()
		{
			return Containing(typeof(T));
		}

		/// <summary>
		///   Scans the assembly containing specified type for types implementing <see cref = "IWindsorInstaller" />, instantiates using given <see
		///    cref = "InstallerFactory" /> and returns so that <see cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Containing<T>(InstallerFactory installerFactory)
		{
			return Containing(typeof(T), installerFactory);
		}

		/// <summary>
		///   Scans assemblies in directory specified by <paramref name = "filter" /> for types implementing <see
		///    cref = "IWindsorInstaller" />, instantiates and returns so that <see cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <param name = "filter"></param>
		/// <returns></returns>
		public static IWindsorInstaller InDirectory(AssemblyFilter filter)
		{
			return InDirectory(filter, new InstallerFactory());
		}

		/// <summary>
		///   Scans assemblies in directory specified by <paramref name = "filter" /> for types implementing <see
		///    cref = "IWindsorInstaller" />, instantiates using given <see cref = "InstallerFactory" /> and returns so that <see
		///    cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <param name = "filter"></param>
		/// <param name = "installerFactory"></param>
		/// <returns></returns>
		public static IWindsorInstaller InDirectory(AssemblyFilter filter, InstallerFactory installerFactory)
		{
			var assemblies = new HashSet<Assembly>(ReflectionUtil.GetAssemblies(filter));
			var installer = new CompositeInstaller();
			foreach (var assembly in assemblies)
			{
				installer.Add(Instance(assembly, installerFactory));
			}
			return installer;
		}

		/// <summary>
		///   Scans current assembly and all refernced assemblies with the same first part of the name for types implementing <see
		///    cref = "IWindsorInstaller" />, instantiates and returns so that <see cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		///   Assemblies are considered to belong to the same application based on the first part of the name.
		///   For example if the method is called from within <c>MyApp.exe</c> and <c>MyApp.exe</c> references <c>MyApp.SuperFeatures.dll</c>,
		///   <c>mscorlib.dll</c> and <c>ThirdPartyCompany.UberControls.dll</c> the <c>MyApp.exe</c> and <c>MyApp.SuperFeatures.dll</c> 
		///   will be scanned for installers, and other assemblies will be ignored.
		/// </remarks>
		public static IWindsorInstaller InThisApplication()
		{
			var assembly = Assembly.GetCallingAssembly();
			return ApplicationAssemblies(assembly, new InstallerFactory());
		}

		/// <summary>
		///   Scans current assembly and all refernced assemblies with the same first part of the name for types implementing <see
		///    cref = "IWindsorInstaller" />, instantiates using given <see cref = "InstallerFactory" /> and returns so that <see
		///    cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <param name = "installerFactory"></param>
		/// <returns></returns>
		/// <remarks>
		///   Assemblies are considered to belong to the same application based on the first part of the name.
		///   For example if the method is called from within <c>MyApp.exe</c> and <c>MyApp.exe</c> references <c>MyApp.SuperFeatures.dll</c>,
		///   <c>mscorlib.dll</c> and <c>ThirdPartyCompany.UberControls.dll</c> the <c>MyApp.exe</c> and <c>MyApp.SuperFeatures.dll</c> 
		///   will be scanned for installers, and other assemblies will be ignored.
		/// </remarks>
		public static IWindsorInstaller InThisApplication(InstallerFactory installerFactory)
		{
			var assembly = Assembly.GetCallingAssembly();
			return ApplicationAssemblies(assembly, installerFactory);
		}

		/// <summary>
		///   Scans the specified assembly with specified name for types implementing <see cref = "IWindsorInstaller" />, instantiates them and returns so that <see
		///    cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Instance(Assembly assembly)
		{
			return Instance(assembly, new InstallerFactory());
		}

		/// <summary>
		///   Scans the specified assembly with specified name for types implementing <see cref = "IWindsorInstaller" />, instantiates using given <see
		///    cref = "InstallerFactory" /> and returns so that <see cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Instance(Assembly assembly, InstallerFactory installerFactory)
		{
			return new AssemblyInstaller(assembly, installerFactory);
		}

		/// <summary>
		///   Scans the assembly with specified name for types implementing <see cref = "IWindsorInstaller" />, instantiates them and returns so that <see
		///    cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Named(string assemblyName)
		{
			var assembly = ReflectionUtil.GetAssemblyNamed(assemblyName);
			return Instance(assembly);
		}

		/// <summary>
		///   Scans the assembly with specified name for types implementing <see cref = "IWindsorInstaller" />, instantiates using given <see
		///    cref = "InstallerFactory" /> and returns so that <see cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Named(string assemblyName, InstallerFactory installerFactory)
		{
			var assembly = ReflectionUtil.GetAssemblyNamed(assemblyName);
			return Instance(assembly, installerFactory);
		}

		/// <summary>
		///   Scans assembly that contains code calling this method for types implementing <see cref = "IWindsorInstaller" />, 
		///   instantiates them and returns so that <see cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller This()
		{
			return Instance(Assembly.GetCallingAssembly());
		}

		/// <summary>
		///   Scans assembly that contains code calling this method for types implementing <see cref = "IWindsorInstaller" />, instantiates using given <see
		///    cref = "InstallerFactory" /> and returns so that <see cref = "IWindsorContainer.Install" /> can install them.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller This(InstallerFactory installerFactory)
		{
			return Instance(Assembly.GetCallingAssembly(), installerFactory);
		}

		private static IWindsorInstaller ApplicationAssemblies(Assembly rootAssembly, InstallerFactory installerFactory)
		{
			var assemblies = new HashSet<Assembly>(ReflectionUtil.GetApplicationAssemblies(rootAssembly));
			var installer = new CompositeInstaller();
			foreach (var assembly in assemblies)
			{
				installer.Add(Instance(assembly, installerFactory));
			}
			return installer;
		}
	}
}