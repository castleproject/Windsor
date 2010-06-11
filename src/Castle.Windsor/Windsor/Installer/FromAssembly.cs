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
	using System;
	using System.Reflection;

	using Castle.Core.Internal;

	public class FromAssembly
	{
		/// <summary>
		/// Scans the assembly containing specified type for types implementing <see cref="IWindsorInstaller"/>, instantiates them and installs.
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
		/// Scans the assembly containing specified type for types implementing <see cref="IWindsorInstaller"/>, instantiates using given <see cref="InstallerFactory"/> and installs.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Containing(Type type, InstallerFactory installerFactory)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			var assembly = type.Assembly;
			return Given(assembly, installerFactory);
		}

		/// <summary>
		/// Scans the assembly containing specified type for types implementing <see cref="IWindsorInstaller"/>, instantiates them and installs.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Containing<T>()
		{
			return Containing(typeof(T));
		}

		/// <summary>
		/// Scans the assembly containing specified type for types implementing <see cref="IWindsorInstaller"/>, instantiates using given <see cref="InstallerFactory"/> and installs.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Containing<T>(InstallerFactory installerFactory)
		{
			return Containing(typeof(T), installerFactory);
		}

		/// <summary>
		/// Scans the specified assembly with specified name for types implementing <see cref="IWindsorInstaller"/>, instantiates them and installs.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Instance(Assembly assembly)
		{
			return Given(assembly, new InstallerFactory());
		}

		/// <summary>
		/// Scans the specified assembly with specified name for types implementing <see cref="IWindsorInstaller"/>, instantiates using given <see cref="InstallerFactory"/> and installs.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Given(Assembly assembly, InstallerFactory installerFactory)
		{
			return new AssemblyInstaller(assembly, installerFactory);
		}

		/// <summary>
		/// Scans the assembly with specified name for types implementing <see cref="IWindsorInstaller"/>, instantiates them and installs.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Named(string assemblyName)
		{
			var assembly = ReflectionUtil.GetAssemblyNamed(assemblyName);
			return Instance(assembly);
		}

		/// <summary>
		/// Scans the assembly with specified name for types implementing <see cref="IWindsorInstaller"/>, instantiates using given <see cref="InstallerFactory"/> and installs.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller Named(string assemblyName, InstallerFactory installerFactory)
		{
			var assembly = ReflectionUtil.GetAssemblyNamed(assemblyName);
			return Given(assembly, installerFactory);
		}

		/// <summary>
		/// Scans assembly that contains code calling this method for types implementing <see cref="IWindsorInstaller"/>, instantiates them and installs.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller This()
		{
			return Instance(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Scans assembly that contains code calling this method for types implementing <see cref="IWindsorInstaller"/>, instantiates using given <see cref="InstallerFactory"/> and installs.
		/// </summary>
		/// <returns></returns>
		public static IWindsorInstaller This(InstallerFactory installerFactory)
		{
			return Given(Assembly.GetCallingAssembly(), installerFactory);
		}
	}
}