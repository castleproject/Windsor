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

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using Castle.MicroKernel.Registration;

	public static class WcfIntegrationExtensions
	{
		public static T PublishMEX<T>(this WcfServiceModel<T> serviceModel)
			where T : WcfServiceModel<T>
		{
			serviceModel.AddExtensions(new WcfMexExtension());
			return (T)serviceModel;
		}

		public static T PublishMEX<T>(this WcfServiceModel<T> serviceModel, Action<WcfMexExtension> mex)
				where T : WcfServiceModel<T>
		{
			var mexExtension = new WcfMexExtension();
			if (mex != null) mex(mexExtension);
			serviceModel.AddExtensions(mexExtension);
			return (T)serviceModel;
		}

		public static T Discoverable<T>(this WcfServiceModel<T> serviceModel)
			where T : WcfServiceModel<T>
		{
			serviceModel.AddExtensions(new WcfDiscoveryExtension());
			return (T)serviceModel;
		}

		public static T Discoverable<T>(this WcfServiceModel<T> serviceModel, Action<WcfDiscoveryExtension> discover)
			where T : WcfServiceModel<T>
		{
			var discoveryExtension = new WcfDiscoveryExtension();
			if (discover != null) discover(discoveryExtension);
			serviceModel.AddExtensions(discoveryExtension);
			return (T)serviceModel;
		}

		public static ComponentRegistration<T> AsWcfClient<T>(this ComponentRegistration<T> registration)
		{
			return registration.ActAs(new DefaultClientModel());
		}

		public static ComponentRegistration<T> AsWcfClient<T>(this ComponentRegistration<T> registration,
															  params IWcfClientModel[] clientModels)
		{
			return registration.ActAs(clientModels);
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
	}
}
