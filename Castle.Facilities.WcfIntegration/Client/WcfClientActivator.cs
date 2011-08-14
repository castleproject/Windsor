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
#if DOTNET40
	using System.Collections.Concurrent;
#endif
	using System.Linq;
	using System.Reflection;
	using System.ServiceModel;

	using Castle.Core;
	using Castle.Facilities.WcfIntegration.Internal;
	using Castle.Facilities.WcfIntegration.Proxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ComponentActivator;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Facilities;

	using System.ServiceModel.Channels;

	public class WcfClientActivator : DefaultComponentActivator
	{
		private readonly WcfClientExtension clients;
		private readonly WcfProxyFactory proxyFactory;
		private IWcfBurden channelBurden;
		private ChannelCreator createChannel;

		public WcfClientActivator(ComponentModel model, IKernel kernel,
		                          ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
			: base(model, kernel, onCreation, onDestruction)
		{
			clients = kernel.Resolve<WcfClientExtension>();
			proxyFactory = new WcfProxyFactory(clients.ProxyGenerator, clients);
		}

		protected override object Instantiate(CreationContext context)
		{
			IWcfBurden burden;
			var channelCreator = GetChannelCreator(context, out burden);

			try
			{
				var channelHolder = new WcfChannelHolder(channelCreator, burden, clients.CloseTimeout);
				var channel = (IChannel)proxyFactory.Create(Kernel, channelHolder, Model, context);
				NotifyChannelCreatedOrAvailable(channel, burden, false);
				return channel;
			}
			catch (CommunicationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new ComponentActivatorException("WcfClientActivator: could not proxy component " + Model.Name, ex);
			}
		}

		protected override void SetUpProperties(object instance, CreationContext context)
		{
			//we don't... there should be no properties on WCF clients
		}

		/// <summary>
		///   Creates the channel creator.
		/// </summary>
		/// <param name = "context">The context for the creator.</param>
		/// <param name = "burden">Receives the channel burden.</param>
		/// <returns>The channel creator.</returns>
		/// <remarks>
		///   Always Open the channel before being used to prevent serialization of requests.
		///   http://blogs.msdn.com/wenlong/archive/2007/10/26/best-practice-always-open-wcf-client-proxy-explicitly-when-it-is-shared.aspx
		/// </remarks>
		private ChannelCreator GetChannelCreator(CreationContext context, out IWcfBurden burden)
		{
			burden = channelBurden;
			var creator = createChannel;
			var clientModel = ObtainClientModel(Model, context);

			if (clientModel != null)
			{
				var inner = CreateChannelCreator(Kernel, Model, clientModel, out burden);
				var scopedBurden = burden;

				creator = () =>
				{
					var client = (IChannel)inner();
					if (client is IContextChannel)
					{
						((IContextChannel)client).Extensions.Add(new WcfBurdenExtension<IContextChannel>(scopedBurden));
					}
					else
					{
						var parameters = client.GetProperty<ChannelParameterCollection>();
						if (parameters != null)
						{
							parameters.Add(scopedBurden);
						}
					}
					NotifyChannelCreatedOrAvailable(client, scopedBurden, true);
					client.Open();
					return client;
				};
			}
			else if (createChannel == null)
			{
				clientModel = ObtainClientModel(Model);
				var inner = CreateChannelCreator(Kernel, Model, clientModel, out channelBurden);
				creator = createChannel = () =>
				{
					var client = (IChannel)inner();
					NotifyChannelCreatedOrAvailable(client, channelBurden, true);
					client.Open();
					return client;
				};
				burden = channelBurden;
				clients.TrackBurden(burden);
			}

			return creator;
		}

		private static ChannelCreator CreateChannelCreator(IKernel kernel, ComponentModel model, IWcfClientModel clientModel, out IWcfBurden burden)
		{
			ValidateClientModel(clientModel, model);

			var createChannelDelegate = createChannelCache.GetOrAdd(clientModel.GetType(), clientModelType =>
			{
				return (CreateChannelDelegate)Delegate.CreateDelegate(typeof(CreateChannelDelegate),
				                                                      createChannelMethod.MakeGenericMethod(clientModelType));
			});

			var channelCreator = createChannelDelegate(kernel, clientModel, model, out burden);
			if (channelCreator == null)
			{
				throw new CommunicationException("Unable to generate the channel creator.  " +
				                                 "Either the endpoint could be be created or it's a bug so please report it.");
			}
			return channelCreator;
		}

		private static ChannelCreator CreateChannelCreatorInternal<TModel>(
			IKernel kernel, IWcfClientModel clientModel, ComponentModel model, out IWcfBurden burden)
			where TModel : IWcfClientModel
		{
			var channelBuilder = kernel.Resolve<IChannelBuilder<TModel>>();
			return channelBuilder.GetChannelCreator((TModel)clientModel, model.GetServiceContract(), out burden);
		}

		private static void NotifyChannelCreatedOrAvailable(IChannel channel, IWcfBurden burden, bool created)
		{
			var channelFactory = burden.Dependencies.OfType<ChannelFactoryHolder>()
				.Select(holder => holder.ChannelFactory).FirstOrDefault();

			if (channelFactory != null)
			{
				foreach (var observer in burden.Dependencies.OfType<IChannelFactoryAware>())
				{
					if (created)
					{
						observer.ChannelCreated(channelFactory, channel);
					}
					else
					{
						observer.ChannelAvailable(channelFactory, channel);
					}
				}
			}
		}

		private static IWcfClientModel ObtainClientModel(ComponentModel model)
		{
			return (IWcfClientModel)model.ExtendedProperties[WcfConstants.ClientModelKey];
		}

		private static IWcfClientModel ObtainClientModel(ComponentModel model, CreationContext context)
		{
			var clientModel = WcfUtils.FindDependencies<IWcfClientModel>(context.AdditionalArguments).FirstOrDefault();
			var endpoint = WcfUtils.FindDependencies<IWcfEndpoint>(context.AdditionalArguments).FirstOrDefault();

			if (endpoint != null)
			{
				if (clientModel == null)
				{
					clientModel = ObtainClientModel(model);
				}

				clientModel = clientModel.ForEndpoint(endpoint);
			}

			return clientModel;
		}

		private static void ValidateClientModel(IWcfClientModel clientModel, ComponentModel model)
		{
			if (clientModel.Endpoint == null)
			{
				throw new FacilityException("The client model requires an endpoint.");
			}

			var contract = clientModel.Contract ?? model.GetServiceContract();

			if (contract == null)
			{
				throw new FacilityException("The service contract for the client endpoint could not be determined.");
			}

			if (model.Services.Contains(contract) == false)
			{
				throw new FacilityException(string.Format(
					"The service contract {0} is not supported by the component {0} or any of its services.",
					clientModel.Contract.FullName, model.Implementation.FullName));
			}
		}

		private delegate ChannelCreator CreateChannelDelegate(IKernel kernel, IWcfClientModel clientModel, ComponentModel model, out IWcfBurden burden);

		private static readonly ConcurrentDictionary<Type, CreateChannelDelegate>
			createChannelCache = new ConcurrentDictionary<Type, CreateChannelDelegate>();

		private static readonly MethodInfo createChannelMethod =
			typeof(WcfClientActivator).GetMethod("CreateChannelCreatorInternal",
												 BindingFlags.NonPublic | BindingFlags.Static, null,
												 new[]
			                                     {
			                                     	typeof(IKernel), typeof(IWcfClientModel),
			                                     	typeof(ComponentModel), typeof(IWcfBurden).MakeByRefType()
			                                     },
												 null
				);
	}
}