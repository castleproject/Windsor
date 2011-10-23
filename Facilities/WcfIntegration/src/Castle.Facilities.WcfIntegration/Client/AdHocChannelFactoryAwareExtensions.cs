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
	using System.ServiceModel;
	using System.ServiceModel.Channels;

	public static class AdHocChannelFactoryAwareExtensions
	{
		public static T OnCreated<T>(this WcfClientModel<T> clientModel, Action<ChannelFactory> action) where T : WcfClientModel<T>
		{
			return Subscribe<T>(clientModel, adHoc => adHoc.OnCreated(action));
		}

		public static T OnOpening<T>(this WcfClientModel<T> clientModel, Action<ChannelFactory> action) where T : WcfClientModel<T>
		{
			return Subscribe<T>(clientModel, adHoc => adHoc.OnOpening(action));
		}

		public static T OnOpened<T>(this WcfClientModel<T> clientModel, Action<ChannelFactory> action) where T : WcfClientModel<T>
		{
			return Subscribe<T>(clientModel, adHoc => adHoc.OnOpened(action));
		}

		public static T OnClosing<T>(this WcfClientModel<T> clientModel, Action<ChannelFactory> action) where T : WcfClientModel<T>
		{
			return Subscribe<T>(clientModel, adHoc => adHoc.OnClosing(action));
		}

		public static T OnClosed<T>(this WcfClientModel<T> clientModel, Action<ChannelFactory> action) where T : WcfClientModel<T>
		{
			return Subscribe<T>(clientModel, adHoc => adHoc.OnClosed(action));
		}

		public static T OnFaulted<T>(this WcfClientModel<T> clientModel, Action<ChannelFactory> action) where T : WcfClientModel<T>
		{
			return Subscribe<T>(clientModel, adHoc => adHoc.OnFaulted(action));
		}

		public static T OnChannelCreated<T>(this WcfClientModel<T> clientModel, Action<ChannelFactory, IChannel> action) where T : WcfClientModel<T>
		{
			return Subscribe<T>(clientModel, adHoc => adHoc.OnChannelCreated(action));
		}

		public static T OnChannelAvailable<T>(this WcfClientModel<T> clientModel, Action<ChannelFactory, IChannel> action) where T : WcfClientModel<T>
		{
			return Subscribe<T>(clientModel, adHoc => adHoc.OnChannelAvailable(action));
		}

		private static T Subscribe<T>(WcfClientModel<T> clientModel, Action<AdHocChannelFactoryAware> subscribe) where T : WcfClientModel<T>
		{
			var adHoc = new AdHocChannelFactoryAware();
			subscribe(adHoc);
			return clientModel.AddExtensions(adHoc);
		}
	}
}

