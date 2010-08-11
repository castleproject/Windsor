// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System;
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	/// Inspects the component configuration and type looking for information
	/// that can influence the generation of a proxy for that component.
	/// <para>
	/// We specifically look for <c>useSingleInterfaceProxy</c> and <c>marshalByRefProxy</c> 
	/// on the component configuration or the <see cref="ComponentProxyBehaviorAttribute"/> 
	/// attribute.
	/// </para>
	/// </summary>
#if (!SILVERLIGHT)
	[Serializable]
#endif
	public class ComponentProxyInspector : IContributeComponentModelConstruction
	{
		/// <summary>
		/// Seaches for proxy behavior in the configuration and, if unsuccessful
		/// look for the <see cref="ComponentProxyBehaviorAttribute"/> attribute in 
		/// the implementation type.
		/// </summary>
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			ReadProxyBehavior(kernel, model);
		}

		/// <summary>
		/// Reads the proxy behavior associated with the 
		/// component configuration/type and applies it to the model.
		/// </summary>
		/// <exception cref="System.Exception">
		/// If the conversion fails
		/// </exception>
		/// <param name="kernel"></param>
		/// <param name="model"></param>
		protected virtual void ReadProxyBehavior(IKernel kernel, ComponentModel model)
		{
			ComponentProxyBehaviorAttribute proxyBehaviorAtt = GetProxyBehaviorFromType(model.Implementation);

			if (proxyBehaviorAtt == null)
			{
				proxyBehaviorAtt = new ComponentProxyBehaviorAttribute();
			}

			string useSingleInterfaceProxyAttrib = model.Configuration != null ? model.Configuration.Attributes["useSingleInterfaceProxy"] : null;
			
#if !SILVERLIGHT
			string marshalByRefProxyAttrib = model.Configuration != null ? model.Configuration.Attributes["marshalByRefProxy"] : null;
#endif

			var converter = kernel.GetConversionManager();
			if (useSingleInterfaceProxyAttrib != null)
			{
					proxyBehaviorAtt.UseSingleInterfaceProxy =
						converter.PerformConversion<bool?>(useSingleInterfaceProxyAttrib).GetValueOrDefault(false);
			}
#if !SILVERLIGHT
			if (marshalByRefProxyAttrib != null)
			{
				proxyBehaviorAtt.UseMarshalByRefProxy =
					converter.PerformConversion<bool?>(marshalByRefProxyAttrib).GetValueOrDefault(false);
			}
#endif
			ApplyProxyBehavior(proxyBehaviorAtt, model);
		}

		/// <summary>
		/// Returns a <see cref="ComponentProxyBehaviorAttribute"/> instance if the type
		/// uses the attribute. Otherwise returns null.
		/// </summary>
		/// <param name="implementation"></param>
		protected virtual ComponentProxyBehaviorAttribute GetProxyBehaviorFromType(Type implementation)
		{
			return implementation.GetAttributes<ComponentProxyBehaviorAttribute>().FirstOrDefault();
		}

		private static void ApplyProxyBehavior(ComponentProxyBehaviorAttribute behavior, ComponentModel model)
		{
			if (behavior.UseSingleInterfaceProxy
#if (!SILVERLIGHT)
				|| behavior.UseMarshalByRefProxy
#endif
				)
			{
				EnsureComponentRegisteredWithInterface(model);
			}

			ProxyOptions options = ProxyUtil.ObtainProxyOptions(model, true);

			options.UseSingleInterfaceProxy = behavior.UseSingleInterfaceProxy;
#if (!SILVERLIGHT)
			options.UseMarshalByRefAsBaseClass = behavior.UseMarshalByRefProxy;
#endif
			options.AddAdditionalInterfaces(behavior.AdditionalInterfaces);
		}

		private static void EnsureComponentRegisteredWithInterface(ComponentModel model)
		{
			if (!model.Service.IsInterface)
			{
				String message = String.Format("The class {0} requested a single interface proxy, " +
				                               "however the service {1} does not represent an interface",
				                               model.Implementation.FullName, model.Service.FullName);

				throw new ComponentRegistrationException(message);
			}
		}
	}
}