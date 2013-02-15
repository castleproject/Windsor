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

#if !DOTNET35
namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.Linq;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Discovery;
	using Castle.Facilities.WcfIntegration.Internal;

	public abstract partial class AbstractChannelBuilder
	{
		private static readonly TimeSpan DefaultDuration = new TimeSpan(0, 0, 8);

		public DiscoveryEndpointProvider DiscoveryEndpointProvider  { get; set; }

		public static event EventHandler<DiscoveryEndpointFaultEventArgs> DiscoveryEndpointFaulted;

		internal object DiscoverChannel(DiscoveredEndpointModel model, IChannelBuilderScope scope)
		{
			var discovered = PerformEndpointSearch(scope.Contract, model);

			if (discovered == null || discovered.Endpoints.Count == 0)
			{
				throw new EndpointNotFoundException(string.Format(
					"Unable to discover the endpoint for contract {0}.  " +
					"Either no service exists or it does not support discovery.",
					scope.Contract.FullName));
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
						"However, an endpoint was not selected.  This is most likely " +
						"a bug with the user-defined endpoint preference.",
						scope.Contract.FullName));
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

			var channel = scope.GetChannel(scope.Contract, binding, address)();

			if (channel is IContextChannel)
			{
				var metadata = new DiscoveredEndpointMetadata(endpointMetadata);
				((IContextChannel)channel).Extensions.Add(metadata);
			}

			return channel;
		}

		private FindResponse PerformEndpointSearch(Type contract, DiscoveredEndpointModel model)
		{
			var criteria = CreateSearchCriteria(contract, model);

			for (int i = 0; i < 2; ++i)
			{
				var discoveryEndpoint = GetDiscoveryEndpoint(model);
				WcfBindingUtils.ConfigureQuotas(discoveryEndpoint.Binding, int.MaxValue);
				var discoveryClient = new DiscoveryClient(discoveryEndpoint);

				try
				{
					return discoveryClient.Find(criteria);
				}
				catch 
				{
					// ignore failures
				}
				finally
				{
					try
					{
						discoveryClient.Close();
					}
					catch 
					{
						// Discovery client often fails on close
					}
				}

				// Possible stale discovery proxy...

				if (i == 0)
				{
					var discoveryEndpointFaulted = DiscoveryEndpointFaulted;
					if (discoveryEndpointFaulted != null)
					{
						discoveryEndpointFaulted(this, new DiscoveryEndpointFaultEventArgs(discoveryEndpoint));
					}
				}
			}

			return null;
		}

		private DiscoveryEndpoint GetDiscoveryEndpoint(DiscoveredEndpointModel model)
		{
			if (model.DiscoveryEndpoint != null)
			{
				return model.DiscoveryEndpoint;
			}

			var provider = model.DiscoveryEndpointProvider ?? DiscoveryEndpointProvider;

			if (provider != null)
			{
				var discoveryEndpoint = provider.GetDiscoveryEndpoint();
				if (discoveryEndpoint != null)
				{
					return discoveryEndpoint;
				}
			}

			return new UdpDiscoveryEndpoint();
		}

		private static FindCriteria CreateSearchCriteria(Type contract, DiscoveredEndpointModel model)
		{
			var searchContract = model.SearchContract ?? contract;
			var criteria = model.SearchAnyContract ? new FindCriteria() : new FindCriteria(searchContract);
			criteria.Duration = model.Duration.GetValueOrDefault(DefaultDuration);
			criteria.MaxResults = model.MaxResults;

			var discovery = model.Extensions.OfType<WcfInstanceExtension>()
				.Select(extension => extension.Instance)
				.OfType<WcfEndpointDiscoveryMetadata>()
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
				return importer.ImportAllBindings().FirstOrDefault();
			}
		}
	}
}
#endif