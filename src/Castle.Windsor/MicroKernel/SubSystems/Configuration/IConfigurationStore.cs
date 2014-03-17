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

namespace Castle.MicroKernel.SubSystems.Configuration
{
	using System;

	using Castle.Core.Configuration;
	using Castle.Core.Resource;

	/// <summary>
	///   The contract used by the kernel to obtain
	///   external configuration for the components and
	///   facilities.
	/// </summary>
	public interface IConfigurationStore : ISubSystem
	{
		/// <summary>
		///   Adds the child container configuration.
		/// </summary>
		/// <param name = "name">The container's name.</param>
		/// <param name = "config">The config.</param>
		void AddChildContainerConfiguration(String name, IConfiguration config);

		/// <summary>
		///   Associates a configuration node with a component key
		/// </summary>
		/// <param name = "key">item key</param>
		/// <param name = "config">Configuration node</param>
		void AddComponentConfiguration(String key, IConfiguration config);

		/// <summary>
		///   Associates a configuration node with a facility key
		/// </summary>
		/// <param name = "key">item key</param>
		/// <param name = "config">Configuration node</param>
		void AddFacilityConfiguration(String key, IConfiguration config);

		void AddInstallerConfiguration(IConfiguration config);

		/// <summary>
		///   Returns the configuration node associated with 
		///   the specified child container key. Should return null
		///   if no association exists.
		/// </summary>
		/// <param name = "key">item key</param>
		/// <returns></returns>
		IConfiguration GetChildContainerConfiguration(String key);

		/// <summary>
		///   Returns the configuration node associated with 
		///   the specified component key. Should return null
		///   if no association exists.
		/// </summary>
		/// <param name = "key">item key</param>
		/// <returns></returns>
		IConfiguration GetComponentConfiguration(String key);

		/// <summary>
		///   Returns all configuration nodes for components
		/// </summary>
		/// <returns></returns>
		IConfiguration[] GetComponents();

		/// <summary>
		///   Gets the child containers configuration nodes.
		/// </summary>
		/// <returns></returns>
		IConfiguration[] GetConfigurationForChildContainers();

		/// <summary>
		///   Returns all configuration nodes for facilities
		/// </summary>
		/// <returns></returns>
		IConfiguration[] GetFacilities();

		/// <summary>
		///   Returns the configuration node associated with 
		///   the specified facility key. Should return null
		///   if no association exists.
		/// </summary>
		/// <param name = "key">item key</param>
		/// <returns></returns>
		IConfiguration GetFacilityConfiguration(String key);

		/// <summary>
		///   Returns all configuration nodes for installers
		/// </summary>
		/// <returns></returns>
		IConfiguration[] GetInstallers();

		/// <summary>
		/// </summary>
		/// <param name = "resourceUri"></param>
		/// <param name = "resource"></param>
		/// <returns></returns>
		IResource GetResource(String resourceUri, IResource resource);
	}
}