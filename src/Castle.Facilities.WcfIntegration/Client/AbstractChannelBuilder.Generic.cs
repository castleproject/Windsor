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
	using System.ServiceModel.Description;
	using Castle.Facilities.WcfIntegration.Internal;
	using Castle.MicroKernel;

	public abstract partial class AbstractChannelBuilder<M> : AbstractChannelBuilder, IChannelBuilder<M>
			where M : IWcfClientModel
	{
		protected AbstractChannelBuilder(IKernel kernel, IChannelFactoryBuilder<M> channelFactoryBuilder)
			: base(kernel)
		{
			ChannelFactoryBuilder = channelFactoryBuilder;
		}

		protected IChannelFactoryBuilder<M> ChannelFactoryBuilder { get; private set; }

		/// <summary>
		/// Get a delegate capable of creating channels.
		/// </summary>
		/// <param name="clientModel">The client model.</param>
		/// <param name="burden">Receives the client burden.</param>
		/// <returns>The <see cref="ChannelCreator"/></returns>
		public ChannelCreator GetChannelCreator(M clientModel, out IWcfBurden burden)
		{
			burden = new WcfBurden(Kernel);
			var scope = new Scope(null, clientModel, this, burden);
			return scope.BuildChannelCreator();
		}

		/// <summary>
		/// Get a delegate capable of creating channels.
		/// </summary>
		/// <param name="clientModel">The client model.</param>
		/// <param name="contract">The contract override.</param>
		/// <param name="burden">Receives the client burden.</param>
		/// <returns>The <see cref="ChannelCreator"/></returns>
		public ChannelCreator GetChannelCreator(M clientModel, Type contract, out IWcfBurden burden)
		{
			burden = new WcfBurden(Kernel);
			var scope = new Scope(contract, clientModel, this, burden);
			return scope.BuildChannelCreator();
		}

		protected virtual ChannelCreator GetChannel(M clientModel, Type contract, IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, contract);
		}

		protected virtual ChannelCreator GetChannel(M clientModel, Type contract, ServiceEndpoint endpoint, IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, endpoint);
		}

		protected virtual ChannelCreator GetChannel(M clientModel, Type contract, string configurationName, IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, configurationName);
		}

		protected virtual ChannelCreator GetChannel(M clientModel, Type contract, Binding binding, string address, IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, binding, address);
		}

		protected virtual ChannelCreator GetChannel(M clientModel, Type contract, Binding binding, EndpointAddress address, IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, binding, address);
		}

		protected virtual void ConfigureChannelFactory(ChannelFactory channelFactory, M clientModel, IWcfBurden burden)
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

		protected virtual ChannelCreator CreateChannelCreator(Type contract, M clientModel, IChannelBuilderScope scope, params object[] channelFactoryArgs)
		{
			var type = typeof(ChannelFactory<>).MakeGenericType(new[] { contract });
			var channelFactory = ChannelFactoryBuilder.CreateChannelFactory(type, clientModel, channelFactoryArgs);
			scope.ConfigureChannelFactory(channelFactory);

			var methodInfo = type.GetMethod("CreateChannel", Type.EmptyTypes);
			return (ChannelCreator)Delegate.CreateDelegate(typeof(ChannelCreator), channelFactory, methodInfo);
		}
	}
}