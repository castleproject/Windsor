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
#if DOTNET40
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;

	public class AdHocServiceCatalogProbe : AbstractServiceHostAware, IDisposable
	{
		private ServiceHost announcementHost;
		private readonly IServiceCatalogImplementation serviceCatalog;
		private readonly List<Func<EndpointDiscoveryMetadata, bool>> filters;

		public AdHocServiceCatalogProbe(IServiceCatalogImplementation serviceCatalog)
		{
			this.serviceCatalog = serviceCatalog;
			filters = new List<Func<EndpointDiscoveryMetadata, bool>>();
		}

		public FindCriteria ProbeCriteria { get; set; }

		public UdpDiscoveryEndpoint UdpDiscoveryEndpoint { get; set; }

		public UdpAnnouncementEndpoint UdpAnnouncementEndpoint { get; set; }

		public AdHocServiceCatalogProbe AddFilter(Func<EndpointDiscoveryMetadata, bool> filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException("filter");
			}
			filters.Add(filter);
			return this;
		}

		protected override void Opening(ServiceHost serviceHost)
		{
			base.Opening(serviceHost);

			ConfigureDomain(serviceHost);
			MonitorAnnouncements(serviceHost);
			ProbeInitialServices(serviceHost);
		}

		private void RegisterService(ServiceHost serviceHost, EndpointDiscoveryMetadata endpoint)
		{
			if (FilterService(serviceHost, endpoint) == false)
			{
				serviceCatalog.RegisterService(endpoint);
			}
		}

		private void RemoveService(ServiceHost serviceHost, EndpointDiscoveryMetadata endpoint)
		{
			if (FilterService(serviceHost, endpoint) == false)
			{
				serviceCatalog.RemoveService(endpoint);
			}
		}

		private bool FilterService(ServiceHost serviceHost, EndpointDiscoveryMetadata endpoint)
		{
			return IsSelfDiscovery(serviceHost, endpoint) || filters.Any(filter => filter(endpoint));
		}

		private void ConfigureDomain(ServiceHost serviceHost)
		{
			var domainScopes =
				(from domain in serviceHost.Extensions.OfType<WcfDiscoveryDomain>()
				 from scope in domain.Scopes
				 select scope).ToArray();

			if (domainScopes.Length > 0)
			{
				AddFilter(endpoint =>
				{
					foreach (var domainScope in domainScopes)
					{
						if (endpoint.Scopes.Contains(domainScope) == false)
						{
							return true;
						}
					}
					return false;
				});
			}
		}

		private void ProbeInitialServices(ServiceHost serviceHost)
		{
			var probe = new DiscoveryClient(UdpDiscoveryEndpoint ?? new UdpDiscoveryEndpoint());
			probe.FindProgressChanged += (_, args) => RegisterService(serviceHost, args.EndpointDiscoveryMetadata);
			probe.FindCompleted += (_, args) => probe.Close();
			probe.FindAsync(ProbeCriteria ?? new FindCriteria());
		}

		private void MonitorAnnouncements(ServiceHost serviceHost)
		{
			var announcements = new AnnouncementService();
			announcementHost = new ServiceHost(announcements, new Uri[0]);
			announcementHost.Description.Behaviors.Find<ServiceBehaviorAttribute>().UseSynchronizationContext = false;
			announcementHost.AddServiceEndpoint(UdpAnnouncementEndpoint ?? new UdpAnnouncementEndpoint());
			announcements.OnlineAnnouncementReceived += (_, args) => RegisterService(serviceHost, args.EndpointDiscoveryMetadata);
			announcements.OfflineAnnouncementReceived += (_, args) => RemoveService(serviceHost, args.EndpointDiscoveryMetadata);
			announcementHost.Open();
		}

		private static bool IsSelfDiscovery(ServiceHost serviceHost, EndpointDiscoveryMetadata endpoint)
		{
			return serviceHost.Description.Endpoints.Any<ServiceEndpoint>(e => e.Address == endpoint.Address);
		}

		void IDisposable.Dispose()
		{
			if (announcementHost != null)
			{
				announcementHost.Close();
			}
		}
	}
#endif
}