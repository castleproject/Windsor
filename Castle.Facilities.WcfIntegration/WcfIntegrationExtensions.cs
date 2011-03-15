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

namespace Castle.Facilities.WcfIntegration
{
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceModel.Description;
	using Castle.MicroKernel.Registration;

	public static class WcfIntegrationExtensions
	{
#if DOTNET40
		public static IEnumerable<ServiceEndpoint> SystemEndpoints(this ServiceDescription description)
		{
			return description.Endpoints.Where(endpoint => endpoint.IsSystemEndpoint);
		}

		public static IEnumerable<ServiceEndpoint> NonSystemEndpoints(this ServiceDescription description)
		{
			return description.Endpoints.Where(endpoint => endpoint.IsSystemEndpoint == false);
		}
#endif

#pragma warning disable 612,618
		public static ComponentRegistration<T> AsWcfClient<T>(this ComponentRegistration<T> registration)
		{
			return registration.ActAs(new DefaultClientModel());
		}

		public static ComponentRegistration<T> AsWcfClient<T>(this ComponentRegistration<T> registration,
															  params IWcfClientModel[] clientModels)
		{
			return registration.ActAs(clientModels);
		}

		public static ComponentRegistration<T> AsWcfClient<T>(this ComponentRegistration<T> registration, IWcfEndpoint endpoint)
		{
			return registration.ActAs(new DefaultClientModel(endpoint));
		}

		public static ComponentRegistration<T> AsWcfService<T>(this ComponentRegistration<T> registration)
		{
			return registration.ActAs(new DefaultServiceModel());
		}

		public static ComponentRegistration<T> AsWcfService<T>(this ComponentRegistration<T> registration,
															   params IWcfServiceModel[] serviceModels)
		{
			return registration.ActAs(serviceModels);
		}
#pragma warning restore 612,618
	}
}
