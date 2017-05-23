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
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core.Configuration;
	using Castle.Core.Resource;
	using Castle.MicroKernel.SubSystems.Resource;

	/// <summary>
	///   This implementation of <see cref = "IConfigurationStore" />
	///   does not try to obtain an external configuration by any means.
	///   Its only purpose is to serve as a base class for subclasses
	///   that might obtain the configuration node from anywhere.
	/// </summary>
	[Serializable]
	public class DefaultConfigurationStore : AbstractSubSystem, IConfigurationStore
	{
		private readonly IDictionary<string, IConfiguration> childContainers = new Dictionary<string, IConfiguration>();
		private readonly IDictionary<string, IConfiguration> components = new Dictionary<string, IConfiguration>();
		private readonly IDictionary<string, IConfiguration> facilities = new Dictionary<string, IConfiguration>();
		private readonly ICollection<IConfiguration> installers = new List<IConfiguration>();
		private readonly object syncLock = new object();

		/// <summary>
		///   Adds the child container configuration.
		/// </summary>
		/// <param name = "key">The key.</param>
		/// <param name = "config">The config.</param>
		public void AddChildContainerConfiguration(String key, IConfiguration config)
		{
			lock(syncLock)
			{
				childContainers[key] = config;
			}
		}

		/// <summary>
		///   Associates a configuration node with a component key
		/// </summary>
		/// <param name = "key">item key</param>
		/// <param name = "config">Configuration node</param>
		public void AddComponentConfiguration(String key, IConfiguration config)
		{
			lock (syncLock)
			{
				components[key] = config;
			}		
		}

		/// <summary>
		///   Associates a configuration node with a facility key
		/// </summary>
		/// <param name = "key">item key</param>
		/// <param name = "config">Configuration node</param>
		public void AddFacilityConfiguration(String key, IConfiguration config)
		{
			lock(syncLock)
			{
				facilities[key] = config;
			}
		}

		public void AddInstallerConfiguration(IConfiguration config)
		{
			lock(syncLock)
			{
				installers.Add(config);
			}
		}

		/// <summary>
		///   Returns the configuration node associated with
		///   the specified child container key. Should return null
		///   if no association exists.
		/// </summary>
		/// <param name = "key">item key</param>
		/// <returns></returns>
		public IConfiguration GetChildContainerConfiguration(String key)
		{
			lock(syncLock)
			{
				IConfiguration value;
				childContainers.TryGetValue(key, out value);
				return value;
			}
		}

		/// <summary>
		///   Returns the configuration node associated with
		///   the specified component key. Should return null
		///   if no association exists.
		/// </summary>
		/// <param name = "key">item key</param>
		/// <returns></returns>
		public IConfiguration GetComponentConfiguration(String key)
		{
			lock (syncLock)
			{
				IConfiguration value;
				components.TryGetValue(key, out value);
				return value;
			}
		}

		/// <summary>
		///   Returns all configuration nodes for components
		/// </summary>
		/// <returns></returns>
		public IConfiguration[] GetComponents()
		{
			lock (syncLock)
			{
				return components.Values.ToArray();
			}
		}

		/// <summary>
		///   Returns all configuration nodes for child containers
		/// </summary>
		/// <returns></returns>
		public IConfiguration[] GetConfigurationForChildContainers()
		{
			lock (syncLock)
			{
				return childContainers.Values.ToArray();
			}
		}

		/// <summary>
		///   Returns all configuration nodes for facilities
		/// </summary>
		/// <returns></returns>
		public IConfiguration[] GetFacilities()
		{
			lock (syncLock)
			{
				return facilities.Values.ToArray();
			}
		}

		/// <summary>
		///   Returns the configuration node associated with
		///   the specified facility key. Should return null
		///   if no association exists.
		/// </summary>
		/// <param name = "key">item key</param>
		/// <returns></returns>
		public IConfiguration GetFacilityConfiguration(String key)
		{
			lock (syncLock)
			{
				IConfiguration value;
				facilities.TryGetValue(key, out value);
				return value;
			}
		}

		public IConfiguration[] GetInstallers()
		{
			lock (syncLock)
			{
				return installers.ToArray();
			}
		}

		public IResource GetResource(String resourceUri, IResource resource)
		{
			if (resourceUri.IndexOf(Uri.SchemeDelimiter) == -1)
			{
				return resource.CreateRelative(resourceUri);
			}

			var subSystem = (IResourceSubSystem)Kernel.GetSubSystem(SubSystemConstants.ResourceKey);

			return subSystem.CreateResource(resourceUri, resource.FileBasePath);
		}

		public override void Terminate()
		{
		}
	}
}