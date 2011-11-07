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
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
#if DOTNET40
	using System.ServiceModel.Discovery;
#endif
	using Castle.MicroKernel;
	using Castle.Facilities.WcfIntegration.Internal;

	public abstract class AbstractChannelBuilder : IWcfEndpointVisitor
	{
		private Type contract;
		private ChannelCreator channelCreator;

		protected AbstractChannelBuilder(IKernel kernel)
		{
			Kernel = kernel;
		}

		protected IKernel Kernel { get; private set; }

		public WcfClientExtension Clients { get; set; }
	
		protected void ConfigureChannelFactory(ChannelFactory channelFactory, IWcfClientModel clientModel, IWcfBurden burden)
		{
			var extensions = new ChannelFactoryExtensions(channelFactory, Kernel)
				.Install(burden, new WcfChannelExtensions());

			var endpointExtensions = new ServiceEndpointExtensions(channelFactory.Endpoint, true, Kernel)
				.Install(burden, new WcfEndpointExtensions(WcfExtensionScope.Clients));

			if (clientModel != null)
			{
				extensions.Install(clientModel.Extensions, burden);
				endpointExtensions.Install(clientModel.Extensions, burden);
				endpointExtensions.Install(clientModel.Endpoint.Extensions, burden);
			}

			burden.Add(new ChannelFactoryHolder(channelFactory));
		}

		protected ChannelCreator GetEndpointChannelCreator(IWcfEndpoint endpoint)
		{
			return GetEndpointChannelCreator(endpoint, null);
		}

		protected ChannelCreator GetEndpointChannelCreator(IWcfEndpoint endpoint, Type contract)
		{
			this.contract = contract ?? endpoint.Contract;
			endpoint.Accept(this);
			return channelCreator;
		}

		protected abstract ChannelCreator GetChannel(Type contract);
		protected abstract ChannelCreator GetChannel(Type contract, ServiceEndpoint endpoint);
		protected abstract ChannelCreator GetChannel(Type contract, string configurationName);
		protected abstract ChannelCreator GetChannel(Type contract, Binding binding, string address);
		protected abstract ChannelCreator GetChannel(Type contract, Binding binding, EndpointAddress address);

		#region IWcfEndpointVisitor Members

		void IWcfEndpointVisitor.VisitContractEndpoint(ContractEndpointModel model)
		{
			channelCreator = GetChannel(contract);
		}
        
		void IWcfEndpointVisitor.VisitServiceEndpoint(ServiceEndpointModel model)
		{
			channelCreator = GetChannel(contract, model.ServiceEndpoint);
		}

		void IWcfEndpointVisitor.VisitConfigurationEndpoint(ConfigurationEndpointModel model)
		{
			channelCreator = GetChannel(contract, model.EndpointName);
		}

		void IWcfEndpointVisitor.VisitBindingEndpoint(BindingEndpointModel model)
		{
			channelCreator = GetChannel(contract, GetEffectiveBinding(model.Binding, null), string.Empty);
		}

		void IWcfEndpointVisitor.VisitBindingAddressEndpoint(BindingAddressEndpointModel model)
		{
			if (model.HasViaAddress)
			{
				var address = model.EndpointAddress ?? new EndpointAddress(model.Address);
				var description = ContractDescription.GetContract(contract);
				var binding = GetEffectiveBinding(model.Binding, address.Uri);
				var endpoint = new ServiceEndpoint(description, binding, address);
				endpoint.Behaviors.Add(new ClientViaBehavior(model.ViaAddress));
				channelCreator = GetChannel(contract, endpoint);
			}
			else
			{
				if (model.EndpointAddress != null)
				{
					var binding = GetEffectiveBinding(model.Binding, model.EndpointAddress.Uri);
					channelCreator = GetChannel(contract, binding, model.EndpointAddress);
				}
				else
				{
					var binding = GetEffectiveBinding(model.Binding, new Uri(model.Address));
					channelCreator = GetChannel(contract, binding, model.Address);
				}
			}
		}
#if DOTNET40
		void IWcfEndpointVisitor.VisitBindingDiscoveredEndpoint(DiscoveredEndpointModel model)
		{
			var discoveryEndpoint = model.DiscoveryEndpoint ?? new UdpDiscoveryEndpoint();
			DiscoverEndpoint(discoveryEndpoint, model);
		}

		private void DiscoverEndpoint(DiscoveryEndpoint discoveryEndpoint, DiscoveredEndpointModel model)
		{
			FindResponse discovered = null;
			try
			{
				using (var discover = new DiscoveryClient(discoveryEndpoint))
				{
					var criteria = CreateSearchCriteria(model);
					discovered = discover.Find(criteria);
				}
			}
			catch
			{
				// Possible failure when closing DiscoveryClient
			}

			if (discovered == null || discovered.Endpoints.Count == 0)
			{
				throw new EndpointNotFoundException(string.Format(
					"Unable to discover the endpoint for contract {0}.  " +
					"Either no service exists or it does not support discovery.",
					contract.FullName));
			}

			var binding = model.Binding;
			var endpointMetadata = discovered.Endpoints[0];
			if (discovered.Endpoints.Count > 1 && model.EndpointPreference != null)
			{
				endpointMetadata = model.EndpointPreference(discovered.Endpoints);
				if (endpointMetadata == null)
				{
					throw new EndpointNotFoundException(string.Format(
						"More than one endpoint was discovered for contract {0}.  " +
						"However, an endpoint could be selected.  This is most likely " +
						"a bug with the user-defined endpoint preference.",
						contract.FullName));
				}
			}

			if (binding == null && model.DeriveBinding == false)
			{
				binding = GetBindingFromMetadata(endpointMetadata);
			}
					
			var address = endpointMetadata.Address;
			if (model.Identity != null)
			{
				address = new EndpointAddress(address.Uri, model.Identity, address.Headers);
			}

			binding = GetEffectiveBinding(binding, address.Uri);
			var innerCreator = GetChannel(contract, binding, address);
			channelCreator = () =>
			{
				var channel = (IChannel)innerCreator();
				if (channel is IContextChannel)
				{
					var metadata = new DiscoveredEndpointMetadata(endpointMetadata);
					((IContextChannel)channel).Extensions.Add(metadata);
				}
				return channel;
			};
		}
#endif
		#endregion

		protected virtual Binding InferBinding(Uri address)
		{
			if (Clients != null)
			{
				return Clients.InferBinding(address) ?? Clients.DefaultBinding;
			}
			return null;
		}
#if DOTNET40
		private FindCriteria CreateSearchCriteria(DiscoveredEndpointModel model)
		{
			var searchContract = model.SearchContract ?? contract;
			var criteria = new FindCriteria(searchContract) { MaxResults = model.MaxResults };

			if (model.Duration.HasValue)
			{
				criteria.Duration = model.Duration.Value;
			}

			var discovery = model.Extensions.OfType<WcfInstanceExtension>()
				.Select(extension => extension.Instance)
				.OfType<EndpointDiscoveryBehavior>()
				.FirstOrDefault();

			if (discovery != null)
			{
				criteria.Scopes.AddAll(discovery.Scopes);

				if (model.ScopeMatchBy != null)
				{
					criteria.ScopeMatchBy = model.ScopeMatchBy;
				}

				criteria.Extensions.AddAll(discovery.Extensions);
			}

			return criteria;
		}

		internal static Binding GetBindingFromMetadata(EndpointDiscoveryMetadata metadata)
		{
			var metadataExtension = 
				(from extension in metadata.Extensions
				 where extension.Name == WcfConstants.EndpointMetadata
				 select extension).FirstOrDefault();
			if (metadataExtension == null) return null;

			var endpointMetadata = metadataExtension.Elements().FirstOrDefault();
			if (endpointMetadata == null) return null;

			using (var xmlReader = endpointMetadata.CreateReader())
			{
				var metadataSet = MetadataSet.ReadFrom(xmlReader);
				var importer = new WsdlImporter(metadataSet);
				var endpoints = importer.ImportAllEndpoints();
				if (endpoints.Count > 0)
				{
					return endpoints[0].Binding;
				}
			}

			return null;
		}
#endif

		private Binding GetEffectiveBinding(Binding binding, Uri address)
		{
			return binding ?? InferBinding(address);
		}
	}
}