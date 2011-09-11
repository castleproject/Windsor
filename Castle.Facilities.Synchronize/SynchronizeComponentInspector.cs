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
	using System.ComponentModel;
	using System.Reflection;
	using System.Windows.Threading;

	using Castle.Core;
	using Castle.Core.Internal;
	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.ModelBuilder.Inspectors;
	using Castle.MicroKernel.Proxy;

	///<summary>
	///  Obtain synchronization configuration based on the
	///  component configuration check for the attributes if
	///  not available.
	///</summary>
	///<example>
	///  <component id = "component1" synchronized = "true" service = "SyncTest.IService, SyncTest"
	///    type = "SyncTest.IService, SyncTest">
	///    <synchronize contextRef = "DefaultContextKey">
	///      <method name = "Method1" contextRef = "MyContextKey"></method>
	///      <method name = "Method2" contextType = "SynchornizationContext"></method>
	///    </synchronize>
	///  </component>
	///</example>
	internal class SynchronizeComponentInspector : MethodMetaInspector
	{
		private static readonly String SynchronizeNodeName = "synchronize";
		private readonly SynchronizeMetaInfoStore metaStore;

		/// <summary>
		///   Initializes a new instance of the <see cref = "SynchronizeComponentInspector" /> class.
		/// </summary>
		/// <param name = "metaStore"></param>
		public SynchronizeComponentInspector(SynchronizeMetaInfoStore metaStore)
		{
			this.metaStore = metaStore;
		}

		/// <summary>
		///   Checks for synchronization configuration information or
		///   attributes and applies them if valid.
		/// </summary>
		/// <param name = "kernel">The kernel.</param>
		/// <param name = "model">The model.</param>
		public override void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (CheckFromConfiguration(model))
			{
				base.ProcessModel(kernel, model);
			}
			else
			{
				CheckFromAttributes(model);
			}

			if (ValidateSynchronization(model))
			{
				ApplySynchronization(model, kernel);
			}
		}

		/// <summary>
		///   Obtains the name of the node.
		/// </summary>
		/// <returns></returns>
		protected override String ObtainNodeName()
		{
			return SynchronizeNodeName;
		}

		/// <summary>
		///   Processes the meta information available on the component
		///   configuration.
		/// </summary>
		/// <param name = "model">The model.</param>
		/// <param name = "methods">The methods.</param>
		/// <param name = "metaModel">The meta model.</param>
		protected override void ProcessMeta(ComponentModel model, IList<MethodInfo> methods, MethodMetaModel metaModel)
		{
			metaStore.PopulateMetaFromConfig(model.Implementation, methods, metaModel.ConfigNode);
		}

		/// <summary>
		///   Applies the synchronization support to the model.
		/// </summary>
		/// <param name = "model">The model.</param>
		/// <param name = "kernel">The kernel.</param>
		private void ApplySynchronization(ComponentModel model, IKernel kernel)
		{
			var options = model.ObtainProxyOptions();

			model.Interceptors.Add(new InterceptorReference(typeof(SynchronizeInterceptor)));

			var metaInfo = metaStore.GetMetaFor(model.Implementation);

			if (metaInfo != null)
			{
				IInterceptorSelector userSelector = null;
				if (options.Selector != null)
				{
					userSelector = options.Selector.Resolve(kernel, CreationContext.CreateEmpty());
				}

				options.Selector = new InstanceReference<IInterceptorSelector>(new SynchronizeInterceptorSelector(metaInfo, userSelector));
				foreach (var reference in metaInfo.GetUniqueSynchContextReferences())
				{
					reference.Attach(model);
				}
			}
		}

		/// <summary>
		///   Obtains synchronization information from the attributes.
		/// </summary>
		/// <param name = "model">The model.</param>
		private void CheckFromAttributes(ComponentModel model)
		{
			var attributes = model.Implementation.GetAttributes<SynchronizeAttribute>();
			if (attributes.Length != 0)
			{
				metaStore.CreateMetaFromType(model.Implementation);
			}
		}

		/// <summary>
		///   Obtains synchronization information from the configuration.
		/// </summary>
		/// <param name = "model">The model.</param>
		/// <returns>
		///   <c>true</c> if available from configuration; otherwise, <c>false</c>.
		/// </returns>
		private bool CheckFromConfiguration(ComponentModel model)
		{
			if (model.Configuration == null || "true" != model.Configuration.Attributes[Constants.SynchronizedAttrib])
			{
				return false;
			}

			var methodsNode = model.Configuration.Children[ObtainNodeName()];
			metaStore.CreateMetaInfoFromConfig(model.Implementation, methodsNode);
			return true;
		}

		/// <summary>
		///   Validates the synchronization to be applied.
		/// </summary>
		/// <param name = "model">The model.</param>
		/// <returns>
		///   <c>true</c> if valid synchronization; otherwise, <c>false</c>.
		/// </returns>
		private bool ValidateSynchronization(ComponentModel model)
		{
			var meta = metaStore.GetMetaFor(model.Implementation);

			if (meta == null)
			{
				return HasImplicitSynchronization(model);
			}

			if (model.HasClassServices == false)
			{
				return true;
			}

			var methodCulprits = new List<String>();

			foreach (var method in meta.Methods)
			{
				if (method.IsVirtual == false)
				{
					methodCulprits.Add(method.Name);
				}
			}

			if (methodCulprits.Count != 0)
			{
				var methodNames = methodCulprits.ToArray();

				var message = String.Format("The class {0} wants to use synchronize interception, " +
				                            "however the methods must be marked as virtual in order to do so. Please correct " +
				                            "the following methods: {1}", model.Implementation.FullName,
				                            String.Join(", ", methodNames));

				throw new FacilityException(message);
			}

			return true;
		}

		/// <summary>
		///   Determines whether the model has implicit synchronization.
		/// </summary>
		/// <param name = "model">The model.</param>
		/// <returns>
		///   <c>true</c> if has implicit synchronization; otherwise, <c>false</c>.
		/// </returns>
		private static bool HasImplicitSynchronization(ComponentModel model)
		{
			return model.Implementation.Is<ISynchronizeInvoke>() || model.Implementation.Is<DispatcherObject>();
		}
	}
}