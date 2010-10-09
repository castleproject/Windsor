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
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using Castle.Core;
	using Castle.Facilities.TypedFactory;
	using Castle.Facilities.WcfIntegration.Async;
	using Castle.Facilities.WcfIntegration.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.LifecycleConcerns;
	using Castle.MicroKernel.Registration;

	public class WcfClientExtension : IDisposable
	{
		private IKernel kernel;
		private WcfFacility facility;
		private Binding defaultBinding;
		private TimeSpan? closeTimeout;
		private List<Func<Uri, Binding>> bindingPolicies;
		private Action _afterInit;

		public WcfClientExtension()
		{
			DefaultChannelPolicy = new ChannelReconnectPolicy();
			bindingPolicies = new List<Func<Uri,Binding>>();
		}

		public Binding DefaultBinding
		{
			get { return defaultBinding ?? facility.DefaultBinding; }
			set { defaultBinding = value; }
		}

		public TimeSpan? CloseTimeout
		{
			get { return closeTimeout ?? facility.CloseTimeout; }
			set { closeTimeout = value; }
		}

		public IChannelActionPolicy DefaultChannelPolicy { get; set; }

		public WcfClientExtension AddChannelBuilder<TBuilder>()
			where TBuilder : IChannelBuilder
		{
			return AddChannelBuilder(typeof(TBuilder));
		}

		public WcfClientExtension AddChannelBuilder(Type builder)
		{
			AddChannelBuilder(builder, true);
			return this;
		}

		public WcfClientExtension AddBindingPolicy(Func<Uri, Binding> policy)
		{
			bindingPolicies.Insert(0, policy);
			return this;
		}

		public Binding InferBinding(Uri address)
		{
			return bindingPolicies.Select(policy => policy(address)).FirstOrDefault(binding => binding != null);
		}

		internal void Init(WcfFacility facility)
		{
			this.facility = facility;
			kernel = facility.Kernel;

			AddDefaultChannelBuilders();
			AddDefaultBindingPolicies();

			kernel.Register(
				Component.For(typeof(IChannelFactoryBuilder<>))
					.ImplementedBy(typeof(AsynChannelFactoryBuilder<>))
					.Unless(Component.ServiceAlreadyRegistered)
					);

			if (kernel.GetFacilities().OfType<TypedFactoryFacility>().Any())
			{
				kernel.Register(
					Component.For<IWcfClientFactory>().AsFactory(c => c.SelectedWith(new WcfClientFactorySelector()))
					);
			}

			kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
			kernel.ComponentUnregistered += Kernel_ComponentUnregistered;

			if (_afterInit != null)
			{
				_afterInit();
				_afterInit = null;
			}
		}

		private void Kernel_ComponentModelCreated(ComponentModel model)
		{
			var clientModel = ResolveClientModel(model);

			if (clientModel != null && model.Implementation == model.Service)
			{
				model.CustomComponentActivator = typeof(WcfClientActivator);
				model.ExtendedProperties[WcfConstants.ClientModelKey] = clientModel;
				model.Lifecycle.Add(DisposalConcern.Instance);

				var dependencies = new ExtensionDependencies(model, kernel)
					.Apply(new WcfEndpointExtensions(WcfExtensionScope.Clients))
					.Apply(clientModel.Extensions);

				if (clientModel.Endpoint != null)
					dependencies.Apply(clientModel.Endpoint.Extensions);
			}
		}

		private static void Kernel_ComponentUnregistered(string key, IHandler handler)
		{
			ComponentModel model = handler.ComponentModel;
			var burden = model.ExtendedProperties[WcfConstants.ClientBurdenKey] as IWcfBurden;
			if (burden != null) burden.CleanUp();
		}

		private void AddDefaultChannelBuilders()
		{
			AddChannelBuilder(typeof(DefaultChannelBuilder), false);
			AddChannelBuilder(typeof(DuplexChannelBuilder), false);
			AddChannelBuilder(typeof(RestChannelBuilder), false);
        }

		private void AddDefaultBindingPolicies()
		{
			var httpBinding = new BasicHttpBinding();
			AddBindingPolicy(address => address.Scheme == Uri.UriSchemeHttp ? httpBinding : null);

			var httpsBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
			AddBindingPolicy(address => address.Scheme == Uri.UriSchemeHttps ? httpsBinding : null);

			var tcpBinding = new NetTcpBinding { PortSharingEnabled = true };
			AddBindingPolicy(address => address.Scheme == Uri.UriSchemeNetTcp ? tcpBinding : null);

			var pipeBinding = new NetNamedPipeBinding();
			AddBindingPolicy(address => address.Scheme == Uri.UriSchemeNetPipe ? pipeBinding : null);
		}

		internal void AddChannelBuilder(Type builder, bool force)
		{
			if (typeof(IChannelBuilder).IsAssignableFrom(builder) == false)
			{
				throw new ArgumentException(string.Format(
					"The type {0} does not represent an IChannelBuilder.",
					builder.FullName), "builder");
			}

			var channelBuilder = WcfUtils.GetClosedGenericDefinition(typeof(IChannelBuilder<>), builder);

			if (channelBuilder == null)
			{
				throw new ArgumentException(string.Format(
					"The client model cannot be inferred from the builder {0}.  Did you implement IChannelBuilder<>?",
					builder.FullName), "builder");
			}

			if (kernel == null)
			{
				_afterInit += () => RegisterChannelBuilder(channelBuilder, builder, force);
			}
			else
			{
				RegisterChannelBuilder(channelBuilder, builder, force);
			}
		}

		private void RegisterChannelBuilder(Type channelBuilder, Type builder, bool force)
		{
			if (force || kernel.HasComponent(channelBuilder) == false)
			{
				kernel.Register(Component.For(channelBuilder).ImplementedBy(builder));
			}
		}

		private static IWcfClientModel ResolveClientModel(ComponentModel model)
		{
			if (model.Service.IsInterface)
			{
				var clientModel = WcfUtils.FindDependencies<IWcfClientModel>(model.CustomDependencies).FirstOrDefault();
				if(clientModel != null)
				{
					return clientModel;
				}
			}

			if (model.Configuration != null)
			{
				string endpointConfiguration =
					model.Configuration.Attributes[WcfConstants.EndpointConfiguration];

				if (!string.IsNullOrEmpty(endpointConfiguration))
				{
					return new DefaultClientModel(WcfEndpoint.FromConfiguration(endpointConfiguration));
				}
			}

			return null;
		}

		public void Dispose()
		{
		}
	}
}
