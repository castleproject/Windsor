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
// limitations under the License

namespace Castle.Facilities.WcfIntegration.Proxy
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Remoting.Messaging;
	using Castle.DynamicProxy;

	public class WcfRemotingInterceptor : IWcfInterceptor
	{
		private readonly IWcfChannelPolicy[] pipeline;

		public WcfRemotingInterceptor(WcfClientExtension clients, IWcfChannelHolder channelHolder)
		{
			pipeline = CreateChannelPipeline(clients, channelHolder);
		}

		public void Intercept(IInvocation invocation)
		{
			var channelHolder = invocation.Proxy as IWcfChannelHolder;

			if (channelHolder == null)
			{
				throw new ArgumentException("The given Proxy is not valid WCF dynamic proxy.");
			}

			PerformInvocation(invocation, channelHolder);
		}

		protected virtual void PerformInvocation(IInvocation invocation, IWcfChannelHolder channelHolder)
		{
			Action sendAction = () =>
			{
				var realProxy = channelHolder.RealProxy;
				var message = new MethodCallMessage(invocation.Method, invocation.Arguments);
				var returnMessage = (IMethodReturnMessage)realProxy.Invoke(message);
				if (returnMessage.Exception != null)
				{
					throw returnMessage.Exception;
				}
				invocation.ReturnValue = returnMessage.ReturnValue;
			};
			InvokeChannelPipeline(invocation, channelHolder, sendAction);
		}

		bool IWcfInterceptor.Handles(MethodInfo method)
		{
			return Handles(method);
		}

		protected virtual bool Handles(MethodInfo method)
		{
			return true;
		}

		protected void InvokeChannelPipeline(IInvocation invocation, IWcfChannelHolder channelHolder, Action action)
		{
			if (pipeline == null)
			{
				action();
			}
			else
			{
				InvokeChannelPipeline(0, new ChannelInvocation(invocation, channelHolder), action);
			}
		}

		private void InvokeChannelPipeline(int policyIndex, ChannelInvocation invocation, Action action)
		{
			int nextIndex;
			if (policyIndex >= pipeline.Length)
			{
				action();
			}
			else
			{
				nextIndex = policyIndex + 1;
				invocation.SetProceedDelegate(() => InvokeChannelPipeline(nextIndex, invocation, action));
				pipeline[policyIndex].Intercept(invocation);
			}
		}

		private static IWcfChannelPolicy[] CreateChannelPipeline(WcfClientExtension clients, IWcfChannelHolder channelHolder)
		{
			var policies = channelHolder.ChannelBurden.Dependencies.OfType<IWcfChannelPolicy>()
				.OrderBy(policy => policy.ExecutionOrder).ToArray();

			return (policies.Length == 0) ? clients.DefaultChannelPolicy.ToArray() : policies;
		}
	}
}