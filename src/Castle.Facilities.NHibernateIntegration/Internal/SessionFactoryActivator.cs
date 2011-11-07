#region License

//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 

#endregion

namespace Castle.Facilities.NHibernateIntegration.Internal
{
	using Core;
	using MicroKernel;
	using MicroKernel.ComponentActivator;
	using MicroKernel.Context;
	using NHibernate;
	using NHibernate.Cfg;

	/// <summary>
	/// Postpones the initiation of SessionFactory until Resolve
	/// </summary>
	public class SessionFactoryActivator : DefaultComponentActivator
	{
		/// <summary>
		/// Constructor for SessionFactoryActivator
		/// </summary>
		/// <param name="model"></param>
		/// <param name="kernel"></param>
		/// <param name="onCreation"></param>
		/// <param name="onDestruction"></param>
		public SessionFactoryActivator(ComponentModel model, IKernel kernel,
		                               ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
		}

		/// <summary>
		/// Calls the contributors
		/// </summary>
		protected virtual void RaiseCreatingSessionFactory()
		{
			var configuration = Model.ExtendedProperties[Constants.SessionFactoryConfiguration] as Configuration;
			var contributors = Kernel.ResolveAll<IConfigurationContributor>();
			foreach (var contributor in contributors)
			{
				contributor.Process(Model.Name, configuration);
			}
		}

		/// <summary>
		/// Creates the <see cref="ISessionFactory"/> from the configuration
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public override object Create(CreationContext context)
		{
			RaiseCreatingSessionFactory();
			var configuration = Model.ExtendedProperties[Constants.SessionFactoryConfiguration]
			                    as Configuration;
			return configuration.BuildSessionFactory();
		}
	}
}