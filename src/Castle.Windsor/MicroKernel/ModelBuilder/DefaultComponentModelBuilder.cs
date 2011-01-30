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

namespace Castle.MicroKernel.ModelBuilder
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.MicroKernel.ModelBuilder.Inspectors;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	///   Summary description for DefaultComponentModelBuilder.
	/// </summary>
	[Serializable]
	public class DefaultComponentModelFactory : IComponentModelFactory
	{
		private readonly List<IContributeComponentModelConstruction> contributors = new List<IContributeComponentModelConstruction>();
		private readonly IKernel kernel;

		/// <summary>
		///   Initializes a new instance of the <see cref = "DefaultComponentModelFactory" /> class.
		/// </summary>
		/// <param name = "kernel">The kernel.</param>
		public DefaultComponentModelFactory(IKernel kernel)
		{
			this.kernel = kernel;
			InitializeContributors();
		}

		/// <summary>
		///   Gets the contributors.
		/// </summary>
		/// <value>The contributors.</value>
		public IContributeComponentModelConstruction[] Contributors
		{
			get { return contributors.ToArray(); }
		}

		/// <summary>
		///   "To give or supply in common with others; give to a
		///   common fund or for a common purpose". The contributor
		///   should inspect the component, or even the configuration
		///   associated with the component, to add or change information
		///   in the model that can be used later.
		/// </summary>
		/// <param name = "contributor"></param>
		public void AddContributor(IContributeComponentModelConstruction contributor)
		{
			contributors.Add(contributor);
		}

		/// <summary>
		///   Constructs a new ComponentModel by invoking
		///   the registered contributors.
		/// </summary>
		/// <param name = "name"></param>
		/// <param name = "services"></param>
		/// <param name = "classType"></param>
		/// <param name = "extendedProperties"></param>
		/// <returns></returns>
		public ComponentModel BuildModel(ComponentName name, Type[] services, Type classType, IDictionary extendedProperties)
		{
			var model = new ComponentModel(name, services, classType, extendedProperties);
			contributors.ForEach(c => c.ProcessModel(kernel, model));

			return model;
		}

		public ComponentModel BuildModel(IContributeComponentModelConstruction[] customContributors)
		{
			var model = new ComponentModel();
			Array.ForEach(customContributors, c => c.ProcessModel(kernel, model));
			contributors.ForEach(c => c.ProcessModel(kernel, model));
			return model;
		}

		/// <summary>
		///   Removes the specified contributor
		/// </summary>
		/// <param name = "contributor"></param>
		public void RemoveContributor(IContributeComponentModelConstruction contributor)
		{
			contributors.Remove(contributor);
		}

		/// <summary>
		///   Initializes the default contributors.
		/// </summary>
		protected virtual void InitializeContributors()
		{
			var conversionManager = kernel.GetConversionManager();
			AddContributor(new GenericInspector());
			AddContributor(new ConfigurationModelInspector());
			AddContributor(new ConfigurationParametersInspector());
			AddContributor(new LifestyleModelInspector(conversionManager));
			AddContributor(new ConstructorDependenciesModelInspector(conversionManager));
			AddContributor(new PropertiesDependenciesModelInspector(conversionManager));
			AddContributor(new LifecycleModelInspector());
			AddContributor(new InterceptorInspector());
			AddContributor(new MixinInspector());
			AddContributor(new ComponentActivatorInspector(conversionManager));
			AddContributor(new ComponentProxyInspector(conversionManager));
		}
	}
}