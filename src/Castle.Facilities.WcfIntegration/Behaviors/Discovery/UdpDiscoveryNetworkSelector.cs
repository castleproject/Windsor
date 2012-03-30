// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
#if DOTNET40
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Net.NetworkInformation;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Discovery;
	using System.Text.RegularExpressions;

	public class UdpDiscoveryNetworkSelector : IChannelFactoryAware, IServiceHostAware
	{
		private string networkId;
		private Regex networkNameRegex;
		private string networkNamePattern;
		private Regex dnsSuffixRegex;
		private string dnxSuffixPattern;
		private IEnumerable<NetworkInterfaceType> supportedNetworkTypes;
		private IEnumerable<Func<NetworkInterface, IPInterfaceProperties, bool>> networkSelectors;

		public string NetworkId
		{
			get { return networkId; }
			set { networkId = value; }
		}

		public string NetworkNamePattern
		{
			get { return networkNamePattern; }
			set
            {
				networkNamePattern = value;
				networkNameRegex = networkNamePattern != null
					? new Regex(networkNamePattern)
					: null;
            }
		}

		public string DnsSuffixPattern
		{
			get { return networkNamePattern; }
			set
			{
				dnxSuffixPattern = value;
				dnsSuffixRegex = dnxSuffixPattern != null
					? new Regex(dnxSuffixPattern)
					: null;
			}
		}

		public IEnumerable<NetworkInterfaceType> SupportedNetworkTypes
		{
			get { return supportedNetworkTypes; }
			set { supportedNetworkTypes = value; }
		}

		public IEnumerable<Func<NetworkInterface, IPInterfaceProperties, bool>> NetworkSelectors
		{
			get { return networkSelectors; }
			set { networkSelectors = value; }
		}

		#region IChannelFactoryAware

		void IChannelFactoryAware.Created(ChannelFactory channelFactory)
		{
		}

		void IChannelFactoryAware.Opening(ChannelFactory channelFactory)
		{
			SelectUdpNetworkAdapter(channelFactory.Endpoint);
		}

		void IChannelFactoryAware.Opened(ChannelFactory channelFactory)
		{
		}

		void IChannelFactoryAware.Closing(ChannelFactory channelFactory)
		{
		}

		void IChannelFactoryAware.Closed(ChannelFactory channelFactory)
		{
		}

		void IChannelFactoryAware.Faulted(ChannelFactory channelFactory)
		{
		}

		void IChannelFactoryAware.ChannelCreated(ChannelFactory channelFactory, IChannel channel)
		{
		}

		void IChannelFactoryAware.ChannelAvailable(ChannelFactory channelFactory, IChannel channel)
		{
		}

		void IChannelFactoryAware.ChannelRefreshed(ChannelFactory channelFactory, IChannel oldChannel, IChannel newChannel)
		{
		}
		#endregion

		#region IServiceHostAware

		void IServiceHostAware.Created(ServiceHost serviceHost)
		{
		}

		void IServiceHostAware.Opening(ServiceHost serviceHost)
		{
			foreach (var endpoint in serviceHost.Description.Endpoints)
			{
				SelectUdpNetworkAdapter(endpoint);
			}
		}

		void IServiceHostAware.Opened(ServiceHost serviceHost)
		{
		}

		void IServiceHostAware.Closing(ServiceHost serviceHost)
		{
		}

		void IServiceHostAware.Closed(ServiceHost serviceHost)
		{
		}

		void IServiceHostAware.Faulted(ServiceHost serviceHost)
		{
		}
		#endregion

		private void SelectUdpNetworkAdapter(ServiceEndpoint endpoint)
		{
			var udpDiscoveryEndpoint = endpoint as UdpDiscoveryEndpoint;
			if (udpDiscoveryEndpoint == null)
			{
				return;
			}

			var udpTransportSettings = udpDiscoveryEndpoint.TransportSettings;
			if (string.IsNullOrEmpty(udpTransportSettings.MulticastInterfaceId) == false)
			{
				return;
			}

			var networkAdapter = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(AcceptUdpNetworkAdapter);
			if (networkAdapter != null)
			{
				udpTransportSettings.MulticastInterfaceId = networkAdapter.Id;
			}
		}

		private bool AcceptUdpNetworkAdapter(NetworkInterface networkAdapter)
		{
			if (networkAdapter.SupportsMulticast == false)
			{
				return false;
			}

			if (string.IsNullOrEmpty(networkId) == false && string.Equals(networkAdapter.Id, networkId) == false)
			{
				return false;
			}

			if (networkNameRegex != null && networkNameRegex.IsMatch(networkAdapter.Name) == false)
			{
				return false;
			}

			if (supportedNetworkTypes != null && supportedNetworkTypes.Contains(networkAdapter.NetworkInterfaceType) == false)
			{
				return false;
			}

			var ipProperties = networkAdapter.GetIPProperties();

			if (dnsSuffixRegex != null && dnsSuffixRegex.IsMatch(ipProperties.DnsSuffix) == false)
			{
				return false;
			}

			if (networkSelectors != null)
			{
				return networkSelectors.All(selector => selector(networkAdapter, ipProperties));
			}

			return true;
		}
	}
#endif
}
