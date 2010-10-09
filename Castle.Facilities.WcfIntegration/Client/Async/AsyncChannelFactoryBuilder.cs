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

namespace Castle.Facilities.WcfIntegration.Async
{
	using System;
	using System.Diagnostics;
	using System.Reflection;
	using System.ServiceModel;
	using System.ServiceModel.Description;

	using Castle.DynamicProxy;
	using Castle.Facilities.WcfIntegration.Async.TypeSystem;

	public class AsynChannelFactoryBuilder<M> : DefaultChannelFactoryBuilder<M>
		where M : IWcfClientModel
	{
		private readonly ProxyGenerator generator;
		private readonly AsyncChannelFactoryProxyHook asyncChannelFactoryProxyHook;

		public AsynChannelFactoryBuilder()
		{
			generator = new ProxyGenerator();
			asyncChannelFactoryProxyHook = new AsyncChannelFactoryProxyHook();
		}

		public override ChannelFactory CreateChannelFactory(Type channelFactoryType, M clientModel, 
															params object[] constructorArgs)
		{
			if (!clientModel.WantsAsyncCapability)
			{
				return base.CreateChannelFactory(channelFactoryType, clientModel, constructorArgs);
			}

			EnsureValidChannelFactoryType(channelFactoryType);

			ReplaceServiceEndpointAsyncContracts(constructorArgs);

			var interceptor = new CreateDescriptionInterceptor();
			var proxyOptions = new ProxyGenerationOptions(asyncChannelFactoryProxyHook);
			return (ChannelFactory)generator.CreateClassProxy(
				channelFactoryType, Type.EmptyTypes, proxyOptions, constructorArgs, interceptor);
		}

		private static void ReplaceServiceEndpointAsyncContracts(object[] constructorArgs)
		{
			for (int i = 0; i < constructorArgs.Length; ++i)
			{
				var endpoint = constructorArgs[i] as ServiceEndpoint;
				if (endpoint != null)
				{
					var asyncEndpoint = new ServiceEndpoint(ContractDescription.GetContract(
						AsyncType.GetAsyncType(endpoint.Contract.ContractType)))
					{
						Name = endpoint.Name,
						Address = endpoint.Address,
						Binding = endpoint.Binding,
						ListenUri = endpoint.ListenUri,
						ListenUriMode = endpoint.ListenUriMode
					};

					asyncEndpoint.Behaviors.Clear();
					foreach (var behavior in endpoint.Behaviors)
					{
						asyncEndpoint.Behaviors.Add(behavior);
					}

					constructorArgs[i] = asyncEndpoint;
				}
			}
		}
	}

	#region Class AsyncChannelFactoryProxyHook
	
	class AsyncChannelFactoryProxyHook : IProxyGenerationHook
	{
		public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
		{
			if (methodInfo.Name == "CreateDescription")
			{
				return true;
			}
			return false;
		}

		public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
		{
		}

		public void MethodsInspected()
		{
		}

		public override bool Equals(object obj)
		{
			return obj != null && GetType().Equals(obj.GetType());
		}

		public override int GetHashCode()
		{
			return GetType().GetHashCode();
		}
	}

	#endregion

	#region Class CreateDescriptionInterceptor

	class CreateDescriptionInterceptor : IInterceptor
	{
		private bool applied;

		public void Intercept(IInvocation invocation)
		{
			if (!applied)
			{
				SetAsyncTypeAsTargetType(invocation.InvocationTarget);
				applied = true;
			}

			invocation.Proceed();
		}

		private static void SetAsyncTypeAsTargetType(object target)
		{
			var channelFactoryType = FindTypeContainingChannelTypeField(target.GetType());
			if (channelFactoryType == null)
			{
				// This should literally never happen...
				return;
			}

			var channelTypeField = GetChannelTypeField(channelFactoryType);
			var channelType = (Type)channelTypeField.GetValue(target);
			channelTypeField.SetValue(target, AsyncType.GetAsyncType(channelType));
		}

		private static FieldInfo GetChannelTypeField(Type channelFactoryType)
		{
			return channelFactoryType.GetField("channelType", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		private static Type FindTypeContainingChannelTypeField(Type channelFactoryType)
		{
			Debug.Assert(typeof(ChannelFactory).IsAssignableFrom(channelFactoryType),
			             "typeof(ChannelFactory).IsAssignableFrom(channelFactoryType)");
			do
			{
				if (channelFactoryType.BaseType == typeof(ChannelFactory))
				{
					return channelFactoryType;
				}

				channelFactoryType = channelFactoryType.BaseType;
			} while (channelFactoryType != null);

			return null;
		}
	}

	#endregion
}
