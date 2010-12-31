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

namespace Castle.Windsor.Configuration.Interpreters
{
	using System.Collections.Generic;
	using System;

	using Castle.Core.Resource;
	using Castle.Core.Configuration;
	using Castle.MicroKernel.SubSystems.Configuration;

	/// <summary>
	///   Provides common methods for those who wants 
	///   to implement <see cref = "IConfigurationInterpreter" />
	/// </summary>
	public abstract class AbstractInterpreter : IConfigurationInterpreter
	{
		protected static readonly string ContainersNodeName = "containers";
		protected static readonly string ContainerNodeName = "container";
		protected static readonly string FacilitiesNodeName = "facilities";
		protected static readonly string FacilityNodeName = "facility";
		protected static readonly string ComponentsNodeName = "components";
		protected static readonly string ComponentNodeName = "component";
		protected static readonly string InstallersNodeName = "installers";
		protected static readonly string InstallNodeName = "install";

		private readonly IResource source;
		private readonly Stack<IResource> resourceStack = new Stack<IResource>();

		protected AbstractInterpreter(IResource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source", "IResource is null");
			}

			this.source = source;

			PushResource(source);
		}

		public AbstractInterpreter(string filename) : this(new FileResource(filename))
		{
		}

#if !SILVERLIGHT
		public AbstractInterpreter() : this(new ConfigResource())
		{
		}
#endif

		/// <summary>
		///   Should obtain the contents from the resource,
		///   interpret it and populate the <see cref = "IConfigurationStore" />
		///   accordingly.
		/// </summary>
		/// <param name = "resource"></param>
		/// <param name = "store"></param>
		public abstract void ProcessResource(IResource resource, IConfigurationStore store);

		protected void PushResource(IResource resource)
		{
			resourceStack.Push(resource);
		}

		protected void PopResource()
		{
			resourceStack.Pop();
		}

		protected IResource CurrentResource
		{
			get
			{
				if (resourceStack.Count == 0)
				{
					return null;
				}

				return resourceStack.Peek();
			}
		}

		/// <summary>
		///   Exposes the reference to <see cref = "IResource" />
		///   which the interpreter is likely to hold
		/// </summary>
		/// <value></value>
		public IResource Source
		{
			get { return source; }
		}

		/// <summary>
		///   Gets or sets the name of the environment.
		/// </summary>
		/// <value>The name of the environment.</value>
		public string EnvironmentName { get; set; }

		protected static void AddChildContainerConfig(string name, IConfiguration childContainer, IConfigurationStore store)
		{
			AssertValidId(name);

			// TODO: Use import collection on type attribute (if it exists)

			store.AddChildContainerConfiguration(name, childContainer);
		}

		protected static void AddFacilityConfig(string id, IConfiguration facility, IConfigurationStore store)
		{
			AssertValidId(id);

			// TODO: Use import collection on type attribute (if it exists)

			store.AddFacilityConfiguration(id, facility);
		}

		protected static void AddComponentConfig(string id, IConfiguration component, IConfigurationStore store)
		{
			AssertValidId(id);

			// TODO: Use import collection on type and service attribute (if they exist)

			store.AddComponentConfiguration(id, component);
		}

		protected static void AddInstallerConfig(IConfiguration installer, IConfigurationStore store)
		{
			store.AddInstallerConfiguration(installer);
		}

		private static void AssertValidId(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				const string message = "Component or Facility was declared without a proper 'id' or 'type' attribute.";
				throw new Exception(message);
			}
		}
	}
}