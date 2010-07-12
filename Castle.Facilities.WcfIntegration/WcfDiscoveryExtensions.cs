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

	public static class WcfDiscoveryExtensions
	{
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
	}
}
