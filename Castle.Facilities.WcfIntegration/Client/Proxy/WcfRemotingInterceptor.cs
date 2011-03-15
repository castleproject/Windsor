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
		private readonly IWcfPolicy[] pipeline;

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
			PerformInvocation(invocation, channelHolder, wcfInvocation =>
			{
				var realProxy = channelHolder.RealProxy;
				var message = new MethodCallMessage(wcfInvocation.Method, wcfInvocation.Arguments);
				var returnMessage = (IMethodReturnMessage)realProxy.Invoke(message);
				if (returnMessage.Exception != null)
				{
					throw returnMessage.Exception;
				}
				wcfInvocation.ReturnValue = returnMessage.ReturnValue;
			});
		}

		bool IWcfInterceptor.Handles(MethodInfo method)
		{
			return Handles(method);
		}

		protected virtual bool Handles(MethodInfo method)
		{
			return true;
		}

		protected void PerformInvocation(IInvocation invocation, IWcfChannelHolder channelHolder, Action<WcfInvocation> action)
		{
			var wcfInvocation = new WcfInvocation(channelHolder, invocation);
			InvokeChannelPipeline(0, wcfInvocation, action);
			invocation.ReturnValue = wcfInvocation.ReturnValue;
		}

		private void InvokeChannelPipeline(int policyIndex, WcfInvocation wcfInvocation, Action<WcfInvocation> action)
		{
			if (policyIndex >= pipeline.Length)
			{
				action(wcfInvocation);
				return;
			}
			var nextIndex = policyIndex + 1;
			wcfInvocation.SetProceedDelegate(() => InvokeChannelPipeline(nextIndex, wcfInvocation, action));
			pipeline[policyIndex].Apply(wcfInvocation);
		}

		private static IWcfPolicy[] CreateChannelPipeline(WcfClientExtension clients, IWcfChannelHolder channelHolder)
		{
			var policies = channelHolder.ChannelBurden.Dependencies.OfType<IWcfPolicy>()
				.OrderBy(policy => policy.ExecutionOrder).ToArray();

			if (policies.Length == 0 && clients.DefaultChannelPolicy != null)
			{
				policies = clients.DefaultChannelPolicy.ToArray();
			}

			return policies;
		}
	}
}