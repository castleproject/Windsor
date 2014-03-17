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

namespace Castle.MicroKernel.ModelBuilder.Inspectors
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.MicroKernel.Proxy;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	///   Inspects the component configuration and type looking for information
	///   that can influence the generation of a proxy for that component.
	///   <para>
	///     We specifically look for <c>additionalInterfaces</c> and <c>marshalByRefProxy</c> 
	///     on the component configuration or the <see cref = "ComponentProxyBehaviorAttribute" /> 
	///     attribute.
	///   </para>
	/// </summary>
	[Serializable]
	public class ComponentProxyInspector : IContributeComponentModelConstruction
	{
		private readonly IConversionManager converter;

		public ComponentProxyInspector(IConversionManager converter)
		{
			this.converter = converter;
		}

		/// <summary>
		///   Searches for proxy behavior in the configuration and, if unsuccessful
		///   look for the <see cref = "ComponentProxyBehaviorAttribute" /> attribute in 
		///   the implementation type.
		/// </summary>
		public virtual void ProcessModel(IKernel kernel, ComponentModel model)
		{
			ReadProxyBehavior(kernel, model);
		}

		/// <summary>
		///   Returns a <see cref = "ComponentProxyBehaviorAttribute" /> instance if the type
		///   uses the attribute. Otherwise returns null.
		/// </summary>
		/// <param name = "implementation"></param>
		protected virtual ComponentProxyBehaviorAttribute ReadProxyBehaviorFromType(Type implementation)
		{
			return implementation.GetAttributes<ComponentProxyBehaviorAttribute>().FirstOrDefault();
		}

		/// <summary>
		///   Reads the proxy behavior associated with the 
		///   component configuration/type and applies it to the model.
		/// </summary>
		/// <exception cref = "System.Exception">
		///   If the conversion fails
		/// </exception>
		/// <param name = "kernel"></param>
		/// <param name = "model"></param>
		protected virtual void ReadProxyBehavior(IKernel kernel, ComponentModel model)
		{
			var proxyBehaviorAttribute = ReadProxyBehaviorFromType(model.Implementation);
			if (proxyBehaviorAttribute == null)
			{
				proxyBehaviorAttribute = new ComponentProxyBehaviorAttribute();
			}

			ReadProxyBehaviorFromConfig(model, proxyBehaviorAttribute);

			ApplyProxyBehavior(proxyBehaviorAttribute, model);
		}

		private void ReadProxyBehaviorFromConfig(ComponentModel model, ComponentProxyBehaviorAttribute behavior)
		{
			if (model.Configuration == null)
			{
				return;
			}
#if !SILVERLIGHT
			var mbrProxy = model.Configuration.Attributes["marshalByRefProxy"];
			if (mbrProxy != null)
			{
				behavior.UseMarshalByRefProxy = converter.PerformConversion<bool?>(mbrProxy).GetValueOrDefault(false);
			}
#endif
			var interfaces = model.Configuration.Children["additionalInterfaces"];
			if (interfaces == null)
			{
				return;
			}
			var list = new List<Type>(behavior.AdditionalInterfaces);
			foreach (var node in interfaces.Children)
			{
				var interfaceTypeName = node.Attributes["interface"];
				var @interface = converter.PerformConversion<Type>(interfaceTypeName);
				list.Add(@interface);
			}
			behavior.AdditionalInterfaces = list.ToArray();
		}

		private static void ApplyProxyBehavior(ComponentProxyBehaviorAttribute behavior, ComponentModel model)
		{
			var options = model.ObtainProxyOptions();
#if !SILVERLIGHT
			if (behavior.UseMarshalByRefProxy)
			{
				EnsureComponentRegisteredWithInterface(model);
			}
			options.UseMarshalByRefAsBaseClass = behavior.UseMarshalByRefProxy;
#endif
			options.AddAdditionalInterfaces(behavior.AdditionalInterfaces);
			if(model.Implementation.IsInterface)
			{
				options.OmitTarget = true;
			}
		}

#if !SILVERLIGHT
		private static void EnsureComponentRegisteredWithInterface(ComponentModel model)
		{
			if (model.HasClassServices)
			{
				var message = String.Format("The class {0} requested a single interface proxy, " +
				                            "however the service {1} does not represent an interface",
				                            model.Implementation.FullName, model.Services.First().FullName);

				throw new ComponentRegistrationException(message);
			}
		}
#endif
	}
}