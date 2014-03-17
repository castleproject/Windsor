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

namespace Castle.Facilities.Synchronize
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.Core.Configuration;
	using Castle.Core.Internal;
	using Castle.MicroKernel.SubSystems.Conversion;

	/// <summary>
	///   Maintains the synchronization meta-info for all components.
	/// </summary>
	public class SynchronizeMetaInfoStore
	{
		private const BindingFlags MethodBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

		private readonly IConversionManager converter;

		private readonly IDictionary<Type, SynchronizeMetaInfo> type2MetaInfo =
			new Dictionary<Type, SynchronizeMetaInfo>();

		/// <summary>
		///   Initializes a new instance of the <see cref = "SynchronizeMetaInfoStore" /> class.
		/// </summary>
		/// <param name = "conversionManager"></param>
		public SynchronizeMetaInfoStore(IConversionManager conversionManager)
		{
			converter = conversionManager;
		}

		/// <summary>
		///   Creates the meta-info from the specified type.
		/// </summary>
		/// <param name = "implementation">The implementation type.</param>
		/// <returns>The corresponding meta-info.</returns>
		public SynchronizeMetaInfo CreateMetaFromType(Type implementation)
		{
			var syncAttrib = implementation.GetAttributes<SynchronizeAttribute>()[0];
			var metaInfo = new SynchronizeMetaInfo(syncAttrib);

			PopulateMetaInfoFromType(metaInfo, implementation);

			Register(implementation, metaInfo);

			return metaInfo;
		}

		/// <summary>
		///   Creates the meta-info from the configuration.
		/// </summary>
		/// <param name = "implementation">The implementation type.</param>
		/// <param name = "config">The configuration.</param>
		/// <returns>The corresponding meta-info.</returns>
		public SynchronizeMetaInfo CreateMetaInfoFromConfig(Type implementation, IConfiguration config)
		{
			var syncAttrib = CreateAttributeFromConfig(config);
			var metaInfo = new SynchronizeMetaInfo(syncAttrib);

			Register(implementation, metaInfo);

			return metaInfo;
		}

		/// <summary>
		///   Gets the meta-info for the specified implementation type.
		/// </summary>
		/// <param name = "implementation">The implementation type.</param>
		/// <returns>The corresponding meta-info.</returns>
		public SynchronizeMetaInfo GetMetaFor(Type implementation)
		{
			SynchronizeMetaInfo metaInfo;
			type2MetaInfo.TryGetValue(implementation, out metaInfo);
			return metaInfo;
		}

		/// <summary>
		///   Populates the meta-info from the configuration.
		/// </summary>
		/// <param name = "implementation">The implementation.</param>
		/// <param name = "methods">The methods.</param>
		/// <param name = "config">The config.</param>
		public void PopulateMetaFromConfig(Type implementation, IList<MethodInfo> methods, IConfiguration config)
		{
			var metaInfo = GetMetaFor(implementation);

			if (metaInfo != null)
			{
				foreach (var method in methods)
				{
					var syncAttrib = CreateAttributeFromConfig(config);
					metaInfo.Add(method, syncAttrib);
				}
			}
		}

		/// <summary>
		///   Creates the synchronization attribute from configuration.
		/// </summary>
		/// <param name = "config">The configuration.</param>
		/// <returns>The corresponding synchronization attribute.</returns>
		private SynchronizeAttribute CreateAttributeFromConfig(IConfiguration config)
		{
			SynchronizeAttribute syncAttrib = null;

			if (config != null)
			{
				var contextRef = config.Attributes[Constants.ContextRefAttribute];

				if (contextRef != null)
				{
					syncAttrib = new SynchronizeAttribute(contextRef);
				}
				else
				{
					var contextType = config.Attributes[Constants.ContextTypeAttribute];

					if (contextType != null)
					{
						var type = converter.PerformConversion<Type>(contextType);
						syncAttrib = new SynchronizeAttribute(type);
					}
				}

				var useAmbientContext = config.Attributes[Constants.AmbientContextAttribute];

				if (useAmbientContext != null)
				{
					syncAttrib = syncAttrib ?? new SynchronizeAttribute();
					syncAttrib.UseAmbientContext = converter.PerformConversion<bool>(useAmbientContext);
				}
			}

			return syncAttrib ?? new SynchronizeAttribute();
		}

		/// <summary>
		///   Registers the meta-info for the specified implementation type.
		/// </summary>
		/// <param name = "implementation">The implementation type.</param>
		/// <param name = "metaInfo">The meta-info.</param>
		private void Register(Type implementation, SynchronizeMetaInfo metaInfo)
		{
			type2MetaInfo[implementation] = metaInfo;
		}

		/// <summary>
		///   Populates the meta-info from the attributes.
		/// </summary>
		/// <param name = "metaInfo">The meta info.</param>
		/// <param name = "implementation">The implementation type.</param>
		private static void PopulateMetaInfoFromType(SynchronizeMetaInfo metaInfo,
		                                             Type implementation)
		{
			if (implementation == typeof(object) || implementation == typeof(MarshalByRefObject))
			{
				return;
			}

			var methods = implementation.GetMethods(MethodBindingFlags);

			foreach (var method in methods)
			{
				var atts = method.GetAttributes<SynchronizeAttribute>();

				if (atts.Length != 0)
				{
					metaInfo.Add(method, atts[0]);
				}
			}

			PopulateMetaInfoFromType(metaInfo, implementation.BaseType);
		}
	}
}