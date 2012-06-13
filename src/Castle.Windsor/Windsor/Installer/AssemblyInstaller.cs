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
	using System.Linq;
	using System.Reflection;

	using Castle.Core.Internal;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;

	public class AssemblyInstaller : IWindsorInstaller
	{
		private readonly Assembly assembly;
		private readonly InstallerFactory factory;

		public AssemblyInstaller(Assembly assembly, InstallerFactory factory)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			this.assembly = assembly;
			this.factory = factory;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			var installerTypes = factory.Select(FilterInstallerTypes(assembly.GetAvailableTypes()));
			if (installerTypes == null)
			{
				return;
			}

			foreach (var installerType in installerTypes)
			{
				var installer = factory.CreateInstance(installerType);
				installer.Install(container, store);
			}
		}

		private IEnumerable<Type> FilterInstallerTypes(IEnumerable<Type> types)
		{
			return types.Where(t => t.IsClass &&
			                        t.IsAbstract == false &&
			                        t.IsGenericTypeDefinition == false &&
			                        t.Is<IWindsorInstaller>());
		}
	}
}