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

	public class AdHocChannelFactoryAware : AbstractChannelFactoryAware
	{
		private Action<ChannelFactory> onCreated;
		private Action<ChannelFactory> onOpened;
		private Action<ChannelFactory> onOpening;
		private Action<ChannelFactory> onClosed;
		private Action<ChannelFactory> onClosing;
		private Action<ChannelFactory> onFaulted;
		private Action<ChannelFactory, IChannel> onChannelCreated;
		private Action<ChannelFactory, IChannel> onChannelAvailable;

		public override void Created(ChannelFactory channelFactory)
		{
			Apply(channelFactory, onCreated);
		}

		public AdHocChannelFactoryAware OnCreated(Action<ChannelFactory> action)
		{
			onCreated += action;
			return this;
		}

		public override void Opening(ChannelFactory channelFactory)
		{
			Apply(channelFactory, onOpening);
		}

		public AdHocChannelFactoryAware OnOpening(Action<ChannelFactory> action)
		{
			onOpening += action;
			return this;
		}

		public override void Opened(ChannelFactory channelFactory)
		{
			Apply(channelFactory, onOpened);
		}

		public AdHocChannelFactoryAware OnOpened(Action<ChannelFactory> action)
		{
			onOpened += action;
			return this;
		}

		public override void Closing(ChannelFactory channelFactory)
		{
			Apply(channelFactory, onClosing);
		}

		public AdHocChannelFactoryAware OnClosing(Action<ChannelFactory> action)
		{
			onClosing += action;
			return this;
		}

		public override void Closed(ChannelFactory channelFactory)
		{
			Apply(channelFactory, onClosed);
		}

		public AdHocChannelFactoryAware OnClosed(Action<ChannelFactory> action)
		{
			onClosed += action;
			return this;
		}

		public override void Faulted(ChannelFactory channelFactory)
		{
			Apply(channelFactory, onFaulted);
		}

		public AdHocChannelFactoryAware OnFaulted(Action<ChannelFactory> action)
		{
			onFaulted += action;
			return this;
		}

		public override void ChannelCreated(ChannelFactory channelFactory, IChannel channel)
		{
			Apply(channelFactory, channel, onChannelCreated);
		}

		public AdHocChannelFactoryAware OnChannelCreated(Action<ChannelFactory, IChannel> action)
		{
			onChannelCreated += action;
			return this;
		}

		public override void ChannelAvailable(ChannelFactory channelFactory, IChannel channel)
		{
			Apply(channelFactory, channel, onChannelAvailable);
		}

		public AdHocChannelFactoryAware OnChannelAvailable(Action<ChannelFactory, IChannel> action)
		{
			onChannelAvailable += action;
			return this;
		}

		private static void Apply(ChannelFactory channelFactory, Action<ChannelFactory> action)
		{
			if (action != null)
			{
				action(channelFactory);
			}
		}

		private static void Apply(ChannelFactory channelFactory, IChannel channel, Action<ChannelFactory, IChannel> action)
		{
			if (action != null)
			{
				action(channelFactory, channel);
			}
		}
	}
}

