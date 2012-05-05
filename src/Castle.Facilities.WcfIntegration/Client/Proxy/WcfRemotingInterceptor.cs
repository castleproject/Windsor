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
// limitations under the License

namespace Castle.Facilities.WcfIntegration.Proxy
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.Remoting.Messaging;
	using System.Runtime.Remoting.Proxies;
	using Castle.DynamicProxy;
	using Castle.Facilities.WcfIntegration.Internal;

	public class WcfRemotingInterceptor : IWcfInterceptor
	{
		private readonly IWcfPolicy[] pipeline;
		protected readonly IWcfChannelHolder channelHolder;

		public WcfRemotingInterceptor(WcfClientExtension clients, IWcfChannelHolder channelHolder)
		{
			if (channelHolder == null)
			{
				throw new ArgumentException("The given Proxy is not valid WCF dynamic proxy.");
			}

			this.channelHolder = channelHolder;
			pipeline = CreateChannelPipeline(clients);
		}

		public void Intercept(IInvocation invocation)
		{
			PerformInvocation(invocation);
		}

		protected virtual void PerformInvocation(IInvocation invocation)
		{
			PerformInvocation(invocation, wcfInvocation =>
			{
				var realProxy = channelHolder.RealProxy;
				if (realProxy == null)
				{
					InvokeChannel(invocation, wcfInvocation);
				}
				else
				{
					InvokeRealProxy(realProxy, wcfInvocation);
				}
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

		private static void InvokeChannel(IInvocation invocation, WcfInvocation wcfInvocation)
		{
			invocation.Proceed();
			wcfInvocation.ReturnValue = invocation.ReturnValue;
		}

		private static void InvokeRealProxy(RealProxy realProxy, WcfInvocation wcfInvocation)
		{
			var message = new MethodCallMessage(wcfInvocation.Method, wcfInvocation.Arguments);
			var returnMessage = (IMethodReturnMessage)realProxy.Invoke(message);
			if (returnMessage.Exception != null)
			{
				var exception = ExceptionHelper.PreserveStackTrace(returnMessage.Exception);
				throw exception;
			}
			wcfInvocation.ReturnValue = returnMessage.ReturnValue;
		}

		protected void PerformInvocation(IInvocation invocation, Action<WcfInvocation> action)
		{
			var wcfInvocation = new WcfInvocation(channelHolder, invocation);
			ApplyChannelPipeline(0, wcfInvocation, action);
			invocation.ReturnValue = wcfInvocation.ReturnValue;
		}

		private void ApplyChannelPipeline(int policyIndex, WcfInvocation wcfInvocation, Action<WcfInvocation> action)
		{
			if (policyIndex >= pipeline.Length)
			{
				action(wcfInvocation);
				return;
			}
			var nextIndex = policyIndex + 1;
			wcfInvocation.SetProceedDelegate(() => ApplyChannelPipeline(nextIndex, wcfInvocation, action));
			pipeline[policyIndex].Apply(wcfInvocation);
		}

		private IWcfPolicy[] CreateChannelPipeline(WcfClientExtension clients)
		{
			var policies = channelHolder.ChannelBurden.Dependencies.OfType<IWcfPolicy>()
				.OrderBy(policy => policy.ExecutionOrder).ToArray();

			if (policies.Length == 0 && clients.DefaultChannelPolicy != null)
			{
				policies = clients.DefaultChannelPolicy;
			}

			return policies;
		}
	}
}