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

namespace Castle.Facilities.TypedFactory
{
	using System;
	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	/// Summary description for TypedFactoryFacility.
	/// </summary>
	public class TypedFactoryFacility : AbstractFacility
	{
		public static readonly string InterceptorKey = "Castle.TypedFactory.Interceptor";
		public static readonly string DelegateFactoryKey = "Castle.TypedFactory.DelegateFactory";
		public static readonly string DelegateBuilderKey = "Castle.TypedFactory.DelegateBuilder";

		[Obsolete("This method is obsolete. Use AsFactory() extension method on fluent registration API instead.")]
		public void AddTypedFactoryEntry(FactoryEntry entry)
		{
			var model = new ComponentModel(entry.Id, entry.FactoryInterface, typeof(Empty)) { LifestyleType = LifestyleType.Singleton };

			model.ExtendedProperties["typed.fac.entry"] = entry;
			model.Interceptors.Add(new InterceptorReference(typeof(FactoryInterceptor)));

			var proxyOptions = ProxyUtil.ObtainProxyOptions(model, true);
			proxyOptions.OmitTarget = true;

			Kernel.AddCustomComponent(model);
		}

		protected override void Init()
		{
			InitInterfaceBasedFactory();
			InitDelegateBasedFactory();

			LegacyInit();
		}

		private void InitDelegateBasedFactory()
		{
			Kernel.Resolver.AddSubResolver(new ParametersBinder());

			Kernel.Register(Component.For<ILazyComponentLoader>()
			                	.ImplementedBy<LightweightFactory>()
			                	.Named(DelegateFactoryKey)
			                	.Unless(Component.ServiceAlreadyRegistered),
			                Component.For<IDelegateBuilder>()
			                	.ImplementedBy<ExpressionTreeBasedDelegateBuilder>()
			                	.Named(DelegateBuilderKey)
			                	.Unless(Component.ServiceAlreadyRegistered));
		}

		private void InitInterfaceBasedFactory()
		{
			Kernel.Register(Component.For<TypedFactoryInterceptor>()
			                	.Named(InterceptorKey)
			                	.Unless(Component.ServiceAlreadyRegistered));
		}

		private void LegacyInit()
		{
			Kernel.AddComponent("typed.fac.interceptor", typeof(FactoryInterceptor));

			var converter = (ITypeConverter)
			                Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);

			AddFactories(FacilityConfig, converter);
		}

		protected virtual void AddFactories(IConfiguration facilityConfig, ITypeConverter converter)
		{
			if (facilityConfig == null)
			{
				return;
			}

			foreach (var config in facilityConfig.Children["factories"].Children)
			{
				var id = config.Attributes["id"];
				var creation = config.Attributes["creation"];
				var destruction = config.Attributes["destruction"];

				var factoryType = (Type)converter.PerformConversion(config.Attributes["interface"], typeof(Type));
				if(string.IsNullOrEmpty(creation))
				{
					RegisterFactory(id, factoryType);
					continue;
				}

				RegisterFactoryLegacy(creation, id, factoryType, destruction);
			}
		}

		private void RegisterFactory(string id, Type type)
		{
			var model = new ComponentModel(id, type, type);
			model.Interceptors.AddLast(new InterceptorReference(InterceptorKey));
			ProxyUtil.ObtainProxyOptions(model, true).OmitTarget = true;

			Kernel.AddCustomComponent(model);
		}

		private void RegisterFactoryLegacy(string creation, string id, Type factoryType, string destruction)
		{
			try
			{
#pragma warning disable 0618 //call to obsolete method
				AddTypedFactoryEntry(new FactoryEntry(id, factoryType, creation, destruction));
#pragma warning restore
			}
			catch (Exception)
			{
				string message = "Invalid factory entry in configuration";

				throw new Exception(message);
			}
		}
	}
}
