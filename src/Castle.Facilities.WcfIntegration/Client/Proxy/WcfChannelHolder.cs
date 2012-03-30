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
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Runtime.Remoting;
	using System.Runtime.Remoting.Proxies;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.Threading;
	using Castle.Core.Internal;
	using Castle.Facilities.WcfIntegration.Internal;

	public class WcfChannelHolder : IWcfChannelHolder
	{
		private readonly IWcfBurden burden;
		private readonly TimeSpan? closeTimeout;
		private readonly ChannelFactory channelFactory;
		private readonly ChannelCreator channelCreator;
		private IChannel channel;
		private RealProxy realProxy;
		private int disposed;
		private readonly Lock @lock = Lock.Create();

		public WcfChannelHolder(ChannelCreator channelCreator, IWcfBurden burden, TimeSpan? closeTimeout)
		{
			this.channelCreator = channelCreator;
			this.burden = burden;
			this.closeTimeout = closeTimeout;

			CreateChannel();

			channelFactory = ObtainChannelFactory();
		}

		public IChannel Channel
		{
			get 
			{
				using (@lock.ForReading())
				{
					return channel;
				}
			}
		}

		public RealProxy RealProxy
		{
			get 
			{
				using (@lock.ForReading())
				{
					return realProxy;
				}
			}
		}

		public ChannelFactory ChannelFactory
		{
			get { return channelFactory; }
		}

		public IWcfBurden ChannelBurden
		{
			get { return burden; }
		}

		public TimeSpan? CloseTimeout
		{
			get { return closeTimeout; }
		}

		public bool IsChannelReady
		{
			get
			{
				using (@lock.ForReading())
				{
					return IsChannelUsable();
				}
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public IChannel RefreshChannel(bool force)
		{
			if (disposed != 0)
				return channel;

			if (force == false)
			{
				using (@lock.ForReading())
				{
					if (IsChannelUsable())
						return channel;
				}
			}

			using (var locker = @lock.ForReadingUpgradeable())
			{
				if (force || IsChannelUsable() == false)
				{
					locker.Upgrade();

					var oldChannel = channel;
					if (oldChannel != null)
					{
						WcfUtils.ReleaseCommunicationObject(oldChannel, closeTimeout);
					}

					CreateChannel();

					NotifyChannelRefreshed(oldChannel, channel);
				}
				return channel;
			}
		}

		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
				return;

			var context = channel as IContextChannel;
			if (context != null)
			{
				foreach (var cleanUp in context.CleanUp())
				{
					cleanUp.CleanUp();
				}
			}

			var parameters = channel.GetProperty<ChannelParameterCollection>();
			if (parameters != null)
			{
				foreach (var cleanUp in parameters.OfType<IWcfCleanUp>())
				{
					cleanUp.CleanUp();
				}
			}

			if (channel != null)
			{
				WcfUtils.ReleaseCommunicationObject(channel, closeTimeout);
			}
		}

		private void CreateChannel()
		{
			channel = (IChannel)channelCreator();
			realProxy = RemotingServices.GetRealProxy(channel);
		}

		private bool IsChannelUsable()
		{
			return channel != null && WcfUtils.IsCommunicationObjectReady(channel);
		}

		private ChannelFactory ObtainChannelFactory()
		{
			var channelFactoryHolder = ChannelBurden.Dependencies.OfType<ChannelFactoryHolder>().FirstOrDefault();
			return (channelFactoryHolder != null) ? channelFactoryHolder.ChannelFactory : null;
		}

		private void NotifyChannelRefreshed(IChannel oldChannel, IChannel newChannel)
		{
			if (channelFactory != null)
			{
				foreach (var observer in ChannelBurden.Dependencies.OfType<IChannelFactoryAware>())
				{
					observer.ChannelRefreshed(channelFactory, oldChannel, newChannel);
				}
			}
		}
	}
}