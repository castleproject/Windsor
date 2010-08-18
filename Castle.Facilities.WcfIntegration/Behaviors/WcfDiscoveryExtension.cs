// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Description;
	using System.ServiceModel.Discovery;
	using System.Xml.Linq;
	using Castle.Facilities.WcfIntegration.Service;
	using Castle.Facilities.WcfIntegration.Internal;

	public class WcfDiscoveryExtension : AbstractServiceHostAware
	{
		private bool strict;
		private DiscoveryEndpoint discoveryEndpoint;
		private readonly List<Uri> scopes = new List<Uri>();
		private readonly List<XElement> metadata = new List<XElement>();

		public WcfDiscoveryExtension Strict()
		{
			strict = true;
			return this;
		}

		public WcfDiscoveryExtension InScope(params Uri[] scopes)
		{
			this.scopes.AddRange(scopes);
			return this;
		}

		public WcfDiscoveryExtension InScope(params string[] scopes)
		{
			this.scopes.AddRange(scopes.Select(scope => new Uri(scope)));
			return this;
		}

		public WcfDiscoveryExtension WithMetadata(params XElement[] metadata)
		{
			this.metadata.AddRange(metadata);
			return this;
		}

		public WcfDiscoveryExtension AtEndpoint(DiscoveryEndpoint endpoint)
		{
			discoveryEndpoint = endpoint;
			return this;
		}

		protected override void Opening(ServiceHost serviceHost)
		{
			var serviceDiscovery = serviceHost.Description.Behaviors.Find<ServiceDiscoveryBehavior>();
			if (serviceDiscovery == null)
			{
				serviceHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
			}

			foreach (var endpoint in serviceHost.Description.NonSystemEndpoints())
			{
				var discovery = endpoint.Behaviors.Find<EndpointDiscoveryBehavior>();
				if (discovery == null)
				{
					discovery = new EndpointDiscoveryBehavior();
					endpoint.Behaviors.Add(discovery);
				}

				discovery.Scopes.AddAll(scopes);

				discovery.Extensions.AddAll(metadata);

				AddAdditionalMetadata(serviceHost, discovery);

				if (strict == false)
				{
					ExportMetadata(endpoint, discovery);
				}
			}

			AddDiscoveryEndpoint(serviceHost);
		}

		private void AddDiscoveryEndpoint(ServiceHost serviceHost)
		{
			if (serviceHost.Description.Endpoints.OfType<DiscoveryEndpoint>().Any() == false)
			{
				var endpoint = discoveryEndpoint ?? new UdpDiscoveryEndpoint();
				serviceHost.Description.Endpoints.Add(endpoint);
			}
		}

		private static void AddAdditionalMetadata(ServiceHost serviceHost, EndpointDiscoveryBehavior discovery)
		{
			var meatadata = serviceHost.Extensions.FindAll<IWcfMetadataProvider>();

			discovery.Scopes.AddAll(meatadata.SelectMany(meta => meta.Scopes));

			discovery.Extensions.AddAll(meatadata.SelectMany(meta => meta.Extensions));
		}

		private static void ExportMetadata(ServiceEndpoint endpoint, EndpointDiscoveryBehavior discovery)
		{
			var exporter = new WsdlExporter();
			exporter.ExportEndpoint(endpoint);
			var metadata = exporter.GetGeneratedMetadata();

			var document = new XDocument();
			using (var xmlWriter = document.CreateWriter())
			{
				xmlWriter.WriteStartElement(WcfConstants.EndpointMetadata.LocalName, 
											WcfConstants.EndpointMetadata.Namespace.NamespaceName);
				metadata.WriteTo(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();
			}

			discovery.Extensions.Add(document.Root);
		}
	}
#endif
}
