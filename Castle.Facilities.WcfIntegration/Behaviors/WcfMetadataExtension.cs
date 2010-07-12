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
// limitations under the License.

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using Castle.Facilities.WcfIntegration.Service;

	public class WcfMetadataExtension : AbstractServiceHostAware
	{
		private bool _enableHttpGet;
		private string _mexAddress = "mex";

		public WcfMetadataExtension EnableHttpGet()
		{
			_enableHttpGet = true;
			return this;
		}

		public WcfMetadataExtension AtAddress(string address)
		{
			_mexAddress = address;
			return this;
		}

		protected override void Opening(ServiceHost serviceHost)
		{
			var serviceMetadata = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
			if (serviceMetadata == null)
			{
				serviceHost.Description.Behaviors.Add(CreateMetadataBehavior(serviceHost));
			}

			AddWellKnownMexEndpoints(serviceHost);
		}

		private void AddWellKnownMexEndpoints(ServiceHost serviceHost)
		{
			bool relativeAddress;
			foreach (var baseAddress in GetBaseAddresses(serviceHost, out relativeAddress))
			{
				Binding binding = null;
				var mexAddress = _mexAddress;
				var scheme = baseAddress.Scheme;

				if (scheme == Uri.UriSchemeHttp)
				{
					binding = MetadataExchangeBindings.CreateMexHttpBinding();
					mexAddress = relativeAddress ? string.Empty : baseAddress.AbsoluteUri;
				}
				else if (scheme == Uri.UriSchemeHttps)
				{
					binding = MetadataExchangeBindings.CreateMexHttpsBinding();
					mexAddress = relativeAddress ? string.Empty : baseAddress.AbsoluteUri;
				}
				else if (scheme == Uri.UriSchemeNetTcp)
				{
					binding = MetadataExchangeBindings.CreateMexTcpBinding();
					var tcpBinding = new CustomBinding(binding);
					var transport = tcpBinding.Elements.OfType<TcpTransportBindingElement>().Single();
					transport.PortSharingEnabled = true;
					binding = tcpBinding;
					if (relativeAddress == false)
						mexAddress = string.Format("{0}/{1}", baseAddress.AbsoluteUri, _mexAddress);
				}
				else if (scheme == Uri.UriSchemeNetPipe)
				{
					binding = MetadataExchangeBindings.CreateMexNamedPipeBinding();
					if (relativeAddress == false)
						mexAddress = string.Format("{0}/{1}", baseAddress.AbsoluteUri, _mexAddress);
				}

				if (binding != null)
				{
					serviceHost.AddServiceEndpoint(typeof(IMetadataExchange), binding, mexAddress);
				}
			}
		}

		private ServiceMetadataBehavior CreateMetadataBehavior(ServiceHost serviceHost)
		{
			var metadataBehavior = new ServiceMetadataBehavior();

			bool relativeAddress;
			foreach (var baseAddress in  GetBaseAddresses(serviceHost, out relativeAddress))
			{
				if (baseAddress.Scheme == Uri.UriSchemeHttp)
				{
					metadataBehavior.HttpGetEnabled = _enableHttpGet;
				}
				else if (baseAddress.Scheme == Uri.UriSchemeHttp)
				{
					metadataBehavior.HttpsGetEnabled = _enableHttpGet;
				}
			}

			return metadataBehavior;
		}

		private  static Uri[] GetBaseAddresses(ServiceHost serviceHost, out bool relative)
		{
			relative = true;
			var baseAddresses = serviceHost.BaseAddresses;
			if (baseAddresses.Count > 0) return baseAddresses.ToArray();

			relative = false;
			return serviceHost.Description.Endpoints.Select(endpoint => endpoint.Address.Uri).ToArray();
		}
	}
}
