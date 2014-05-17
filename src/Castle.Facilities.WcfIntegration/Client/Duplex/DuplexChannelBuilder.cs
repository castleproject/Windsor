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

	using Castle.MicroKernel;

	public class DuplexChannelBuilder : AbstractChannelBuilder<DuplexClientModel>
	{
		public DuplexChannelBuilder(IKernel kernel, IChannelFactoryBuilder<DuplexClientModel> channelFactoryBuilder)
			: base(kernel, channelFactoryBuilder)
		{
		}

		protected override ChannelCreator CreateChannelCreator(Type contract, DuplexClientModel clientModel, IChannelBuilderScope scope,
		                                                       params object[] channelFactoryArgs)
		{
			var type = typeof(DuplexChannelFactory<>).MakeGenericType(new[] { contract });
			var channelFactory = ChannelFactoryBuilder.CreateChannelFactory(type, clientModel, channelFactoryArgs);
			scope.ConfigureChannelFactory(channelFactory);

			var methodInfo = type.GetMethod("CreateChannel", Type.EmptyTypes);
			return (ChannelCreator)Delegate.CreateDelegate(typeof(ChannelCreator), channelFactory, methodInfo);
		}

		protected override ChannelCreator GetChannel(DuplexClientModel clientModel, Type contract, IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, clientModel.CallbackContext);
 		}
 
		protected override ChannelCreator GetChannel(DuplexClientModel clientModel, Type contract, ServiceEndpoint endpoint, 
                                                      IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, clientModel.CallbackContext, endpoint);
		}
     
		protected override ChannelCreator GetChannel(DuplexClientModel clientModel, Type contract, string configurationName, 
                                                      IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, clientModel.CallbackContext, configurationName);
		}

		protected override ChannelCreator GetChannel(DuplexClientModel clientModel, Type contract, Binding binding, string address,
													 IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, clientModel.CallbackContext, binding, address);
		}

		protected override ChannelCreator GetChannel(DuplexClientModel clientModel, Type contract, Binding binding, EndpointAddress address,
													 IChannelBuilderScope scope)
		{
			return CreateChannelCreator(contract, clientModel, scope, clientModel.CallbackContext, binding, address);
		}
	}
}