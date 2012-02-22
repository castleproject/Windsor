// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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
	using System.ServiceModel.Discovery;
	using Castle.Core.Internal;

	public class DiscoveryProxyCache : DiscoveryEndpointProvider, IDisposable
	{
		private readonly DiscoveryEndpointProvider inner;
		private volatile DiscoveryEndpoint endpoint;
		private readonly Lock @lock = Lock.Create();

		public DiscoveryProxyCache(DiscoveryEndpointProvider inner)
		{
			if (inner == null)
				throw new ArgumentNullException("inner");

			this.inner = inner;

			AbstractChannelBuilder.DiscoveryEndpointFaulted += DiscoveryEndpointFaulted;
		}

		public override DiscoveryEndpoint GetDiscoveryEndpoint()
		{
			using (var locker = @lock.ForReadingUpgradeable())
			{
				if (endpoint != null)
					return endpoint;

				locker.Upgrade();

				if (endpoint == null)
					endpoint = inner.GetDiscoveryEndpoint();

				return endpoint;
			}
		}

		private void DiscoveryEndpointFaulted(object sender, DiscoveryEndpointFaultEventArgs args)
		{
			using (var locker = @lock.ForReadingUpgradeable())
			{
				if (args.Culprit != endpoint)
					return;

				locker.Upgrade();

				endpoint = null;
			}
		}

		void IDisposable.Dispose()
		{
			AbstractChannelBuilder.DiscoveryEndpointFaulted -= DiscoveryEndpointFaulted;
		}
	}
#endif
}
