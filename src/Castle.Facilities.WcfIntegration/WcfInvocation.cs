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
	using System.Reflection;
	using Castle.DynamicProxy;

	public class WcfInvocation
	{
		private Action proceed;
		private readonly IWcfChannelHolder channelHolder;
		private readonly IInvocation invocation;

		public WcfInvocation(IWcfChannelHolder channelHolder, IInvocation invocation)
		{
			this.channelHolder = channelHolder;
			this.invocation = invocation;
		}

		public IWcfChannelHolder ChannelHolder
		{
			get { return channelHolder; }
		}

		public object[] Arguments
		{
			get { return invocation.Arguments; }
		}

		public MethodInfo Method
		{
			get { return invocation.Method; }
		}

		public object ReturnValue { get; set; }

		public void Proceed()
		{
			var next = proceed;
			try
			{
				proceed();
			}
			finally
			{
				proceed = next;
			}
		}

		public WcfInvocation Refresh(bool force)
		{
			var oldChannel = channelHolder.Channel;
			var channel = channelHolder.RefreshChannel(force);
			if ((channel != oldChannel) && (invocation is IChangeProxyTarget))
			{
				var changeTarget = (IChangeProxyTarget)invocation;
				changeTarget.ChangeInvocationTarget(channel);
				changeTarget.ChangeProxyTarget(channel);
			}
			return this;
		}

		internal void SetProceedDelegate(Action value)
		{
			proceed = value;
		}
	}
}