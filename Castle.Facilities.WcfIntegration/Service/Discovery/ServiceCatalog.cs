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
	using System.Linq;
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

		protected override IAsyncResult OnBeginFind(FindRequestContext findRequestContext, AsyncCallback callback, object state)
		{
			implementation.FindService(findRequestContext);
			return new SynchronousResult(callback, state);
		}

		protected override void OnEndFind(IAsyncResult result)
		{
			AsyncResult.End(result);
		}

		protected override IAsyncResult OnBeginResolve(ResolveCriteria resolveCriteria, AsyncCallback callback, object state)
		{
			return new SynchronousResult(callback, state, implementation.ResolveService(resolveCriteria));
		}

		protected override EndpointDiscoveryMetadata OnEndResolve(IAsyncResult result)
		{
			return AsyncResult.End<EndpointDiscoveryMetadata>(result);
		}

		protected override IAsyncResult OnBeginOnlineAnnouncement(
			DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata, 
			AsyncCallback callback, object state)
		{
			implementation.RegisterService(endpointDiscoveryMetadata);
			return new SynchronousResult(callback, state);
		}

		protected override void OnEndOnlineAnnouncement(IAsyncResult result)
		{
			AsyncResult.End(result);
		}

		protected override IAsyncResult OnBeginOfflineAnnouncement(
			DiscoveryMessageSequence messageSequence, EndpointDiscoveryMetadata endpointDiscoveryMetadata,
			AsyncCallback callback, object state)
		{
			implementation.RemoveService(endpointDiscoveryMetadata);
			return new SynchronousResult(callback, state);
		}

		protected override void OnEndOfflineAnnouncement(IAsyncResult result)
		{
			AsyncResult.End(result);
		}

		EndpointDiscoveryMetadata11[] IServiceCatalog.ListServices()
		{
			return null;
		}

		IAsyncResult IServiceCatalog.BeginListServices(AsyncCallback callback, object state)
		{
			return new SynchronousResult(callback, state, implementation.ListServices()
				.Select(service => EndpointDiscoveryMetadata11.FromEndpointDiscoveryMetadata(service)));
		}

		EndpointDiscoveryMetadata11[] IServiceCatalog.EndListServices(IAsyncResult result)
		{
			return AsyncResult.End<EndpointDiscoveryMetadata11[]>(result);
		}

		EndpointDiscoveryMetadata11[] IServiceCatalog.FindServices(FindCriteria11 criteria)
		{
			return null;
		}

        IAsyncResult IServiceCatalog.BeginFindServices(FindCriteria11 criteria, AsyncCallback callback, object state)
        {
            return new SynchronousResult(callback, state, implementation.FindServices(criteria.ToFindCriteria())
				.Select(service => EndpointDiscoveryMetadata11.FromEndpointDiscoveryMetadata(service)));
        }

        EndpointDiscoveryMetadata11[] IServiceCatalog.EndFindServices(IAsyncResult result)
        {
            return AsyncResult.End<EndpointDiscoveryMetadata11[]>(result);
        }
    }
#endif
}

