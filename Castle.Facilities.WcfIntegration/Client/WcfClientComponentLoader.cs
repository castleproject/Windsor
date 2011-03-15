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

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.Collections;
	using System.Linq;
	using System.ServiceModel;
	using Castle.Facilities.WcfIntegration.Internal;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	public class WcfClientComponentLoader : ILazyComponentLoader
	{
		public IRegistration Load(string key, Type service, IDictionary arguments)
		{
			if (service == typeof(IWcfClientFactory))
			{
				throw new FacilityException(
					"The IWcfClientFactory is only available with the TypedFactoryFacility.  " +
					"Did you forget to register that facility? Also make sure that TypedFactoryFacility was registred before WcfFacility.");
			}

			if (service == null || IsServiceContract(service) == false)
			{
				return null;
			}

			var clientModel = WcfUtils.FindDependencies<IWcfClientModel>(arguments)
				.FirstOrDefault() ?? new DefaultClientModel();

			var endpoint = WcfUtils.FindDependencies<IWcfEndpoint>(arguments).FirstOrDefault();

			if (endpoint != null)
			{
				clientModel = clientModel.ForEndpoint(endpoint);
			}
			else if (clientModel.Endpoint == null && string.IsNullOrEmpty(key) == false)
			{
				clientModel = clientModel.ForEndpoint(WcfEndpoint.FromConfiguration(key));
			}

			return Component.For(service).Named(key).LifeStyle.Transient.AsWcfClient(clientModel);
		}

		private static bool IsServiceContract(Type service)
		{
			if (service.IsDefined(typeof(ServiceContractAttribute), true))
			{
				return true;
			}

			if (service.IsInterface)
			{
				return service.GetInterfaces().Any(parent => IsServiceContract(parent));
			}

			return false;
		}
	}
}