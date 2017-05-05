// Copyright 2004-2013 Castle Project - http://www.castleproject.org/
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
	using System.ServiceModel.Discovery;
	using System.ServiceModel.Discovery.Version11;

	using Castle.Facilities.WcfIntegration.Internal;

	public class ServiceCatalog : DiscoveryProxy, IServiceCatalog
	{
		private readonly IServiceCatalogImplementation implementation;

		public ServiceCatalog() : this(new InMemoryServiceCatalog())
		{
		}

		public ServiceCatalog(IServiceCatalogImplementation implementation)
		{
			if (implementation == null)
			{
				throw new ArgumentNullException("implementation");
			}
			this.implementation = implementation;
		}

		public IAsyncResult BeginFindEndpoints(FindCriteria11 criteria, AsyncCallback callback, object state)
		{
			return new SynchronousResult(callback, state, FindEndpoints(criteria));
		}

		public IAsyncResult BeginListEndpoints(AsyncCallback callback, object state)
		{
			return new SynchronousResult(callback, state, ListEndpoints());
		}

		public EndpointDiscoveryMetadata11[] EndFindEndpoints(IAsyncResult result)
		{
			return AsyncResult.End<EndpointDiscoveryMetadata11[]>(result);
		}

		public EndpointDiscoveryMetadata11[] EndListEndpoints(IAsyncResult result)
		{
			return AsyncResult.End<EndpointDiscoveryMetadata11[]>(result);
		}

		public EndpointDiscoveryMetadata11[] FindEndpoints(FindCriteria11 criteria)
		{
			var endpoints = implementation.FindEndpoints(criteria.ToFindCriteria());
			return Array.ConvertAll(endpoints, EndpointDiscoveryMetadata11.FromEndpointDiscoveryMetadata);
		}

		public EndpointDiscoveryMetadata11[] ListEndpoints()
		{
			var endpoints = implementation.ListEndpoints();
			return Array.ConvertAll(endpoints, EndpointDiscoveryMetadata11.FromEndpointDiscoveryMetadata);
		}

		protected override IAsyncResult OnBeginFind(FindRequestContext findRequestContext, AsyncCallback callback, object state)
		{
			implementation.FindEndpoints(findRequestContext);
			return new SynchronousResult(callback, state);
		}

		protected override IAsyncResult OnBeginOfflineAnnouncement(
			DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata,
			AsyncCallback callback, object state)
		{
			implementation.RemoveEndpoint(endpointDiscoveryMetadata);
			return new SynchronousResult(callback, state);
		}

		protected override IAsyncResult OnBeginOnlineAnnouncement(
			DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata,
			AsyncCallback callback, object state)
		{
			implementation.RegisterEndpoint(endpointDiscoveryMetadata);
			return new SynchronousResult(callback, state);
		}

		protected override IAsyncResult OnBeginResolve(ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
		{
			return new SynchronousResult(callback, state, implementation.ResolveEndpoint(resolveCriteria));
		}

		protected override void OnEndFind(IAsyncResult result)
		{
			AsyncResult.End(result);
		}

		protected override void OnEndOfflineAnnouncement(IAsyncResult result)
		{
			AsyncResult.End(result);
		}

		protected override void OnEndOnlineAnnouncement(IAsyncResult result)
		{
			AsyncResult.End(result);
		}

		protected override EndpointDiscoveryMetadata OnEndResolve(IAsyncResult result)
		{
			return AsyncResult.End<EndpointDiscoveryMetadata>(result);
		}
	}
}