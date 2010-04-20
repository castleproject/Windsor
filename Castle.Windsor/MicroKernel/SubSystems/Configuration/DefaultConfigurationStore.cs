// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	using System.Runtime.CompilerServices;

	using Castle.Core.Configuration;
	using Castle.Core.Resource;
	using Castle.MicroKernel.SubSystems.Resource;

	/// <summary>
	/// This implementation of <see cref="IConfigurationStore"/>
	/// does not try to obtain an external configuration by any means.
	/// Its only purpose is to serve as a base class for subclasses
	/// that might obtain the configuration node from anywhere.
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class DefaultConfigurationStore : AbstractSubSystem, IConfigurationStore
	{
		private readonly IDictionary<string, IConfiguration> childContainers = new Dictionary<string, IConfiguration>();
		private readonly IDictionary<string, IConfiguration> facilities = new Dictionary<string, IConfiguration>();
		private readonly IDictionary<string, IConfiguration> components = new Dictionary<string, IConfiguration>();
		private readonly IDictionary<string, IConfiguration> bootstrapcomponents = new Dictionary<string, IConfiguration>();
		private readonly ICollection<IConfiguration> installers = new List<IConfiguration>();

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultConfigurationStore"/> class.
		/// </summary>
		public DefaultConfigurationStore()
		{
		}

		public override void Init(IKernel kernel)
		{
			base.Init(kernel);
			Kernel.ComponentUnregistered += CleanUpComponentConfiguration;
		}

		private void CleanUpComponentConfiguration(string key, IHandler handler)
		{
			if (key == null)
			{
				return;
			}

			if (components.Remove(key))
			{
				return;
			}

			bootstrapcomponents.Remove(key);
		}

		public override void Terminate()
		{
			Kernel.ComponentUnregistered -= CleanUpComponentConfiguration;
		}

		/// <summary>
		/// Associates a configuration node with a facility key
		/// </summary>
		/// <param name="key">item key</param>
		/// <param name="config">Configuration node</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddFacilityConfiguration(String key, IConfiguration config)
		{
			facilities[key] = config;
		}

		/// <summary>
		/// Associates a configuration node with a component key
		/// </summary>
		/// <param name="key">item key</param>
		/// <param name="config">Configuration node</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddComponentConfiguration(String key, IConfiguration config)
		{
			components[key] = config;
		}

		/// <summary>
		/// Associates a configuration node with a bootstrap component key
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddBootstrapComponentConfiguration(string key, IConfiguration config)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds the child container configuration.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="config">The config.</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddChildContainerConfiguration(String key, IConfiguration config)
		{
			childContainers[key] = config;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void AddInstallerConfiguration(IConfiguration config)
		{
			installers.Add(config);
		}

		/// <summary>
		/// Returns the configuration node associated with
		/// the specified facility key. Should return null
		/// if no association exists.
		/// </summary>
		/// <param name="key">item key</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public IConfiguration GetFacilityConfiguration(String key)
		{
			IConfiguration value;
			facilities.TryGetValue(key, out value);
			return value;
		}

		/// <summary>
		/// Returns the configuration node associated with
		/// the specified child container key. Should return null
		/// if no association exists.
		/// </summary>
		/// <param name="key">item key</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public IConfiguration GetChildContainerConfiguration(String key)
		{
			IConfiguration value;
			childContainers.TryGetValue(key, out value);
			return value;
		}

		/// <summary>
		/// Returns the configuration node associated with
		/// the specified component key. Should return null
		/// if no association exists.
		/// </summary>
		/// <param name="key">item key</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public IConfiguration GetComponentConfiguration(String key)
		{
			IConfiguration value;
			components.TryGetValue(key, out value);
			return value;
		}

		/// <summary>
		/// Returns the configuration node associated with 
		/// the specified component key. Should return null
		/// if no association exists.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public IConfiguration GetBootstrapComponentConfiguration(string key)
		{
			IConfiguration value;
			bootstrapcomponents.TryGetValue(key, out value);
			return value;
		}

		/// <summary>
		/// Returns all configuration nodes for facilities
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public IConfiguration[] GetFacilities()
		{
			return facilities.Values.ToArray();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public IConfiguration[] GetInstallers()
		{
			return installers.ToArray();
		}

		/// <summary>
		/// Returns all configuration nodes for bootstrap components
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public IConfiguration[] GetBootstrapComponents()
		{
			return bootstrapcomponents.Values.ToArray();
		}

		/// <summary>
		/// Returns all configuration nodes for child containers
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public IConfiguration[] GetConfigurationForChildContainers()
		{
			return childContainers.Values.ToArray();
		}

		/// <summary>
		/// Returns all configuration nodes for components
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public IConfiguration[] GetComponents()
		{
			return components.Values.ToArray();
		}

		public IResource GetResource(String resourceUri, IResource resource)
		{
			if (resourceUri.IndexOf(Uri.SchemeDelimiter) == -1)
			{
				return resource.CreateRelative(resourceUri);
			}

			IResourceSubSystem subSystem = (IResourceSubSystem)
				Kernel.GetSubSystem(SubSystemConstants.ResourceKey);

			return subSystem.CreateResource(resourceUri, resource.FileBasePath);
		}
	}
}
