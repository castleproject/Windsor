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

namespace Castle.MicroKernel.SubSystems.Resource
{
	using System;
	using System.Collections.Generic;

	using Castle.Core.Resource;

	/// <summary>
	///   Pendent
	/// </summary>
	public class DefaultResourceSubSystem : AbstractSubSystem, IResourceSubSystem
	{
		private readonly List<IResourceFactory> resourceFactories = new List<IResourceFactory>();

		public DefaultResourceSubSystem()
		{
			InitDefaultResourceFactories();
		}

		public IResource CreateResource(String resource)
		{
			if (resource == null)
			{
				throw new ArgumentNullException("resource");
			}

			return CreateResource(new CustomUri(resource));
		}

		public IResource CreateResource(String resource, String basePath)
		{
			if (resource == null)
			{
				throw new ArgumentNullException("resource");
			}

			return CreateResource(new CustomUri(resource), basePath);
		}

		public IResource CreateResource(CustomUri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}

			foreach (var resFactory in resourceFactories)
			{
				if (resFactory.Accept(uri))
				{
					return resFactory.Create(uri);
				}
			}

			throw new KernelException("No Resource factory was able to " +
			                          "deal with Uri " + uri);
		}

		public IResource CreateResource(CustomUri uri, String basePath)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			if (basePath == null)
			{
				throw new ArgumentNullException("basePath");
			}

			foreach (var resFactory in resourceFactories)
			{
				if (resFactory.Accept(uri))
				{
					return resFactory.Create(uri, basePath);
				}
			}

			throw new KernelException("No Resource factory was able to " +
			                          "deal with Uri " + uri);
		}

		public void RegisterResourceFactory(IResourceFactory resourceFactory)
		{
			if (resourceFactory == null)
			{
				throw new ArgumentNullException("resourceFactory");
			}

			resourceFactories.Add(resourceFactory);
		}

		protected virtual void InitDefaultResourceFactories()
		{
			RegisterResourceFactory(new AssemblyResourceFactory());
			RegisterResourceFactory(new UncResourceFactory());
#if !SILVERLIGHT
			RegisterResourceFactory(new FileResourceFactory());
#if FEATURE_SYSTEM_CONFIGURATION
			RegisterResourceFactory(new ConfigResourceFactory());
#endif
#endif
        }
    }
}