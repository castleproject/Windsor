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

namespace Castle.Facilities.WcfIntegration.Proxy
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Remoting;
	using System.ServiceModel;
	using Castle.Core;
	using Castle.DynamicProxy;
	using Castle.Facilities.WcfIntegration.Async;
	using Castle.Facilities.WcfIntegration.Async.TypeSystem;
	using Castle.Facilities.WcfIntegration.Internal;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Proxy;
	using Castle.Windsor.Proxy;

	public class WcfProxyFactory : AbstractProxyFactory
	{
		private readonly ProxyGenerator generator;
		private readonly WcfClientExtension clients;
		private AsyncType asyncType;
		private readonly WcfProxyGenerationHook wcfProxyGenerationHook;

		public WcfProxyFactory(ProxyGenerator generator, WcfClientExtension clients)
		{
			this.generator = generator;
			this.clients = clients;
			wcfProxyGenerationHook = new WcfProxyGenerationHook(null);
		}

		public override object Create(IProxyFactoryExtension customFactory, IKernel kernel, ComponentModel model,
									  CreationContext context, params object[] constructorArguments)
		{
			throw new NotSupportedException();
		}

		public override object Create(IKernel kernel, object instance, ComponentModel model, 
									  CreationContext context, params object[] constructorArguments)
		{
			var channelHolder = instance as IWcfChannelHolder;

			if (channelHolder == null)
			{
				throw new ArgumentException(string.Format("Given instance is not an {0}", typeof(IWcfChannelHolder)), "instance");
			}

			if (channelHolder.RealProxy == null)
			{
				return channelHolder.Channel;
			}
			if(model.Services.Count() > 1)
			{
				throw new ArgumentException(
					string.Format("Component {0}, which was designated as a WCF proxy exposes {1} services. The facility currently only supports single-service components.",
					              model.Name, model.Services.Count()));
			}

			var isDuplex = IsDuplex(channelHolder.RealProxy);
			var proxyOptions = model.ObtainProxyOptions();
			var generationOptions = CreateProxyGenerationOptions(model.Services.Single(), proxyOptions, kernel, context);
			var additionalInterfaces = GetInterfaces(model.Services, proxyOptions, isDuplex);
			var interceptors = GetInterceptors(kernel, model, channelHolder, context);

			return generator.CreateInterfaceProxyWithTarget(typeof(IWcfChannelHolder),
				additionalInterfaces, channelHolder, generationOptions, interceptors);
		}

		public override bool RequiresTargetInstance(IKernel kernel, ComponentModel model)
		{
			return true;
		}

		protected static bool IsDuplex(object realProxy)
		{
			var typeInfo = (IRemotingTypeInfo)realProxy;
			return typeInfo.CanCastTo(typeof(IDuplexContextChannel), null);
		}

		protected virtual Type[] GetInterfaces(IEnumerable<Type> services, ProxyOptions proxyOptions, bool isDuplex)
		{
			var interfaces = services.ToList();
			if(proxyOptions.AdditionalInterfaces != null)
			{
				interfaces.AddRange(proxyOptions.AdditionalInterfaces);
			}
			interfaces.Add(typeof(IServiceChannel));
			interfaces.Add(typeof(IClientChannel));

			if (isDuplex)
			{
				interfaces.Add(typeof(IDuplexContextChannel));
			}
			return interfaces.ToArray();
		}

		private IInterceptor[] GetInterceptors(IKernel kernel, ComponentModel model, IWcfChannelHolder channelHolder, CreationContext context)
		{
			var interceptors = ObtainInterceptors(kernel, model, context);

			// TODO: this should be static and happen in IContributeComponentModelConstruction preferably
			var clientModel = (IWcfClientModel)model.ExtendedProperties[WcfConstants.ClientModelKey];
			Array.Resize(ref interceptors, interceptors.Length + (clientModel.WantsAsyncCapability ? 2 : 1));
			int index = interceptors.Length;

			interceptors[--index] = new WcfRemotingInterceptor(clients, channelHolder);

			if (clientModel.WantsAsyncCapability)
			{
				var getAsyncType = WcfUtils.SafeInitialize(ref asyncType,
					() => AsyncType.GetAsyncType(model.Services.Single()));
				interceptors[--index] = new WcfRemotingAsyncInterceptor(getAsyncType, clients, channelHolder);
			}

			return interceptors;
		}

		private ProxyGenerationOptions CreateProxyGenerationOptions(Type service, ProxyOptions proxyOptions, IKernel kernel, CreationContext context)
		{
			if (proxyOptions.MixIns != null && proxyOptions.MixIns.Count() > 0)
			{
				throw new NotImplementedException(
					"Support for mixins is not yet implemented. How about contributing a patch?");
			}

			var userProvidedSelector = (proxyOptions.Selector != null) ? proxyOptions.Selector.Resolve(kernel, context) : null;

			var proxyGenOptions = new ProxyGenerationOptions(wcfProxyGenerationHook)
			{
				Selector = new WcfInterceptorSelector(service, userProvidedSelector)
			};

			return proxyGenOptions;
		}
	}
}
