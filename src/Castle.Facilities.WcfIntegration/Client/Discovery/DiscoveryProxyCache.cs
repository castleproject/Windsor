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
	using System;
	using System.ServiceModel.Discovery;
	using System.Threading;

	using Castle.Core.Internal;

	public class DiscoveryProxyCache : DiscoveryEndpointProvider, IDisposable
	{
		private readonly DiscoveryEndpointProvider inner;
		private volatile DiscoveryEndpoint endpoint;
		private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim();

		public DiscoveryProxyCache(DiscoveryEndpointProvider inner)
		{
			if (inner == null)
				throw new ArgumentNullException("inner");

			this.inner = inner;

			AbstractChannelBuilder.DiscoveryEndpointFaulted += DiscoveryEndpointFaulted;
		}

		public override DiscoveryEndpoint GetDiscoveryEndpoint()
		{
			@lock.EnterUpgradeableReadLock();
			try
			{
				if (endpoint != null)
					return endpoint;
				@lock.EnterWriteLock();
				try
				{
					if (endpoint == null)
						endpoint = inner.GetDiscoveryEndpoint();

					return endpoint;
				}
				finally
				{
					@lock.ExitWriteLock();
				}
			}
			finally
			{
				@lock.ExitUpgradeableReadLock();
			}
		}

		private void DiscoveryEndpointFaulted(object sender, DiscoveryEndpointFaultEventArgs args)
		{
			@lock.EnterUpgradeableReadLock();
			try
			{
				if (args.Culprit != endpoint)
					return;
				@lock.EnterWriteLock();
				try
				{
					endpoint = null;
				}
				finally
				{
					@lock.ExitWriteLock();
				}
			}
			finally
			{
				@lock.ExitUpgradeableReadLock();
			}
		}

		void IDisposable.Dispose()
		{
			AbstractChannelBuilder.DiscoveryEndpointFaulted -= DiscoveryEndpointFaulted;
			@lock.Dispose();
		}
	}
}
