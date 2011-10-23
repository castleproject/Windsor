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

	using Castle.Core.Internal;
	using Castle.MicroKernel.Registration;

	/// <summary>
	///   Helper class used by <see cref = "FromAssembly" /> to filter/order and instantiate <see cref = "IWindsorInstaller" /> implementations
	/// </summary>
	public class InstallerFactory
	{
		/// <summary>
		///   Performs custom instantiation of given <param name = "installerType" />
		/// </summary>
		/// <remarks>
		///   Default implementation uses public parameterless constructor to create the instance.
		/// </remarks>
		public virtual IWindsorInstaller CreateInstance(Type installerType)
		{
			return installerType.CreateInstance<IWindsorInstaller>();
		}

		/// <summary>
		///   Performs custom filtering/ordering of given set of types.
		/// </summary>
		/// <param name = "installerTypes">Set of concrete class types implementing <see cref = "IWindsorInstaller" /> interface.</param>
		/// <returns>Transformed <paramref name = "installerTypes" />.</returns>
		/// <remarks>
		///   Default implementation simply returns types passed into it.
		/// </remarks>
		public virtual IEnumerable<Type> Select(IEnumerable<Type> installerTypes)
		{
			return installerTypes;
		}
	}
}