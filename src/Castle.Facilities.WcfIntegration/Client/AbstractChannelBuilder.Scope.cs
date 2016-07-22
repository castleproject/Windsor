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
using System.ServiceModel.Channels;

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Description;

	public abstract partial class AbstractChannelBuilder<M>
	{
		class Scope : IChannelBuilderScope, IWcfEndpointVisitor
		{
			private readonly Type contract;
            private readonly M clientModel;
			private readonly AbstractChannelBuilder<M> builder;
			private readonly IWcfBurden burden;
			private ChannelCreator channelCreator;
   
			public Scope(Type contract, M clientModel, AbstractChannelBuilder<M> builder, IWcfBurden burden)
			{
				this.contract = contract ?? clientModel.Endpoint.Contract;
                this.clientModel = clientModel;
                this.builder = builder;
				this.burden = burden;
			}

			public Type Contract
			{
				get { return contract; }
			}

			public IWcfBurden Burden
			{
				get { return burden; }
			}

			public ChannelCreator BuildChannelCreator()
			{
				clientModel.Endpoint.Accept(this);
				return channelCreator;
			}

			public ChannelCreator GetChannel(Type contract)
			{
				return builder.GetChannel(clientModel, contract, this);
			}

			public ChannelCreator GetChannel(Type contract, ServiceEndpoint endpoint)
			{
				return builder.GetChannel(clientModel, contract, endpoint, this);
			}

			public ChannelCreator GetChannel(Type contract, string configurationName)
			{
				return builder.GetChannel(clientModel, contract, configurationName, this);
			}

			public ChannelCreator GetChannel(Type contract, Binding binding, string address)
			{
				return builder.GetChannel(clientModel, contract, binding, address, this);
			}

			public ChannelCreator GetChannel(Type contract, Binding binding, EndpointAddress address)
			{
				return builder.GetChannel(clientModel, contract, binding, address, this);
			}

			public void ConfigureChannelFactory(ChannelFactory channelFactory)
			{
				builder.ConfigureChannelFactory(channelFactory, clientModel, burden);
			}

			#region IWcfEndpointVisitor

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
				channelCreator = GetChannel(contract, builder.GetEffectiveBinding(model.Binding, null), string.Empty);
			}

			void IWcfEndpointVisitor.VisitBindingAddressEndpoint(BindingAddressEndpointModel model)
			{
				if (model.HasViaAddress)
				{
					var address = model.EndpointAddress ?? new EndpointAddress(model.Address);
					var description = ContractDescription.GetContract(contract);
					var binding = builder.GetEffectiveBinding(model.Binding, address.Uri);
					var endpoint = new ServiceEndpoint(description, binding, address);
					endpoint.Behaviors.Add(new ClientViaBehavior(model.ViaAddress));
					channelCreator = GetChannel(contract, endpoint);
				}
				else
				{
					if (model.EndpointAddress != null)
					{
						var binding = builder.GetEffectiveBinding(model.Binding, model.EndpointAddress.Uri);
						channelCreator = GetChannel(contract, binding, model.EndpointAddress);
					}
					else
					{
						var binding = builder.GetEffectiveBinding(model.Binding, new Uri(model.Address));
						channelCreator = GetChannel(contract, binding, model.Address);
					}
				}
			}

#if !DOTNET35
			void IWcfEndpointVisitor.VisitBindingDiscoveredEndpoint(DiscoveredEndpointModel model)
			{
				channelCreator = () => builder.DiscoverChannel(model, this);
			}
#endif
			#endregion
		}
	}
}